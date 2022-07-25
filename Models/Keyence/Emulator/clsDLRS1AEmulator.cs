using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Keyence.Emulator
{
    public class clsDLRS1AEmulator : ModuleSerialPort
    {
        public override bool Open(string com, int baudrate = 9600)
        {
            bool connected = base.Open(com, baudrate);
            if (connected)
            {
                serialPort.DataReceived += SerialPort_DataReceived;
            }
            else
            {
                Task.Run(async () =>
                {
                    while (!Open())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    Console.WriteLine("模擬器COM PORT重新連線");
                    serialPort.DataReceived += SerialPort_DataReceived;
                });
            }
            return connected;
        }

        private bool Open()
        {
            return base.Open(com, baudrate);
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            serialPort.DataReceived -= SerialPort_DataReceived;
            string cmd = "";
            try
            {
                cmd = serialPort.ReadExisting();
            }
            catch (Exception)
            {
            }
            if (cmd != "" && cmd.Contains("\r\n"))
            {
                try
                {
                    string[] splited = cmd.Replace("\r\n", "").Split(',');
                    string response = "\r\n";
                    //data num = 2
                    string dataNum = splited[2];
                    if (dataNum == "193")
                    {
                        response = cmd.Insert(9, ",1234");
                    }
                    else if (dataNum == "195")
                    {
                        response = cmd.Insert(9, ",0001");
                    }
                    else if (dataNum == "038")
                    {
                        //response len :7
                        int id = int.Parse(splited[1]);
                        string resValue = $"-{id.ToString("00")}{DateTime.Now.Second.ToString("00")}.{DateTime.Now.Millisecond.ToString().Substring(0, 1)}";
                        response = cmd.Insert(9, $",{resValue}");
                    }
                    serialPort.Write(response);
                }
                catch (Exception ex)
                {

                }

            }
            else
            {

            }

            serialPort.DataReceived += SerialPort_DataReceived;


        }
    }
}
