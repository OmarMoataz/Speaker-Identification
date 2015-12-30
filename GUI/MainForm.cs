using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.DirectSound;
using Accord.Audio.Filters;
using Recorder.Recorder;
using Recorder.MFCC;
using Recorder.MainFuctions;
using Recorder.GUI;
using System.Diagnostics;
using System.Collections.Generic;

namespace Recorder
{
    /// <summary>
    ///   Speaker Identification application.
    /// </summary>
    /// 
    public partial class MainForm : Form
    {
        /// <summary>
        /// Data of the opened audio file, contains:
        ///     1. signal data
        ///     2. sample rate
        ///     3. signal length in ms
        /// </summary>
        private AudioSignal signal = null;


        private string path;

        private Encoder encoder;
        private Decoder decoder;
        private Sequence sequence;
        private bool isRecorded;

        public MainForm()
        {
            InitializeComponent();

            // Configure the wavechart
            chart.SimpleMode = true;
            chart.AddWaveform("wave", Color.Green, 1, false);
            updateButtons();

            List<User> listOfUsers = TestcaseLoader.LoadTestcase1Testing("E:\\Complete SpeakerID Dataset\\TestingList.txt");

            foreach (User user in listOfUsers)
            {
                foreach (AudioSignal signal in user.UserTemplates)
                {
                    AudioSignal signal_ = signal;
                    sequence = AudioOperations.ExtractFeatures(ref signal_);

                    ClosestMatch match = FileOperations.GetUserName(sequence, signal, true);
                    ClosestMatch match1 = FileOperations.GetUserName(sequence, signal, false);

                    Console.WriteLine(user.UserName + " : " + match.Username + ", " + match1.Username);
                }
            }

            Console.WriteLine("Done");
        }


        /// <summary>
        ///   Starts recording audio from the sound card
        /// </summary>
        /// 
        private void btnRecord_Click(object sender, EventArgs e)
        {
            isRecorded = true;
            this.encoder = new Encoder(source_NewFrame, source_AudioSourceError);
            this.encoder.Start();
            updateButtons();
        }

        /// <summary>
        ///   Plays the recorded audio stream.
        /// </summary>
        /// 
        private void btnPlay_Click(object sender, EventArgs e)
        {
            InitializeDecoder();
            // Configure the track bar so the cursor
            // can show the proper current position
            if (trackBar1.Value < this.decoder.frames)
                this.decoder.Seek(trackBar1.Value);
            trackBar1.Maximum = this.decoder.samples;
            this.decoder.Start();
            updateButtons();
        }

        private void InitializeDecoder()
        {
            if (isRecorded)
            {
                // First, we rewind the stream
                this.encoder.stream.Seek(0, SeekOrigin.Begin);
                this.decoder = new Decoder(this.encoder.stream, this.Handle, output_AudioOutputError, output_FramePlayingStarted, output_NewFrameRequested, output_PlayingFinished);
            }
            else
            {
                this.decoder = new Decoder(this.path, this.Handle, output_AudioOutputError, output_FramePlayingStarted, output_NewFrameRequested, output_PlayingFinished);
            }
        }

        /// <summary>
        ///   Stops recording or playing a stream.
        /// </summary>
        /// 
        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
            updateButtons();
            updateWaveform(new float[BaseRecorder.FRAME_SIZE], BaseRecorder.FRAME_SIZE);
        }

        /// <summary>
        ///   This callback will be called when there is some error with the audio 
        ///   source. It can be used to route exceptions so they don't compromise 
        ///   the audio processing pipeline.
        /// </summary>
        /// 
        private void source_AudioSourceError(object sender, AudioSourceErrorEventArgs e)
        {
            throw new Exception(e.Description);
        }

        /// <summary>
        ///   This method will be called whenever there is a new input audio frame 
        ///   to be processed. This would be the case for samples arriving at the 
        ///   computer's microphone
        /// </summary>
        /// 
        private void source_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            this.encoder.addNewFrame(eventArgs.Signal);
            updateWaveform(this.encoder.current, eventArgs.Signal.Length);
        }


        /// <summary>
        ///   This event will be triggered as soon as the audio starts playing in the 
        ///   computer speakers. It can be used to update the UI and to notify that soon
        ///   we will be requesting additional frames.
        /// </summary>
        /// 
        private void output_FramePlayingStarted(object sender, PlayFrameEventArgs e)
        {
            updateTrackbar(e.FrameIndex);

            if (e.FrameIndex + e.Count < this.decoder.frames)
            {
                int previous = this.decoder.Position;
                decoder.Seek(e.FrameIndex);

                Signal s = this.decoder.Decode(e.Count);
                decoder.Seek(previous);

                updateWaveform(s.ToFloat(), s.Length);
            }
        }

        /// <summary>
        ///   This event will be triggered when the output device finishes
        ///   playing the audio stream. Again we can use it to update the UI.
        /// </summary>
        /// 
        private void output_PlayingFinished(object sender, EventArgs e)
        {
            updateButtons();
            updateWaveform(new float[BaseRecorder.FRAME_SIZE], BaseRecorder.FRAME_SIZE);
        }

        /// <summary>
        ///   This event is triggered when the sound card needs more samples to be
        ///   played. When this happens, we have to feed it additional frames so it
        ///   can continue playing.
        /// </summary>
        /// 
        private void output_NewFrameRequested(object sender, NewFrameRequestedEventArgs e)
        {
            this.decoder.FillNewFrame(e);
        }


        void output_AudioOutputError(object sender, AudioOutputErrorEventArgs e)
        {
            throw new Exception(e.Description);
        }

        /// <summary>
        ///   Updates the audio display in the wave chart
        /// </summary>
        /// 
        private void updateWaveform(float[] samples, int length)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    chart.UpdateWaveform("wave", samples, length);
                }));
            }
            else
            {
                if (this.encoder != null) { chart.UpdateWaveform("wave", this.encoder.current, length); }
            }
        }

        /// <summary>
        ///   Updates the current position at the trackbar.
        /// </summary>
        /// 
        private void updateTrackbar(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    trackBar1.Value = Math.Max(trackBar1.Minimum, Math.Min(trackBar1.Maximum, value));
                }));
            }
            else
            {
                trackBar1.Value = Math.Max(trackBar1.Minimum, Math.Min(trackBar1.Maximum, value));
            }
        }

        public void updateButtons()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(updateButtons));
                return;
            }

            if (this.encoder != null && this.encoder.IsRunning())
            {
                btnAdd.Enabled = false;
                btnIdentify.Enabled = false;
                btnPlay.Enabled = false;
                btnStop.Enabled = true;
                btnRecord.Enabled = false;
                trackBar1.Enabled = false;
            }
            else if (this.decoder != null && this.decoder.IsRunning())
            {
                btnAdd.Enabled = false;
                btnIdentify.Enabled = false;
                btnPlay.Enabled = false;
                btnStop.Enabled = true;
                btnRecord.Enabled = false;
                trackBar1.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = this.path != null || this.encoder != null;
                btnIdentify.Enabled = SavedRadio.Checked || RecordRadio.Checked; 
                btnPlay.Enabled = this.path != null || this.encoder != null;//stream != null;
                btnStop.Enabled = false;
                btnRecord.Enabled = true;
                trackBar1.Enabled = this.decoder != null;
                trackBar1.Value = 0;
            }
        }

        private void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.encoder != null)
            {
                Stream fileStream = saveFileDialog1.OpenFile();
                this.encoder.Save(fileStream);

                path = saveFileDialog1.FileName;
                signal = AudioOperations.OpenAudioFile(path);
                sequence = AudioOperations.ExtractFeatures(ref signal);

            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog(this);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (this.encoder != null) { lbLength.Text = String.Format("Length: {0:00.00} sec.", this.encoder.duration / 1000.0); }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        public void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                isRecorded = false;
                path = open.FileName;
                //Open the selected audio file
                signal = AudioOperations.OpenAudioFile(path);
                sequence = AudioOperations.ExtractFeatures(ref signal);

                updateButtons();
            }
        }

        private void Stop()
        {
            if (this.encoder != null) { this.encoder.Stop(); }
            if (this.decoder != null) { this.decoder.Stop(); }
        }

        //Add button opens file explorer to save the audio file as well as adding its sequence to the database
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // UPDATE: Now the user has the ability to save the WAV file on the disk or not to save it before saving the records into the file
            DialogResult userChoise = MessageBox.Show("Do You want to save the .WAV file on the disk?" + "\n" + "If you clicked on YES a WAV file will be created on your drive, and the voice will be extracted and saved into the database." + "\n" + "If you clicked on NO, the voice will only be extracted to the database without saving it on the drive.", "Storage Mode ..", MessageBoxButtons.YesNo);
            if (userChoise == DialogResult.Yes)
            {
                saveToolStripMenuItem_Click(sender, e);
                if (sequence == null) return;
                AddUser s = new AddUser(sequence, signal);
                s.Show();
            }

            else if (userChoise == DialogResult.No)
            {
                if (sequence == null) return;
                AddUser s = new AddUser(sequence, signal);
                s.Show();
            }
        }

        //Identify button opens the file explorer to choose a pre existing audio file or recorded sound to be identified
        private void btnIdentify_Click(object sender, EventArgs e)
        {
            ClosestMatch User = new ClosestMatch();

            if (sequence != null && RecordRadio.Checked == false)
            {
                var watch = Stopwatch.StartNew();

                User = FileOperations.GetUserName(sequence, signal, WithPruningRadioBTN.Checked);

                watch.Stop();

                var elapsedMs = watch.ElapsedMilliseconds;

                Console.WriteLine("Elapsed Milliseconds = " + elapsedMs);

                MessageBox.Show("Username: " + User.Username +"\nWith Minimum Difference: " + User.MinimumDistance.ToString());
            }
            else if (SavedRadio.Checked)
            {
                OpenFileDialog open = new OpenFileDialog();
                if (open.ShowDialog() == DialogResult.OK)
                {
                    isRecorded = false;
                    path = open.FileName;
                    //Open the selected audio file
                    signal = AudioOperations.OpenAudioFile(path);
                    sequence = AudioOperations.ExtractFeatures(ref signal);
                    
                    var watch = Stopwatch.StartNew();

                    User = FileOperations.GetUserName(sequence, signal, WithPruningRadioBTN.Checked);

                    watch.Stop();

                    var elapsedMs = watch.ElapsedMilliseconds;

                    Console.WriteLine("Elapsed Milliseconds = " + elapsedMs);

                    MessageBox.Show("Username: " + User.Username + "\nWith Minimum Difference: " + User.MinimumDistance.ToString());
                }
            }
            //Dev: Omar Moataz Abdel-Wahed Attia
            else
            {
                if (isRecorded)
                {
                    InitializeDecoder();        //Initializes a decoder to get the value of the recorded stream.
                    AudioSignal signal = new AudioSignal(); //Signal sent to Feature extraction function.
                    signal.data = new double[this.decoder.frames];  //Reserve space for double array that will be filled later.
                    signal.sampleRate = this.decoder.GetTempSignal().SampleRate;
                    //TempSignal has the double array I need to extract features, Check function Decoder::getWholeSignal() for more explanation.
                    this.decoder.GetTempSignal().CopyTo(signal.data);
                    //Copies the values of the signal to an object "signal" of type AudioSignal which is sent to feature extraction.
                    sequence = AudioOperations.ExtractFeatures(ref signal);
                    //Get name of user that has the closest match.
                    var watch = Stopwatch.StartNew();

                    User = FileOperations.GetUserName(sequence, signal, WithPruningRadioBTN.Checked);

                    watch.Stop();

                    var elapsedMs = watch.ElapsedMilliseconds;

                    Console.WriteLine("Elapsed Milliseconds = " + elapsedMs);

                    MessageBox.Show("Username: " + User.Username + "\nWith Minimum Difference: " + User.MinimumDistance.ToString());
                }
                else
                {
                    MessageBox.Show("Please record your voice first!"); //In case the user tries to identify without recording any sound.
                }
            }

            sequence = null;
            updateButtons();
        }

        private void SavedRadio_CheckedChanged(object sender, EventArgs e)
        {
            updateButtons();
        }

        private void RecordRadio_CheckedChanged(object sender, EventArgs e)
        {
            updateButtons();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            updateButtons();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            updateButtons();
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}