using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AlohaKumu.Models
{
    public class Settings
    {
        public static TimeSpan waitTime = new TimeSpan(0, 5, 0); //h,m,s
        public static String SeeSelectInstructions = "See-Select instructions appear here.  Please test your speakers with the audio clip below before participating.";
        public static String HearSelectInstructions = "Hear-Select instructions appear here.  Please test your speakers with the audio clip below before participating.";
        public static String ImagePath = "/Images/";
        public static int wordDisplaysPerTrialBlock = 2;
    }
}