using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AlohaKumu.Models;
using TestApp.Models;

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
            return serializer.Serialize(new SimpleWord(DataAccessor.getWord(key)));
        }

        [HttpPost]
        public ActionResult adminPanel(int studyID, int userID)
        {
            if (DataAccessor.validStudyAndUserIDs(studyID, userID))
            {
                ViewData["UserID"] = userID;
                ViewData["StudyID"] = studyID;
                return PartialView("adminPanel", DataAccessor.userStudyBlocks(userID, studyID));
            }
            else
                return PartialView("SelectSomething");
        }

        [HttpPost]
        public string saveTrialBlock(TrialBlockData results)
        {
            results.parseStrings();
            bool complete = DataAccessor.recordTrialBlock(results);
            if (complete) return "You have now completed the study.  Thank you for participating.";
            bool? offerAnother = DataAccessor.allowTrial(DataAccessor.getUserByID(results.userID));
            if (offerAnother == null) return "You have now completed the study.  Thank you for participating.";
            else if (offerAnother == true) return "Your results have been recorded.  Please participate again today. <INPUT TYPE=\"button\" onClick=\"history.go(0)\" VALUE=\"Go again now.\">";
            return "Thank you for your continuing participation.  That will be all for today.";
        }

        [HttpPost]
        public bool checkUserMovable(int userID)
        {
            return DataAccessor.userEligibileToMove(userID);
        }

        [HttpPost]
        public string userUpdate(int userID, bool userActive, string userPassword, int studyID, int studyUserGroupID)
        {
            User u = DataAccessor.getUserByID(userID);
            User uu = DataAccessor.login(u.Username, userPassword);
            StudiesUser su = DataAccessor.studiesUserFromUser(u);
            bool movable = DataAccessor.userEligibileToMove(userID);

            if (DataAccessor.studyIDFromUser(u) != studyID || su.UserGroupID != studyUserGroupID)
            {
                if (!movable) return "Error: Cannot move user.";
                DataAccessor.updateStudiesUser(userID, studyID, studyUserGroupID);
            }
            
            if(uu == null || (u.Active != userActive) )
            {
                DataAccessor.updateUser(userID, userActive, userPassword);
            }
            return "Update successful.";
        }

        [HttpPost]
        public string userCreate(string userName, bool userActive, string userPassword, int studyID, int studyUserGroupID)
        {
            //this should be a transaction, but I'm not being that careful.
            User u = DataAccessor.createUser(userName, userActive, userPassword);
            StudiesUser su = DataAccessor.createStudiesUsers(u.ID, studyID, studyUserGroupID);
            return (u.Username + " is now part of " + su.Study.Name + ".");
        }

        [HttpPost]
        public string studyUpdate(int studyID, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, double fluency)
        {
            DataAccessor.updateStudy(studyID, hearIn, seeIn, hours, minutes, seconds, trials, fluency);
            return "Update successful.";
        }

        [HttpPost]
        public string studyCreate(string studyName, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, int target, int adminID)
        {
            Study s = DataAccessor.createStudy(studyName, hearIn, seeIn, hours, minutes, seconds, trials, target);
            StudiesAdmin sa = DataAccessor.createStudiesAdmin(s.ID, adminID);
            return ("Created " + s.Name + " with admin " + sa.Admin.Username + ".");
        }
    }
}
