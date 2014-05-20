using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AlohaKumu.Models;
using TestApp.Models;
using System.Transactions;

namespace TestApp.Controllers
{
    public class AjaxController : Controller
    {
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();
        //
        // GET: /Ajax/

        public ActionResult Index()
        {
            return View();
        }

        public String getWord(int key)
        {
            DataAccessor database = new DataAccessor();
            return serializer.Serialize(new SimpleWord(database.getWord(key)));
        }

        [HttpPost]
        public ActionResult adminPanel(int studyID, int userID)
        {
            DataAccessor database = new DataAccessor();
            if (database.validStudyAndUserIDs(studyID, userID))
            {
                ViewData["UserID"] = userID;
                ViewData["StudyID"] = studyID;
                return PartialView("adminPanel", database.userStudyBlocks(userID, studyID));
            }
            else
                return PartialView("SelectSomething");
        }

        [HttpPost]
        public string saveTrialBlock(TrialBlockData results)
        {
            DataAccessor database = new DataAccessor();
            results.parseStrings();
            bool complete = database.recordTrialBlock(results);
            if (complete) return "You have now completed the study.  Thank you for participating.";
            bool? offerAnother = database.allowTrial(database.getUserByID(results.userID));
            if (offerAnother == null) return "You have now completed the study.  Thank you for participating.";
            else if (offerAnother == true) return "Your results have been recorded.  Please participate again today. <INPUT TYPE=\"button\" onClick=\"history.go(0)\" VALUE=\"Go again now.\">";
            return "Thank you for your continuing participation.  That will be all for today.";
        }

        [HttpPost]
        public bool checkUserMovable(int userID)
        {
            DataAccessor database = new DataAccessor();
            return database.userEligibileToMove(userID);
        }

        [HttpPost]
        public string userUpdate(int userID, bool userActive, string userPassword, int studyID, int studyUserGroupID)
        {
            DataAccessor database = new DataAccessor();
            User u = database.getUserByID(userID);
            User uu = database.login(u.Username, userPassword);
            StudiesUser su = database.studiesUserFromUser(u);
            bool movable = database.userEligibileToMove(userID);

            if (database.studyIDFromUser(u) != studyID || su.UserGroupID != studyUserGroupID)
            {
                if (!movable) return "Error: Cannot move user.";
                database.updateStudiesUser(userID, studyID, studyUserGroupID);
            }
            
            if(uu == null || (u.Active != userActive) )
            {
                database.updateUser(userID, userActive, userPassword);
            }
            return "Update successful.";
        }

        [HttpPost]
        public string userCreate(string userName, bool userActive, string userPassword, int studyID, int studyUserGroupID)
        {
            DataAccessor database = new DataAccessor();
            User u;
            StudiesUser su;
            using (var transaction = new TransactionScope())
            {
                u = database.createUser(userName, userActive, userPassword);
                su = database.createStudiesUsers(u.ID, studyID, studyUserGroupID);
            }
            return (u.Username + " is now part of " + su.Study.Name + ".");
        }

        [HttpPost]
        public string studyUpdate(int studyID, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, double fluency)
        {
            DataAccessor database = new DataAccessor();
            database.updateStudy(studyID, hearIn, seeIn, hours, minutes, seconds, trials, fluency);
            return "Update successful.";
        }

        [HttpPost]
        public string studyCreate(string studyName, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, int target, int adminID)
        {
            DataAccessor database = new DataAccessor();
            Study s = database.createStudy(studyName, hearIn, seeIn, hours, minutes, seconds, trials, target);
            StudiesAdmin sa = database.createStudiesAdmin(s.ID, adminID);
            return ("Created " + s.Name + " with admin " + sa.Admin.Username + ".");
        }
    }
}