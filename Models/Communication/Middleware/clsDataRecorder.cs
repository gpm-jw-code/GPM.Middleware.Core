using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using GPM.Middleware.Core.Models.Log;
using System.IO;
using System.Drawing;
using static GPM.Middleware.Core.Models.Communication.clsServer;
using System.Linq;
using System.Diagnostics;
using static GPM.Middleware.Core.Models.Communication.Middleware.MiddlewareEmums;
using GPM.Middleware.Core.Utility;

namespace GPM.Middleware.Core.Models.Communication.Middleware
{

    public class clsDataRecorder : IDisposable
    {
        public class clsBitmapSaveModel
        {

            public DateTime Time { get; }
            public Bitmap Bitmap { get; }
            public clsBitmapSaveModel(DateTime time, Bitmap bitmap)
            {
                Time = time;
                Bitmap = bitmap;
            }
        }

        public DateTime startTime;
        public DateTime endTime;
        public string saveFolder { get; private set; }
        public Queue<clsBitmapSaveModel> bitmapSaveQueue = new Queue<clsBitmapSaveModel>();
        CancellationTokenSource cancelTokenSurce;
        Client _client;
        private bool disposedValue;

        public void ResumeRecord()
        {
            if (videoRecord)
            {
                manualStopRecord = false;
            }
        }

        public int TotalDataQty { get; private set; }
        private long _ClientRecordTime;
        public long ClientRecordTime
        {
            get => _ClientRecordTime;
            private set
            {
                _ClientRecordTime = value;
                ClientRecordDateTime = Tools.GetDateTimeFromTimeStamp(value);
            }
        }
        public DateTime ClientRecordDateTime;
        public readonly bool videoRecord;

        public bool isRecording { get; private set; } = false;

        public clsDataRecorder(Client client, int TotalDataQty, long ClientRecordTime, bool videoRecord)
        {
            Initialize();
            startTime = DateTime.Now;
            _client = client;
            this.TotalDataQty = TotalDataQty;
            this.ClientRecordTime = ClientRecordTime;
            this.videoRecord = videoRecord;
        }
        public clsDataRecorder(Client client, int TotalDataQty, long ClientRecordTime)
        {
            Initialize();
            startTime = DateTime.Now;
            _client = client;
            this.TotalDataQty = TotalDataQty;
            this.ClientRecordTime = ClientRecordTime;
        }

        public clsDataRecorder(Client client, int TotalDataQty, bool videoRecord)
        {
            startTime = DateTime.Now;
            _client = client;
            this.TotalDataQty = TotalDataQty;
            this.videoRecord = videoRecord;
        }


        public void StartRecord(DATA_FORMAT data_format, int dataNum)
        {
            Initialize();
            isRecording = true;
            cancelTokenSurce = new CancellationTokenSource();
            Task.Factory.StartNew(async () => await RecordingProcess(data_format, dataNum), cancelTokenSurce.Token);
        }


        public async Task EndRecord()
        {
            cancelTokenSurce.Cancel();
        }

        private void Initialize()
        {
            startTime = DateTime.Now;
            bitmapSaveQueue.Clear();
            saveFolder = $"Record/{ClientRecordDateTime.ToString("yyyy-MM-dd-HH-mm-ss")}";
            Directory.CreateDirectory(saveFolder);

        }

        private void Initialize(long clientRecordTime)
        {
            startTime = DateTime.Now;
            bitmapSaveQueue.Clear();
            saveFolder = $"Record/{clientRecordTime}";
            Directory.CreateDirectory(saveFolder);
        }


        private async Task RecordingProcess(DATA_FORMAT data_format, int dataNum) //5
        {
            int dataID = 0;
            string dataStr = "";
            Stopwatch watch = Stopwatch.StartNew();
            List<byte> dataBytes = new List<byte>();
            while (dataID < dataNum)
            {
                try
                {
                    await Task.Delay(40, cancelTokenSurce.Token);
                    DateTime time = Utility.StaUtility.selectRegionModel.Time;
                    var timeStampeASCII = time.ToString("yyyy/MM/dd HH:mm:ss.ffff");
                    var timeStampeLong = Tools.GetTimeStamp(time);
                    double maxTempVal = Utility.StaUtility.selectRegionModel.MaxTemp;
                    dataStr += $"{dataID.ToString("0000")},{timeStampeASCII},{maxTempVal.ToString("00.0")}#";
                    //data_format =HEX
                    dataBytes.AddRange(BitConverter.GetBytes(dataID).Reverse().ToArray());
                    dataBytes.AddRange(BitConverter.GetBytes(timeStampeLong).Reverse().ToArray());
                    dataBytes.Add((byte)int.Parse(Math.Round(maxTempVal * 10) + ""));
                    dataBytes.Add(Encoding.ASCII.GetBytes("#")[0]);
                    _ = Task.Factory.StartNew(() =>
                    {
                        Bitmap bitmap = (Bitmap)Utility.StaUtility.selectRegionModel.CopyBitmap.Clone();
                        bitmap.Save(Path.Combine(saveFolder, time.ToString("yyyy-MM-dd HH-mm-ss-ffff") + ".png"));
                    });
                    dataID += 1;
                }
                catch (TaskCanceledException ex)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    continue;
                }

            }
            try
            {
                //byte[idH,idL,] //65535? 
                watch.Stop();
                int lenthOfData = 1;
                byte[] headBytes = new byte[4];
                byte[] sendOutBytes = data_format == DATA_FORMAT.ASCII ? ASCIIDataByteBuild(dataStr, out lenthOfData, out headBytes)
                                                                            : HEXDataByteBuild(dataBytes, out lenthOfData, out headBytes);
                int sendHeadLen = _client.socket.Send(headBytes);
                Logger.Info($"Client({_client.EndPoint})發送{sendHeadLen} {headBytes}");
                int sendDataLen = _client.socket.Send(sendOutBytes);
                Logger.Info($"Client({_client.EndPoint})請求完成,歷時 {watch.Elapsed} ||| 發送{sendDataLen} bytes(Head:4, data {lenthOfData}");
            }
            catch (SocketException ex)
            {
                _client.DisconnectInvoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            isRecording = false;
        }

        private byte[] HEXDataByteBuild(List<byte> dataBytes, out int datalen, out byte[] headBytes)
        {
            List<byte> output = new List<byte>();
            dataBytes.RemoveAt(dataBytes.Count - 1);
            output.AddRange(dataBytes);
            output.AddRange(new byte[2] { 0x0d, 0x0a });
            datalen = output.Count;
            headBytes = BitConverter.GetBytes(datalen).Reverse().ToArray();
            return output.ToArray();
        }

        private byte[] ASCIIDataByteBuild(string dataStr, out int datalen, out byte[] headBytes)
        {
            string _DataStrTrimed = dataStr.TrimEnd(new char[] { '#' });
            List<byte> sendOutDatabytes = new List<byte>();
            sendOutDatabytes.AddRange(Encoding.ASCII.GetBytes(_DataStrTrimed));
            sendOutDatabytes.AddRange(new byte[2] { 0x0d, 0x0a });
            datalen = sendOutDatabytes.Count;
            headBytes = BitConverter.GetBytes(datalen).Reverse().ToArray();

            return sendOutDatabytes.ToArray();
        }
        internal int RecordedDataNum { get; private set; } = 0;
        public bool manualStopRecord { get; set; } = false;
        public string RecordedString { get; private set; } = "";

        public string ErrorMsg = string.Empty;

        public async Task GetDataAndSave(int dataQty, long clientReocrdTime)
        {
            var sesingValType = _client.clientType;
            cancelTokenSurce = new CancellationTokenSource();
            TotalDataQty = dataQty;
            RecordedString = "";
            RecordedDataNum = 0;
            Stopwatch watch = Stopwatch.StartNew();
            isRecording = false;
            Logger.Info($"Client請求{dataQty}筆[{sesingValType}]DATA, Request Time in client side :{clientReocrdTime}");
            int dataIndex = 0;
            RecordedString = "CollectDataInquireAck#";
            try
            {
                while (dataIndex < dataQty)
                {
                    isRecording = true;
                    await Task.Delay(40);
                    if (cancelTokenSurce.IsCancellationRequested)
                        return;
                    if (TryGetSensingDataByType(sesingValType, out double sensingValue, out DateTime time, out ErrorMsg))
                    {
                        string serverRecordTime = time.ToString("yyyyMMddHHmmssfff");
                        #region IR Image record
                        if (sesingValType == Client.CLIENT_TYPE.IR && videoRecord && !manualStopRecord)
                        {
                            _ = Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    Bitmap bitmap = (Bitmap)StaUtility.selectRegionModel.CopyBitmap.Clone();
                                    bitmap.Save(Path.Combine(saveFolder, time.ToString("yyyy-MM-dd HH-mm-ss-ffff") + ".png"));
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);
                                }
                            });
                        }
                        #endregion
                        RecordedString += string.Format("ID,{0},{1},{2}#", dataIndex, serverRecordTime, sensingValue);
                        dataIndex += 1;
                        RecordedDataNum += 1;
                    }
                }
                watch.Stop();
                isRecording = false;
                Logger.Info($"Client({_client.EndPoint})請求完成,歷時 {watch.Elapsed} ");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private bool TryGetSensingDataByType(Client.CLIENT_TYPE clientType, out double sensingValue, out DateTime time, out string errMsg)
        {
            sensingValue = -1;
            time = DateTime.MinValue;
            errMsg = string.Empty;

            if (clientType == Client.CLIENT_TYPE.IR)
            {
                time = StaUtility.selectRegionModel.Time;
                sensingValue = StaUtility.selectRegionModel.MaxTemp;
            }
            else
            {
                if (!StaUtility.SysParams.IsSimulation)
                {
                    var valueState = StaUtility.dlRS1Interface.currentMesValue.Values.FirstOrDefault();
                    if (valueState != null)
                    {
                        time = valueState.Time;
                        sensingValue = valueState.Value;
                    }
                    else
                        return false;
                }
                else
                {
                    time = DateTime.Now;
                    sensingValue = time.Millisecond / 10.0;
                }
            }

            return true;
        }

        public void StopRecord()
        {
            manualStopRecord = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                RecordedString = null;

                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~clsThermalRecord()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


}
