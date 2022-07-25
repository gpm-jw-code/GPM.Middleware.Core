using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GPM.Middleware.Core.Models.Communication.Middleware;
using GPM.Middleware.Core.Models.Log;
using gpm_vibration_module_api;
using Newtonsoft.Json;

namespace GPM.Middleware.Core.Models.SSM
{

    public class clsSSMInterface
    {
        public class clsInquireDataState
        {
            public int dataNum { get; set; } = 1;
            public List<DataSet> Datas = new List<DataSet>();
            internal bool isCollectDone => Datas.Count == dataNum;
        }


        private CancellationTokenSource fetchworkCancelTokSoure = new CancellationTokenSource();
        private CancellationTokenSource retryConnworkCancelTokSoure = new CancellationTokenSource();
        private ConcurrentDictionary<long, clsInquireDataState> InquiringBuffers = new ConcurrentDictionary<long, clsInquireDataState>();
        public StateWebsocketsMiddleware LogUseWebsocketMiddleware { get; set; } = new StateWebsocketsMiddleware();
        public StateWebsocketsMiddleware SensingDataUseWebsocketMiddleware { get; set; } = new StateWebsocketsMiddleware();
        public StateWebsocketsMiddleware StateMonitorUseWebsocketMiddleware { get; set; } = new StateWebsocketsMiddleware();

        public GPMModuleAPI ssmAPI { get; private set; }
        public bool connected => ssmAPI.Connected;
        public string ip { get; private set; }
        public int port { get; private set; }
        public string EndPoint => $"{ip}:{port}";

        internal bool dataFetchingFlag = false;

        private DataSet _dataSet;
        public DataSet dataSet
        {
            get => _dataSet;
            internal set
            {
                _dataSet = value;
                _dataSet.RecieveTime = DateTime.Now;
                InjectDataToInquireDataBufs(_dataSet);
                SensingDataUseWebsocketMiddleware.Brocast($"{JsonConvert.SerializeObject(_dataSet)}");
            }
        }

        private void InjectDataToInquireDataBufs(DataSet dataSet)
        {
            foreach (var buf in InquiringBuffers.Values)
            {
                Task.Run(() =>
                {
                    if (!buf.isCollectDone)
                        buf.Datas.Add(dataSet);
                });
            }
        }

        internal void Disconnect()
        {
            try
            {
                ssmAPI.Disconnect();
            }
            catch (Exception)
            {
            }
        }

        public async Task<int> ConnectAsync(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            int errorCode = await Task.Delay(1).ContinueWith((tsk) =>
            {
                ssmAPI = new GPMModuleAPI();
                return ssmAPI.Connect(ip, port).Result;
            });

            if (errorCode == 0)
            {
                StartDataFatchProcess();
            }
            else
            {
                RetryConnect();
            }
            StateMonitorUseWebsocketMiddleware.Brocast(errorCode == 0 ? "Connected" : "Disconnected");
            return errorCode;
        }

        public async Task<int> SetMeasureReage(int range)
        {
            LogInfo($"User set measure range:{range}G");
            fetchworkCancelTokSoure.Cancel();
            while (dataFetchingFlag)
            {
                await Task.Delay(1);
            }
            var rangEnum = getMREnum(range);
            if (rangEnum == clsEnum.Module_Setting_Enum.MEASURE_RANGE.KNOWN)
            {
                LogInfo($"{range}G is not allowable value , reject ");
                StartDataFatchProcess();
                return 404;
            }
            int errorCode = await ssmAPI.Measure_Range_Setting(rangEnum);
            StartDataFatchProcess();
            LogInfo($"Measure range setting {(errorCode == 0 ? "OK" : "FAIL")}({errorCode})");
            return errorCode;
        }

        private clsEnum.Module_Setting_Enum.MEASURE_RANGE getMREnum(int intRange)
        {
            if (intRange == 2)
                return clsEnum.Module_Setting_Enum.MEASURE_RANGE.MR_2G;
            if (intRange == 4)
                return clsEnum.Module_Setting_Enum.MEASURE_RANGE.MR_4G;
            if (intRange == 8)
                return clsEnum.Module_Setting_Enum.MEASURE_RANGE.MR_8G;
            if (intRange == 16)
                return clsEnum.Module_Setting_Enum.MEASURE_RANGE.MR_16G;
            if (intRange == 32)
                return clsEnum.Module_Setting_Enum.MEASURE_RANGE.MR_32G;
            if (intRange == 64)
                return clsEnum.Module_Setting_Enum.MEASURE_RANGE.MR_64G;

            return clsEnum.Module_Setting_Enum.MEASURE_RANGE.KNOWN;
        }

        public void StopWorks()
        {
            LogInfo($"StopWorks");
            retryConnworkCancelTokSoure.Cancel();
            fetchworkCancelTokSoure.Cancel();
        }

        private void RetryConnect()
        {
            retryConnworkCancelTokSoure = new CancellationTokenSource();
            Task.Run(() =>
            {
                StateMonitorUseWebsocketMiddleware.Brocast("Disconnected");
                while (ssmAPI.Connect(ip, port).Result != 0)
                {
                    LogInfo($"Trying connect...");
                    StateMonitorUseWebsocketMiddleware.Brocast("Connecting");
                    Thread.Sleep(1000);
                    if (retryConnworkCancelTokSoure.IsCancellationRequested)
                        return;
                }
                StateMonitorUseWebsocketMiddleware.Brocast("Connected");
                StartDataFatchProcess();
            });
        }

        internal async Task<List<DataSet>> GetInquireData(int dataNum)
        {
            long key = DateTime.Now.Ticks;

            if (InquiringBuffers.TryAdd(key, new clsInquireDataState() { dataNum = dataNum }))
            {
                Stopwatch watch = Stopwatch.StartNew();
                while (!InquiringBuffers[key].isCollectDone)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(0.01));
                }
                watch.Stop();
                LogInfo($"GetInquireData,data num:{dataNum} , Time Spend : {watch.ElapsedMilliseconds} ms");
                if (InquiringBuffers.TryRemove(key, out clsInquireDataState asyncState))
                    return asyncState.Datas;
                else
                    return new List<DataSet>();

            }
            else
                return new List<DataSet>();

        }

        private void StartDataFatchProcess()
        {
            fetchworkCancelTokSoure = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (true)
                {
                    dataFetchingFlag = true;
                    StateMonitorUseWebsocketMiddleware.Brocast("Connected");
                    if (fetchworkCancelTokSoure.IsCancellationRequested)
                        break;

                    ///real
                    dataSet = await ssmAPI.GetData(true, true);

                    //模擬
                    //dataSet = new DataSet()
                    //{
                    //    RecieveTime = DateTime.Now,
                    //    ErrorCode = 0,
                    //};
                    Thread.Sleep(1);

                }
                dataFetchingFlag = false;
                LogInfo($"DataFatchProcess END by user");
            });
        }

        public async Task<DataSet.clsOtherFeatures.clsP2p> GetP2PValueAsync()
        {
            if (!connected)
                throw new Exception("尚未連線");
            DataSet dataSet = await ssmAPI.GetData(false, true);
            return dataSet.Features.AccP2P;
        }


        public async Task<DataSet.clsOtherFeatures.clsAccRMS> GetRMSValueAsync()
        {
            if (!connected)
                throw new Exception("尚未連線");
            DataSet dataSet = await ssmAPI.GetData(false, true);
            return dataSet.Features.AccRMS;
        }

        public async Task<DataSet.clsOtherFeatures.clsEnergy> GetVEValueAsync()
        {
            if (!connected)
                throw new Exception("尚未連線");
            DataSet dataSet = await ssmAPI.GetData(false, true);
            return dataSet.Features.VibrationEnergy;
        }

        public async Task<DataSet.clsAcc> GetRawGValuesAsync()
        {
            if (!connected)
                throw new Exception("尚未連線");
            DataSet dataSet = await ssmAPI.GetData(false, true);
            return dataSet.AccData;
        }

        #region Log interface 

        private void LogInfo(string message, bool websocketBrocast = true)
        {
            string msg = $"SSM({EndPoint}) {message}";
            Logger.Info(msg);
            if (websocketBrocast)
                WebSocketBrocast(msg);
        }

        private void LogError(Exception ex, bool websocketBrocast = true)
        {
            Logger.Error(ex);
            string msg = $"SSM({EndPoint}) {ex.Message}";
            if (websocketBrocast)
                WebSocketBrocast(msg);
        }

        private void WebSocketBrocast(string message)
        {
            LogUseWebsocketMiddleware.Brocast(message);
        }
        #endregion
    }
}
