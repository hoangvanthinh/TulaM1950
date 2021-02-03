using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using EasyModbus;

namespace MINATO_M1950
{
    class ModbusRTU : EasyModbus.ModbusClient
    {
       
        public static int Status = 0;
        public static int dem = 0;
        /*---------------------------------- ADDRESS MODBUS ---------------------------------------------------------------*/
        public static int ADD_CONN = 1;
        public static int ADD_BUSY = 2;
        public static int ADD_PASS = 3;
        public static int ADD_ERROR = 4;

        public static int ADD_START = 5;
        public static int ADD_CLR = 6;
        public static int ADD_VRF = 7;
        public static int ADD_CTHT = 8;
        //------------- var --------------/
        public static int Status_CONN;
        public static int Status_BUSY;
        public static int Status_PASS;
        public static int Status_ERROR;
        public static int Status_CTHT;

        //public static int Command_CANCEL_CLR;
        //public static int Command_VRF;
        //public static int Command_START;




        public ModbusRTU(string SerialPort, byte ID, int baudrate, Parity parity, StopBits stopBits, int connectionTimeout)
            : base(SerialPort)
        {
            this.Baudrate = baudrate;
            this.Parity = parity;
            this.StopBits = stopBits;
            this.ConnectionTimeout = connectionTimeout;
        }
        
        public  void Start()
        {

            try
            {
                if (this.Connected == true) this.Disconnect();
                this.Connect();
                Thread newThread = new Thread((obj) =>
                    {
                        while(true)
                        {
                            try
                            {
                                Status_CONN = this.ReadHoldingRegisters(ADD_CONN, 1)[0];
                                Status_BUSY =   this.ReadHoldingRegisters(ADD_BUSY, 1)[0];
                                Status_ERROR =  this.ReadHoldingRegisters(ADD_ERROR, 1)[0];
                                Status_PASS =   this.ReadHoldingRegisters(ADD_PASS, 1)[0];
                                Status_CTHT = this.ReadHoldingRegisters(ADD_CTHT, 1)[0];

                                dem++;
                                //if (Main.LED_MCU.BackColor == Color.Red)
                                //    Main.LED_MCU.BackColor = Color.DodgerBlue;
                                //else
                                //    Main.LED_MCU.BackColor = Color.Red;
                                Thread.Sleep(300);
                            }
                            catch
                            {
                               
                            }
      
                        }
                    });
                newThread.IsBackground = true;
                newThread.Start();
            }
            catch (Exception ex)
            {
            //    throw ex;
                
            }

        }
        
    }
}
