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

        public static bool validStudyAndUserIDs(int sid, int uid)
        {
            return (((from s in database.Studies
                        where (s.ID == sid)
                        select s).Count() > 0 )
                        &&
                    ((from u in database.Users
                      where (u.ID == uid)
                      select u).Count() > 0 ));
        }

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
        
        public static bool recordTrialBlock(TrialBlockData results)
        {
            StudiesUser x = (from su in database.StudiesUsers
                             where su.StudyID == results.studyID && su.UserID == results.userID
                             select su).Single();
            TrialBlock newBlock = new TrialBlock();
            newBlock.StudyID = results.studyID;
            newBlock.StartTime = results.taken;
            newBlock.UserID = results.userID;
            newBlock.TrialTypeID = results.typeID;
            newBlock.WordListID = x.WordListID;
            newBlock.WordSublistID = x.WordSublistID;
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
            return evalPerformance(newBlock);
        }

        public static bool evalPerformance(TrialBlock test)
        {
            double target = test.Study.TargetWordsPerMinute;
            List<Trial> trials = blockTrials(test);
            int correct = 0;
            foreach (Trial t in trials)
            {
                if (t.OptionIDClicked == t.WordID) correct++;
            }
            double finish = trials[trials.Count - 1].TimeOptionClicked / 60000; //milliseconds to minutes
            double rate = trials.Count / finish;
            if (rate >= target && correct == trials.Count) return advanceUserInStudy(studiesUserFromUser(test.User));
            return false;
        }

        public static List<Trial> blockTrials(TrialBlock block)
        {
            List<Trial> trials = (from t in database.Trials
                                  where t.TrialBlockID == block.ID
                                  select t).ToList();
            return trials.OrderBy(x => x.TimeOptionClicked).ToList();
        }

        public static bool advanceUserInStudy(StudiesUser u)
        {
            StudyUserGroup g = u.StudyUserGroup;

            var subkeys = (from s in database.WordSublists
                           select s.ID);
            var typekeys = (from s in database.TrialTypes
                            select s.ID);

            int firstSublist = subkeys.Min();
            int lastSublist = subkeys.Max();
            int firstType = typekeys.Min();
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
            else if (u.WordListID == g.FirstListID)
            {
                u.WordListID = g.SecondListID;
                u.WordSublist = getSublistByID(firstSublist);
                u.TrialType = getTrialTypeByID(firstType);
            }
            else
            {
                u.Complete = true;
            }
            database.SubmitChanges();
            return u.Complete;
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

        public static List<TrialBlock> userStudyBlocks(int uid, int sid)
        {
            return (from t in database.TrialBlocks
                    where (t.UserID == uid && t.StudyID == sid)
                    select t).ToList();
        }

        public static bool? allowTrial(User current)
        {
            StudiesUser currentStudy = studiesUserFromUser(current);
            if (currentStudy.TrialType.Name == "Completed") return null;
            //return true; //for rapid testing

            DateTime today = DateTime.Now.Date;

            List<TrialBlock> blocks = userBlocks(current);
            TrialBlock last = null;
            if (blocks.Count > 0) last = blocks[0];
            List<TrialBlock> todaysBlocks = new List<TrialBlock>();
            foreach (TrialBlock t in blocks)
            {
                if (today == t.StartTime.Date) todaysBlocks.Add(t);
                if (DateTime.Compare(t.StartTime, last.StartTime) > 0) last = t;
            }
            //none performed yet
            if (last == null) return true;
            //none today, and less than min wait since the last
            if ( (DateTime.Compare(today, last.StartTime.Date) > 0) && ((DateTime.Now - last.StartTime) > currentStudy.Study.getWaitTime())) return true;
            //fewer than 2 today
            if ( todaysBlocks.Count < 2) return true;
            //otherwise two have been performed today or not enough time since previous day
            return false;
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
                    where (su.UserID == requested.ID)
                    select su).Single();
        }

        public static List<StudiesUser> allStudiesFromUserID(int uid)
        {
            return (from su in database.StudiesUsers
                    where su.UserID == uid
                    select su).ToList();
        }

        public static int studyIDFromUser(User requested)
        {
            return (from su in requested.StudiesUsers
                    join st in database.Studies
                    on su.StudyID equals st.ID
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

        public static bool receiveSoundFeedback(User current)
        {
            StudiesUser su = studiesUserFromUser(current);
            bool startControl = su.StudyUserGroup.StartControl;
            if (su.StudyUserGroup.FirstListID == su.WordListID)
                return startControl;
            else return !startControl;
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

        public static List<Study> getAllStudies()
        {
            return (from s in database.Studies
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

        public static List<StudyUserGroup> getUserGroups()
        {
            return (from sug in database.StudyUserGroups
                    select sug).ToList();
        }

        public static User getUserByID(int uid)
        {
            return (from u in database.Users
                    where u.ID == uid
                    select u).Single();
        }

        public static StudyUserGroup getStudyUserGroupByID(int sugid)
        {
            return (from sug in database.StudyUserGroups
                    where sug.ID == sugid
                    select sug).Single();
        }

        public static void updateUser(int userID, bool userActive, string userPassword)
        {
            User u = getUserByID(userID);
            u.Active = userActive;
            if( userPassword != "" ) u.PassHash = hashPass(userPassword, u.Salt);
            database.SubmitChanges();
        }

        public static void updateStudiesUser(int userID, int studyID, int studyUserGroupID)
        {
            StudiesUser current_su = studiesUserFromUser(getUserByID(userID));
            
            if (current_su != null)
            {
                database.StudiesUsers.DeleteOnSubmit(current_su);
                database.SubmitChanges();
            }

            StudiesUser nsu = new StudiesUser();
            nsu.UserID = userID;
            nsu.StudyID = studyID;
            nsu.UserGroupID = studyUserGroupID;
            nsu.Complete = false;
            nsu.Mix = false;
            nsu.TrialTypeID = 1;
            nsu.WordListID = 1;
            nsu.WordSublistID = 1;
            database.StudiesUsers.InsertOnSubmit(nsu);
            database.SubmitChanges();
        }

        public static bool userEligibileToMove(int userID)
        {
            List<StudiesUser> studies = DataAccessor.allStudiesFromUserID(userID);
            bool eligibileToMove = true;
            foreach (StudiesUser su in studies)
            {
                List<TrialBlock> tb = DataAccessor.userStudyBlocks(userID, su.StudyID);
                if (!su.Complete && tb.Count > 0)
                {
                    eligibileToMove = false;
                }
            }
            return eligibileToMove;
        }

        public static void updateStudy(int studyID, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, int fluency)
        {
            Study s = studyFromID(studyID);
            s.HearInstructions = hearIn;
            s.SeeInstructions = seeIn;
            s.WaitHours = hours;
            s.WaitMins = minutes;
            s.WaitSecs = seconds;
            s.WordTrialsPerBlock = trials;
            s.TargetWordsPerMinute = fluency;
            s.Active = true;
            database.SubmitChanges();
        }

        public static User createUser(string userName, bool userActive, string userPassword)
        {
            User newUser = new User();
            newUser.Username = userName;
            newUser.Salt = getSalt();
            newUser.PassHash = hashPass(userPassword, newUser.Salt);
            newUser.Active = userActive;
            database.Users.InsertOnSubmit(newUser);
            database.SubmitChanges();
            return newUser;
        }

        public static int StudyUserGroupStartListKey(int sugID)
        {
            StudyUserGroup sug = (from s in database.StudyUserGroups
                                  where (s.ID == sugID)
                                  select s).Single();
            return sug.FirstListID;
        }

        public static StudiesUser createStudiesUsers(int userID, int studyID, int studyUserGroupID)
        {
            StudiesUser su = new StudiesUser();
            su.StudyID = studyID;
            su.Complete = false;
            su.Mix = false;
            su.UserGroupID = studyUserGroupID;
            su.UserID = userID;
            su.WordSublistID = 1;

            /*
             * Ugly, but I designed the database wrong on this point.
             * StudyUserGroups.StartControl is a bool but should be a FK to TrialType
             * For now, use "True" as meaning "See Select" (1) and "False" as "Hear Select" (2)
             */

            su.WordListID = StudyUserGroupStartListKey(studyUserGroupID);
            su.TrialTypeID = 1;
            database.StudiesUsers.InsertOnSubmit(su);
            database.SubmitChanges();
            return su;
        }
        
        public static Study createStudy(string studyName, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, int target)
        {
            Study s = new Study();
            s.Name = studyName;
            s.HearInstructions = hearIn;
            s.SeeInstructions = seeIn;
            s.WaitHours = hours;
            s.WaitMins = minutes;
            s.WaitSecs = seconds;
            s.WordTrialsPerBlock = trials;
            s.TargetWordsPerMinute = target;
            s.Active = true;
            database.Studies.InsertOnSubmit(s);
            database.SubmitChanges();
            return s;
        }

        public static StudiesAdmin createStudiesAdmin(int sid, int aid)
        {
            StudiesAdmin sa = new StudiesAdmin();
            sa.AdminID = aid;
            sa.StudyID = sid;
            database.StudiesAdmins.InsertOnSubmit(sa);
            database.SubmitChanges();
            return sa;
        }
    }
}