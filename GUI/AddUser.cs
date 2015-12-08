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
        Sequence sequence = new Sequence();
        public AddUser(Sequence seq)
        {
            sequence = seq;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (FileOperations.CheckIfUserExist(textBox1.Text))
            {
                MessageBox.Show("User Exists!");
                this.Hide();
            }

            else
            {
                FileOperations.SaveSequenceInDatabase(sequence, textBox1.Text);
                this.Hide();
            }
        }

        private void AddUser_Load(object sender, EventArgs e)
        {

        }
    }
}
