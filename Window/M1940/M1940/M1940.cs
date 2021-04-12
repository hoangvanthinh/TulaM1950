using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;


namespace M1940
{
    class M1950_RS232 : SerialPort
    {
        public static int flag = 10;
        public static int Req_Pragram = 0, Req_checksum_file = 0, Req_checksum_MCU = 0, Req_VR = 0, Req_COT = 0, Req_BL = 0, Req_ER = 0;
        public static int num_Pass = 0, num_ERROR = 0, num_Sum = 0;
        public static int[] Socket_NG = new int[16];
        public static int[] Socket_OK= new int[16];
        //============================== M1950 command ================================//
        private byte[] remote = new byte[2] { 0x05, 0x05};
        private string End_remote = "E\r\n";
        private string Copy = "OP\r\n";
        private string Command_Erase = "ER\r\n";
        private string Command_Blank = "BL\r\n";

        private string Command_Program = "W\r\n";
        private string Command_Verify = "VF\r\n";
        private string Command_CONTINUOUS = "CT\r\n";

        private string Command_Checksum4D = "BO\r\n";
        private string Command_Checksum8D = "BO8\r\n";


        //============================================================================

        public M1950_RS232(string SerialPort, int baudrate, Parity parity, StopBits stopBits)
            : base(SerialPort)
        {
            this.BaudRate = baudrate;
            this.Parity = parity;
            this.StopBits = stopBits;
            //    this.PortName = "COM15";

        }
        public void Start_RS232()
        {
            try
            {
                this.Open();
            }
            catch
            {
                MessageBox.Show("M1950 Disconnect!");
            }
        }
        public void REMOTE()
        {
            if(this.IsOpen)
            {
                this.Write(remote, 0,remote.Length);
            }
        }
        public void Program()
        {
            if (this.IsOpen)
            {
                this.Write(Command_Program);
            }
        }
        public void Verify()
        {
            if (this.IsOpen)
            {
                this.Write(Command_Verify);
            }
        }
        public void Continous()
        {
            if (this.IsOpen)
            {
                this.Write(Command_CONTINUOUS);
            }
        }
        public void eR()
        {
            if (this.IsOpen)
            {
                this.Write(Command_Erase);
            }
        }
        public void Blank()
        {
            if (this.IsOpen)
            {
                this.Write(Command_Blank);
            }
        }
        public void Checksum8D()
        {
            if (this.IsOpen)
            {
                this.Write(Command_Checksum8D);
            }
        }
        public void Checksum4D()
        {
            if (this.IsOpen)
            {
                this.Write(Command_Checksum4D);
            }
        }

    }
}
