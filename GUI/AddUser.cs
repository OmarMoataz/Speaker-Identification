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

namespace Recorder.GUI
{
    public partial class AddUser : Form
    { 
        public AddUser()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {   
            AudioSignal n = AudioOperations.OpenAudioFile("file.wav");
            Sequence s = AudioOperations.ExtractFeatures(n);
            FileOperations.SaveSequenceInDatabase(s,textBox1.Text);
            this.Hide();
        }

        private void AddUser_Load(object sender, EventArgs e)
        {

        }
    }
}
