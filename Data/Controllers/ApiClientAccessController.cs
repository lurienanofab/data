using Data.Controllers.Api;
using Data.Models.Api;
using LNF;
using LNF.Data;
using LNF.PhysicalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ApiClientAccessController : DataApiController
    {
        public ApiClientAccessController(IProvider provider) : base(provider) { }

        [HttpGet, Route("api/client/access/check")]
        public AccessCheck Check(int id)
        {
            AccessCheck result = null;

            IClient c = Provider.Data.Client.GetClient(id);
            var gc = Provider.Data.Cost.GetGlobalCosts().First();
            var hasActiveAccts = Provider.Data.Client.GetActiveAccountCount(id) > 0;

            if (c != null)
                result = AccessCheck.Create(Provider.PhysicalAccess, c, gc, hasActiveAccts);

            return result;
        }

        [Route("api/client/access/inlab")]
        public InLabArea[] GetInLab()
        {
            IEnumerable<Badge> query = Provider.PhysicalAccess.GetCurrentlyInArea("all");

            IList<InLabArea> areas = query.Select(x => x.CurrentAreaName).Distinct().Select(x => new InLabArea() { AreaName = x }).OrderBy(x => x.AreaName).ToList();
            foreach (InLabArea a in areas)
                a.Clients = query.Where(x => x.CurrentAreaName == a.AreaName).Select(GetInLabClient).OrderBy(x => x.FullName).ToList();

            return areas.ToArray();
        }

        [Route("api/client/access/inlab/socket")]
        public HttpResponseMessage GetInLabSocket()
        {
            if (HttpContext.Current.IsWebSocketRequest)
                HttpContext.Current.AcceptWebSocketRequest(ProcessInLabSocket);
            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }

        private async Task ProcessInLabSocket(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;

            while (true)
            {
                if (socket.State == WebSocketState.Open)
                {
                    InLabArea[] areas = GetInLab();
                    int count = areas.SelectMany(x => x.Clients).Count();

                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
                    var response = new { Timestamp = DateTime.Now, Count = count, Areas = areas };

                    buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(Provider.Utility.Serialization.Json.SerializeObject(response)));
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    await Task.Delay(10000);
                }
                else
                    break;
            }
        }

        private Models.Api.InLabClient GetInLabClient(Badge b)
        {
            var result = new InLabClient()
            {
                LastName = b.LastName,
                FirstName = b.FirstName,
                AccessEventTime = b.CurrentAccessTime.Value
            };

            return result;
        }
    }
}
