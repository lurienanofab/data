﻿using Data.Models;
using LNF;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;

namespace Data.Controllers
{
    public class TimeModel
    {
        public DateTime Current { get; set; }
        public string Source { get; set; }
    }

    public class ApiClockController : ApiController
    {
        [Route("api/clock/{source}")]
        public TimeModel Get(string source = "server")
        {
            if (source == "server")
                return new TimeModel() { Current = ClockUtility.GetServerTime(), Source = source };
            else
                return new TimeModel() { Current = ClockUtility.GetNetworkTime(ConfigurationManager.AppSettings["NtpServer"]), Source = source };
        }

        [Route("api/clock/{source}/socket")]
        public HttpResponseMessage GetSocket(string source)
        {
            HttpContext.Current.Items["source"] = source;
            if (HttpContext.Current.IsWebSocketRequest)
                HttpContext.Current.AcceptWebSocketRequest(ProcessSocket);
            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }

        private string Serialize(object obj)
        {
            return Startup.WebApp.Context.GetInstance<IProvider>().Utility.Serialization.Json.SerializeObject(obj);
        }

        private async Task ProcessSocket(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;

            while (true)
            {
                if (socket.State == WebSocketState.Open)
                {
                    string source = context.Items["source"].ToString();
                    var response = Get(source);

                    ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(Serialize(response)));
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    await Task.Delay(1000);
                }
                else
                    break;
            }
        }
    }
}
