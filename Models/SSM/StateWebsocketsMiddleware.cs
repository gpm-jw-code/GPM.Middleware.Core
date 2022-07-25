using GPM.Middleware.Core.Models.Communication.Websocket;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.SSM
{
    public class StateWebsocketsMiddleware
    {
        public List<WebSocket> websocketClients = new List<WebSocket>();

        public StateWebsocketsMiddleware()
        {

        }

        public void Push(WebSocket client)
        {
            websocketClients.Add(client);
        }

        public void Brocast(string message)
        {
            if (websocketClients.Count == 0)
                return;

            clsBrocastMessage brocastMessage = new clsBrocastMessage(DateTime.Now, message);
            ArraySegment<byte> ArraySegment = brocastMessage.GetArraySegment();

            foreach (var client in websocketClients.ToArray())
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        client.SendAsync(ArraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        if (client.State != WebSocketState.Open)
                            websocketClients.Remove(client);
                    }
                });
            }
        }
    }
}
