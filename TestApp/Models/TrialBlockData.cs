using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace AlohaKumu.Models
{
    public class TrialBlockData
    {
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        public String timeStarted { get; set; }
        public int userID { get; set; }
        public int goTime { get; set; }
        public int studyID { get; set; }
        public int typeID { get; set; }
        public String wordOrder { get; set; }
        public String ID1shown { get; set; }
        public String ID1times { get; set; }
        public String ID2shown { get; set; }
        public String ID2times { get; set; }
        public String choiceIDs { get; set; }
        public String optionsPresented { get; set; }
        public String guessTimes { get; set; }
        public String guessesMade { get; set; }

        //Strings are int arrays
        public DateTime taken;
        public int[] words;
        public int[] showID1s;
        public int[] clickID1s;
        public int[] showID2s;
        public int[] clickID2s;
        public int[][] optionIDs;
        public int[] optionsShown;
        public int[] clickOptionTimes;
        public int[] optionIDsClicked;

        public void parseStrings()
        {
            taken = serializer.Deserialize<DateTime>(timeStarted);
            words = serializer.Deserialize<int[]>(wordOrder);
            showID1s = serializer.Deserialize<int[]>(ID1shown);
            clickID1s = serializer.Deserialize<int[]>(ID1times);
            showID2s = serializer.Deserialize<int[]>(ID2shown);
            clickID2s = serializer.Deserialize<int[]>(ID2times);
            optionIDs = serializer.Deserialize<int[][]>(choiceIDs);
            optionsShown = serializer.Deserialize<int[]>(optionsPresented);
            clickOptionTimes = serializer.Deserialize<int[]>(guessTimes);
            optionIDsClicked = serializer.Deserialize<int[]>(guessesMade);
        }
    }
}