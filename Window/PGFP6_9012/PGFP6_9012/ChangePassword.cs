﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MINATO_M1950
{
    public partial class ChangePassword : Form
    {
        public ChangePassword()
        {
            InitializeComponent();
        }

        private void OldPass_TextChanged(object sender, EventArgs e)
        {
            if (OldPass.Text == MINATO_M1950.Properties.Settings.Default.password)
            {
                OldPass.BackColor = Color.LimeGreen;
            }
            else
                OldPass.BackColor = Color.Red;
        }

        private void VerifyPass_TextChanged(object sender, EventArgs e)
        {
            if (NewPass.Text == VerifyPass.Text)
            {
                VerifyPass.BackColor = Color.LimeGreen;
            }
            else
                VerifyPass.BackColor = Color.Red;
        }

        private void Ch_password_Click(object sender, EventArgs e)
        {
            MINATO_M1950.Properties.Settings.Default.password = VerifyPass.Text;
            MINATO_M1950.Properties.Settings.Default.Save();
            this.Close();
        }


    }
}
