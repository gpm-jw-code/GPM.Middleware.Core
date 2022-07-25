using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.Communication
{
    public class clsSocketState
    {
        public int revDataLen => revBytes.Count;
        public string ASCII => Encoding.ASCII.GetString(revBytes.ToArray());
        public List<byte> revBytes = new List<byte>();
        public Socket socketInstance;
        public byte[] Buffer = new byte[1024];
        public clsSocketState()
        {
            socketInstance = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        internal void AddData(int revLen)
        {
            byte[] data = new byte[revLen];
            Array.Copy(Buffer, 0, data, 0, revLen);
            revBytes.AddRange(data);
        }

        internal void Reset()
        {
            revBytes.Clear();
        }
    }
}
