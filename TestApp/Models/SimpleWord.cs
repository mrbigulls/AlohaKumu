using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AlohaKumu.Models;

namespace AlohaKumu.Models
{
    public class SimpleWord
    {
        public String english;
        public String hawaiian;
        public String spoken;
        public String picture;
        public String consequence;
        public int id;
        public int listID;

        public SimpleWord(Word complex)
        {
            english = complex.English;
            hawaiian = complex.Hawaiian;
            spoken = complex.HawaiianSpoken;
            picture = complex.Picture;
            consequence = complex.Consequence;
            id = complex.ID;
            listID = complex.WordListID;
        }
    }
}