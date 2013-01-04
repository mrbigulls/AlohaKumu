using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AlohaKumu.Models
{
    public class Settings
    {
        public static TimeSpan waitTime = new TimeSpan(12, 0, 0);
        public static String SeeSelectInstructions = "See-Select instructions appear here.";
        public static String HearSelectInstructions = "Hear-Select instructions appear here.";
        public static String ImagePath = "/Images/";
    }
}