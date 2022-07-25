using GPM.Middleware.Core.Models.Communication;
using GPM.Middleware.Core.Models.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.SSM
{
    public class EmulatorHepler
    {
        public static void CreateSSMEmulators(List<int> portList)
        {
            foreach (int port in portList)
            {

                clsModuleEmulate emulater = new clsModuleEmulate(port);
            }

        }
    }

    public class clsModuleEmulate : clsServer
    {

        public int Port { get; }
        public clsModuleEmulate(int port) : base(Client.CLIENT_TYPE.SSM)
        {
            Port = port;
            StartService("127.0.0.1", Port);
        }


        public override void Listen()
        {
            try
            {
                state.socketInstance.Listen(10);
                while (true)
                {
                    Socket clientSocket = state.socketInstance.Accept();
                    var clientstate = new clsSocketState()
                    {
                        socketInstance = clientSocket,
                    };
                    clientSocket.BeginReceive(clientstate.Buffer, 0, clientstate.Buffer.Length, SocketFlags.None, new AsyncCallback(CallBack), clientstate);

                }
            }
            catch (TaskCanceledException ex)
            {
            }
            catch (Exception ex)
            {
            }
        }

        private void CallBack(IAsyncResult ar)
        {
            try
            {
                clsSocketState state = (clsSocketState)ar.AsyncState;
                int revLen = state.socketInstance.EndReceive(ar);
                if (revLen > 0)
                {
                    string cmd = Encoding.ASCII.GetString(state.Buffer).Replace("\0", "");
                    if (cmd == "READVALUE\r\n")
                    {
                        state.socketInstance.Send(GenFakeDataByte());
                    }
                    else
                    {
                        if (revLen == 11)
                        {
                            ArraySegment<byte> buffer = new ArraySegment<byte>(state.Buffer, 1, 8);
                            state.socketInstance.Send(buffer.ToArray());
                        }
                    }
                    state.Buffer = new byte[state.Buffer.Length];
                    state.socketInstance.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, (CallBack), state);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }

        private byte[] GenFakeDataByte()
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 3072; i++)
            {
                bytes.Add((byte)(i * new Random().Next()));
            }
            return bytes.ToArray();
        }
    }

}
