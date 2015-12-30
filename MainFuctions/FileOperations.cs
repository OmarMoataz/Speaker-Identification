using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Recorder.DynamicTimeWarping;

namespace Recorder.MainFuctions
{
    //Dev: Omar Moataz Abdel-Wahed Attia
    public class  ClosestMatch
    {
        public string Username;    //Name of the closest match.
        public double MinimumDistance; //Minimum distance to the closest match.
        public ClosestMatch()
        {
            this.MinimumDistance = double.MaxValue;
            this.Username = "";
        }
    }
    //Dev: Abdelrahman Othman Helal
    static class FileOperations
    {
        public static void SaveSequenceInDatabase(Sequence toBeSavedSequence, string username, AudioSignal signal)
        {
            //UPDATE
            //you should save the four values in the last row before the username, with the order (first, last, min, max) respectively

            FileStream SavingStream = new FileStream("savedSequences.txt", FileMode.Append);
            StreamWriter Saving = new StreamWriter(SavingStream);
            double TempFeature = 0f;
            StringBuilder FramesRow = new StringBuilder();

            int size = signal.data.Length;
            double maxElement = double.MinValue,
                    minElement = double.MaxValue,
                    firstElement, lastElement;
            
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < toBeSavedSequence.Frames.Length; j++)
                {
                    TempFeature = toBeSavedSequence.Frames[j].Features[i];
                    FramesRow.Append(TempFeature.ToString() + "|");
                }
                Saving.WriteLine(FramesRow);
                FramesRow.Clear(); //clear it to start a new row (VIP)
            }

            firstElement = signal.data[0];
            lastElement = signal.data[size - 1];

            for (int i = 0; i < size; i++)
            {
                maxElement = Math.Max(maxElement, signal.data[i]);
                minElement = Math.Min(minElement, signal.data[i]);
            }

            // UPDATE: On 26/12 @ 6:50 - Writing the 4 values into the file
            Saving.WriteLine(firstElement + " " + lastElement + " " + minElement + " " + maxElement);
            Saving.WriteLine("Username:" + username);
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
                    if (line.StartsWith("Username"))// Check if line string starts with Username
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
        public static ClosestMatch GetUserName(Sequence sequence, AudioSignal signal, bool pruned) 
        {
            ClosestMatch User = new ClosestMatch();
            //Opening the file.
            using (StreamReader Reader = new StreamReader("savedSequences.txt"))
            {
                //Initializing a new sequence.
                Sequence ToBeCompared = new Sequence();
                //This line string contains every line I go through in the file
                string Line;
                //Holds the value of the current frame
                int Index = 0;
                bool flag = true, Updated = false;
                //Variables used in lowerbounding
                double FirstElement = 0, LastElement = 0, MaxElement = 0, MinElement = 0; 
                while ((Line = Reader.ReadLine()) != null)
                {
                    if (Index == 13)
                    {
                        double TrueDistance;
                        string[] Temp = Line.Split(' ');       /*Just a string array that holds the values I'll take into FirstElement, 
                        LastElement, MinElement and MaxElement.*/
                        FirstElement = double.Parse(Temp[0]);
                        LastElement = double.Parse(Temp[1]);
                        MinElement = double.Parse(Temp[2]);
                        MaxElement = double.Parse(Temp[3]);
                        
                        if (pruned)
                        {
                            double LowerBoundDistance = DynamicTimeWarpingOperations.LowerBound_Kim(signal, FirstElement, LastElement, MinElement, MaxElement);
                            if (LowerBoundDistance > User.MinimumDistance) goto skip;
                            TrueDistance = DynamicTimeWarpingOperations.Pruned_DTW_Distance(sequence, ToBeCompared);
                        }
                        else
                        {
                            TrueDistance = DynamicTimeWarpingOperations.DTW_Distance(sequence, ToBeCompared);
                        }
                        //Here I compare the two Distances together to see if I need to update the minimum or not.
                        if (TrueDistance < User.MinimumDistance)
                        {
                            //Here I update the minimum distance between two values.
                            User.MinimumDistance = TrueDistance;
                            Updated = true;
                        }
                    skip:
                        //This is a reinitialization just to clear out old values from the previous iteration
                        flag = true;
                        ToBeCompared = new Sequence();
                    }
                    else if(Index == 14)
                    {
                        if (Updated)
                        {
                            //I update the name of the person to line because on the 13th index line, it'll have the name of the person.
                            User.Username = Line.Substring(9, Line.Length - 9);
                        }
                        Updated = false; //resetting the update value.
                        Index = -1;
                        //So, it goes back to 0 when the loop continues.
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
                    //I increment the index of the 2D array for the next iteration through the file.
                    ++Index;
                }
            }
            //I return type ClosestMatch.
            return User;
        }
    }
}
