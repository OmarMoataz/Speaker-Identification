using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Recorder.DynamicTimeWarping;

namespace Recorder.MainFuctions
{
    //Dev: Abdelrahman Othman Helal
    static class FileOperations
    {
        public static void SaveSequenceInDatabase(Sequence ToBeSavedSequence, string UserName)
        {
            FileStream SavingStream = new FileStream("savedSequences.txt", FileMode.Append);
            StreamWriter Saving = new StreamWriter(SavingStream);
            double TempFeature = 0f;
            StringBuilder FramesRow = new StringBuilder();

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
            Saving.Close();
        }

        //Dev: Muhammad Gamal
        public static bool CheckIfUserExist(string name)
        {
            if (File.Exists("savedSequences.txt")) // Check if the file exists 
            {
                FileStream ReadingStream = new FileStream("savedSequences.txt", FileMode.Open);
                StreamReader Reading = new StreamReader(ReadingStream);

                string line;
                //string[] tokens;
                while (Reading.Peek() != -1) // Reading the lines in the file line by line from the file
                {
                    line = Reading.ReadLine();//Saving first line in string ( line )  
                    if (line.StartsWith("UserName"))// Check if line string starts with Username
                    {
                        if (line.Contains(name))// Check if the string contains any previous IDs
                        {
                            Reading.Close(); // Close the file if he found the ID  
                            return true;
                        }
                    }
                }

                Reading.Close();
                return false;
            }
            return false;
        }

        //======================================================
        /* 
        Dev: Omar Moataz Abdel-Wahed Attia
        Last Edit: 12/8/2015
        To understand the code in function GetUserName, you need to understand the file structure
        I'm  looping over.
        The file will contain 13 lines which represent a Frame (0-12) (Each Column is a frame)
        on the 14th line, it will contain the name of the person that's tied to the previous sequence.
        */
        //======================================================
        public static string GetUserName(Sequence sequence) 
        {
            String NameOfUserWithMinimumDifference = "";
            //The Value returned from this function, contains the name of the person that's the closest match.
            double MinimumDistanceBetweenTwoSequences = double.MaxValue;
            //Holds the value of the closest after comparing all the sequences to the sequence required.
            using (StreamReader Reader = new StreamReader("savedSequences.txt"))
            {
                //Opening the file.
                Sequence ToBeCompared = new Sequence();
                //Initializing a new sequence.
                string Line;
                //This line string contatins every line I go through in the file
                int Index = 0;
                //Holds the value of the current frame
                bool flag = true;
                while ((Line = Reader.ReadLine()) != null)
                {
                    if (Index == 13)
                    {
                        double LowerBoundDistance = DynamicTimeWarpingOperations.LowerBound_Kim(sequence, ToBeCompared);
                        if (LowerBoundDistance > MinimumDistanceBetweenTwoSequences) goto skip;
                        double TrueDistance = DynamicTimeWarpingOperations.DTW_Distance(sequence, ToBeCompared);
                        //Consider Current a temp variable that holds the minimum distance returned from comparing the two sequences.
                        if (TrueDistance < MinimumDistanceBetweenTwoSequences)
                        //Here I compare the two Distances together to see if I need to update the minimum or not.
                        {
                            MinimumDistanceBetweenTwoSequences = TrueDistance;
                            //Here I update the minimum distance between two values.
                            NameOfUserWithMinimumDifference = Line;
                            //I update the name of the person to line because on the 13th index line, it'll have the name of the person.
                        }
                    skip:
                        flag = true;
                        ToBeCompared = new Sequence();
                        //This is a reinitialization just to clear out old values from the previous iteration
                        Index = -1;
                        //Initialize the index to -1 because it will be incremented at the end of this loop so, I want the value to be 0
                    }
                    else
                    {
                        string[] ExtractedStringsFromLine = Line.Split('|');

                        if (flag == true)
                        {
                            ToBeCompared.Frames = new MFCCFrame[ExtractedStringsFromLine.Length - 1];
                        }
                        //Here I split all the values from every line to an array of Strings.
                        for (int i = 0; i < ExtractedStringsFromLine.Length - 1; i++)
                        {
                            if (flag == true)
                            {
                                ToBeCompared.Frames[i] = new MFCCFrame();
                            }

                            ToBeCompared.Frames[i].Features[Index] = double.Parse(ExtractedStringsFromLine[i]);
                        }
                        flag = false;

                    }
                    ++Index;
                    //I increment the index of the 2D array for the next iteration through the file.
                }
            }
            //I return the name of the closest match.
            return NameOfUserWithMinimumDifference;
        }
    }
}
