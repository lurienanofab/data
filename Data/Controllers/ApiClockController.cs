using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using System.Net.WebSockets;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using LNF;
using Data.Models;

namespace Data.Controllers
{
    public class TimeModel
    {
        public DateTime Current { get; set; }
        public string Source { get; set; }
    }

    public class ApiClockController : ApiController
    {
        public TimeModel Get(string source = "server")
        {
            if (source == "server")
                return new TimeModel() { Current = ClockUtility.GetServerTime(), Source = source };
            else
                return new TimeModel() { Current = ClockUtility.GetNetworkTime(ConfigurationManager.AppSettings["NtpServer"]), Source = source };
        }

        public HttpResponseMessage GetSocket(string source)
        {
            HttpContext.Current.Items["source"] = source;
            if (HttpContext.Current.IsWebSocketRequest)
                HttpContext.Current.AcceptWebSocketRequest(ProcessSocket);
            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }

        private async Task ProcessSocket(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;

            while (true)
            {
                if (socket.State == WebSocketState.Open)
                {
                    string source = context.Items["source"].ToString();

                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
                    var response = Get(source);

                    buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(ServiceProvider.Current.Serialization.Json.SerializeObject(response)));
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    await Task.Delay(1000);
                }
                else
                    break;
            }
        }
    }
}
