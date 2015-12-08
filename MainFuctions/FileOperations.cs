using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Recorder.DynamicTimeWarping;

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
            String NameOfMinimumDistance = "";
            //The Value returned from this function, contains the name of the person that's the closest match.
            double MinimumDistanceBetweenTwoSequences = 1e9;
            //Holds the value of the closest after comparing all the sequences to the sequence required.
            using (StreamReader Reader = new StreamReader("savedSeqs.txt"))
            {
                //Opening the file.
                Sequence ToBeCompared = new Sequence();
                //Initializing a new sequence.
                string Line;
                //This line string contatins every line I go through in the file
                int Index = 0;
                //Holds the value of the current frame
                while ((Line = Reader.ReadLine()) != null)
                {
                    if (Index == 13)
                    {
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
                        ToBeCompared = new Sequence();
                        //This is a reinitialization just to clear out old values from the previous iteration
                        Index = -1;
                        //Initialize the index to -1 because it will be incremented at the end of this loop so, I want the value to be 0
                    }

                    else
                    {
                        string[] ExtractedStringsFromLine = Line.Split(' ');
                        //Here I split all the values from every line to an array of Strings.
                        for (int i = 0; i < ExtractedStringsFromLine.Length; i++)
                        {
                            ToBeCompared.Frames[i].Features[Index] = double.Parse(ExtractedStringsFromLine[i]);
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