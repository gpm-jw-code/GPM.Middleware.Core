using GPM.Middleware.Core.Models.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using static GPM.Middleware.Core.Models.Communication.clsServer.Client;

namespace GPM.Middleware.Core.Models.Communication
{
    public partial class clsServer : clsSocketBase
    {
        private CancellationTokenSource listenTaskTokenSource = new CancellationTokenSource();
        public event EventHandler<bool> ServiceStateOnChanged;
        public static event EventHandler<Client> ClientConnectIn;
        private bool isServing = false;
        public int Port { get; private set; }
        public CLIENT_TYPE clientType { get; private set; }

        public clsServer(CLIENT_TYPE clientType)
        {
            this.clientType = clientType;
        }

        public async Task<bool> StartService(string host = "0.0.0.0", int port = 1330)
        {
            try
            {
                host = host == "localhost" ? "127.0.0.1" : host;
                StopService();
                Logger.Info($"Try build TCP Server - {host}:{port}");
                Port = port;
                EndPoint endPoint = new IPEndPoint(IPAddress.Parse(host), port);
                state.socketInstance.Bind(endPoint);
                listenTaskTokenSource = new CancellationTokenSource();
                Task.Run(() => Listen(), listenTaskTokenSource.Token);

                Logger.Info($"TCP Server Listening  {host}:{port}");
                ServiceStateOnChanged?.Invoke(this, true);
                isServing = true;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await StartService(host, port);
                });
                ServiceStateOnChanged?.Invoke(this, false);
                isServing = false;
                return false;
            }
        }


        public void StopService()
        {
            listenTaskTokenSource.Cancel();
            try
            {
                state.socketInstance.Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            state.socketInstance = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            isServing = false;
        }

        virtual public void Listen()
        {
            try
            {
                state.socketInstance.Listen(10);
                while (true)
                {
                    Socket clientSocket = state.socketInstance.Accept();
                    Client clientState = new Client(clientSocket, this.clientType)
                    {
                        connectedTime = DateTime.Now
                    };

                    clientState.OnSocketClose += ClientState_OnSocketClose;
                    ClientConnectIn?.Invoke(this, clientState);

                    Logger.Info($"Client Connected:{clientSocket.RemoteEndPoint.ToString()}");

                }
            }
            catch (TaskCanceledException ex)
            {
                Logger.Info("Server Listen task canceled");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Logger.Info("Server Listen task canceled");
            }
        }

        private void ClientState_OnSocketClose(object sender, EventArgs e)
        {
            Client clientState = (Client)sender;
            Logger.Info($"Client {clientState.EndPoint} 已經斷線");
        }

    }



}
