using SpOnlineWrapper.Utils;
using System;
using System.Net;

namespace SpOnlineWrapper.Handlers
{
    internal class AuthRequestsHandler
    {
        private const string MicrosoftLoginBaseUrl = "https://login.microsoftonline.com/extSTS.srf";
        private const string SignInUrl = "/_forms/default.aspx?wa=wsignin1.0";
        private const string FormDigestUrl = "/_api/contextinfo";
        
        private readonly string _signInUrl;
        private readonly string _formDigestUrl;

        public AuthRequestsHandler(string baseUrl, string siteUrl)
        {
            _signInUrl = string.Concat(baseUrl, SignInUrl);
            _formDigestUrl = string.Concat(baseUrl, siteUrl, FormDigestUrl);
        }

        public string GetSamlResponse(object template)
        {
            Guard.ThrowIf<ArgumentNullException>(template == null, "template");

            var request = ((HttpWebRequest)WebRequest.Create(MicrosoftLoginBaseUrl)).AddBody(template);
            var response = request.GetHandledResponse();
            
            Guard.ThrowIf<Exception>(response.StatusCode != HttpStatusCode.OK, string.Format("Invalid request: Response status code: {0}", response.StatusCode.ToString()));

            var content = response.Content;

            Guard.ThrowIf<Exception>(content.Contains("Invalid Request"), "Invalid request: SOAPFault");

            return content;
        }

        public CookieCollection GetAuthorizationCookies(string token)
        {
            Guard.ThrowIf<ArgumentNullException>(string.IsNullOrEmpty(token), "token");

            var request = ((HttpWebRequest)WebRequest.Create(_signInUrl)).AddBody(token, "application/x-www-form-urlencoded");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:50.0) Gecko/20100101 Firefox/50.0";
            request.CookieContainer = new CookieContainer();
            request.Accept = "*/*";

            var response = request.GetHandledResponse();

            Guard.ThrowIf<Exception>(response.Cookies.Count < 2, "Authorization cookies cannot be read.");

            return response.Cookies;
        }

        public string GetFormDigestResponse(CookieCollection cookies)
        {
            Guard.ThrowIf<ArgumentNullException>(cookies.Count < 2, "any cookie");

            var container = new CookieContainer();
            container.Add(cookies);

            var request = (HttpWebRequest)WebRequest.Create(_formDigestUrl);
            request.Method = "POST";
            request.CookieContainer = container;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = 0;

            var response = request.GetHandledResponse();

            Guard.ThrowIf<Exception>(response.StatusCode != HttpStatusCode.OK, "Form digest cannot be read.");

            return response.Content;
        }
    }
}
