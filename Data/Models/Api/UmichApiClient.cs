using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Data.Models.Api
{
    public class UmichApiClient : IDisposable
    {
        private static ObjectCache _cache;

        static UmichApiClient()
        {
            _cache = new MemoryCache("UmichApiClientCache");
        }

        public static async Task<UmichApiClient> GetCurrent()
        {
            UmichApiClient current;

            if (_cache["current"] == null)
            {
                string host = ConfigurationManager.AppSettings["UmichApiHost"];
                string clientId = ConfigurationManager.AppSettings["UmichApiClientId"];
                string clientSecret = ConfigurationManager.AppSettings["UmichApiClientSecret"];
                string scope = ConfigurationManager.AppSettings["UmichApiScope"];

                current = new UmichApiClient(host, clientId, clientSecret, scope);
                await current.Authorize();

                _cache["current"] = current;
            }
            else
            {
                current = (UmichApiClient)_cache["current"];
            }

            if (current.AuthorizationIsExpired())
            {
                await current.Authorize();
            }

            return current;
        }

        private string _clientId;
        private string _clientSecret;
        private string _scope;
        private DateTime _expiresAt;

        private HttpClient _hc;

        public TimeSpan AuthorizationTTL()
        {
            return _expiresAt - DateTime.Now;
        }

        public bool AuthorizationIsExpired()
        {
            return DateTime.Now >= _expiresAt;
        }

        public UmichApiClient(string host, string clientId, string clientSecret, string scope)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _scope = scope;

            _hc = new HttpClient();

            _hc.BaseAddress = new Uri(host);
            _hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _hc.DefaultRequestHeaders.Add("x-ibm-client-id", _clientId);
        }

        public async Task Authorize()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("client_id", _clientId);
            dict.Add("client_secret", _clientSecret);

            var content = new FormUrlEncodedContent(dict);
            var msg = await _hc.PostAsync(string.Format("um/bf/oauth2/token?grant_type=client_credentials&scope={0}", _scope), content);
            msg.EnsureSuccessStatusCode();
            var json = await msg.Content.ReadAsStringAsync();
            var access = JsonConvert.DeserializeObject<ApiAccess>(json);
            _expiresAt = DateTime.Now.AddSeconds(access.ExpiresIn);
            _hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access.AccessToken);
        }

        public async Task<ShortCodeResult> GetShortCode(string shortcode)
        {
            var msg = await _hc.GetAsync(string.Format("um/bf/ShortCodes/ShortCodes/{0}", shortcode));
            msg.EnsureSuccessStatusCode();
            var json = await msg.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ShortCodeResult>(json);
            return result;
        }

        public void Dispose()
        {
            if (_hc != null)
                _hc.Dispose();
        }
    }

    public class ApiAccess
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }
    }

    public class ShortCodeResult
    {
        public ShortCodeEntry ShortCodes { get; set; }
    }

    public class ShortCodeEntry
    {
        public ShortCodeItem ShortCode { get; set; }
    }

    public class ShortCodeItem
    {
        [JsonProperty("shortCode")]
        public string ShortCode { get; set; }

        [JsonProperty("shortCodeDescription")]
        public string ShortCodeDescription { get; set; }

        [JsonProperty("shortCodeStatus")]
        public string ShortCodeStatus { get; set; }

        [JsonProperty("ShortCodeStatusDescription")] // yes, it's capitalized
        public string ShortCodeStatusDescription { get; set; }

        [JsonProperty("fundCode")]
        public string FundCode { get; set; }

        [JsonProperty("deptID")]
        public string DeptID { get; set; }

        [JsonProperty("programCode")]
        public string ProgramCode { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("projectGrant")]
        public string ProjectGrant { get; set; }

        [JsonProperty("class2")]
        public string Class2 { get; set; }
    }
}