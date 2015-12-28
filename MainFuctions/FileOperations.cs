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
        public string UserName;    //Name of the closest match.
        public double MinimumDistance; //Minimum distance to the closest match.
    }
    //Dev: Abdelrahman Othman Helal
    static class FileOperations
    {
        public static void SaveSequenceInDatabase(Sequence toBeSavedSequence, string userName, AudioSignal signal)
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
            Saving.WriteLine("UserName:" + userName);
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
        public static ClosestMatch GetUserName(Sequence sequence, AudioSignal signal) 
        {
            //UPDATE1
            //you should fill the four variables (firstelement, lastelement, maxelement, minelement) with the 13th line, respectively
            //and return a struct with a string and double, representing the username and the minimumdistance, respectively

            ClosestMatch User = new ClosestMatch();
            User.UserName = "";
            //The Value returned from this function, contains the name of the person that's the closest match.
            User.MinimumDistance = double.MaxValue;
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
                bool flag = true, Updated = false;
                //Variables used in lowerbounding
                double FirstElement = 0, LastElement = 0, MaxElement = 0, MinElement = 0; 
                while ((Line = Reader.ReadLine()) != null)
                {
                    if (Index == 13)
                    {
                        string[] Temp = Line.Split(' ');       /*Just a string array that holds the values I'll take into FirstElement, 
                        LastElement, MinElement and MaxElement.*/
                        FirstElement = double.Parse(Temp[0]);
                        LastElement = double.Parse(Temp[1]);
                        MaxElement = double.Parse(Temp[2]);
                        MinElement = double.Parse(Temp[3]);
                        double LowerBoundDistance = DynamicTimeWarpingOperations.LowerBound_Kim(signal, FirstElement, LastElement, MinElement, MaxElement);
                        if (LowerBoundDistance > User.MinimumDistance) goto skip;
                        double TrueDistance = DynamicTimeWarpingOperations.DTW_Distance(sequence, ToBeCompared);
                        //Consider Current a temp variable that holds the minimum distance returned from comparing the two sequences.
                        if (TrueDistance < User.MinimumDistance)
                        //Here I compare the two Distances together to see if I need to update the minimum or not.
                        {
                            User.MinimumDistance = TrueDistance;
                            Updated = true;
                            //Here I update the minimum distance between two values.
                        }
                    skip:
                        flag = true;
                        ToBeCompared = new Sequence();
                        //This is a reinitialization just to clear out old values from the previous iteration
                    }
                    else if(Index == 14)
                    {
                        if(Updated)
                            User.UserName = Line;
                            //I update the name of the person to line because on the 13th index line, it'll have the name of the person.
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
                    ++Index;
                    //I increment the index of the 2D array for the next iteration through the file.
                }
            }
            return User;
            //I return type ClosestMatch.
        }
    }
}
