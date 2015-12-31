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
        
        public static double Pruned_DTW_Distance(Sequence sequence1, Sequence sequence2)
        {
            int numberOfFrames_Sequence1 = sequence1.Frames.Count();
            int numberOfFrames_Sequence2 = sequence2.Frames.Count();

            //re set window parameter
            windowSize = Math.Abs(numberOfFrames_Sequence1 - numberOfFrames_Sequence2); 

            double[,] DTW = new double[3, numberOfFrames_Sequence2 + 1];

            for (int i = 0; i <= numberOfFrames_Sequence2; i++)
            {
                DTW[0, i] = DTW[1, i] = DTW[2, i] = double.MaxValue;
            }
            DTW[0, 0] = 0;

            //Applying dimension compression to DTW array
            for (int i = 2; i <= numberOfFrames_Sequence1 + 1; i++)
            {
                for (int j = Math.Max(1, i - windowSize - 1); j <= Math.Min(numberOfFrames_Sequence2, i + windowSize - 1); j++)
                {
                    double cost = distance(sequence1.Frames[i - 2], sequence2.Frames[j - 1]);
                    DTW[i % 3, j] = cost + Math.Min(DTW[i % 3, j - 1],          //horizontal, stretching
                                            Math.Min(DTW[(i + 1) % 3, j - 1],      //diagnol, aligning
                                            DTW[(i + 2) % 3, j - 1]));              //far diagonal, shrinking
                }
            }

            return DTW[(numberOfFrames_Sequence1 + 1) % 3, numberOfFrames_Sequence2];
        }

        //DTW without pruning
        public static double DTW_Distance(Sequence sequence1, Sequence sequence2)
        {
            int numberOfFrames_Sequence1 = sequence1.Frames.Count();
            int numberOfFrames_Sequence2 = sequence2.Frames.Count();

            double[,] DTW = new double[3, numberOfFrames_Sequence2 + 1];

            for (int i = 0; i <= numberOfFrames_Sequence2; i++)
            {
                DTW[0, i] = DTW[1, i] = DTW[2, i] = double.MaxValue;
            }
            DTW[0, 0] = 0;

            //Applying dimension compression to DTW array
            for (int i = 2; i <= numberOfFrames_Sequence1 + 1; i++)
            {
                for (int j = 1; j <= numberOfFrames_Sequence2; j++)
                {
                    double cost = distance(sequence1.Frames[i - 2], sequence2.Frames[j - 1]);
                    DTW[i % 3, j] = cost + Math.Min(DTW[i % 3, j - 1],          //horizontal, stretching
                                            Math.Min(DTW[(i + 1) % 3, j - 1],      //diagnoal, aligning
                                            DTW[(i + 2) % 3, j - 1]));              //far diagonal, shrinking
                }
            }

            return DTW[(numberOfFrames_Sequence1 + 1) % 3, numberOfFrames_Sequence2];
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
        public static double LowerBound_Kim(AudioSignal signal, double firstElement, double lastElement, double minElement, double maxElement)
        {
            int size= signal.data.Length;
            double maxElement1 = double.MinValue,
                    minElement1 = double.MaxValue,
                    firstElement1, lastElement1;

            firstElement1 = signal.data[0];
            lastElement1 = signal.data[size - 1];

            for (int i = 0; i < size; i++)
            {
                maxElement1 = Math.Max(maxElement1, signal.data[i]);
                minElement1 = Math.Min(minElement1, signal.data[i]);
            }

            double differenceBetweenFirsts = Math.Abs(firstElement1 - firstElement);
            double differenceBetweenLasts = Math.Abs(lastElement1 - lastElement);
            double differenceBetweenMinimums = Math.Abs(minElement1 - minElement);
            double differenceBetweenMaximums = Math.Abs(maxElement1 - maxElement);
            
            double lowerBoundValue = Math.Max(
                                    Math.Max(differenceBetweenFirsts * differenceBetweenFirsts, differenceBetweenLasts * differenceBetweenLasts),
                                    Math.Max(differenceBetweenMinimums * differenceBetweenMinimums, differenceBetweenMaximums * differenceBetweenMaximums));

            //return maximum squared difference of the two sequences first, last, minimum and maximum elements
            return lowerBoundValue;
        }
    }
}
