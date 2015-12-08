using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Recorder.MainFuctions
{
    static class FileOperations
    {
        public static void SaveSequenceInDatabase(Sequence ToBeSavedSequence, string UserName)
        {
            FileStream SavingStream = new FileStream("savedSeqs.txt", FileMode.Append);
            StreamWriter Saving = new StreamWriter(SavingStream);
            double TempFeature = 0f;
            StringBuilder FramesRow = new StringBuilder();

            Saving.WriteLine("#"); // Sequence Starts with this
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < ToBeSavedSequence.Frames.Length; j++)
                {
                    TempFeature = ToBeSavedSequence.Frames[j].Features[i];
                    FramesRow.Append(TempFeature.ToString() + "|");
                }
                Saving.WriteLine(FramesRow);
                FramesRow.Clear(); //clear it to start a new row (VIP)
            }

            Saving.WriteLine("UserName:" + UserName);
            Saving.WriteLine("@"); // Sequence Ends with this   
            Saving.Close();
        }

        public static bool CheckIfUserExist(string name)
        {
            if (File.Exists("savedSeqs.txt"))
            {
                FileStream ReadingStream = new FileStream("savedSeqs.txt", FileMode.Open);
                StreamReader Reading = new StreamReader(ReadingStream);
                string line;
                while (Reading.Peek() != -1)
                {
                    line = Reading.ReadLine();
                    if (line.StartsWith("UserName"))
                    {
                        if (line.Contains(name))
                        {
                            Reading.Close();
                            return true;
                        }
                    }
                }

                Reading.Close();
                return false;
            }
            return false;
        }

        public static string GetUserName(Sequence sequence) 
        {
            //sequence.Frames[i].Features[i]
            //loop on file
            //Call DynamicTimeWarping.DTW(sequence)
            return "";
        }
    }
}
