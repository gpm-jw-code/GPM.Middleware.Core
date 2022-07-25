using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace GPM.Middleware.Core.Keyence
{
    public class RS232Interface
    {

        public class clsIRDataState
        {
            public DateTime Time { get; set; } = DateTime.MinValue;
            public double Value { get; set; } = -1;
        }

        public ModuleSerialPort ModuleSerialPort = new ModuleSerialPort();
        public string com => ModuleSerialPort.com;
        public int baudrate => ModuleSerialPort.baudrate;

        public bool Connected => ModuleSerialPort.connected;
        public List<string> ampIDList;
        public Dictionary<string, clsIRDataState> currentMesValue { get; private set; } = new Dictionary<string, clsIRDataState>();
        public DateTime Time { get; private set; }

        public Action<KeyValuePair<string, double>> MesValueOnReceieve;
        public event EventHandler<bool> ConnectStateOnChanged;

        private CancellationTokenSource loopRevDataTaskCancelTokenSource = new CancellationTokenSource() { };
        private bool loopProcessEndFlag = true;
        private DateTime lastRequestTime = DateTime.MinValue;


        public RS232Interface(List<string> ampIDList)
        {
            this.ampIDList = ampIDList;
            loopRevDataTaskCancelTokenSource.Cancel();
        }

        public bool Connect(string com, int baudrate)
        {
            bool connected = ModuleSerialPort.Open(com, baudrate);

            if (connected)
            {
                StartContinueGetMesValue();
            }
            else
                AutoReconnect();
            ConnectStateOnChanged?.BeginInvoke(this, connected, null, null);
            return connected;
        }

        private bool Connect()
        {
            return ModuleSerialPort.Open(com, baudrate);

        }


        public async Task<string> SendCommand(string cmdStr, int timeout = 5000)
        {
            //bool loopingRequestRunning = !loopRevDataTaskCancelTokenSource.Token.IsCancellationRequested;
            //if (loopingRequestRunning)
            //{
            //    loopRevDataTaskCancelTokenSource?.Cancel();
            //    await Task.Delay(100);
            //}
            await RequestDelayCheck();
            //if (loopingRequestRunning)
            //    StartContinueGetMesValue();
            return await Task.Run(async () =>
            {
                var response = await ModuleSerialPort.Write(cmdStr, timeout: timeout);
                return response;
            });

        }

        /// <summary>
        /// 取得當前量測讀值
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetCurrentMeasureValueAsync(string ampID, bool isLoopUse = false)
        {
            if (!isLoopUse)
                await EndContinueGetMesValue();

            string cmdStr = CommandStrBuilder(ampID, Commands.READ_COMMANDS.SR, DataNumbers.DATA_NUMBERS.Internal_measurement_value);
            string response = await SendCommand(cmdStr);

            if (response == "PORT CLOSE")
            {
                return -1;
            }

            if (!CheckResponse(cmdStr, response, 7))
            {
                return -1;
            }
            else
            {
                //±***.***
                string resData = response.Split(',')[3].Substring(0, 7);
                double val = Convert.ToDouble(resData);
                if (MesValueOnReceieve != null)
                    MesValueOnReceieve(new KeyValuePair<string, double>(ampID, val));
                return val;
            }

        }

        /// <summary>
        /// 取得Product Code
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetProductCode(string ampID)
        {
            await EndContinueGetMesValue();
            string cmdStr = CommandStrBuilder(ampID, Commands.READ_COMMANDS.SR, DataNumbers.DATA_NUMBERS.Product_Code);
            string response = await SendCommand(cmdStr);
            if (!CheckResponse(cmdStr, response, 4))
            {
                return "Response error";
            }
            else
            {
                //±***.***
                string resData = response.Split(',')[3].Substring(0, 4);
                return resData;
            }
        }


        /// <summary>
        /// 取得感測頭TYPE\r\n
        /// 0000= No connection;
        /// **0001= IL-030**;
        /// 0002= IL-065;
        /// 0003= IL-100;
        /// 0004= IL-300;
        /// 0005= IL-600;
        /// 0106= IL-S025;
        /// 0107= IL-S065;
        /// 0208= IL-S100;
        /// 0311= IL-2000;
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetConnected_sensor_head(string ampID)
        {
            await EndContinueGetMesValue();
            string cmdStr = CommandStrBuilder(ampID, Commands.READ_COMMANDS.SR, DataNumbers.DATA_NUMBERS.Connected_sensor_head);
            string response = await SendCommand(cmdStr);
            if (!CheckResponse(cmdStr, response, 4))
            {
                return "Response error";
            }
            else
            {
                //±***.***
                string resData = response.Split(',')[3].Substring(0, 4);
                return GetSensorType(resData);
            }
        }

        private string GetSensorType(string deviceResStr)
        {
            switch (deviceResStr)
            {
                case "0000":
                    return "NO-CONNECTED";
                case "0001":
                    return "IL-030";
                case "0002":
                    return "IL-065";
                case "0003":
                    return "IL-100";
                case "0004":
                    return "IL-300";
                case "0005":
                    return "IL-600";
                case "0106":
                    return "IL-S025";
                case "0107":
                    return "IL-S065";
                case "0208":
                    return "IL-S100";
                case "0311":
                    return "IL-2000";
                default:
                    return "Known";
            }
        }

        public async Task ZeroOffSetAsync(string ampID)
        {
            await EndContinueGetMesValue();
            string cmdStr = CommandStrBuilder(ampID, DataNumbers.DATA_NUMBERS.Zero_Shift_Request, "0");
            string response = await SendCommand(cmdStr);
            if (!CheckResponse(cmdStr, response, 0))
            {
            }
            else
            {
                cmdStr = CommandStrBuilder(ampID, DataNumbers.DATA_NUMBERS.Zero_Shift_Request, "1");
                response = await SendCommand(cmdStr);
            }
            StartContinueGetMesValue();
        }

        public async Task Check_Judgment_Ooutput_Status(string ampID)
        {
            string cmdStr = CommandStrBuilder(ampID, Commands.READ_COMMANDS.SR, DataNumbers.DATA_NUMBERS.Judgment_output_Alarm_output);
            string response = await SendCommand(cmdStr);
            if (!CheckResponse(cmdStr, response, 2))
            {
            }
            else
            {

            }
        }

        public void StartContinueGetMesValue()
        {
            if (!loopProcessEndFlag)
                return;

            loopRevDataTaskCancelTokenSource = new CancellationTokenSource();
            Task.Run(async () => await ContinueGetMesValueProcess(ampIDList));
        }

        public async Task EndContinueGetMesValue()
        {
            try
            {
                if (loopRevDataTaskCancelTokenSource.Token.IsCancellationRequested)
                    return;

                loopRevDataTaskCancelTokenSource.Cancel();
                while (!loopProcessEndFlag)
                {
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ContinueGetMesValueProcess(List<string> ampIDList)
        {
            bool portCloseFlag = false;
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    if (loopRevDataTaskCancelTokenSource.Token.IsCancellationRequested)
                        break;

                    foreach (string ampID in ampIDList)
                    {
                        if (!currentMesValue.ContainsKey(ampID))
                            currentMesValue.Add(ampID, new clsIRDataState());

                        var sesingValue = GetCurrentMeasureValueAsync(ampID, true).Result;
                        await Check_Judgment_Ooutput_Status(ampID);
                        portCloseFlag = sesingValue == -1;
                        if (portCloseFlag)
                        {
                            break;
                        }
                        currentMesValue[ampID].Time = DateTime.Now;
                        currentMesValue[ampID].Value = sesingValue;
                    }
                    if (portCloseFlag)
                        break;

                    loopProcessEndFlag = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }

            if (portCloseFlag)
            {
                Console.WriteLine("Port close...");
                AutoReconnect();
            }
            loopProcessEndFlag = true;
            Console.WriteLine("Stop continue get mes value process");
        }

        private void AutoReconnect()
        {
            try
            {
                ModuleSerialPort.Close();
            }
            catch (Exception ex)
            {
            }
            Task.Run(async () =>
            {
                while (!Connect())
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    Console.WriteLine("Reconnecting...");
                    ConnectStateOnChanged?.BeginInvoke(this, false, null, null);
                }
                ConnectStateOnChanged?.BeginInvoke(this, true, null, null);
                StartContinueGetMesValue();

            });
        }

        private string CommandStrBuilder(string ampID, Commands.READ_COMMANDS command, DataNumbers.DATA_NUMBERS data_number)
        {
            return $"{command},{ampID},{(int)data_number:000}\r\n";
        }



        private string CommandStrBuilder(string ampID, DataNumbers.DATA_NUMBERS data_number, string settingValue)
        {
            return $"{Commands.WRITE_COMMANDS.SW},{ampID},{(int)data_number:000},{settingValue}\r\n";
        }

        private bool CheckResponse(string send, string response, int resDataLen = 0)
        {
            bool checkResult = send.Substring(0, 7) == response.Substring(0, 7);
            if (checkResult && resDataLen > 0)
            {
                checkResult = response.Replace("\r\n", "").Split(',')[3].Length == resDataLen;
            }
            //Console.WriteLine("Send:{0}\r\nRev:{1}\r\n :::Check result :{2}", send, response, checkResult);
            return checkResult;
        }

        private async Task RequestDelayCheck()
        {
            while ((DateTime.Now - lastRequestTime).TotalMilliseconds < 40)
            {
                await Task.Delay(1);
            }
        }

    }
}
