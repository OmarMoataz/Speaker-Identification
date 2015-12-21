using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Recorder.DynamicTimeWarping
{
    class DynamicTimeWarpingOperations
    {
        private static int windowSize = 0;
        public static void DTW(Sequence sequenceToBeCompared)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                string path = open.FileName;
                //Open the selected audio file
                AudioSignal signal = AudioOperations.OpenAudioFile(path);
                Sequence seq = AudioOperations.ExtractFeatures(signal);

                Console.WriteLine(DTW_Distance(sequenceToBeCompared, seq));
            }
        }

        public static double DTW_Distance(Sequence sequence1, Sequence sequence2)
        {
            int numberOfFrames_Sequence1 = sequence1.Frames.Count();
            int numberOfFrames_Sequence2 = sequence2.Frames.Count();

            //re set window parameter
            windowSize = Math.Max(windowSize, Math.Abs(numberOfFrames_Sequence1 - numberOfFrames_Sequence2)); 

            double[,] DTW = new double[2, numberOfFrames_Sequence2 + 1];

            for (int i = 1; i <= numberOfFrames_Sequence2; i++)
            {
                DTW[1, i] = DTW[0, i] = double.MaxValue;
            }
            DTW[0, 0] = 0;
            DTW[1, 0] = double.MaxValue;

            //Applying dimension compression to DTW array
            for (int i = 1; i <= numberOfFrames_Sequence1; i++)
            {
                for (int j = Math.Max(1, i - windowSize); j <= Math.Min(numberOfFrames_Sequence2, i + windowSize); j++)
                {
                    double cost = distance(sequence1.Frames[i - 1], sequence2.Frames[j - 1]);
                    DTW[i % 2, j] = cost + Math.Min(DTW[(i + 1) %2, j],
                                            Math.Min(DTW[i % 2, j - 1],
                                            DTW[(i + 1) % 2, j - 1]));
                }
                DTW[0, 0] = double.MaxValue;
            }

            return DTW[numberOfFrames_Sequence1 % 2, numberOfFrames_Sequence2];
        }

        //Calculates the distance between two frames
        private static double distance(MFCCFrame frame1, MFCCFrame frame2)
        {
            double difference_distance = 0;

            for (int i = 0; i < 13; i++)
            {
                difference_distance +=
                    (frame1.Features[i] - frame2.Features[i]) * (frame1.Features[i] - frame2.Features[i]);
            }

            return Math.Sqrt(difference_distance);
        }

        //Lower bounding function used for pruning
        public static double LowerBound_Kim(Sequence sequence1, Sequence sequence2)
        {
            int sizeOfSequence1= sequence1.Frames.Count();
            int sizeOfSequence2= sequence2.Frames.Count();

            MFCCFrame firstElementInSequence1 = sequence1.Frames[0];
            MFCCFrame firstElementInSequence2 = sequence2.Frames[0];
            MFCCFrame lastElementInSequence1 = sequence1.Frames[sizeOfSequence1 - 1];
            MFCCFrame lastElementInSequence2 = sequence2.Frames[sizeOfSequence2 - 1];

            //order the two sequences to get maximum and minimum elements
            sequence1.Frames.OrderBy(f => f.Features);
            sequence2.Frames.OrderBy(f => f.Features);

            MFCCFrame minimumElementInSequence1 = sequence1.Frames[0];
            MFCCFrame minimumElementInSequence2 = sequence2.Frames[0];
            MFCCFrame maximumElementInSequence1 = sequence1.Frames[sizeOfSequence1 - 1];
            MFCCFrame maximumElementInSequence2 = sequence2.Frames[sizeOfSequence2 - 1];
            
            double differenceBetweenFirsts = distance(firstElementInSequence1, firstElementInSequence2);
            double differenceBetweenLasts = distance(lastElementInSequence1, lastElementInSequence2);
            double differenceBetweenMinimums = distance(minimumElementInSequence1, minimumElementInSequence2);
            double differenceBetweenMaximums = distance(maximumElementInSequence1, maximumElementInSequence2);
            
            double lowerBoundValue = Math.Max(
                                    Math.Max(differenceBetweenFirsts * differenceBetweenFirsts, differenceBetweenLasts * differenceBetweenLasts),
                                    Math.Max(differenceBetweenMinimums * differenceBetweenMinimums, differenceBetweenMaximums * differenceBetweenMaximums));

            //return maximum squared difference of the two sequences first, last, minimum and maximum elements
            return lowerBoundValue;
        }
    }
}
