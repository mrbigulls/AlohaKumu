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
        public void saveTrialBlock(TrialBlockData results)
        {
            results.parseStrings();
            DataAccessor.recordTrialBlock(results);
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
        public string studyUpdate(int studyID, string hearIn, string seeIn, int hours, int minutes, int seconds, int trials, int fluency)
        {
            DataAccessor.updateStudy(studyID, hearIn, seeIn, hours, minutes, seconds, trials, fluency);
            return "Update successful.";
        }
    }
}
