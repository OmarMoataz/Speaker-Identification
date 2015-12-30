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
        private Sequence sequence = new Sequence();
        private AudioSignal signal = new AudioSignal();
        public AddUser(Sequence sequence_, AudioSignal signal_)
        {
            sequence = sequence_;
            signal = signal_;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileOperations.SaveSequenceInDatabase(sequence, textBox1.Text, signal);
            MessageBox.Show("The user has been saved!");
            this.Hide();
        }

        private void AddUser_Load(object sender, EventArgs e)
        {

        }
    }
}
