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
            FileStream SavingStream = new FileStream("savedSeqs.txt", FileMode.Append);
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
<<<<<<< HEAD
            Saving.WriteLine("UserName:" + UserName);  
=======

            Saving.WriteLine("UserName:" + UserName);
>>>>>>> 45a01955c9381c92586b0b7bbfb8fa10f0f4f993
            Saving.Close();
        }

        //Dev: Muhammad Gamal
        public static bool CheckIfUserExist(string name)
        {
            if (File.Exists("savedSeqs.txt")) // Check if the file exists 
            {
                FileStream ReadingStream = new FileStream("savedSeqs.txt", FileMode.Open);
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

<<<<<<< HEAD
        //======================================================
        /* 
        To understand the code in function GetUserName, you need to understand the file structure
        I'm  looping over.
        The file will contain 13 lines which represent a Frame (0-12)
        on the 14th line, it will contain the name of the person that's tied to the previous sequence.
        */
        //======================================================
        public static string GetUserName(Sequence sequence)
        {
            string NameOfMinimumDistance = "";
=======

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
            String NameOfMinimumDistance = "";
>>>>>>> 45a01955c9381c92586b0b7bbfb8fa10f0f4f993
            //The Value returned from this function, contains the name of the person that's the closest match.
            double MinimumDistanceBetweenTwoSequences = 1e9;
            //Holds the value of the closest after comparing all the sequences to the sequence required.
            using (StreamReader Reader = new StreamReader("savedSeqs.txt"))
            {
                //Opening the file.
<<<<<<< HEAD
                Sequence ToBeCompared = sequence;
                //Initializing a new sequence.
                string Line;
                //This line string contatins every line I go through in the file
                int Index = 0;
                //Holds the value of the current frame
=======
                Sequence ToBeCompared = new Sequence();
                //Initializing a new sequence.
                string Line;
                //This line string contatins every line I go through in the file.
                int Index = 0;
                //Holds the value of the current frame.
>>>>>>> 45a01955c9381c92586b0b7bbfb8fa10f0f4f993
                while ((Line = Reader.ReadLine()) != null)
                {
                    if (Index == 13)
                    {
<<<<<<< HEAD
                        //flag = true;
=======
>>>>>>> 45a01955c9381c92586b0b7bbfb8fa10f0f4f993
                        double Current = DynamicTimeWarpingOperations.DTW_Distance(sequence, ToBeCompared);
                        //Consider Current a temp variable that holds the minimum distance returned from comparing the two sequences.
                        if (Current < MinimumDistanceBetweenTwoSequences)
                        //Here I compare the two Distances together to see if I need to update the minimum or not.
                        {
                            MinimumDistanceBetweenTwoSequences = Current;
                            //Here I update the minimum distance between two values.
                            NameOfMinimumDistance = Line;
                            //I update the name of the person to line because on the 13th index line, it'll have the name of the person.
                        }
<<<<<<< HEAD
                        ToBeCompared = sequence;
                        //This is a reinitialization just to clear out old values from the previous iteration
                        Index = -1;
                        //Initialize the index to -1 because it will be incremented at the end of this loop so, I want the value to be 0
                    }
                    else
                    {
                        string[] ExtractedStringsFromLine = Line.Split('|');
                        int maxNumberOfFramesToBeCompared = Math.Min(ExtractedStringsFromLine.Length - 1, sequence.Frames.Count());
                        //Here I split all the values from every line to an array of Strings.
                        for (int i = 0; i < maxNumberOfFramesToBeCompared; i++)
                        {
                            ToBeCompared.Frames[i].Features[Index] = double.Parse(ExtractedStringsFromLine[i]);
=======
                        ToBeCompared = new Sequence();
                        //This is a reinitialization just to clear out old values from the previous iteration.
                        Index = -1;
                        //Initialize the index to -1 because it will be incremented at the end of this loop so, I want the value to be 0.
                    }

                    else
                    {
                        string[] ExtractedStringsFromLine = Line.Split('|');
                        //Here I split all the values from every line to an array of Strings.
                        ToBeCompared.Frames = new MFCCFrame[ExtractedStringsFromLine.Length];
                        for (int i = 0; i < ExtractedStringsFromLine.Length-1; i++)
                        {
                            double Temp = double.Parse(ExtractedStringsFromLine[i]);
                            ToBeCompared.Frames[i].Features[Index] = Temp;
>>>>>>> 45a01955c9381c92586b0b7bbfb8fa10f0f4f993
                        }
                        //This loop iterates through every string value I read from the line and converts it to a double.
                    }
                    ++Index;
                    //I increment the index of the 2D array for the next iteration through the file.
                }
            }
            //I return the name of the closest match.
            return NameOfMinimumDistance;
        }
    }
}
