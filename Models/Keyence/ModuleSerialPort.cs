using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Keyence
{
    public class ModuleSerialPort
    {
        public SerialPort serialPort { get; private set; } = new SerialPort();
        public string com { get; private set; }
        public int baudrate { get; private set; }
        public bool connected => serialPort.IsOpen;
        virtual public bool Open(string com, int baudrate = 9600)
        {
            try
            {
                this.com = com;
                this.baudrate = baudrate;
                serialPort.PortName = com;
                serialPort.BaudRate = baudrate;
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.NewLine = "\r\n";
                serialPort.Open();
                return serialPort.IsOpen;

            }
            catch (Exception)
            {
                return false;
            }
        }

        internal async Task<string> Write(string cmdStr, string checkEndStr = "\r\n", int timeout = 5000)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Write(cmdStr);
                    return await WaitResponse(checkEndStr, timeout);
                }
                else
                    return "PORT CLOSE";
            }
            catch (Exception ex)
            {
                return "PORT CLOSE";
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkEndStr"></param>
        /// <param name="timeout">ms</param>
        /// <returns></returns>
        internal async Task<string> WaitResponse(string checkEndStr = "\r\n", int timeout = 5000)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string response = "";

            while (!response.Contains(checkEndStr))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));

                if (sw.ElapsedMilliseconds >= timeout)
                {
                    response = "TIMEOUT";
                    break;
                }

                if (serialPort.BytesToRead == 0)
                {
                    continue;
                }
                string readInStr = serialPort.ReadExisting();
                if (readInStr != "")
                {
                    response += readInStr;
                    //Console.WriteLine("Module response : {0}", response);
                }
            }
            sw.Stop();
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
            // Console.WriteLine("response time spend: {0} ms", sw.ElapsedMilliseconds);

            return response;
        }

        internal void Close()
        {
            serialPort.Close();
        }
    }
}
