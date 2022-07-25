using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using GPM.Middleware.Core.Models.Communication.Middleware;
using GPM.Middleware.Core.Utility;

namespace GPM.Middleware.Core.Models.Communication
{
    public partial class clsServer
    {
        public class Client : IDisposable
        {
            public enum CLIENT_TYPE
            {
                IR, LR, SSM
            }

            public clsSocketState state = new clsSocketState();
            public event EventHandler OnSocketClose;
            public event EventHandler<string> CmdReceieved;
            private Stopwatch activeWatch = new Stopwatch();
            public static event EventHandler<string> OnCmdReceieving;
            private bool disposedValue;
            public readonly CLIENT_TYPE clientType;
            public DateTime connectedTime;
            public DateTime lastRequestTime;
            public string lastRequestCmd = "";

            public TimeSpan IdlingSec => activeWatch.Elapsed;

            public static event EventHandler<Client> LongTimeNoAction;

            public bool isRecording
            {
                get
                {
                    StaUtility.ControlMiddleware.tcpClientRecords.TryGetValue(this, out clsDataRecorder instance);
                    return instance == null ? false : instance.isRecording;
                }
            }

            public Socket socket => state.socketInstance;
            public string EndPoint
            {
                get
                {
                    try
                    {
                        return state.socketInstance.RemoteEndPoint.ToString();
                    }
                    catch (Exception)
                    {
                        return ":";
                    }
                }
            }

            public uscClientStateUI StateUI { get; set; }

            public Client(Socket clientSocket, CLIENT_TYPE clientType)
            {
                this.clientType = clientType;
                state.socketInstance = clientSocket;
                clientSocket.BeginReceive(state.Buffer, 0, 1024, SocketFlags.None, new AsyncCallback(MsgRecieveHandle), state);


                activeWatch = Stopwatch.StartNew();
                Task.Run(() => NoActionDetection());

            }

            private void NoActionDetection()
            {
                while (true)
                {
                    Thread.Sleep(1);
                    if (isRecording)
                        activeWatch.Restart();
                    if (disposedValue)
                        return;

                    try
                    {
                        if (!socket.Connected)
                        {
                            LongTimeNoAction?.Invoke(null, this);
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    if (StaUtility.SysParams.AutoKickOutNoActiveTcpClient)
                    {
                        if (activeWatch.ElapsedMilliseconds > (Debugger.IsAttached ? 30000 : StaUtility.SysParams.NoAcitveAllowTime * 1000))
                        {
                            try
                            {
                                socket.Disconnect(true);
                            }
                            catch (Exception ex)
                            {
                            }
                            LongTimeNoAction?.Invoke(null, this);
                            return;
                        }
                    }
                }
            }

            private async void MsgRecieveHandle(IAsyncResult ar)
            {
                try
                {
                    clsSocketState state = (clsSocketState)ar.AsyncState;
                    int recLen = state.socketInstance.EndReceive(ar);
                    if (recLen > 0)
                    {
                        activeWatch.Restart();
                        string revCmd = Encoding.ASCII.GetString(state.Buffer).Replace("\0", "");
                        lastRequestTime = DateTime.Now;
                        lastRequestCmd = revCmd;
                        OnCmdReceieving?.Invoke(this, revCmd);
                        CmdReceieved?.Invoke(this, revCmd);
                        state.Buffer = new byte[1024];
                        state.socketInstance.BeginReceive(state.Buffer, 0, 1024, SocketFlags.None, new AsyncCallback(MsgRecieveHandle), state);
                        System.SysStateChangeInvoke.Invoke();
                    }
                }
                catch (SocketException ex)
                {
                    DisconnectInvoke();
                }

            }

            internal void DisconnectInvoke()
            {
                OnSocketClose?.BeginInvoke(this, null, null, null);
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

                    disposedValue = true;
                }
            }

            // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
            // ~Client()
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



}
