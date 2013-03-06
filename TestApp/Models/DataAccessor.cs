using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AlohaKumu.Models;
using TestApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace AlohaKumu.Models
{
    public class DataAccessor
    {
        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        private static HashAlgorithm hasher = new SHA256Managed();

        private static ASCIIEncoding encoder = new ASCIIEncoding();

        private static StringBuilder stringer = new StringBuilder();

        private static readonly DataClasses1DataContext database = new DataClasses1DataContext();

        private static String hashPass(String password, String salt)
        {
            StringBuilder stringer = new StringBuilder();
            byte[] hashed = hasher.ComputeHash(encoder.GetBytes(password + salt));
            for (int i = 0; i < hashed.Length; i++)
            {
                stringer.Append(hashed[i].ToString("x2"));
            }
            return stringer.ToString();
        }

        public static String getSalt()
        {
            byte[] salt = new byte[32];
            rng.GetNonZeroBytes(salt);
            StringBuilder stringer = new StringBuilder();
            for (int i = 0; i < salt.Length; i++)
            {
                stringer.Append(salt[i].ToString("x2"));
            }
            return stringer.ToString();
        }
        
        public static void recordTrialBlock(TrialBlockData results)
        {
            TrialBlock newBlock = new TrialBlock();
            newBlock.StudyID = results.studyID;
            newBlock.StartTime = results.taken;
            newBlock.UserID = results.userID;
            newBlock.TrialTypeID = results.typeID;
            database.TrialBlocks.InsertOnSubmit(newBlock);
            database.SubmitChanges();
            int trialcount = results.clickID1s.Length;
            for (int i = 0; i < trialcount; i++)
            {
                Trial newTrial = new Trial();
                newTrial.TrialBlockID = newBlock.ID;
                newTrial.WordID = results.words[i];
                newTrial.TimeFirstIDpresented = results.showID1s[i];
                newTrial.TimeFirstIDclicked = results.clickID1s[i];
                newTrial.TimeSecondIDpresented = results.showID2s[i];
                newTrial.TimeSecondIDclicked = results.clickID2s[i];
                newTrial.TimeOptionsPresented = results.optionsShown[i];
                newTrial.Option1ID = results.optionIDs[i][0];
                newTrial.Option2ID = results.optionIDs[i][1];
                newTrial.Option3ID = results.optionIDs[i][2];
                newTrial.TimeOptionClicked = results.clickOptionTimes[i];
                newTrial.OptionIDClicked = results.optionIDsClicked[i];
                database.Trials.InsertOnSubmit(newTrial);
                database.SubmitChanges();
            }
            evalPerformance(newBlock);
        }

        public static void evalPerformance(TrialBlock test)
        {
            double target = test.Study.TargetWordsPerMinute;
            List<Trial> trials = blockTrials(test);
            int correct = 0;
            foreach (Trial t in trials)
            {
                if (t.OptionIDClicked == t.WordID) correct++;
            }
            double finish = trials[trials.Count - 1].TimeOptionClicked / 60000; //milliseconds to minutes
            double rate = 0;
            if(correct > 0) rate = correct / finish;
            if(rate >= target) advanceUserInStudy(studiesUserFromUser(test.User));
        }

        public static List<Trial> blockTrials(TrialBlock block)
        {
            List<Trial> trials = (from t in database.Trials
                                  where t.TrialBlockID == block.ID
                                  select t).ToList();
            return trials.OrderBy(x => x.TimeOptionClicked).ToList();
        }

        public static void advanceUserInStudy(StudiesUser u)
        {
            var subkeys = (from s in database.WordSublists
                           select s.ID);
            var typekeys = (from s in database.TrialTypes
                            select s.ID);

            int firstSublist = subkeys.Min();
            int lastSublist = subkeys.Max();
            int lastType = typekeys.Max();

            if ((u.WordSublistID < lastSublist) && u.Mix)
            {
                u.WordSublist = getSublistByID(u.WordSublistID + 1);
                u.Mix = false;
            }
            else if (!u.Mix)
            {
                u.Mix = true;
            }
            else if (u.TrialTypeID < lastType)
            {
                u.WordSublist = getSublistByID(firstSublist);
                u.TrialType = getTrialTypeByID(u.TrialTypeID + 1);
                u.Mix = false;
            }
            database.SubmitChanges();
        }

        public static WordSublist getSublistByID(int id)
        {
            return (from sl in database.WordSublists
                    where sl.ID == id
                    select sl).Single();
        }

        public static TrialType getTrialTypeByID(int id)
        {
            return (from tt in database.TrialTypes
                    where tt.ID == id
                    select tt).Single();
        }

        public static bool isUser(String name)
        {
            IQueryable<User> users = getUsers();
            foreach (User u in users)
            {
                if (u.Username == name) return true;
            }
            return false;
        }

        public static bool isAdmin(String name)
        {
            IQueryable<Admin> admins = getAdmins();
            foreach (Admin a in admins)
            {
                if (a.Username == name) return true;
            }
            return false;
        }

        public static User login(String name, String pass)
        {
            User requested = (from u in database.Users
                              where (u.Username == name)
                              select u).Single();
            if (requested == null) return null;
            if (requested.PassHash != hashPass(pass, requested.Salt)) return null;
            return requested;
        }

        public static Admin loginAdmin(String name, String pass)
        {
            Admin requested = (from u in database.Admins
                              where (u.Username == name)
                              select u).Single();
            if (requested == null) return null;
            if (requested.PassHash != hashPass(pass, requested.Salt)) return null;
            return requested;
        }

        public static List<TrialBlock> userBlocks(User current)
        {
            return (from t in database.TrialBlocks
                    where (t.UserID == current.ID)
                    select t).ToList();
        }

        public static bool? allowTrial(User current)
        {
            StudiesUser currentStudy = studiesUserFromUser(current);
            if (currentStudy.TrialType.Name == "Completed") return null;
            return true; //for rapid testing
            /*
            List<TrialBlock> blocks = userBlocks(current);
            TrialBlock last = null;
            foreach (TrialBlock t in blocks)
            {
                if (last == null) last = t;
                else if ( DateTime.Compare(t.StartTime, last.StartTime) > 0 ) last = t;
            }
            if (last == null || (DateTime.Compare(DateTime.Now.Date, last.StartTime.Date) > 0) || (DateTime.Now - last.StartTime) > currentStudy.Study.getWaitTime() ) return true;
            return false;
            */
        }

        public static List<Word> getWordList(int listKey, int subListKey, bool mixed)
        {
            List<Word> fullList = Enumerable.Empty<Word>().ToList();
            List<Word> subList;
            if (mixed)
            {
                for (int i = 1; i <= subListKey; i++)
                {
                    subList = (from w in database.Words
                               where (w.WordListID == listKey && w.WordSublistID == i)
                               select w).ToList();
                    fullList = fullList.Concat(subList).ToList();
                }
            }
            else
            {
                fullList = (from w in database.Words
                            where (w.WordListID == listKey && w.WordSublistID == subListKey)
                            select w).ToList();
            }
            return fullList;
        }

        public static Word getWord(int key)
        {
            return (from w in database.Words
                    where (w.ID == key)
                    select w).Single();
        }

        public static IQueryable<User> getUsers()
        {
            return (from n in database.Users
                    select n);
        }

        public static IQueryable<Admin> getAdmins()
        {
            return (from n in database.Admins
                    select n);
        }

        public static List<Word> testList(User requested)
        {
            StudiesUser current = studiesUserFromUser(requested);
            List<Word> list = getWordList(current.WordListID, current.WordSublistID, current.Mix);
            return list;
        }

        public static String testType(User requested)
        {
            return studiesUserFromUser(requested).TrialType.Name;
        }

        public static StudiesUser studiesUserFromUser(User requested)
        {
            return (from su in database.StudiesUsers
                    join s in database.Studies
                    on su.StudyID equals s.ID
                    where (su.UserID == requested.ID) && (s.Active)
                    select su).Single();
        }

        public static int studyIDFromUser(User requested)
        {
            return (from su in requested.StudiesUsers
                    join st in database.Studies
                    on su.StudyID equals st.ID
                    where st.Active
                    select st.ID).Single();
        }

        public static int getTrialTypeKeyFromName(String name)
        {
            return (from t in database.TrialTypes
                    where (t.Name == name)
                    select t.ID).Single();
        }

        public static List<String> getTrialTypeNames()
        {
            return (from t in database.TrialTypes
                    select t.Name).ToList();
        }

        public static bool currentlyControlGroup(User current)
        {
            return studiesUserFromUser(current).ControlGroup;
        }

        public static Study studyFromID(int sid)
        {
            return (from s in database.Studies
                    where (s.ID == sid)
                    select s).Single();
        }

        public static List<Study> getStudiesByAdminID(int aid)
        {
            return (from sa in database.StudiesAdmins
                    join s in database.Studies
                    on sa.StudyID equals s.ID
                    where sa.AdminID == aid
                    select s).ToList();
        }

        public static List<WordList> getWordLists()
        {
            return (from wl in database.WordLists
                    select wl).ToList();
        }

        public static List<WordSublist> getWordSublists()
        {
            return (from wl in database.WordSublists
                    select wl).ToList();
        }
    }
}