using System;
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



namespace M1950
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

        public CFW()
        {
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;
            Systems_setup();

        }
        void Systems_setup()
        {
            Checksum_TargetMCU.Text = M1950.Properties.Settings.Default.checksum;
            Folder.Text = M1950.Properties.Settings.Default.pathLogfile;
            En_Dwn.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.En_Download);
            En_Checksum.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.En_Checksum);
            Erase.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.En_Ers);
            En_Buzz.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.En_Buzz);

            sc1.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S1);
            sc2.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S2);
            sc3.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S3);
            sc4.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S4);
            sc5.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S5);
            sc6.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S6);
            sc7.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S7);
            sc8.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S8);
            sc9.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S9);
            sc10.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S10);
            sc11.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S11);
            sc12.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S12);
            sc13.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S13);
            sc14.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S14);
            sc15.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S15);
            sc16.Checked = Convert.ToBoolean(M1950.Properties.Settings.Default.S16);



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
                string pathTotal = M1950.Properties.Settings.Default.pathLogfile;
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
        }

        private void Cksum_Click(object sender, EventArgs e)
        {
            M1950_RS232.Req_checksum_MCU = 1;
            Command.Text = null;
            try
            {
                M1950_command.Checksum8D();
            }
            catch
            {
                MessageBox.Show("M1950 Lost connect!");
            }
       
        }

        private void Dw_Click(object sender, EventArgs e)
        {
            M1950_RS232.Req_Pragram = 1;
            Command.Text = null;
            try
            {
                M1950_command.Program();
            }
            catch
            {
                MessageBox.Show("M1950 Lost connect!");
            }
            if (sc1.Checked == true) SOCKET1.BackColor = Color.White;
            if (sc2.Checked == true) SOCKET2.BackColor = Color.White;
            if (sc3.Checked == true) SOCKET3.BackColor = Color.White;
            if (sc4.Checked == true) SOCKET4.BackColor = Color.White;
            if (sc5.Checked == true) SOCKET5.BackColor = Color.White;
            if (sc6.Checked == true) SOCKET6.BackColor = Color.White;
            if (sc7.Checked == true) SOCKET7.BackColor = Color.White;
            if (sc8.Checked == true) SOCKET8.BackColor = Color.White;
            if (sc9.Checked == true) SOCKET9.BackColor = Color.White;
            if (sc10.Checked == true) SOCKET10.BackColor = Color.White;
            if (sc11.Checked == true) SOCKET11.BackColor = Color.White;
            if (sc12.Checked == true) SOCKET12.BackColor = Color.White;
            if (sc13.Checked == true) SOCKET13.BackColor = Color.White;

            if (sc14.Checked == true) SOCKET14.BackColor = Color.White;
            if (sc15.Checked == true) SOCKET15.BackColor = Color.White;
            if (sc16.Checked == true) SOCKET16.BackColor = Color.White;

        }


        private void Command_TextChanged(object sender, EventArgs e)
        {
            Command.SelectionStart = Command.Text.Length;
            Command.ScrollToCaret();
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
            if(M1950_RS232.Req_Pragram > 0 && Serial_Connect.Text == "Connected")
            {
                M1950_RS232.Req_Pragram++;
                if (M1950_RS232.Req_Pragram > 50)
                {
                    M1950_RS232.Req_Pragram = 0;
                    try
                    {
                        //MessageBox.Show("CHECK READ");
                        Read_result_Program();
                    }
                    catch
                    {

                    }
                    
                }
            }
            if(M1950_RS232.Req_checksum_MCU > 0 && Serial_Connect.Text == "Connected")
            {
                M1950_RS232.Req_checksum_MCU++;
                if (M1950_RS232.Req_checksum_MCU > 20)
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
            if (sc1.Checked == true)
            {
                if (temp[0] == 'o') SOCKET1.BackColor = Color.LimeGreen;
                else if (temp[0] == 'x') SOCKET1.BackColor = Color.Red;
                else if (temp[0] == '-') SOCKET1.BackColor = Color.Gray;
            }
            // SOCKET 2
            if (sc2.Checked == true)
            {
                if (temp[1] == 'o') SOCKET2.BackColor = Color.LimeGreen;
                else if (temp[1] == 'x') SOCKET2.BackColor = Color.Red;
                else if (temp[1] == '-') SOCKET2.BackColor = Color.Gray;
            }
            // SOCKET 3
            if (sc3.Checked == true)
            {
                if (temp[2] == 'o') SOCKET3.BackColor = Color.LimeGreen;
                else if (temp[2] == 'x') SOCKET3.BackColor = Color.Red;
                else if (temp[2] == '-') SOCKET3.BackColor = Color.Gray;
            }
            // SOCKET 4
            if (sc4.Checked == true)
            {
                if (temp[3] == 'o') SOCKET4.BackColor = Color.LimeGreen;
                else if (temp[3] == 'x') SOCKET4.BackColor = Color.Red;
                else if (temp[3] == '-') SOCKET4.BackColor = Color.Gray;
            }
            // SOCKET 5
            if (sc5.Checked == true)
            {
                if (temp[4] == 'o') SOCKET5.BackColor = Color.LimeGreen;
                else if (temp[4] == 'x') SOCKET5.BackColor = Color.Red;
                else if (temp[4] == '-') SOCKET5.BackColor = Color.Gray;
            }
            // SOCKET 6
            if (sc6.Checked == true)
            {
                if (temp[5] == 'o') SOCKET6.BackColor = Color.LimeGreen;
                else if (temp[5] == 'x') SOCKET6.BackColor = Color.Red;
                else if (temp[5] == '-') SOCKET6.BackColor = Color.Gray;
            }
            // SOCKET 7
            if (sc7.Checked == true)
            {
                if (temp[6] == 'o') SOCKET7.BackColor = Color.LimeGreen;
                else if (temp[6] == 'x') SOCKET7.BackColor = Color.Red;
                else if (temp[6] == '-') SOCKET7.BackColor = Color.Gray;
            }
            // SOCKET 8
            if (sc8.Checked == true)
            {
                if (temp[7] == 'o') SOCKET8.BackColor = Color.LimeGreen;
                else if (temp[7] == 'x') SOCKET8.BackColor = Color.Red;
                else if (temp[7] == '-') SOCKET8.BackColor = Color.Gray;
            }
            // SOCKET 9
            if (sc9.Checked == true)
            {
                if (temp[8] == 'o') SOCKET9.BackColor = Color.LimeGreen;
                else if (temp[8] == 'x') SOCKET9.BackColor = Color.Red;
                else if (temp[8] == '-') SOCKET9.BackColor = Color.Gray;
            }

            // SOCKET 10
            if (sc10.Checked == true)
            {
                if (temp[9] == 'o') SOCKET10.BackColor = Color.LimeGreen;
                else if (temp[9] == 'x') SOCKET10.BackColor = Color.Red;
                else if (temp[9] == '-') SOCKET10.BackColor = Color.Gray;
            }
            // SOCKET 11
            if (sc11.Checked == true)
            {
                if (temp[10] == 'o') SOCKET11.BackColor = Color.LimeGreen;
                else if (temp[10] == 'x') SOCKET11.BackColor = Color.Red;
                else if (temp[10] == '-') SOCKET11.BackColor = Color.Gray;
            }
            // SOCKET 12
            if (sc12.Checked == true)
            {
                if (temp[11] == 'o') SOCKET12.BackColor = Color.LimeGreen;
                else if (temp[11] == 'x') SOCKET12.BackColor = Color.Red;
                else if (temp[11] == '-') SOCKET12.BackColor = Color.Gray;
            }
            // SOCKET 13
            if (sc13.Checked == true)
            {
                if (temp[12] == 'o') SOCKET13.BackColor = Color.LimeGreen;
                else if (temp[12] == 'x') SOCKET13.BackColor = Color.Red;
                else if (temp[12] == '-') SOCKET13.BackColor = Color.Gray;
            }
            // SOCKET 14
            if (sc14.Checked == true)
            {
                if (temp[13] == 'o') SOCKET14.BackColor = Color.LimeGreen;
                else if (temp[13] == 'x') SOCKET14.BackColor = Color.Red;
                else if (temp[13] == '-') SOCKET14.BackColor = Color.Gray;
            }
            // SOCKET 15
            if (sc15.Checked == true)
            {
                if (temp[14] == 'o') SOCKET15.BackColor = Color.LimeGreen;
                else if (temp[14] == 'x') SOCKET15.BackColor = Color.Red;
                else if (temp[14] == '-') SOCKET15.BackColor = Color.Gray;
            }
            // SOCKET 16
            if (sc16.Checked == true)
            {
                if (temp[15] == 'o') SOCKET16.BackColor = Color.LimeGreen;
                else if (temp[15] == 'x') SOCKET16.BackColor = Color.Red;
                else if (temp[15] == '-') SOCKET16.BackColor = Color.Gray;
            }

            for(int i = 0;i<16;i++)
            {
                if (temp[i] == 'o')
                {
                    M1950_RS232.num_Pass++;
                    M1950_RS232.Socket_NG[i] = 0;
                    M1950_RS232.Socket_OK[i] = 1;
                }
                else if (temp[i] == 'x')
                {
                    M1950_RS232.num_ERROR++;
                    M1950_RS232.Socket_NG[i] = 1;
                    M1950_RS232.Socket_OK[i] = 0;
                }
            }
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



        private void Checksum_TargetMCU_TextChanged(object sender, EventArgs e)
        {
            Target_Infor.Text = Checksum_TargetMCU.Text;
        }

        private void Save_checksum_Click(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.checksum = Checksum_TargetMCU.Text;
            M1950.Properties.Settings.Default.Save();
            MessageBox.Show("Save Checksum MCU Complete!");
        }

        private void Browse_File_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Folder.Text = folderBrowserDialog1.SelectedPath;

                M1950.Properties.Settings.Default.pathLogfile = Folder.Text;
                M1950.Properties.Settings.Default.Save();

            }
        }



        private void En_Dwn_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.En_Download = En_Dwn.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (En_Dwn.Checked == true)
                Dw.Visible = true;
            else
                Dw.Visible = false;
        }

        private void En_Checksum_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.En_Checksum = En_Checksum.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (En_Checksum.Checked == true)
                Cksum.Visible = true;
            else
                Cksum.Visible = false;
        }

        private void Erase_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.En_Ers = Erase.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (Erase.Checked == true)
                Ers.Visible = true;
            else
                Ers.Visible = false;
        }

        private void Login_Click(object sender, EventArgs e)
        {
            if (Password.Text == M1950.Properties.Settings.Default.password)
            {
                Password.BackColor = Color.Lime;
                SettingProcess.Enabled = true;
            }
            else
            {
                Password.BackColor = Color.Red;
                SettingProcess.Enabled = false;
            }
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            SettingProcess.Enabled = false;
            Password.BackColor = Color.White;
            Password.Text = "";
        }

        private void Ers_Click(object sender, EventArgs e)
        {
            Req_Erase();
        }

        private void Req_Erase()
        {
            M1950_command.Write("ers\r\n");
        }

        private void En_Buzz_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.En_Buzz = En_Buzz.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
           
        }

        void Logfiletxt()
        {
            string pathTotal = M1950.Properties.Settings.Default.pathLogfile;
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
                    string Full_path_NG_file = Path.Combine(Full_path_NG,"[SOCKET "+ j +"] W" + time + ".txt");
                    using (FileStream fileStream = File.Create(Full_path_NG_file))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine("SOCKET" + j+ "___" + DateTime.Now.ToString("MM-dd-yyyy-HH:mm:ss"));

                    }
                }
                else if(M1950_RS232.Socket_OK[i] == 1)
                {
                    string Full_path_OK_file = Path.Combine(Full_path_OK, "[SOCKET " + j + "] W" + time + ".txt");
                    using (FileStream fileStream = File.Create(Full_path_OK_file))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine("SOCKET" + j + "___" + DateTime.Now.ToString("MM-dd-yyyy-HH:mm:ss"));

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
            M1950.Properties.Settings.Default.S1 = sc1.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc1.Checked == true)
                SOCKET1.Enabled = true;
            else
                SOCKET1.Enabled = false;
        }

        private void sc2_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S2 = sc2.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc2.Checked == true)
                SOCKET2.Enabled = true;
            else
                SOCKET2.Enabled = false;
        }

        private void sc3_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S3 = sc3.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc3.Checked == true)
                SOCKET3.Enabled = true;
            else
                SOCKET3.Enabled = false;
        }

        private void sc4_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S4 = sc4.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc4.Checked == true)
                SOCKET4.Enabled = true;
            else
                SOCKET4.Enabled = false;
        }

        private void sc5_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S5 = sc5.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc5.Checked == true)
                SOCKET5.Enabled = true;
            else
                SOCKET5.Enabled = false;
        }

        private void sc6_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S6 = sc6.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc6.Checked == true)
                SOCKET6.Enabled = true;
            else
                SOCKET6.Enabled = false;
        }

        private void sc7_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S7 = sc7.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc7.Checked == true)
                SOCKET7.Enabled = true;
            else
                SOCKET7.Enabled = false;
        }

        private void sc8_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S8 = sc8.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc8.Checked == true)
                SOCKET8.Enabled = true;
            else
                SOCKET8.Enabled = false;
        }

        private void sc9_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S9 = sc9.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc9.Checked == true)
                SOCKET9.Enabled = true;
            else
                SOCKET9.Enabled = false;
        }

        private void sc10_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S10 = sc10.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc10.Checked == true)
                SOCKET10.Enabled = true;
            else
                SOCKET10.Enabled = false;
        }

        private void sc11_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S11 = sc11.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc11.Checked == true)
                SOCKET11.Enabled = true;
            else
                SOCKET11.Enabled = false;
        }

        private void sc12_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S12 = sc12.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc12.Checked == true)
                SOCKET12.Enabled = true;
            else
                SOCKET12.Enabled = false;
        }

        private void sc13_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S13 = sc13.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc13.Checked == true)
                SOCKET13.Enabled = true;
            else
                SOCKET13.Enabled = false;
        }

        private void sc14_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S14 = sc14.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc14.Checked == true)
                SOCKET14.Enabled = true;
            else
                SOCKET14.Enabled = false;
        }

        private void sc15_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S15 = sc15.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc15.Checked == true)
                SOCKET15.Enabled = true;
            else
                SOCKET15.Enabled = false;
        }

        private void sc16_CheckedChanged(object sender, EventArgs e)
        {
            M1950.Properties.Settings.Default.S16 = sc16.Checked.ToString();
            M1950.Properties.Settings.Default.Save();
            if (sc16.Checked == true)
                SOCKET16.Enabled = true;
            else
                SOCKET16.Enabled = false;
        }

        private void SOCKET_Enter(object sender, EventArgs e)
        {

        }

 
    }
}
