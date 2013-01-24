﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AlohaKumu.Models;
using TestApp.Models;

namespace AlohaKumu.Models
{
    public class DataAccessor
    {
        private static readonly DataClasses1DataContext database = new DataClasses1DataContext();

        public static void recordTrialBlock(TrialBlockData results)
        {
            TrialBlock newBlock = new TrialBlock();
            newBlock.StartTime = results.taken;
            newBlock.UserID = results.userID;
            database.TrialBlocks.InsertOnSubmit(newBlock);
            database.SubmitChanges();
            int trialcount = results.clickID1s.Length;
            for (int i = 0; i < trialcount; i++)
            {
                Trial newTrial = new Trial();
                newTrial.TrialBlockID = newBlock.ID;
                newTrial.WordID = results.words[i];
                newTrial.TimeFirstIDpresented = results.showID1s[i];
                newTrial.TimeFirstIDclicked = results.clickID1s[i];
                newTrial.TimeSecondIDpresented = results.showID2s[i];
                newTrial.TimeSecondIDclicked = results.clickID2s[i];
                newTrial.TimeOptionsPresented = results.optionsShown[i];
                newTrial.Option1ID = results.optionIDs[i][0];
                newTrial.Option2ID = results.optionIDs[i][1];
                newTrial.Option3ID = results.optionIDs[i][2];
                newTrial.TimeOptionClicked = results.clickOptionTimes[i];
                newTrial.OptionIDClicked = results.optionIDsClicked[i];
                newTrial.TrialTypeID = 1;
                //^ magic number marks this a SeeSelect trial, dev out later
                database.Trials.InsertOnSubmit(newTrial);
                database.SubmitChanges();
            }
        }

        public static bool isUser(String name)
        {
            IQueryable<User> users = getUsers();
            foreach (User u in users)
            {
                if (u.Username == name) return true;
            }
            return false;
        }

        public static User login(String name, String pass)
        {
            User requested = (from u in database.Users
                              where (u.Username == name)
                              select u).Single();
            if (requested == null || requested.Password != pass) return null;
            return requested;
        }

        public static IQueryable<TrialBlock> userBlocks(User current)
        {
            return (from t in database.TrialBlocks
                    where (t.User == current)
                    select t);
        }

        public static bool allowTrial(User current)
        {
            IQueryable<TrialBlock> blocks = userBlocks(current);
            TrialBlock last = null;
            foreach (TrialBlock t in blocks)
            {
                if (last == null) last = t;
                else if (t.StartTime > last.StartTime) last = t;
            }
            if (last == null || (DateTime.Now - last.StartTime) > Settings.waitTime) return true;
            return false;
        }

        public static List<Word> getWordList(int listKey, int subListKey)
        {
            return (from w in database.Words
                    where (w.WordListID == listKey && w.WordSublistID == subListKey)
                    select w).ToList();
        }

        public static Word getWord(int key)
        {
            return (from w in database.Words
                    where (w.ID == key)
                    select w).Single();
        }

        public static IQueryable<User> getUsers()
        {
            return (from n in database.Users
                    select n);
        }

        //define logic of choosing which list here later
        public static List<Word> testList(User requested)
        {
            List<Word> list = getWordList(1,1);
            list.Shuffle();
            return list;
        }
    }
}