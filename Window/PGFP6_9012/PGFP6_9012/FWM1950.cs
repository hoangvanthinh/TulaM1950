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



namespace MINATO_M1950
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
            Checksum_TargetMCU.Text = MINATO_M1950.Properties.Settings.Default.checksum;
            Folder.Text = MINATO_M1950.Properties.Settings.Default.pathLogfile;
            En_Dwn.Checked = Convert.ToBoolean(MINATO_M1950.Properties.Settings.Default.En_Download);
            En_Checksum.Checked = Convert.ToBoolean(MINATO_M1950.Properties.Settings.Default.En_Checksum);
            Erase.Checked = Convert.ToBoolean(MINATO_M1950.Properties.Settings.Default.En_Ers);
            En_Buzz.Checked = Convert.ToBoolean(MINATO_M1950.Properties.Settings.Default.En_Buzz);
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
                R_COM.SelectedItem = "COM40";
                R_Baud.SelectedItem = "9600";
                R_DataBit.SelectedItem = "8";
                R_ParityBit.SelectedItem = "None";
                R_StopBit.SelectedItem = "1";

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
                string pathTotal = MINATO_M1950.Properties.Settings.Default.pathLogfile;
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

            try
            {
                M1950_command.Program();
            }
            catch
            {
                MessageBox.Show("M1950 Lost connect!");
            }
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
                if (M1950_RS232.Req_Pragram > 20)
                {
                    M1950_RS232.Req_Pragram = 0;
                    try
                    {
                        Read_result_Program();
                    }
                    catch
                    {

                    }
                    
                }
            }
        }
        private void Read_result_Program()
        {
            string temp;
            int pStart;
            temp = Command.Text;
            pStart = temp.LastIndexOf(";");
            temp = temp.Substring(pStart - 19, 16);
            textBox1.Text = temp;

            // SOCKET 1
            if (temp[0] == 'o') SOCKET1.BackColor = Color.LimeGreen;
            else if (temp[0] == 'x') SOCKET1.BackColor = Color.Red;
            else if (temp[0] == '-') SOCKET1.BackColor = Color.Gray;
                
            // SOCKET 2
            if (temp[1] == 'o') SOCKET2.BackColor = Color.LimeGreen;
            else if (temp[1] == 'x') SOCKET2.BackColor = Color.Red;
            else if (temp[1] == '-') SOCKET2.BackColor = Color.Gray;
            // SOCKET 3
            if (temp[2] == 'o') SOCKET3.BackColor = Color.LimeGreen;
            else if (temp[2] == 'x') SOCKET3.BackColor = Color.Red;
            else if (temp[2] == '-') SOCKET3.BackColor = Color.Gray;
            // SOCKET 4
            if (temp[3] == 'o') SOCKET4.BackColor = Color.LimeGreen;
            else if (temp[3] == 'x') SOCKET4.BackColor = Color.Red;
            else if (temp[3] == '-') SOCKET4.BackColor = Color.Gray;
            // SOCKET 5
            if (temp[4] == 'o') SOCKET5.BackColor = Color.LimeGreen;
            else if (temp[4] == 'x') SOCKET5.BackColor = Color.Red;
            else if (temp[4] == '-') SOCKET5.BackColor = Color.Gray;
            // SOCKET 6
            if (temp[5] == 'o') SOCKET6.BackColor = Color.LimeGreen;
            else if (temp[5] == 'x') SOCKET6.BackColor = Color.Red;
            else if (temp[5] == '-') SOCKET6.BackColor = Color.Gray;
            // SOCKET 7
            if (temp[6] == 'o') SOCKET7.BackColor = Color.LimeGreen;
            else if (temp[6] == 'x') SOCKET7.BackColor = Color.Red;
            else if (temp[6] == '-') SOCKET7.BackColor = Color.Gray;
            // SOCKET 8
            if (temp[7] == 'o') SOCKET8.BackColor = Color.LimeGreen;
            else if (temp[7] == 'x') SOCKET8.BackColor = Color.Red;
            else if (temp[7] == '-') SOCKET8.BackColor = Color.Gray;

            // SOCKET 9
            if (temp[8] == 'o') SOCKET9.BackColor = Color.LimeGreen;
            else if (temp[8] == 'x') SOCKET9.BackColor = Color.Red;
            else if (temp[8] == '-') SOCKET9.BackColor = Color.Gray;

            // SOCKET 10
            if (temp[9] == 'o') SOCKET10.BackColor = Color.LimeGreen;
            else if (temp[9] == 'x') SOCKET10.BackColor = Color.Red;
            else if (temp[9] == '-') SOCKET10.BackColor = Color.Gray;
            // SOCKET 11
            if (temp[10] == 'o') SOCKET11.BackColor = Color.LimeGreen;
            else if (temp[10] == 'x') SOCKET11.BackColor = Color.Red;
            else if (temp[10] == '-') SOCKET11.BackColor = Color.Gray;
            // SOCKET 12
            if (temp[12] == 'o') SOCKET12.BackColor = Color.LimeGreen;
            else if (temp[12] == 'x') SOCKET12.BackColor = Color.Red;
            else if (temp[12] == '-') SOCKET12.BackColor = Color.Gray;
            // SOCKET 13
            if (temp[12] == 'o') SOCKET13.BackColor = Color.LimeGreen;
            else if (temp[12] == 'x') SOCKET13.BackColor = Color.Red;
            else if (temp[12] == '-') SOCKET13.BackColor = Color.Gray;
            // SOCKET 14
            if (temp[13] == 'o') SOCKET14.BackColor = Color.LimeGreen;
            else if (temp[13] == 'x') SOCKET14.BackColor = Color.Red;
            else if (temp[13] == '-') SOCKET14.BackColor = Color.Gray;
            // SOCKET 15
            if (temp[14] == 'o') SOCKET15.BackColor = Color.LimeGreen;
            else if (temp[14] == 'x') SOCKET15.BackColor = Color.Red;
            else if (temp[14] == '-') SOCKET15.BackColor = Color.Gray;
            // SOCKET 16
            if (temp[15] == 'o') SOCKET16.BackColor = Color.LimeGreen;
            else if (temp[15] == 'x') SOCKET16.BackColor = Color.Red;
            else if (temp[15] == '-') SOCKET16.BackColor = Color.Gray;

            for(int i = 0;i<16;i++)
            {
                if (temp[i] == 'o')
                {
                    M1950_RS232.num_Pass++;
                    M1950_RS232.Socket_NG[i] = 0;
                }
                else if (temp[i] == 'x')
                {
                    M1950_RS232.num_ERROR++;
                    M1950_RS232.Socket_NG[i] = 1;
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
            MINATO_M1950.Properties.Settings.Default.checksum = Checksum_TargetMCU.Text;
            MINATO_M1950.Properties.Settings.Default.Save();
            MessageBox.Show("Save Checksum MCU Complete!");
        }

        private void Browse_File_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Folder.Text = folderBrowserDialog1.SelectedPath;

                MINATO_M1950.Properties.Settings.Default.pathLogfile = Folder.Text;
                MINATO_M1950.Properties.Settings.Default.Save();

            }
        }



        private void En_Dwn_CheckedChanged(object sender, EventArgs e)
        {
            MINATO_M1950.Properties.Settings.Default.En_Download = En_Dwn.Checked.ToString();
            MINATO_M1950.Properties.Settings.Default.Save();
            if (En_Dwn.Checked == true)
                Dw.Visible = true;
            else
                Dw.Visible = false;
        }

        private void En_Checksum_CheckedChanged(object sender, EventArgs e)
        {
            MINATO_M1950.Properties.Settings.Default.En_Checksum = En_Checksum.Checked.ToString();
            MINATO_M1950.Properties.Settings.Default.Save();
            if (En_Checksum.Checked == true)
                Cksum.Visible = true;
            else
                Cksum.Visible = false;
        }

        private void Erase_CheckedChanged(object sender, EventArgs e)
        {
            MINATO_M1950.Properties.Settings.Default.En_Ers = Erase.Checked.ToString();
            MINATO_M1950.Properties.Settings.Default.Save();
            if (Erase.Checked == true)
                Ers.Visible = true;
            else
                Ers.Visible = false;
        }

        private void Login_Click(object sender, EventArgs e)
        {
            if (Password.Text == MINATO_M1950.Properties.Settings.Default.password)
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
            MINATO_M1950.Properties.Settings.Default.En_Buzz = En_Buzz.Checked.ToString();
            MINATO_M1950.Properties.Settings.Default.Save();
           
        }

        void Logfiletxt()
        {
            string pathTotal = MINATO_M1950.Properties.Settings.Default.pathLogfile;
            string path_Day = DateTime.Now.ToString("MM-dd-yyyy").ToString();
            string time = DateTime.Now.ToString("HH-mm-ss").ToString();

            string Full_path = Path.Combine(pathTotal, path_Day);
            string Full_path_OK = Path.Combine(Full_path, "OK");
            string Full_path_NG = Path.Combine(Full_path, "NG");

            for(int i = 0;i<16;i++)
            {
                if(M1950_RS232.Socket_NG[i] == 1)
                {
                    string Full_path_NG_file = Path.Combine(Full_path_NG,"[SOCKET "+ i +"] W" + time + ".txt");
                    using (FileStream fileStream = File.Create(Full_path_NG_file))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine("SOCKET" + i + "___" + DateTime.Now.ToString("MM-dd-yyyy-HH:mm:ss"));

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

 
    }
}
