using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AlohaKumu.Models;
using TestApp.Models;

namespace AlohaKumu.Models
{
    public class SimpleWord
    {
        public String english;
        public String hawaiian;
        public String spoken;
        public String picture;
        public int consequenceID;
        public int id;
        public int listID;

        public SimpleWord(Word complex)
        {
            english = complex.English;
            hawaiian = complex.Hawaiian;
            spoken = complex.HawaiianSpoken;
            picture = complex.Picture;
            consequenceID = complex.Consequence.ID;
            id = complex.ID;
            listID = complex.WordListID;
        }
    }
}