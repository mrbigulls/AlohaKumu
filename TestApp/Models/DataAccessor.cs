using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AlohaKumu.Models;
using TestApp.Models;
using System.Security.Cryptography;
using System.Text;
using System.Data.Linq;
using System.Transactions;

namespace AlohaKumu.Models
{
    public class DataAccessor
    {
        private RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        private HashAlgorithm hasher = new SHA256Managed();

        private ASCIIEncoding encoder = new ASCIIEncoding();

        private StringBuilder stringer = new StringBuilder();

        private RefreshMode refresh_mode = RefreshMode.OverwriteCurrentValues;

        public bool validStudyAndUserIDs(int sid, int uid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (((from s in database.Studies
                        where (s.ID == sid)
                        select s).Count() > 0 )
                        &&
                    ((from u in database.Users
                      where (u.ID == uid)
                      select u).Count() > 0 ));
        }

        private String hashPass(String password, String salt)
        {
            StringBuilder stringer = new StringBuilder();
            byte[] hashed = hasher.ComputeHash(encoder.GetBytes(password + salt));
            for (int i = 0; i < hashed.Length; i++)
            {
                stringer.Append(hashed[i].ToString("x2"));
            }
            return stringer.ToString();
        }

        public String getSalt()
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
        
        public bool recordTrialBlock(TrialBlockData results)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
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
            return evalPerformance(database, newBlock.ID, newBlock.Study.TargetWordsPerMinute, results.studyID, results.userID);
        }

        public bool evalPerformance(DataClasses1DataContext database, int blockID, double target, int SID, int UID)
        {
            //double target = test.Study.TargetWordsPerMinute;
            double latency = 0.0;
            List<Trial> trials = (from t in database.Trials
                                  where t.TrialBlockID == blockID
                                  select t).OrderBy(x => x.TimeOptionClicked).ToList();
            int correct = 0;
            foreach (Trial t in trials)
            {
                latency = latency + (t.TimeOptionClicked - t.TimeSecondIDclicked);
                if (t.OptionIDClicked == t.WordID)
                {
                    correct++;
                }
            }
            if ( ((trials.Count / (latency / 60000)) >= target) && (correct == trials.Count))
            {
                //return advanceUserInStudy(database, SID, UID);
                database.Dispose();
                return advanceUserInStudy(SID, UID);
            }
            return false;
        }

        public List<Trial> blockTrials(TrialBlock block)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            List<Trial> trials = (from t in database.Trials
                                  where t.TrialBlockID == block.ID
                                  select t).ToList();
            return trials.OrderBy(x => x.TimeOptionClicked).ToList();
        }

        //public bool advanceUserInStudy(DataClasses1DataContext database, int SID, int UID)
        public bool advanceUserInStudy(int SID, int UID)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            StudiesUser u = (from user in database.StudiesUsers
                             where user.StudyID == SID && user.UserID == UID
                             select user).Single();
            //database.Refresh(refresh_mode, new Object[] {u, database.WordSublists, database.TrialTypes});
            StudyUserGroup g = u.StudyUserGroup;
            //database.Refresh(refresh_mode, g);

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
                u.WordSublistID++;
                u.Mix = false;
            }
            else if (!u.Mix)
            {
                if (u.WordSublistID == firstSublist) //nothing to mix at the beginning
                {
                    u.WordSublistID++;
                }
                else
                {
                    u.Mix = true;
                }
            }
            else if (u.TrialTypeID < lastType)
            {
                u.WordSublistID = firstSublist;
                u.TrialTypeID++;
                u.Mix = false;
            }
            else if (u.WordListID == g.FirstListID)
            {
                u.WordListID = g.SecondListID;
                u.WordSublistID = firstSublist;
                u.TrialTypeID = firstType;
            }
            else
            {
                u.Complete = true;
            }
            database.SubmitChanges();
            return u.Complete;
        }

        public WordSublist getSublistByID(int id)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from sl in database.WordSublists
                    where sl.ID == id
                    select sl).Single();
        }

        public TrialType getTrialTypeByID(int id)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from tt in database.TrialTypes
                    where tt.ID == id
                    select tt).Single();
        }

        public bool isUser(String name)
        {
            IQueryable<User> users = getUsers();
            foreach (User u in users)
            {
                if (u.Username == name) return true;
            }
            return false;
        }

        public bool isAdmin(String name)
        {
            IQueryable<Admin> admins = getAdmins();
            foreach (Admin a in admins)
            {
                if (a.Username == name) return true;
            }
            return false;
        }

        public User login(String name, String pass)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            User requested = (from u in database.Users
                              where (u.Username == name)
                              select u).Single();
            if (requested == null) return null;
            if (requested.PassHash != hashPass(pass, requested.Salt)) return null;
            return requested;
        }

        public Admin loginAdmin(String name, String pass)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            Admin requested = (from u in database.Admins
                              where (u.Username == name)
                              select u).Single();
            if (requested == null) return null;
            if (requested.PassHash != hashPass(pass, requested.Salt)) return null;
            return requested;
        }

        public List<TrialBlock> userBlocks(User current)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from t in database.TrialBlocks
                    where (t.UserID == current.ID)
                    select t).ToList();
        }

        public List<TrialBlock> userStudyBlocks(int uid, int sid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from t in database.TrialBlocks
                    where (t.UserID == uid && t.StudyID == sid)
                    select t).ToList();
        }

        public bool? allowTrial(User current)
        {
            //database.Refresh(refresh_mode, current);
            StudiesUser currentStudy = studiesUserFromUser(current);
            if (currentStudy.TrialType.Name == "Completed") return null;
            return true; //for rapid testing
            /*
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
            */
        }

        public List<Word> getWordList(int listKey, int subListKey, bool mixed)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
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

        public Word getWord(int key)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from w in database.Words
                    where (w.ID == key)
                    select w).Single();
        }

        public IQueryable<User> getUsers()
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from n in database.Users
                    select n);
        }

        public IQueryable<Admin> getAdmins()
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from n in database.Admins
                    select n);
        }

        public List<Word> testList(User requested)
        {
            //database.Refresh(refresh_mode, requested);
            StudiesUser current = studiesUserFromUser(requested);
            List<Word> list = getWordList(current.WordListID, current.WordSublistID, current.Mix);
            return list;
        }

        public String testType(User requested)
        {
            //database.Refresh(refresh_mode, requested);
            return studiesUserFromUser(requested).TrialType.Name;
        }

        public StudiesUser studiesUserFromUser(User requested)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from su in database.StudiesUsers
                    join s in database.Studies
                    on su.StudyID equals s.ID
                    where (su.UserID == requested.ID)
                    select su).SingleOrDefault();
        }

        public List<StudiesUser> allStudiesFromUserID(int uid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from su in database.StudiesUsers
                    where su.UserID == uid
                    select su).ToList();
        }

        public int studyIDFromUser(User requested)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from su in requested.StudiesUsers
                    join st in database.Studies
                    on su.StudyID equals st.ID
                    select st.ID).Single();
        }

        public int getTrialTypeKeyFromName(String name)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from t in database.TrialTypes
                    where (t.Name == name)
                    select t.ID).Single();
        }

        public List<String> getTrialTypeNames()
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from t in database.TrialTypes
                    select t.Name).ToList();
        }

        public bool receiveSoundFeedback(User current)
        {
            //database.Refresh(refresh_mode, current);
            StudiesUser su = studiesUserFromUser(current);
            bool startControl = su.StudyUserGroup.StartControl;
            if (su.StudyUserGroup.FirstListID == su.WordListID)
                return startControl;
            else return !startControl;
        }

        public Study studyFromID(int sid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from s in database.Studies
                    where (s.ID == sid)
                    select s).Single();
        }

        public List<Study> getStudiesByAdminID(int aid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from sa in database.StudiesAdmins
                    join s in database.Studies
                    on sa.StudyID equals s.ID
                    where sa.AdminID == aid
                    select s).ToList();
        }

        public List<Study> getAllStudies()
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from s in database.Studies
                    select s).ToList();
        }

        public List<WordList> getWordLists()
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from wl in database.WordLists
                    select wl).ToList();
        }

        public List<WordSublist> getWordSublists()
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from wl in database.WordSublists
                    select wl).ToList();
        }

        public List<StudyUserGroup> getUserGroups()
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from sug in database.StudyUserGroups
                    select sug).ToList();
        }

        public User getUserByID(int uid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from u in database.Users
                    where u.ID == uid
                    select u).Single();
        }

        public Study getStudyByID(int sid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from s in database.Studies
                    where s.ID == sid
                    select s).Single();
        }

        public StudyUserGroup getStudyUserGroupByID(int sugid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            return (from sug in database.StudyUserGroups
                    where sug.ID == sugid
                    select sug).Single();
        }

        public void updateUser(int userID, bool userActive, string userPassword)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            User u = getUserByID(userID);
            u.Active = userActive;
            if( userPassword != "" ) u.PassHash = hashPass(userPassword, u.Salt);
            database.SubmitChanges();
        }

        public void updateStudiesUser(int userID, int studyID, int studyUserGroupID)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
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

        public bool userEligibileToMove(int userID)
        {
            List<StudiesUser> studies = allStudiesFromUserID(userID);
            bool eligibileToMove = true;
            foreach (StudiesUser su in studies)
            {
                List<TrialBlock> tb = userStudyBlocks(userID, su.StudyID);
                if (!su.Complete && tb.Count > 0)
                {
                    eligibileToMove = false;
                }
            }
            return eligibileToMove;
        }

        public void updateStudy(int studyID, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, double fluency)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
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

        public User createUser(string userName, bool userActive, string userPassword)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            User newUser = new User();
            newUser.Username = userName;
            newUser.Salt = getSalt();
            newUser.PassHash = hashPass(userPassword, newUser.Salt);
            newUser.Active = userActive;
            database.Users.InsertOnSubmit(newUser);
            database.SubmitChanges();
            return newUser;
        }

        public int StudyUserGroupStartListKey(int sugID)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            StudyUserGroup sug = (from s in database.StudyUserGroups
                                  where (s.ID == sugID)
                                  select s).Single();
            return sug.FirstListID;
        }

        public StudiesUser createStudiesUsers(int userID, int studyID, int studyUserGroupID)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
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
        
        public Study createStudy(string studyName, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, int target)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
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

        public StudiesAdmin createStudiesAdmin(int sid, int aid)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            StudiesAdmin sa = new StudiesAdmin();
            sa.AdminID = aid;
            sa.StudyID = sid;
            database.StudiesAdmins.InsertOnSubmit(sa);
            database.SubmitChanges();
            return sa;
        }

        public bool deleteUser(int doomedID)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            using (var transaction = new TransactionScope())
            {
                User doomed = (from u in database.Users
                               where u.ID == doomedID
                               select u).Single();
                foreach(TrialBlock tb in doomed.TrialBlocks)
                {
                    database.Trials.DeleteAllOnSubmit(tb.Trials);
                }
                database.TrialBlocks.DeleteAllOnSubmit(doomed.TrialBlocks);
                database.StudiesUsers.DeleteAllOnSubmit(doomed.StudiesUsers);
                database.Users.DeleteOnSubmit(doomed);
                database.SubmitChanges();
                transaction.Complete();
            }
            return true;
        }

        public bool deleteStudy(int doomedID)
        {
            DataClasses1DataContext database = new DataClasses1DataContext();
            using (var transaction = new TransactionScope())
            {
                Study doomed = (from d in database.Studies
                                where d.ID == doomedID
                                select d).Single();
                foreach (TrialBlock tb in doomed.TrialBlocks)
                {
                    database.Trials.DeleteAllOnSubmit(tb.Trials);
                }
                database.TrialBlocks.DeleteAllOnSubmit(doomed.TrialBlocks);
                IEnumerable<User> users = (from u in database.Users
                                    join su in database.StudiesUsers on u.ID equals su.UserID
                                    where su.StudyID == doomedID
                                    select u);
                database.StudiesUsers.DeleteAllOnSubmit(doomed.StudiesUsers);
                database.Users.DeleteAllOnSubmit(users);
                database.StudiesAdmins.DeleteAllOnSubmit(doomed.StudiesAdmins);
                database.Studies.DeleteOnSubmit(doomed);
                database.SubmitChanges();
                transaction.Complete();
            }
            return true;
        }
    }
}