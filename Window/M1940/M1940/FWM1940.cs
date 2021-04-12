﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyModbus;
using WMPLib;
using Excel = Microsoft.Office.Interop.Excel;



namespace M1940
{
    public partial class CFW : Form
    {
        string sound_Pass = @"PASS.mp3";
        string sound_NG = @"NG.mp3";
        string tencongdoan = "Nap_Fw";

        ModbusRTU MB;
        M1950_RS232 M1950_command;
        ChangePassword changepassword;

        int Num_count_PASS = 0, Num_count_NG = 0, Num_count_Total = 0;
        WindowsMediaPlayer sound = new WindowsMediaPlayer();

        int RecM1950_status = 0;
        int Status_Process_ERR = 0;
        int DEM_1S = 0;
        public CFW()
        {
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;
            Systems_setup();

        }
        void Systems_setup()
        {
            Checksum_TargetMCU.Text = M1940.Properties.Settings.Default.checksum;
            Folder.Text = M1940.Properties.Settings.Default.pathLogfile;

            En_Dwn.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.En_Download);
            En_Checksum.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.En_Checksum);
            Erase.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.En_Ers);

            VERIFY.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.En_Verify);
            blank.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.En_Blank);
            Continuous.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.En_CONT);

            En_Buzz.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.En_Buzz);

            sc1.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.S1);
            sc2.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.S2);
            sc3.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.S3);
            sc4.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.S4);
            sc5.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.S5);
            sc6.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.S6);
            sc7.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.S7);
            sc8.Checked = Convert.ToBoolean(M1940.Properties.Settings.Default.S8);
         



        }
        void Serial_port_setup()
        {
            string[] portnames = SerialPort.GetPortNames();
            try
            {
                R_COM.Items.Clear();
                T_COM.Items.Clear();
              
                foreach (var p in portnames)
                {
                    R_COM.Items.Add(p);
                    T_COM.Items.Add(p);
                   
                }
                R_COM.SelectedItem = "COM7";
                R_Baud.SelectedItem = "115200";
                R_DataBit.SelectedItem = "8";
                R_ParityBit.SelectedItem = "None";
                R_StopBit.SelectedItem = "2";

                T_COM.SelectedItem = "COM41";
                T_Baud.SelectedItem = "9600";
                T_DataBit.SelectedItem = "8";
                T_ParityBit.SelectedItem = "None";
                T_StopBit.SelectedItem = "1";

       
            }
            catch
            {

            }
        }
        private void Serial_Port_Start()
        {
            try
            {
                //MB = new ModbusRTU(T_COM.SelectedItem.ToString(), 1, int.Parse(T_Baud.SelectedItem.ToString()), Parity.None, StopBits.One, 1000);
                //MB.Start();


                //--------- -RS232 - PG - FP6---------- -//
                M1950_command = new M1950_RS232(R_COM.SelectedItem.ToString(), int.Parse(R_Baud.SelectedItem.ToString()), Parity.None, StopBits.One);
                M1950_command.RtsEnable = true;
                M1950_command.DtrEnable = true;
                M1950_command.ReadBufferSize = 4096;
                M1950_command.ReadTimeout = 1000;
                M1950_command.Start_RS232();

                M1950_command.DataReceived += new SerialDataReceivedEventHandler(RS232_REV);
            }
            catch
            {
                MessageBox.Show("Pleasa check setup port!");
            }
        }
        private void RS232_REV(object sender, SerialDataReceivedEventArgs e)
        {
       
            try
            {
                string line = M1950_command.ReadExisting();
                //MessageBox.Show(line);
                
                //textBox1.Text = line.ToString();
                //Command.Text = line;
                //QRcode.Text = line;
                AppendText_T(Command, Color.Yellow, line);


            }
            catch
            {

            }

        }
        void AppendText_T(RichTextBox box, Color color, string text)
        {
            int start = box.TextLength;
            box.AppendText(text);
            int end = box.TextLength;

            // Textbox may transform chars, so (end-start) != text.Length
            box.Select(start, end - start);
            {
                box.SelectionColor = color;
                // could set box.SelectionBackColor, box.SelectionFont too.
            }
            box.SelectionLength = 0; // clear
        }
        private void Serial_Connect_Click(object sender, EventArgs e)
        {
            try
            {

                if (Serial_Connect.Text == "Disconnect")
                {
                    Serial_Connect.Text = "Connected";
                    Serial_Port_Start();
                }
                else
                {
                    Serial_Connect.Text = "Disconnect";
                    //MB.Disconnect();
                    M1950_command.Close();
                    timer1.Enabled = false;
                }
            }
            catch
            {
                MessageBox.Show("Port Error!");
            }
        }

        private void CFW_Load(object sender, EventArgs e)
        {
            Serial_port_setup();
            #region kiem tra va tao folder theo ngay-thang-nam
            try
            {
                string pathTotal = M1940.Properties.Settings.Default.pathLogfile;
                string path_Day = DateTime.Now.ToString("MM-dd-yyyy").ToString();

                string Full_path = Path.Combine(pathTotal, path_Day);
                string Full_path_OK = Path.Combine(Full_path, "OK");
                string Full_path_NG = Path.Combine(Full_path, "NG");

                if (!(Directory.Exists(Full_path)))
                {
                    Directory.CreateDirectory(Full_path_OK);
                    Directory.CreateDirectory(Full_path_NG);
                }
                else
                {
                    if (!Directory.Exists(Full_path_OK)) Directory.CreateDirectory(Full_path_OK);
                    if (!Directory.Exists(Full_path_NG)) Directory.CreateDirectory(Full_path_NG);
                }
            }
            catch
            {
                MessageBox.Show("Don't Creat logfile");
            }
            #endregion 

            comboBox_user.Items.Add(M1940.Properties.Settings.Default.user1.ToString());
            comboBox_user.Items.Add(M1940.Properties.Settings.Default.user2.ToString());
            comboBox_user.Items.Add(M1940.Properties.Settings.Default.user3.ToString());
            comboBox_user.Items.Add(M1940.Properties.Settings.Default.user4.ToString());
            comboBox_user.Items.Add(M1940.Properties.Settings.Default.user5.ToString());

            for (int i = 0; i < 5; i++)
            {
                var index = dataUser.Rows.Add();
                dataUser.Rows[i].Cells[0].Value = (i + 1).ToString();
            }
            dataUser.Rows[0].Cells[1].Value = M1940.Properties.Settings.Default.user1.ToString();
            dataUser.Rows[1].Cells[1].Value = M1940.Properties.Settings.Default.user2.ToString();
            dataUser.Rows[2].Cells[1].Value = M1940.Properties.Settings.Default.user3.ToString();
            dataUser.Rows[3].Cells[1].Value = M1940.Properties.Settings.Default.user4.ToString();
            dataUser.Rows[4].Cells[1].Value = M1940.Properties.Settings.Default.user5.ToString();

            dataUser.Rows[0].Cells[2].Value = M1940.Properties.Settings.Default.pass1.ToString();
            dataUser.Rows[1].Cells[2].Value = M1940.Properties.Settings.Default.pass2.ToString();
            dataUser.Rows[2].Cells[2].Value = M1940.Properties.Settings.Default.pass3.ToString();
            dataUser.Rows[3].Cells[2].Value = M1940.Properties.Settings.Default.pass4.ToString();
            dataUser.Rows[4].Cells[2].Value = M1940.Properties.Settings.Default.pass5.ToString();

            // DATA PROGRAM
            for (int i = 0; i < 10; i++)
            {
                var index = dataROM.Rows.Add();
                dataROM.Rows[i].Cells[0].Value = (i + 1).ToString();
            }
            dataROM.Rows[0].Cells[1].Value = M1940.Properties.Settings.Default.PR1.ToString();
            dataROM.Rows[1].Cells[1].Value = M1940.Properties.Settings.Default.PR2.ToString();
            dataROM.Rows[2].Cells[1].Value = M1940.Properties.Settings.Default.PR3.ToString();
            dataROM.Rows[3].Cells[1].Value = M1940.Properties.Settings.Default.PR4.ToString();
            dataROM.Rows[4].Cells[1].Value = M1940.Properties.Settings.Default.PR5.ToString();
            dataROM.Rows[5].Cells[1].Value = M1940.Properties.Settings.Default.PR6.ToString();
            dataROM.Rows[6].Cells[1].Value = M1940.Properties.Settings.Default.PR7.ToString();
            dataROM.Rows[7].Cells[1].Value = M1940.Properties.Settings.Default.PR8.ToString();
            dataROM.Rows[8].Cells[1].Value = M1940.Properties.Settings.Default.PR9.ToString();
            dataROM.Rows[9].Cells[1].Value = M1940.Properties.Settings.Default.PR10.ToString();

            dataROM.Rows[0].Cells[2].Value = M1940.Properties.Settings.Default.CK1.ToString();
            dataROM.Rows[1].Cells[2].Value = M1940.Properties.Settings.Default.CK2.ToString();
            dataROM.Rows[2].Cells[2].Value = M1940.Properties.Settings.Default.CK3.ToString();
            dataROM.Rows[3].Cells[2].Value = M1940.Properties.Settings.Default.CK4.ToString();
            dataROM.Rows[4].Cells[2].Value = M1940.Properties.Settings.Default.CK5.ToString();
            dataROM.Rows[5].Cells[2].Value = M1940.Properties.Settings.Default.CK6.ToString();
            dataROM.Rows[6].Cells[2].Value = M1940.Properties.Settings.Default.CK7.ToString();
            dataROM.Rows[7].Cells[2].Value = M1940.Properties.Settings.Default.CK8.ToString();
            dataROM.Rows[8].Cells[2].Value = M1940.Properties.Settings.Default.CK9.ToString();
            dataROM.Rows[9].Cells[2].Value = M1940.Properties.Settings.Default.CK10.ToString();

            dataROM.Rows[0].Cells[3].Value = M1940.Properties.Settings.Default.IC1.ToString();
            dataROM.Rows[1].Cells[3].Value = M1940.Properties.Settings.Default.IC2.ToString();
            dataROM.Rows[2].Cells[3].Value = M1940.Properties.Settings.Default.IC3.ToString();
            dataROM.Rows[3].Cells[3].Value = M1940.Properties.Settings.Default.IC4.ToString();
            dataROM.Rows[4].Cells[3].Value = M1940.Properties.Settings.Default.IC5.ToString();
            dataROM.Rows[5].Cells[3].Value = M1940.Properties.Settings.Default.IC6.ToString();
            dataROM.Rows[6].Cells[3].Value = M1940.Properties.Settings.Default.IC7.ToString();
            dataROM.Rows[7].Cells[3].Value = M1940.Properties.Settings.Default.IC8.ToString();
            dataROM.Rows[8].Cells[3].Value = M1940.Properties.Settings.Default.IC9.ToString();
            dataROM.Rows[9].Cells[3].Value = M1940.Properties.Settings.Default.IC10.ToString();

            dataROM.Rows[0].Cells[4].Value = M1940.Properties.Settings.Default.SK1.ToString();
            dataROM.Rows[1].Cells[4].Value = M1940.Properties.Settings.Default.SK2.ToString();
            dataROM.Rows[2].Cells[4].Value = M1940.Properties.Settings.Default.SK3.ToString();
            dataROM.Rows[3].Cells[4].Value = M1940.Properties.Settings.Default.SK4.ToString();
            dataROM.Rows[4].Cells[4].Value = M1940.Properties.Settings.Default.SK5.ToString();
            dataROM.Rows[5].Cells[4].Value = M1940.Properties.Settings.Default.SK6.ToString();
            dataROM.Rows[6].Cells[4].Value = M1940.Properties.Settings.Default.SK7.ToString();
            dataROM.Rows[7].Cells[4].Value = M1940.Properties.Settings.Default.SK8.ToString();
            dataROM.Rows[8].Cells[4].Value = M1940.Properties.Settings.Default.SK9.ToString();
            dataROM.Rows[9].Cells[4].Value = M1940.Properties.Settings.Default.SK10.ToString();

            dataROM.Rows[0].Cells[5].Value = M1940.Properties.Settings.Default.CP1.ToString();
            dataROM.Rows[1].Cells[5].Value = M1940.Properties.Settings.Default.CP2.ToString();
            dataROM.Rows[2].Cells[5].Value = M1940.Properties.Settings.Default.CP3.ToString();
            dataROM.Rows[3].Cells[5].Value = M1940.Properties.Settings.Default.CP4.ToString();
            dataROM.Rows[4].Cells[5].Value = M1940.Properties.Settings.Default.CP5.ToString();
            dataROM.Rows[5].Cells[5].Value = M1940.Properties.Settings.Default.CP6.ToString();
            dataROM.Rows[6].Cells[5].Value = M1940.Properties.Settings.Default.CP7.ToString();
            dataROM.Rows[7].Cells[5].Value = M1940.Properties.Settings.Default.CP8.ToString();
            dataROM.Rows[8].Cells[5].Value = M1940.Properties.Settings.Default.CP9.ToString();
            dataROM.Rows[9].Cells[5].Value = M1940.Properties.Settings.Default.CP10.ToString();

            dataROM.Rows[0].Cells[6].Value = M1940.Properties.Settings.Default.MK1.ToString();
            dataROM.Rows[1].Cells[6].Value = M1940.Properties.Settings.Default.MK2.ToString();
            dataROM.Rows[2].Cells[6].Value = M1940.Properties.Settings.Default.MK3.ToString();
            dataROM.Rows[3].Cells[6].Value = M1940.Properties.Settings.Default.MK4.ToString();
            dataROM.Rows[4].Cells[6].Value = M1940.Properties.Settings.Default.MK5.ToString();
            dataROM.Rows[5].Cells[6].Value = M1940.Properties.Settings.Default.MK6.ToString();
            dataROM.Rows[6].Cells[6].Value = M1940.Properties.Settings.Default.MK7.ToString();
            dataROM.Rows[7].Cells[6].Value = M1940.Properties.Settings.Default.MK8.ToString();
            dataROM.Rows[8].Cells[6].Value = M1940.Properties.Settings.Default.MK9.ToString();
            dataROM.Rows[9].Cells[6].Value = M1940.Properties.Settings.Default.MK10.ToString();
        }

        private void Cksum_Click(object sender, EventArgs e)
        {
            Process_checksum();
        }
        void Process_checksum()
        {
            M1950_RS232.Req_checksum_MCU = 1;
            Command.Text = null;
            try
            {
                M1950_command.Checksum8D();
            }
            catch
            {
                MessageBox.Show("M1940 Lost connect!");
            }
            if (sc1.Checked == true) SOCKET1.BackColor = Color.White;
            if (sc2.Checked == true) SOCKET2.BackColor = Color.White;
            if (sc3.Checked == true) SOCKET3.BackColor = Color.White;
            if (sc4.Checked == true) SOCKET4.BackColor = Color.White;
            if (sc5.Checked == true) SOCKET5.BackColor = Color.White;
            if (sc6.Checked == true) SOCKET6.BackColor = Color.White;
            if (sc7.Checked == true) SOCKET7.BackColor = Color.White;
            if (sc8.Checked == true) SOCKET8.BackColor = Color.White;

            ST1.Text = string.Empty;
            ST2.Text = string.Empty;
            ST3.Text = string.Empty;
            ST4.Text = string.Empty;
            ST5.Text = string.Empty;
            ST6.Text = string.Empty;
            ST7.Text = string.Empty;
            ST8.Text = string.Empty;
        }
        private void Dw_Click(object sender, EventArgs e)
        {
            M1950_RS232.Req_Pragram = 1;
            progressBar1.Value = 0;
            Command.Text = null;
            try
            {
                M1950_command.Program();
            }
            catch
            {
                MessageBox.Show("M1940 Lost connect!");
            }
            if (sc1.Checked == true) SOCKET1.BackColor = Color.White;
            if (sc2.Checked == true) SOCKET2.BackColor = Color.White;
            if (sc3.Checked == true) SOCKET3.BackColor = Color.White;
            if (sc4.Checked == true) SOCKET4.BackColor = Color.White;
            if (sc5.Checked == true) SOCKET5.BackColor = Color.White;
            if (sc6.Checked == true) SOCKET6.BackColor = Color.White;
            if (sc7.Checked == true) SOCKET7.BackColor = Color.White;
            if (sc8.Checked == true) SOCKET8.BackColor = Color.White;

            ST1.Text = string.Empty;
            ST2.Text = string.Empty;
            ST3.Text = string.Empty;
            ST4.Text = string.Empty;
            ST5.Text = string.Empty;
            ST6.Text = string.Empty;
            ST7.Text = string.Empty;
            ST8.Text = string.Empty;
       

        }


        private void Command_TextChanged(object sender, EventArgs e)
        {
            Command.SelectionStart = Command.Text.Length;
            Command.ScrollToCaret();
            if (Command.Text != string.Empty &&
                (M1950_RS232.Req_Pragram > 0 
                || M1950_RS232.Req_checksum_MCU > 0 
                || M1950_RS232.Req_COT > 0 
                || M1950_RS232.Req_VR > 0
                || M1950_RS232.Req_ER > 0
                || M1950_RS232.Req_BL > 0
                ))
                RecM1950_status = 1;
        }

        private void Serial_Connect_TextChanged(object sender, EventArgs e)
        {
            if(Serial_Connect.Text == "Disconnect")
            {
                Serial_Connect.ForeColor = Color.Red;
            }
            else
                Serial_Connect.ForeColor = Color.Green;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //DEM_1S++;
            //if (DEM_1S > 10)
            //{
            //    DEM_1S = 0;
            //    if (Status_Process_ERR == 1)
            //    {
            //        if (En_Buzz.Checked == true)
            //        {
            //            sound.URL = sound_NG;
            //            sound.controls.play();
            //        }
            //    }
            //}
            if ((M1950_RS232.Req_Pragram > 0 
                || M1950_RS232.Req_VR > 0 
                || M1950_RS232.Req_COT > 0
                || M1950_RS232.Req_BL > 0
                || M1950_RS232.Req_ER > 0) && Serial_Connect.Text == "Connected")
            {
                if(progressBar1.Value < 95)
                    progressBar1.Value += 5;
                //M1950_RS232.Req_Pragram++;
                //if (M1950_RS232.Req_Pragram > 30)
                if(RecM1950_status == 1)
                {
                    M1950_RS232.Req_Pragram = 0;
                    M1950_RS232.Req_ER = 0;
                    M1950_RS232.Req_COT = 0;
                    M1950_RS232.Req_VR = 0;
                    M1950_RS232.Req_BL = 0;
                    try
                    {
                        //MessageBox.Show("CHECK READ");
                        Read_result_Program();
                    }
                    catch
                    {

                    }
                    progressBar1.Value = 100;
                    RecM1950_status = 0;
                }
            }
            if(M1950_RS232.Req_checksum_MCU > 0 && Serial_Connect.Text == "Connected")
            {
                //M1950_RS232.Req_checksum_MCU++;
                //if (M1950_RS232.Req_checksum_MCU > 20)
                if (RecM1950_status == 1)
                {
                    M1950_RS232.Req_checksum_MCU = 0;
                    try
                    {
                        //MessageBox.Show("CHECK READ");
                        Read_result_ChecksumMCU();
                    }
                    catch
                    {

                    }
                    RecM1950_status = 0;
                }
            }
        }

        private void Read_result_ChecksumMCU()
        {
            string temp;
            int pStart;
            temp = Command.Text;
            
            //pStart = temp.LastIndexOf("PASS");
            temp = temp.Substring(0, 8);
            textBox1.Text = temp;
            Checksum_Ans.Text = temp;
        }
        private void Read_result_Program()
        {
            string temp;
            int pStart;
            temp = Command.Text;
            //pStart = temp.LastIndexOf("W");
            //temp = temp.Substring(pStart+3, 16);
            pStart = temp.LastIndexOf(";");
            temp = temp.Substring(pStart-19, 16);
            textBox1.Text = temp;
            //MessageBox.Show("OK");
            // SOCKET 1
            Status_Process_ERR = 0;
            if (sc1.Checked == true)
            {
                if (temp[0] == 'o') SOCKET1.BackColor = Color.LimeGreen;
                else if (temp[0] == 'x' || temp[0] == '-')
                {
                    SOCKET1.BackColor = Color.Red;
                    Status_Process_ERR = 1;
                }
                //else if (temp[0] == '-') SOCKET1.BackColor = Color.Red;
            }
            // SOCKET 2
            //if (sc2.Checked == true)
            //{
            //    if (temp[1] == 'o') SOCKET2.BackColor = Color.LimeGreen;
            //    else if (temp[1] == 'x') SOCKET2.BackColor = Color.Red;
            //    else if (temp[1] == '-') SOCKET2.BackColor = Color.Red;
            //}
            // SOCKET 3
            if (sc3.Checked == true)
            {
                if (temp[2] == 'o') SOCKET3.BackColor = Color.LimeGreen;
                else if (temp[2] == 'x' || temp[2] == '-')
                {
                    SOCKET3.BackColor = Color.Red;
                    Status_Process_ERR = 1;
                }
                //else if (temp[2] == '-') SOCKET3.BackColor = Color.Red;
            }
            // SOCKET 4
            //if (sc4.Checked == true)
            //{
            //    if (temp[3] == 'o') SOCKET4.BackColor = Color.LimeGreen;
            //    else if (temp[3] == 'x') SOCKET4.BackColor = Color.Red;
            //    else if (temp[3] == '-') SOCKET4.BackColor = Color.Red;
            //}
            // SOCKET 5
            if (sc5.Checked == true)
            {
                if (temp[4] == 'o') SOCKET5.BackColor = Color.LimeGreen;
                else if (temp[4] == 'x' || temp[4] == '-')
                {
                    SOCKET5.BackColor = Color.Red;
                    Status_Process_ERR = 1;

                }
                //else if (temp[4] == '-') SOCKET5.BackColor = Color.Red;
            }
            // SOCKET 6
            //if (sc6.Checked == true)
            //{
            //    if (temp[5] == 'o') SOCKET6.BackColor = Color.LimeGreen;
            //    else if (temp[5] == 'x') SOCKET6.BackColor = Color.Red;
            //    else if (temp[5] == '-') SOCKET6.BackColor = Color.Red;
            //}
            // SOCKET 7
            if (sc7.Checked == true)
            {
                if (temp[6] == 'o') SOCKET7.BackColor = Color.LimeGreen;
                else if (temp[6] == 'x' || temp[6] == '-')
                {
                    SOCKET7.BackColor = Color.Red;
                    Status_Process_ERR = 1;
                }
                //else if (temp[6] == '-') SOCKET7.BackColor = Color.Red;
            }
            // SOCKET 8
            //if (sc8.Checked == true)
            //{
            //    if (temp[7] == 'o') SOCKET8.BackColor = Color.LimeGreen;
            //    else if (temp[7] == 'x') SOCKET8.BackColor = Color.Red;
            //    else if (temp[7] == '-') SOCKET8.BackColor = Color.Red;
            //}
            int i = 0;
            if (sc1.Checked == true)
            {
                i = 0;
                if (temp[i] == 'o')
                {
                    M1950_RS232.num_Pass++;
                    M1950_RS232.Socket_NG[i] = 0;
                    M1950_RS232.Socket_OK[i] = 1;
                }
                else if (temp[i] == 'x' || temp[i] == '-')
                {
                    M1950_RS232.num_ERROR++;
                    M1950_RS232.Socket_NG[i] = 1;
                    M1950_RS232.Socket_OK[i] = 0;
                }
            }
            else
            {
                M1950_RS232.Socket_NG[i] = 0;
                M1950_RS232.Socket_OK[i] = 0;
            }
            if (sc3.Checked == true)
            {
                i = 2;
                if (temp[i] == 'o')
                {
                    M1950_RS232.num_Pass++;
                    M1950_RS232.Socket_NG[i] = 0;
                    M1950_RS232.Socket_OK[i] = 1;
                }
                else if (temp[i] == 'x' || temp[i] == '-')
                {
                    M1950_RS232.num_ERROR++;
                    M1950_RS232.Socket_NG[i] = 1;
                    M1950_RS232.Socket_OK[i] = 0;
                }
            }
            else
            {
                M1950_RS232.Socket_NG[i] = 0;
                M1950_RS232.Socket_OK[i] = 0;
            }
            if (sc5.Checked == true)
            {
                i = 4;
                if (temp[i] == 'o')
                {
                    M1950_RS232.num_Pass++;
                    M1950_RS232.Socket_NG[i] = 0;
                    M1950_RS232.Socket_OK[i] = 1;
                }
                else if (temp[i] == 'x' || temp[i] == '-')
                {
                    M1950_RS232.num_ERROR++;
                    M1950_RS232.Socket_NG[i] = 1;
                    M1950_RS232.Socket_OK[i] = 0;
                }
            }
            else
            {
                M1950_RS232.Socket_NG[i] = 0;
                M1950_RS232.Socket_OK[i] = 0;
            }
            if (sc7.Checked == true)
            {
                i = 6;
                if (temp[i] == 'o')
                {
                    M1950_RS232.num_Pass++;
                    M1950_RS232.Socket_NG[i] = 0;
                    M1950_RS232.Socket_OK[i] = 1;
                }
                else if (temp[i] == 'x' || temp[i] == '-')
                {
                    M1950_RS232.num_ERROR++;
                    M1950_RS232.Socket_NG[i] = 1;
                    M1950_RS232.Socket_OK[i] = 0;
                }

            }
            else
            {
                M1950_RS232.Socket_NG[i] = 0;
                M1950_RS232.Socket_OK[i] = 0;
            }

            //for (int i = 0; i < 8; i += 2)
            //{
            //    if (temp[i] == 'o')
            //    {
            //        M1950_RS232.num_Pass++;
            //        M1950_RS232.Socket_NG[i] = 0;
            //        M1950_RS232.Socket_OK[i] = 1;
            //    }
            //    else if (temp[i] == 'x' || temp[i] == '-')
            //    {
            //        M1950_RS232.num_ERROR++;
            //        M1950_RS232.Socket_NG[i] = 1;
            //        M1950_RS232.Socket_OK[i] = 0;
            //    }
            //}

            //Status_Process_ERR = 0;
            //for (int i = 0; i < 8;i+=2 )
            //{
            //    if (temp[i] == 'x' || temp[i] == '-')
            //        Status_Process_ERR = 1;


            //}
            if (Status_Process_ERR == 1)
            {
                if (En_Buzz.Checked == true)
                {
                    //sound.URL = sound_NG;
                    //sound.controls.play();
                    WARRING Warring = new WARRING();
                    // CHECK MODBUS PORT
                    Warring.ShowDialog();
                    if (Warring.DialogResult == DialogResult.OK)
                    {
                        //MessageBox.Show("Kee");
                    }
                }
            }
            else
            {
                if (En_Buzz.Checked == true)
                {
                    sound.URL = sound_Pass;
                    sound.controls.play();
                }
            }
            //sound.URL = sound_Pass;
            //sound.controls.play();


            Count_PASS.Text = M1950_RS232.num_Pass.ToString();
            Count_NG.Text = M1950_RS232.num_ERROR.ToString();
            M1950_RS232.num_Sum = M1950_RS232.num_Pass + M1950_RS232.num_ERROR;
            Count_Total.Text = M1950_RS232.num_Sum.ToString();

            Logfiletxt();

        }
        private void Checksum_MCU()
        {
            string temp;
            int pStart;
            temp = Command.Text;
            pStart = temp.LastIndexOf(":");
            temp = temp.Substring(pStart + 9, 4);          
            Checksum_Ans.Text = temp;

            //==================================
            if(temp != Checksum_TargetMCU.Text)
            {
             
            }
            else
            {

            }
            

            
        }

        private void Backcolor_Socket_Changeed(object sender, EventArgs e)
        {
            if(((Label)sender).Name == "SOCKET1")
            {
                if (((Label)sender).BackColor == Color.Red)
                {
                    ST1.Text = "NG";
                    ST1.ForeColor = Color.Red;

                }
                else if (((Label)sender).BackColor == Color.LimeGreen)
                {
                    ST1.Text = "OK";
                    ST1.ForeColor = Color.Lime;
                }
            }
            if (((Label)sender).Name == "SOCKET2")
            {
                if (((Label)sender).BackColor == Color.Red)
                {
                    ST2.Text = "NG";
                    ST2.ForeColor = Color.Red;

                }
                else if (((Label)sender).BackColor == Color.LimeGreen)
                {
                    ST2.Text = "OK";
                    ST2.ForeColor = Color.Lime;
                }
            }
            if (((Label)sender).Name == "SOCKET3")
            {
                if (((Label)sender).BackColor == Color.Red)
                {
                    ST3.Text = "NG";
                    ST3.ForeColor = Color.Red;

                }
                else if (((Label)sender).BackColor == Color.LimeGreen)
                {
                    ST3.Text = "OK";
                    ST3.ForeColor = Color.Lime;
                }
            }
            if (((Label)sender).Name == "SOCKET4")
            {
                if (((Label)sender).BackColor == Color.Red)
                {
                    ST4.Text = "NG";
                    ST4.ForeColor = Color.Red;

                }
                else if (((Label)sender).BackColor == Color.LimeGreen)
                {
                    ST4.Text = "OK";
                    ST4.ForeColor = Color.Lime;
                }
            }
            if (((Label)sender).Name == "SOCKET5")
            {
                if (((Label)sender).BackColor == Color.Red)
                {
                    ST5.Text = "NG";
                    ST5.ForeColor = Color.Red;

                }
                else if (((Label)sender).BackColor == Color.LimeGreen)
                {
                    ST5.Text = "OK";
                    ST5.ForeColor = Color.Lime;
                }
            }
            if (((Label)sender).Name == "SOCKET6")
            {
                if (((Label)sender).BackColor == Color.Red)
                {
                    ST6.Text = "NG";
                    ST6.ForeColor = Color.Red;

                }
                else if (((Label)sender).BackColor == Color.LimeGreen)
                {
                    ST6.Text = "OK";
                    ST6.ForeColor = Color.Lime;
                }
            }
            if (((Label)sender).Name == "SOCKET7")
            {
                if (((Label)sender).BackColor == Color.Red)
                {
                    ST7.Text = "NG";
                    ST7.ForeColor = Color.Red;

                }
                else if (((Label)sender).BackColor == Color.LimeGreen)
                {
                    ST7.Text = "OK";
                    ST7.ForeColor = Color.Lime;
                }
            }
            if (((Label)sender).Name == "SOCKET8")
            {
                if (((Label)sender).BackColor == Color.Red)
                {
                    ST8.Text = "NG";
                    ST8.ForeColor = Color.Red;

                }
                else if (((Label)sender).BackColor == Color.LimeGreen)
                {
                    ST8.Text = "OK";
                    ST8.ForeColor = Color.Lime;
                }
            }
        }

        private void Checksum_TargetMCU_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void Save_checksum_Click(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.checksum = Checksum_TargetMCU.Text;
            M1940.Properties.Settings.Default.Save();
            MessageBox.Show("Save Checksum MCU Complete!");
        }

        private void Browse_File_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Folder.Text = folderBrowserDialog1.SelectedPath;

                M1940.Properties.Settings.Default.pathLogfile = Folder.Text;
                M1940.Properties.Settings.Default.Save();

            }
        }



        private void En_Dwn_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.En_Download = En_Dwn.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (En_Dwn.Checked == true)
                Dw.Visible = true;
            else
                Dw.Visible = false;
        }

        private void En_Checksum_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.En_Checksum = En_Checksum.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (En_Checksum.Checked == true)
                Cksum.Visible = true;
            else
                Cksum.Visible = false;
        }

        private void Erase_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.En_Ers = Erase.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (Erase.Checked == true)
                Ers.Visible = true;
            else
                Ers.Visible = false;
        }

        private void Login_Click(object sender, EventArgs e)
        {
            if (Password.Text == M1940.Properties.Settings.Default.password)
            {
                Password.BackColor = Color.Lime;
                SettingProcess.Enabled = true;
                data_User_group.Enabled = true;
                Data_Program.Enabled = true;
                data_User_group.Visible = true;

            }
            else
            {
                Password.BackColor = Color.Red;
                SettingProcess.Enabled = false;
                data_User_group.Enabled = false;
                Data_Program.Enabled = false;
            }
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            SettingProcess.Enabled = false;
            data_User_group.Enabled = false;
            Data_Program.Enabled = false;

            Password.BackColor = Color.White;
            Password.Text = "";
            data_User_group.Visible = false;
        }



        private void Req_Erase()
        {
            M1950_command.Write("ER\r\n");
        }

        private void En_Buzz_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.En_Buzz = En_Buzz.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
           
        }

        void Logfiletxt()
        {
            string pathTotal = M1940.Properties.Settings.Default.pathLogfile;
            string path_Day = DateTime.Now.ToString("MM-dd-yyyy").ToString();
            string time = DateTime.Now.ToString("HH-mm-ss").ToString();

            string Full_path = Path.Combine(pathTotal, path_Day);
            string Full_path_OK = Path.Combine(Full_path, "OK");
            string Full_path_NG = Path.Combine(Full_path, "NG");

            for(int i = 0;i<16;i++)
            {
                int j = i + 1;
                if(M1950_RS232.Socket_NG[i] == 1)
                {
                    string Full_path_NG_file = Path.Combine(Full_path_NG, "[SOCKET " + j + "_" + comboBox_user.SelectedItem.ToString() + "] W" + time + ".txt");
                    using (FileStream fileStream = File.Create(Full_path_NG_file))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine("SOCKET" + j + "___" + comboBox_user.SelectedItem.ToString() + "_"+ Checksum_Ans.Text + "_" + DateTime.Now.ToString("MM-dd-yyyy-HH:mm:ss") + "_NG");

                    }
                }
                else if(M1950_RS232.Socket_OK[i] == 1)
                {
                    string Full_path_OK_file = Path.Combine(Full_path_OK, "[SOCKET " + j + "_" + comboBox_user.SelectedItem.ToString() + "] W" + time + ".txt");
                    using (FileStream fileStream = File.Create(Full_path_OK_file))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine("SOCKET" + j + "___" + comboBox_user.SelectedItem.ToString() + "_" + Checksum_Ans.Text + "_" + DateTime.Now.ToString("MM-dd-yyyy-HH:mm:ss") + "_OK");

                    }
                }
            }
            //if ((Report_Label.Text == "PASS" || Report_Label.Text == "NG"))
            //{

            //    string pathTotal = MINATO_M1950.Properties.Settings.Default.pathLogfile;
            //    string path_Day = DateTime.Now.ToString("MM-dd-yyyy").ToString();
            //    string time = DateTime.Now.ToString("HH-mm-ss").ToString();

            //    string Full_path = Path.Combine(pathTotal, path_Day);
            //    string Full_path_OK = Path.Combine(Full_path, "OK");
            //    string Full_path_NG = Path.Combine(Full_path, "NG");

            //    string Full_path_OK_file = Path.Combine(Full_path_OK, time + ".txt");
            //    string Full_path_NG_file = Path.Combine(Full_path_NG, time + ".txt");
            //    //=====================================txt========================================================C:\Users\MEIKO\Desktop\PGFP6_DATA
            //    string T_path;
            //    if (Report_Label.Text == "PASS")
            //    {
            //        T_path = Full_path_OK_file;
            //    }
            //    else
            //        T_path = Full_path_NG_file;
            //    using (FileStream fileStream = File.Create(T_path))
            //    using (StreamWriter writer = new StreamWriter(fileStream))
            //    {
            //        writer.WriteLine(Checksum_Ans.Text + "___" + DateTime.Now.ToString("MM-dd-yyyy-HH:mm:ss"));

            //    }
            //    //================================================================================================

            //}
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            changepassword = new ChangePassword();
            changepassword.ShowDialog();

          
        }

        private void button1_Click(object sender, EventArgs e)
        {

            M1950_command.REMOTE();
        }

        private void sc1_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.S1 = sc1.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (sc1.Checked == true)
                SOCKET1.Enabled = true;
            else
                SOCKET1.Enabled = false;
        }

        private void sc2_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.S2 = sc2.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (sc2.Checked == true)
                SOCKET2.Enabled = true;
            else
                SOCKET2.Enabled = false;
        }

        private void sc3_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.S3 = sc3.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (sc3.Checked == true)
                SOCKET3.Enabled = true;
            else
                SOCKET3.Enabled = false;
        }

        private void sc4_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.S4 = sc4.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (sc4.Checked == true)
                SOCKET4.Enabled = true;
            else
                SOCKET4.Enabled = false;
        }

        private void sc5_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.S5 = sc5.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (sc5.Checked == true)
                SOCKET5.Enabled = true;
            else
                SOCKET5.Enabled = false;
        }

        private void sc6_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.S6 = sc6.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (sc6.Checked == true)
                SOCKET6.Enabled = true;
            else
                SOCKET6.Enabled = false;
        }

        private void sc7_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.S7 = sc7.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (sc7.Checked == true)
                SOCKET7.Enabled = true;
            else
                SOCKET7.Enabled = false;
        }

        private void sc8_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.S8 = sc8.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (sc8.Checked == true)
                SOCKET8.Enabled = true;
            else
                SOCKET8.Enabled = false;
        }

     
        private void SOCKET_Enter(object sender, EventArgs e)
        {

        }

        private void Continuous_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.En_CONT = Continuous.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (Continuous.Checked == true)
                CT.Visible = true;
            else
                CT.Visible = false;
        }

        private void user_login_Click(object sender, EventArgs e)
        {
            if(comboBox_user.SelectedIndex == 0)
            {
              
                if(Password_user.Text == M1940.Properties.Settings.Default.pass1)
                {
                    //MessageBox.Show("0 OK");
                    ControlM1950.Enabled = true;
                }
            }
            else if(comboBox_user.SelectedIndex == 1)
            {
                if (Password_user.Text == M1940.Properties.Settings.Default.pass2)
                {
                    //MessageBox.Show("0 OK");
                    ControlM1950.Enabled = true;

                }  
            }
            else if (comboBox_user.SelectedIndex == 2)
            {
                if (Password_user.Text == M1940.Properties.Settings.Default.pass3)
                {
                    //MessageBox.Show("0 OK");
                    ControlM1950.Enabled = true;

                }
            }
            else if (comboBox_user.SelectedIndex == 3)
            {
                if (Password_user.Text == M1940.Properties.Settings.Default.pass4)
                {
                    //MessageBox.Show("0 OK");
                    ControlM1950.Enabled = true;

                }
            }
            else if (comboBox_user.SelectedIndex == 4)
            {
                if (Password_user.Text == M1940.Properties.Settings.Default.pass5)
                {
                    //MessageBox.Show("0 OK");
                    ControlM1950.Enabled = true;

                }
            }
            else
                ControlM1950.Enabled = false;


            if (ControlM1950.Enabled == true && user_login.Text == "Login")
            {
                user_login.Text = "Logout";
                comboBox_user.Enabled = false;
            }
            else if (user_login.Text == "Logout")
            {
                user_login.Text = "Login";
                Password_user.Text = string.Empty;
                ControlM1950.Enabled = false;
                comboBox_user.Enabled = true;
            }

            // login
            try
            {

                if (Serial_Connect.Text == "Disconnect")
                {
                    Serial_Connect.Text = "Connected";
                    Serial_Port_Start();
                }

            }
            catch
            {
                MessageBox.Show("Port Error!");
            }
            Process_checksum();
        }

        private void Save_data_user_Click(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.user1 = dataUser.Rows[0].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.user2 = dataUser.Rows[1].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.user3 = dataUser.Rows[2].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.user4 = dataUser.Rows[3].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.user5 = dataUser.Rows[4].Cells[1].Value.ToString();

            M1940.Properties.Settings.Default.pass1 = dataUser.Rows[0].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.pass2 = dataUser.Rows[1].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.pass3 = dataUser.Rows[2].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.pass4 = dataUser.Rows[3].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.pass5 = dataUser.Rows[4].Cells[2].Value.ToString();


            M1940.Properties.Settings.Default.Save();
            MessageBox.Show("Save data user complete!");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            SOCKET1.BackColor = Color.Lime;
        }

        private void Save_data_ROM_Click(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.PR1 = dataROM.Rows[0].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR2 = dataROM.Rows[1].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR3 = dataROM.Rows[2].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR4 = dataROM.Rows[3].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR5 = dataROM.Rows[4].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR6 = dataROM.Rows[5].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR7 = dataROM.Rows[6].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR8 = dataROM.Rows[7].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR9 = dataROM.Rows[8].Cells[1].Value.ToString();
            M1940.Properties.Settings.Default.PR10 = dataROM.Rows[9].Cells[1].Value.ToString();

            M1940.Properties.Settings.Default.CK1 = dataROM.Rows[0].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK2 = dataROM.Rows[1].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK3 = dataROM.Rows[2].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK4 = dataROM.Rows[3].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK5 = dataROM.Rows[4].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK6 = dataROM.Rows[5].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK7 = dataROM.Rows[6].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK8 = dataROM.Rows[7].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK9 = dataROM.Rows[8].Cells[2].Value.ToString();
            M1940.Properties.Settings.Default.CK10 = dataROM.Rows[9].Cells[2].Value.ToString();

            M1940.Properties.Settings.Default.IC1 = dataROM.Rows[0].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC2 = dataROM.Rows[1].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC3 = dataROM.Rows[2].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC4 = dataROM.Rows[3].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC5 = dataROM.Rows[4].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC6 = dataROM.Rows[5].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC7 = dataROM.Rows[6].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC8 = dataROM.Rows[7].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC9 = dataROM.Rows[8].Cells[3].Value.ToString();
            M1940.Properties.Settings.Default.IC10 = dataROM.Rows[9].Cells[3].Value.ToString();


            M1940.Properties.Settings.Default.SK1 = dataROM.Rows[0].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK2 = dataROM.Rows[1].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK3 = dataROM.Rows[2].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK4 = dataROM.Rows[3].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK5 = dataROM.Rows[4].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK6 = dataROM.Rows[5].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK7 = dataROM.Rows[6].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK8 = dataROM.Rows[7].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK9 = dataROM.Rows[8].Cells[4].Value.ToString();
            M1940.Properties.Settings.Default.SK10 = dataROM.Rows[9].Cells[4].Value.ToString();


            M1940.Properties.Settings.Default.CP1 = dataROM.Rows[0].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP2 = dataROM.Rows[1].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP3 = dataROM.Rows[2].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP4 = dataROM.Rows[3].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP5 = dataROM.Rows[4].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP6 = dataROM.Rows[5].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP7 = dataROM.Rows[6].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP8 = dataROM.Rows[7].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP9 = dataROM.Rows[8].Cells[5].Value.ToString();
            M1940.Properties.Settings.Default.CP10 = dataROM.Rows[9].Cells[5].Value.ToString();

            M1940.Properties.Settings.Default.MK1 = dataROM.Rows[0].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK2 = dataROM.Rows[1].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK3 = dataROM.Rows[2].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK4 = dataROM.Rows[3].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK5 = dataROM.Rows[4].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK6 = dataROM.Rows[5].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK7 = dataROM.Rows[6].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK8 = dataROM.Rows[7].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK9 = dataROM.Rows[8].Cells[6].Value.ToString();
            M1940.Properties.Settings.Default.MK10 = dataROM.Rows[9].Cells[6].Value.ToString();

            M1940.Properties.Settings.Default.Save();
            MessageBox.Show("Save data ROM complete!");

        }

        private void VF_Click(object sender, EventArgs e)
        {
            M1950_RS232.Req_VR = 1;
            progressBar1.Value = 0;

            Command.Text = null;
            try
            {
                M1950_command.Verify();
            }
            catch
            {
                MessageBox.Show("M1940 Lost connect!");
            }
            if (sc1.Checked == true) SOCKET1.BackColor = Color.White;
            if (sc2.Checked == true) SOCKET2.BackColor = Color.White;
            if (sc3.Checked == true) SOCKET3.BackColor = Color.White;
            if (sc4.Checked == true) SOCKET4.BackColor = Color.White;
            if (sc5.Checked == true) SOCKET5.BackColor = Color.White;
            if (sc6.Checked == true) SOCKET6.BackColor = Color.White;
            if (sc7.Checked == true) SOCKET7.BackColor = Color.White;
            if (sc8.Checked == true) SOCKET8.BackColor = Color.White;

            ST1.Text = string.Empty;
            ST2.Text = string.Empty;
            ST3.Text = string.Empty;
            ST4.Text = string.Empty;
            ST5.Text = string.Empty;
            ST6.Text = string.Empty;
            ST7.Text = string.Empty;
            ST8.Text = string.Empty;
        }

        private void CT_Click(object sender, EventArgs e)
        {
            M1950_RS232.Req_COT = 1;
            progressBar1.Value = 0;
            Command.Text = null;
            try
            {
                M1950_command.Continous();
            }
            catch
            {
                MessageBox.Show("M1940 Lost connect!");
            }
            if (sc1.Checked == true) SOCKET1.BackColor = Color.White;
            if (sc2.Checked == true) SOCKET2.BackColor = Color.White;
            if (sc3.Checked == true) SOCKET3.BackColor = Color.White;
            if (sc4.Checked == true) SOCKET4.BackColor = Color.White;
            if (sc5.Checked == true) SOCKET5.BackColor = Color.White;
            if (sc6.Checked == true) SOCKET6.BackColor = Color.White;
            if (sc7.Checked == true) SOCKET7.BackColor = Color.White;
            if (sc8.Checked == true) SOCKET8.BackColor = Color.White;

            ST1.Text = string.Empty;
            ST2.Text = string.Empty;
            ST3.Text = string.Empty;
            ST4.Text = string.Empty;
            ST5.Text = string.Empty;
            ST6.Text = string.Empty;
            ST7.Text = string.Empty;
            ST8.Text = string.Empty;
        }

        private void BL_Click(object sender, EventArgs e)
        {
            M1950_RS232.Req_BL = 1;
            progressBar1.Value = 0;

            Command.Text = null;
            try
            {
                M1950_command.Blank();
            }
            catch
            {
                MessageBox.Show("M1940 Lost connect!");
            }
            if (sc1.Checked == true) SOCKET1.BackColor = Color.White;
            if (sc2.Checked == true) SOCKET2.BackColor = Color.White;
            if (sc3.Checked == true) SOCKET3.BackColor = Color.White;
            if (sc4.Checked == true) SOCKET4.BackColor = Color.White;
            if (sc5.Checked == true) SOCKET5.BackColor = Color.White;
            if (sc6.Checked == true) SOCKET6.BackColor = Color.White;
            if (sc7.Checked == true) SOCKET7.BackColor = Color.White;
            if (sc8.Checked == true) SOCKET8.BackColor = Color.White;

            ST1.Text = string.Empty;
            ST2.Text = string.Empty;
            ST3.Text = string.Empty;
            ST4.Text = string.Empty;
            ST5.Text = string.Empty;
            ST6.Text = string.Empty;
            ST7.Text = string.Empty;
            ST8.Text = string.Empty;
        }
        private void Ers_Click(object sender, EventArgs e)
        {
            M1950_RS232.Req_ER = 1;
            progressBar1.Value = 0;

            Command.Text = null;
            try
            {
                M1950_command.eR();
            }
            catch
            {
                MessageBox.Show("M1940 Lost connect!");
            }
            if (sc1.Checked == true) SOCKET1.BackColor = Color.White;
            if (sc2.Checked == true) SOCKET2.BackColor = Color.White;
            if (sc3.Checked == true) SOCKET3.BackColor = Color.White;
            if (sc4.Checked == true) SOCKET4.BackColor = Color.White;
            if (sc5.Checked == true) SOCKET5.BackColor = Color.White;
            if (sc6.Checked == true) SOCKET6.BackColor = Color.White;
            if (sc7.Checked == true) SOCKET7.BackColor = Color.White;
            if (sc8.Checked == true) SOCKET8.BackColor = Color.White;

            ST1.Text = string.Empty;
            ST2.Text = string.Empty;
            ST3.Text = string.Empty;
            ST4.Text = string.Empty;
            ST5.Text = string.Empty;
            ST6.Text = string.Empty;
            ST7.Text = string.Empty;
            ST8.Text = string.Empty;
        }

        private void Checksum_Ans_TextChanged(object sender, EventArgs e)
        {
            if(Checksum_Ans.Text == M1940.Properties.Settings.Default.CK1)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC1;
                PD_code.Text = M1940.Properties.Settings.Default.PR1;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK1;
                MK_color.Text = M1940.Properties.Settings.Default.MK1;
            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK2)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC2;
                PD_code.Text = M1940.Properties.Settings.Default.PR2;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK2;
                MK_color.Text = M1940.Properties.Settings.Default.MK2;


            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK3)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC3;
                PD_code.Text = M1940.Properties.Settings.Default.PR3;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK3;
                MK_color.Text = M1940.Properties.Settings.Default.MK3;

            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK4)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC4;
                PD_code.Text = M1940.Properties.Settings.Default.PR4;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK4;
                MK_color.Text = M1940.Properties.Settings.Default.MK3;

            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK5)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC5;
                PD_code.Text = M1940.Properties.Settings.Default.PR5;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK5;
                MK_color.Text = M1940.Properties.Settings.Default.MK5;

            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK6)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC6;
                PD_code.Text = M1940.Properties.Settings.Default.PR6;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK6;
                MK_color.Text = M1940.Properties.Settings.Default.MK6;

            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK7)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC7;
                PD_code.Text = M1940.Properties.Settings.Default.PR7;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK7;
                MK_color.Text = M1940.Properties.Settings.Default.MK7;

            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK8)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC8;
                PD_code.Text = M1940.Properties.Settings.Default.PR8;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK8;
                MK_color.Text = M1940.Properties.Settings.Default.MK8;

            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK9)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC9;
                PD_code.Text = M1940.Properties.Settings.Default.PR9;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK9;
                MK_color.Text = M1940.Properties.Settings.Default.MK9;

            }
            if (Checksum_Ans.Text == M1940.Properties.Settings.Default.CK10)
            {
                IC_code.Text = M1940.Properties.Settings.Default.IC10;
                PD_code.Text = M1940.Properties.Settings.Default.PR10;
                Sk_unit.Text = M1940.Properties.Settings.Default.SK10;
                MK_color.Text = M1940.Properties.Settings.Default.MK10;

            }
        }

        private void blank_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.En_Blank = blank.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (blank.Checked == true)
                BL.Visible = true;
            else
                BL.Visible = false;
        }

        private void VERIFY_CheckedChanged(object sender, EventArgs e)
        {
            M1940.Properties.Settings.Default.En_Verify = VERIFY.Checked.ToString();
            M1940.Properties.Settings.Default.Save();
            if (VERIFY.Checked == true)
                VF.Visible = true;
            else
                VF.Visible = false;
        }

        private void RESET_NG_Click(object sender, EventArgs e)
        {
            Status_Process_ERR = 0;

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            WARRING Warring = new WARRING();
            // CHECK MODBUS PORT
            Warring.ShowDialog();
            if (Warring.DialogResult == DialogResult.OK)
            {
               
            }
        }

        private void Reset_counter_Click(object sender, EventArgs e)
        {
            M1950_RS232.num_Pass = 0;
            M1950_RS232.num_ERROR = 0;
            M1950_RS232.num_Sum = 0;

            Count_NG.Text = "0";
            Count_PASS.Text = "0";
            Count_Total.Text = "0";
        }

  
 
    }
}
