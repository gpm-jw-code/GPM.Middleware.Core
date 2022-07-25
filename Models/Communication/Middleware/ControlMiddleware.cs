using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPM.Middleware.Core.Models.Log;
using System.Diagnostics;
using static GPM.Middleware.Core.Models.Communication.clsServer;
using static GPM.Middleware.Core.Models.Communication.Middleware.MiddlewareEmums;
using GPM.Middleware.Core.Utility;
using System.Net.Sockets;
using System.Windows.Forms;
using gpm_vibration_module_api;

namespace GPM.Middleware.Core.Models.Communication.Middleware
{
    public class ControlMiddleware
    {
        public Dictionary<Client, clsDataRecorder> tcpClientRecords = new Dictionary<Client, clsDataRecorder>();
        public Dictionary<Client, uscClientStateUI> stateUIs = new Dictionary<Client, uscClientStateUI>();

        public ControlMiddleware()
        {
            clsServer.ClientConnectIn += ClsServer_ClientConnectIn;
            Client.OnCmdReceieving += ClientCmdHandling;
            Client.LongTimeNoAction += Client_LongTimeNoAction;
        }

        private void ClsServer_ClientConnectIn(object sender, Client e)
        {
            if (!tcpClientRecords.ContainsKey(e))
            {
                tcpClientRecords.Add(e, null);
                System.SysStateChangeInvoke.Invoke();
            }
        }

        private void Client_LongTimeNoAction(object sender, Client client)
        {
            KickOut(client);
            Logger.Info($"Client {client.EndPoint} 因為長時間沒有活動，連線已被強制中斷。");
        }

        private async void ClientCmdHandling(object sender, string cmd)
        {
            Client client = (Client)sender;
            Logger.Info($"接收到 Client({client.EndPoint})[{client.clientType}] Command: {cmd}");

            if (client.clientType != Client.CLIENT_TYPE.SSM)
            {
                cmd = cmd.ToUpper().Replace(" ", "").Replace("\r\n", "");

                if (cmd.Contains("EXECUTEREQ"))
                {
                    ExecuteReqHandle(client, cmd);
                    return;
                }

                if (cmd.Contains("COLLECTDATAINQUIRE"))
                {
                    CollectDataInQuireHandle(client, cmd);
                    return;
                }
            }
            else
            {
                //TODO 定義 Protocols...
                SSMInQuireHandle(client, cmd);
            }
        }

        private async Task SSMInQuireHandle(Client client, string cmd)
        {

            string[] splited = cmd.Split(',');
            if (splited.Length != 4)
            {
                client.socket.Send(Tools.GetDataBytes("ERR,Argument Insufficient ", false));
                return;
            }
            // READ/WRITE,192.168.0.23:2999,P2P/RMS/RAW,23
            string actiontype = splited[0].ToUpper();
            if (actiontype != COMMAND_TYPE.SR.ToString() && actiontype != COMMAND_TYPE.SW.ToString())
            {
                client.socket.Send(Tools.GetDataBytes("ERR,ActionType Argument input error,must be 'SR' or 'SW' ", false));
                return;
            }
            string endPoint = splited[1];
            var ssminterface = SSM.SSMModuleManager.GetSSMInterfaceByEndPoint(endPoint);
            if (ssminterface == null)
            {
                client.socket.Send(Tools.GetDataBytes($"ERR,{endPoint} is not exist", false));
                return;
            }

            int dataType;
            SSM_DATA_TYPES ssmDataType;
            if (!int.TryParse(splited[2], out dataType))
            {
                client.socket.Send(Tools.GetDataBytes("ERR,DataType Argument input error", false));
                return;
            }
            else
            {
                if (!MiddlewareEmums.TryGetSSMDataType(dataType, out ssmDataType))
                {
                    client.socket.Send(Tools.GetDataBytes("ERR,DataType Argument input error", false));
                    return;
                }

            }

            int dataNum;
            if (!int.TryParse(splited[3], out dataNum))
            {
                client.socket.Send(Tools.GetDataBytes("ERR,DataNum Argument input error", false));
                return;
            }

            //Final ,do action
            if (actiontype == "SR")
            {
                var ouput = await ssminterface.GetInquireData(dataNum);
                string dataStr = GenSSMDataStr(ssmDataType, ouput);
                List<string> responsStrLs = new List<string>() { splited[0], splited[1], splited[2] };
                responsStrLs.Add(dataStr.Length + "");
                responsStrLs.Add(dataStr);
                responsStrLs.Add("\r\n");
                client.socket.Send(Tools.GetDataBytes(string.Join(",", responsStrLs).Replace(",\r\n", "\r\n"), false));
            }
            else
            {

            }

        }

        private string GenSSMDataStr(SSM_DATA_TYPES ssmDataType, List<DataSet> ouput)
        {
            string output = "";
            foreach (DataSet item in ouput)
            {
                output += $"{item.RecieveTime.ToString("yyyyMMdd HH:mm:ss.fff")}:";
                if (ssmDataType == SSM_DATA_TYPES.P2P)
                    output += string.Join(";", item.Features.AccP2P.axisValues) + "#";
                else if (ssmDataType == SSM_DATA_TYPES.RMS)
                    output += string.Join(";", item.Features.AccRMS.axisValues) + "#";
                else if (ssmDataType == SSM_DATA_TYPES.ENERGY)
                    output += string.Join(";", item.Features.VibrationEnergy.axisValues) + "#";
                else if (ssmDataType == SSM_DATA_TYPES.RAW)
                    output += string.Join(";", new string[3] { string.Join("&", item.AccData.X), string.Join("&", item.AccData.Y), string.Join("&", item.AccData.Z) }) + "#";
            }
            return output.TrimEnd(new char[] { '#', ',' });
        }

        public void KickOut(Client client)
        {
            //clientLongTimeNoAction?.Invoke(this, client);
            try
            {
                client.socket.Disconnect(true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            if (tcpClientRecords.TryGetValue(client, out clsDataRecorder recordReqInstance))
            {
                if (recordReqInstance != null && recordReqInstance.isRecording)
                    recordReqInstance.EndRecord();

                tcpClientRecords.Remove(client);
            }

            if (stateUIs.TryGetValue(client, out uscClientStateUI ui))
            {
                ui.BeginInvoke((MethodInvoker)delegate
                {
                    ui.Parent.Controls.Remove(ui);
                });
                stateUIs.Remove(client);
            }
            System.SysStateChangeInvoke.Invoke();
            client.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public int KickOut(string endPoint)
        {
            Client client = tcpClientRecords.Keys.FirstOrDefault(c => c.EndPoint == endPoint);
            if (client == null)
                return 0;
            KickOut(client);
            return 1;
        }
        private void CollectDataInQuireHandle(Client client, string cmd)
        {
            try
            {
                string[] splited = cmd.Split(',');
                int dataQty = int.Parse(splited[1]);
                long clientReocrdTime = long.Parse(splited[2]);

                if (tcpClientRecords.TryGetValue(client, out clsDataRecorder recordReqInstance))
                {
                    if (recordReqInstance.RecordedDataNum != dataQty)
                    {
                        string response = recordReqInstance.ErrorMsg == string.Empty ? "NOTYET" : recordReqInstance.ErrorMsg;
                        client.socket.Send(Tools.GetDataBytes($"CollectDataInquireAck#{response}"));
                    }
                    else
                    {
                        var send = client.socket.Send(Tools.GetDataBytes(recordReqInstance.RecordedString));
                        bool done = recordReqInstance.RecordedString.Length + 4 == send;
                        Logger.Info($"Client({client.EndPoint})請求,確認請求已執行完畢,發送{send} bytes {done}");
                    }
                }
                else
                {
                    client.socket.Send(Tools.GetDataBytes("CollectDataInquireAck#NOTYET"));
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                client.socket.Send(Tools.GetDataBytes("CollectDataInquireAck#NOTYET"));
                Logger.Info($"ERR:[CollectDataInquireAck]指令錯誤({cmd})");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        virtual public void ExecuteReqHandle(Client client, string cmd)
        {
            string[] splited = cmd.Split(',');
            if (client.clientType == Client.CLIENT_TYPE.IR && splited.Length != 4)
            {
                string errMsg = $"ERR:[EXECUTEREG]指令錯誤-缺少參數({cmd})";
                client.socket.Send(Tools.GetDataBytes("ExecuteReqAck,NOTREADY")); //指定錯誤
                client.StateUI.UpdateState(errMsg);
                Logger.Info(errMsg);
                return;
            }
            try
            {
                var sensorType = client.clientType;

                if (!ConnectionCheckInWhenExecute(sensorType))
                {
                    string errMsg = $"Client {client.EndPoint} [{sensorType}] ExecuteReq 被拒絕了，因為裝置連線狀態有異常";
                    Logger.Info(errMsg);
                    client.socket.Send(Tools.GetDataBytes("ExecuteReqAck,NOTREADY"));
                    client.StateUI.UpdateState("Reject,Device Connection Error");
                    return;
                }

                if (tcpClientRecords.ToList().FindAll(i => i.Key.clientType == sensorType).Any(d => d.Value.isRecording))
                {
                    string errMsg = $"Client {client.EndPoint} ExecuteReq 被拒絕了，因為有其他遠端作業正在進行中";
                    Logger.Info(errMsg);
                    client.StateUI.UpdateState(errMsg);
                    client.socket.Send(Tools.GetDataBytes("ExecuteReqAck,NOTREADY"));
                    return;
                }

                int totalDataQty = int.Parse(splited[1]);
                long clientRecordTime = long.Parse(splited[2]);
                bool videoRecord = sensorType == Client.CLIENT_TYPE.IR ? splited[3] == "1" : false;
                clsDataRecorder recordREQ = new clsDataRecorder(client, totalDataQty, clientRecordTime, videoRecord: videoRecord);
                if (!tcpClientRecords.ContainsKey(client))
                    tcpClientRecords.Add(client, recordREQ);
                else
                {
                    if (tcpClientRecords[client].isRecording)
                    {
                        client.socket.Send(Tools.GetDataBytes("ExecuteReqAck,NOTREADY"));
                        return;
                    }
                    tcpClientRecords[client].Dispose();
                    tcpClientRecords[client] = recordREQ;
                }
                recordREQ.GetDataAndSave(totalDataQty, clientRecordTime);
                client.socket.Send(Tools.GetDataBytes("ExecuteReqAck,OK"));
            }
            catch (Exception ex)
            {
                client.socket.Send(Tools.GetDataBytes("ExecuteReqAck,NOTREADY")); //指定錯誤
                string errMsg = $"ERR:[EXECUTEREG]指令錯誤 {ex.Message}({cmd})";
                Logger.Info(errMsg);
                client.StateUI.UpdateState(errMsg);
            }
        }

        bool ConnectionCheckInWhenExecute(Client.CLIENT_TYPE clientType)
        {
            if (StaUtility.SysParams.IsSimulation)
                return true;

            if (clientType == Client.CLIENT_TYPE.IR)
                return StaUtility.DevicesConnectionStates.IsIRConnected;
            else
                return StaUtility.DevicesConnectionStates.IsLRConnected;
        }


    }
}
