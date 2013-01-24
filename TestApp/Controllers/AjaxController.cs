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
        /*
        public String getWordListKeys(int listKey, int subListKey)
        {
            List<Word> words = DataAccessor.getWordList(listKey, subListKey);
            List<int> keys = new List<int>();
            foreach (Word w in words)
            {
                keys.Add(w.ID);
            }
            
            return serializer.Serialize(keys);
            //return this.Json(keys, JsonRequestBehavior.AllowGet);
        }
        */
        public String getWord(int key)
        {
            return serializer.Serialize(new SimpleWord(DataAccessor.getWord(key)));
        }
        /*
        public String getWords(int listKey, int subListKey)
        {
            List<Word> words = DataAccessor.getWordList(listKey, subListKey);
            List<SimpleWord> swords = new List<SimpleWord>();
            foreach (Word w in words)
            {
                swords.Add(new SimpleWord(w));
            }

            return serializer.Serialize(swords);
            //return this.Json(keys, JsonRequestBehavior.AllowGet);
        }
        */
        [HttpPost]
        public void saveTrialBlock(TrialBlockData results)
        {
            results.parseStrings();
            DataAccessor.recordTrialBlock(results);
        }
    }
}
