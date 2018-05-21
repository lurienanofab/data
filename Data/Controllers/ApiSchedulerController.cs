using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using LNF;
using LNF.Repository;
using LNF.Repository.Scheduler;

namespace Data.Controllers
{
    public class ApiSchedulerController : ApiController
    {
        public object[] GetActiveReservations()
        {
            IList<Reservation> query = DA.Current.Query<Reservation>().Where(x => x.IsStarted && x.IsActive && x.ActualEndDateTime == null && x.ActualBeginDateTime <= DateTime.Now).ToList();
            var items = query.Select(GetReservation).ToArray();
            return items;
        }

        public HttpResponseMessage GetActiveReservationsSocket()
        {
            if (HttpContext.Current.IsWebSocketRequest)
            {
                HttpContext.Current.AcceptWebSocketRequest(ProcessActiveReservationsSocket);
                return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
            }
            else
                throw new Exception("Invalid request.");
        }

        private async Task ProcessActiveReservationsSocket(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;

            while (true)
            {
                if (socket.State == WebSocketState.Open)
                {
                    ArraySegment<byte> buffer;
                    try
                    {
                        object[] items = GetActiveReservations();

                        var response = new { Timestamp = DateTime.Now, Count = items.Length, Items = items };
                        buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(ServiceProvider.Current.Serialization.Json.SerializeObject(response)));
                    }
                    catch (Exception ex)
                    {
                        buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(ServiceProvider.Current.Serialization.Json.SerializeObject(new { Timestamp = DateTime.Now, Error = ex.Message })));
                    }

                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    await Task.Delay(10000);
                }
                else
                    break;
            }
        }

        private object GetReservation(Reservation r)
        {
            var result = new
            {
                Resource = r.Resource.ResourceName,
                ProcTech = r.Resource.ProcessTech.ProcessTechName,
                ActualBeginDateTime = r.ActualBeginDateTime.Value,
                r.BeginDateTime,
                r.EndDateTime,
                ScheduledDurationHours = (r.EndDateTime - r.BeginDateTime).TotalSeconds / 60.0 / 60.0,
                DurationHours = (DateTime.Now - r.ActualBeginDateTime.Value).TotalSeconds / 60.0 / 60.0,
                RemainingMinutes = Math.Max(0, (r.EndDateTime - DateTime.Now).TotalSeconds / 60.0),
                OvertimeMinutes = Math.Max(0, (DateTime.Now - r.EndDateTime).TotalSeconds / 60.0),
                Client = new
                {
                    r.Client.ClientID,
                    r.Client.DisplayName
                }
            };
            return result;
        }
    }
}
