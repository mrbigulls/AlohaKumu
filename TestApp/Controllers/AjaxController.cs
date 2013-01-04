using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AlohaKumu.Models;

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

        public String getWordListKeys(int listKey)
        {
            List<Word> words = DataAccessor.getWordList(listKey);
            List<int> keys = new List<int>();
            foreach (Word w in words)
            {
                keys.Add(w.ID);
            }
            
            return serializer.Serialize(keys);
            //return this.Json(keys, JsonRequestBehavior.AllowGet);
        }

        public String getWord(int key)
        {
            return serializer.Serialize(new SimpleWord(DataAccessor.getWord(key)));
        }

        public String getWords(int listKey)
        {
            List<Word> words = DataAccessor.getWordList(listKey);
            List<SimpleWord> swords = new List<SimpleWord>();
            foreach (Word w in words)
            {
                swords.Add(new SimpleWord(w));
            }

            return serializer.Serialize(swords);
            //return this.Json(keys, JsonRequestBehavior.AllowGet);
        }
    }
}
