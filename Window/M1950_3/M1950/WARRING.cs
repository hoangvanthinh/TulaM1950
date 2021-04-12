using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;
namespace M1950
{
    public partial class WARRING : Form
    {
        string sound_Pass = @"PASS.mp3";
        string sound_NG = @"NG.mp3";
        WindowsMediaPlayer sound = new WindowsMediaPlayer();
        public WARRING()
        {
            InitializeComponent();
        }

        private void WARRING_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            sound.URL = sound_NG;
            sound.controls.play();
        }

        private void WARRING_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;
        }
    }
}
