using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LNF.GoogleApi;
using Data.Models;

namespace Data.Controllers
{
    public class GoogleController : Controller
    {
        private const string CLIENT_ID = "495387045568.project.googleusercontent.com";
        private const string CLIENT_SECRET = "U2Zrt9kWozkChFP25URWWgUU";
        private const string SCOPES = "https://spreadsheets.google.com/feeds https://www.googleapis.com/auth/drive profile email";
        private const string AUTH_COOKIE_NAME = "google.auth";
        private const string REDIRECT_URI = "http://lnf-jgett.eecs.umich.edu/data/google/auth-callback";
        private const string ACCESS_TYPE = "offline";

        [Route("google/{FeedAlias?}")]
        public async Task<ActionResult> Index(GoogleModel model)
        {
            HttpCookie cookie = Request.Cookies[AUTH_COOKIE_NAME];
            GoogleAuthorization auth;

            if (cookie != null)
            {
                auth = JsonConvert.DeserializeObject<GoogleAuthorization>(cookie.Value);

                if (auth.IsExpired())
                    auth = await Refresh(auth);

                var gc = new GoogleClient(auth);

                var userInfo = await gc.GetUserInfo();

                model.UserInfo = await gc.GetUserInfo();
                model.Files = await gc.GetFiles();

                return View(model);
            }
            else
            {
                var req = new GoogleAccessRequest()
                {
                    AccessType = AccessType.Offline,
                    ClientId = CLIENT_ID,
                    ForceApproval = false,
                    RedirectUri = REDIRECT_URI,
                    Scopes = SCOPES.Split(' ').ToArray(),
                    State = model.FeedAlias
                };

                return Redirect(req.GetRequestUri());
            }
        }

        [Route("google/auth-callback")]
        public async Task<ActionResult> AuthCallback(string code = null, string state = null)
        {
            if (!string.IsNullOrEmpty(code))
            {
                await Authorize(code);

                if (string.IsNullOrEmpty(state))
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Index", new { Alias = state });
            }
            else
            {
                throw new HttpException(400, "Bad Request");
            }
        }

        private async Task<GoogleAuthorization> Refresh(GoogleAuthorization auth)
        {
            var authService = new GoogleAuthorizationService(new GoogleAuthorizationOptions() { ClientID = CLIENT_ID, ClientSecret = CLIENT_SECRET });

            var result = await authService.Refresh(auth.RefreshToken);

            //the refreshed auth will not have a refresh_token (it's not re-issued everytime) do set it
            result.RefreshToken = auth.RefreshToken;

            var cookie = new HttpCookie(AUTH_COOKIE_NAME, JsonConvert.SerializeObject(result)) { Expires = DateTime.Now.AddYears(1) };

            Response.Cookies.Add(cookie);

            return result;
        }

        private async Task Authorize(string code)
        {
            var authService = new GoogleAuthorizationService(new GoogleAuthorizationOptions() { ClientID = CLIENT_ID, ClientSecret = CLIENT_SECRET, RedirectUri = REDIRECT_URI });
            var auth = await authService.Authorize(code);
            var cookie = new HttpCookie(AUTH_COOKIE_NAME, JsonConvert.SerializeObject(auth)) { Expires = DateTime.Now.AddYears(1) };
            Response.Cookies.Add(cookie);
        }
    }
}