using System;
using System.Text;
using Newtonsoft.Json;

namespace GPM.Middleware.Core.Models.Communication.Websocket
{
    public class clsBrocastMessage
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public clsBrocastMessage(DateTime time, string message)
        {
            Time = time;
            Message = message;
        }

        public ArraySegment<byte> GetArraySegment()
        {
            return new ArraySegment<byte>(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this)));
        }
    }
}
