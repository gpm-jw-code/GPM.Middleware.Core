using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using GPM.Middleware.Core.Models.System;
using GPM.Middleware.Core.Models.Communication.Middleware;

namespace GPM.Middleware.Core.Models.Communication
{
    public class clsIRClient : clsSocketBase
    {
        public class clsDataResult
        {
            public DateTime time { get; }
            public double maxTemp { get; }
            public int index { get; }

            public clsDataResult(int index, DateTime time, double maxTemp)
            {
                this.index = index;
                this.time = time;
                this.maxTemp = maxTemp;
            }
        }
        public bool Connected { get; private set; }
        public Socket socket => state.socketInstance;
        public event EventHandler<List<clsDataResult>> DataReceived;
        private CancellationTokenSource waitServerReplyCancelSource;
        private CancellationTokenSource loopFetchDataTaskCancelSource;
        private clsCurrentThermalValue serverRelpyThermalValue;

        public string RevDataStr { get; private set; }
        public bool Connect(string host, int port)
        {
            try
            {
                socket.Connect(host, port);
                Connected = state.socketInstance.Connected;
                if (Connected)
                {
                    socket.BeginReceive(state.Buffer, 0, 1024, SocketFlags.None, new AsyncCallback(MsgRevHandle), state);
                }
                return Connected;
            }
            catch (Exception ex)
            {
                Connected = false;
                return false;
            }
        }

        private void MsgRevHandle(IAsyncResult ar)
        {
            clsSocketState state = (clsSocketState)ar.AsyncState;
            int revLen = state.socketInstance.EndReceive(ar);
            if (revLen > 0)
            {
                try
                {
                    string str = Encoding.ASCII.GetString(state.Buffer).Replace("\0", "");
                    if (str.Contains("ERR"))
                    {
                    }
                    else
                    {
                        state.AddData(revLen);

                        if (state.ASCII.Contains("\r\n"))
                        {
                            int len = BitConverter.ToInt32(new byte[4] { state.revBytes[3], state.revBytes[2], state.revBytes[1], state.revBytes[0] }, 0);
                            byte[] data = new byte[len];
                            Array.Copy(state.revBytes.ToArray(), 4, data, 0, len);
                            string acii = Encoding.ASCII.GetString(data);
                            List<clsDataResult> results = new List<clsDataResult>();
                            foreach (var dp in acii.Split('#'))
                            {
                                string[] dpSplited = dp.Split(',');
                                int id = int.Parse(dpSplited[0]);
                                DateTime time = DateTime.Parse(dpSplited[1]);
                                double maxtemp = double.Parse(dpSplited[2]);
                                clsDataResult result = new clsDataResult(id, time, maxtemp);
                                results.Add(result);
                            };
                            DataReceived?.BeginInvoke(this, results, null, null);
                            state.Reset();
                            waitServerReplyCancelSource.Cancel();
                        }
                        state.Buffer = new byte[1024];
                        socket.BeginReceive(state.Buffer, 0, 1024, SocketFlags.None, new AsyncCallback(MsgRevHandle), state);
                    }

                }
                catch (Exception ex)
                {
                    socket.BeginReceive(state.Buffer, 0, 1024, SocketFlags.None, new AsyncCallback(MsgRevHandle), state);

                }
            }
        }

        public async Task<clsCurrentThermalValue> GetThermalValue()
        {
            waitServerReplyCancelSource = new CancellationTokenSource();
            if (!Connected)
                throw new Exception("尚未與Server連線");
            var cmd = Encoding.ASCII.GetBytes("CURRENT_VALUE\r\n");
            socket.Send(cmd, 0, cmd.Length, SocketFlags.None);
            await ServerReply();
            return serverRelpyThermalValue;
        }
        public async Task StartRecordThermalValue()
        {
            if (!Connected)
                throw new Exception("尚未與Server連線");
            loopFetchDataTaskCancelSource = new CancellationTokenSource();
            await Task.Run(async () =>
             {
                 while (true)
                 {
                     await Task.Delay(1);
                     if (loopFetchDataTaskCancelSource.IsCancellationRequested)
                         break;
                     waitServerReplyCancelSource = new CancellationTokenSource();
                     RevDataStr = "";
                     socket.Send(Encoding.ASCII.GetBytes("START_RECORD_HEX,10\r\n"));
                     await ServerReply();
                 }
             });

        }
        public async Task EndRecordThermalValue()
        {
            loopFetchDataTaskCancelSource.Cancel();
        }

        private List<clsDataResult> DataParse()
        {
            string[] dataPacks = RevDataStr.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            List<clsDataResult> dataList = new List<clsDataResult>();
            foreach (var line in dataPacks)
            {
                string[] splited = line.Split(',');
                int id = int.Parse(splited[1]);
                long timestamp = long.Parse(splited[2]);
                var time = TicksToDateTime(timestamp);
                double maxtemp = double.Parse(splited[3]);
                clsDataResult result = new clsDataResult(id, time, maxtemp);
                dataList.Add(result);
            }
            return dataList;

        }
        /// <summary>
        /// 根据时间戳timestamp（单位毫秒）计算日期
        /// </summary>
        private DateTime TicksToDateTime(long timestamp)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long t = dt1970.Ticks + timestamp * 10000;
            return new DateTime(t);
        }
        private async Task ServerReply()
        {
            try
            {
                await Task.Delay(-1, waitServerReplyCancelSource.Token);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
