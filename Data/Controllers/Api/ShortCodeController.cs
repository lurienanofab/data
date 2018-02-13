using Data.Models.Api;
using System.Threading.Tasks;
using System.Web.Http;

namespace Data.Controllers.Api
{
    public class ShortCodeController : ApiController
    {
        [Route("api/shortcode/{shortcode}")]
        public async Task<ShortCodeItem> Get(string shortcode)
        {
            var client = await UmichApiClient.GetCurrent();
            var result = await client.GetShortCode(shortcode);
            if (null == result) return null;
            if (null == result.ShortCodes) return null;
            return result.ShortCodes.ShortCode;
        }
    }
}
