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
    }
}
