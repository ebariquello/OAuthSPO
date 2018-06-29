using SpOnlineWrapper.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SpOnlineWrapper.Handlers
{
    internal class AuthRequestsHandler
    {
        //private const string MicrosoftLoginBaseUrl = "https://login.microsoftonline.com/extSTS.srf";
        private const string MicrosoftLoginBaseUrl = "https://adfs.uolinc.com/adfs/services/trust/2005/usernamemixed"; // alterado para url da uol
        private const string MicrosoftFederatedLoginUrl = "https://login.microsoftonline.com/rst2.srf"; // alterado para url da uol


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
            request.ContentType = "application/soap+xml; charset=utf-8";
            var response = request.GetHandledResponse();
            
            Guard.ThrowIf<Exception>(response.StatusCode != HttpStatusCode.OK, string.Format("Invalid request: Response status code: {0}", response.StatusCode.ToString()));

            var content = response.Content;

            Guard.ThrowIf<Exception>(content.Contains("Invalid Request"), "Invalid request: SOAPFault");




            //string response2 = GetContentHttp(template.ToString());


            return content;
        }

        private string GetContentHttp(string template)
        {
            HttpClient client = new HttpClient();

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, MicrosoftLoginBaseUrl);
            //msg.Headers.Add(HttpRequestHeader.ContentType, "application/soap+xml; charset=utf-8");
            
            HttpContent content2 = new System.Net.Http.StringContent(template.ToString(), Encoding.UTF8);
            content2.Headers.ContentType = MediaTypeHeaderValue.Parse("application/soap+xml;charset=UTF-8");
            //content2.Headers.ContentType.CharSet = "utf-8";
            msg.Content = content2;
            
            
            var response3 = client.SendAsync(msg);
            return response3.Result.ToString();
            
            //var response2 = client.PostAsync(MicrosoftLoginBaseUrl, content2);

            return response3.Result.ToString();
               
        }

        internal string GetSamlResponseOffice365WithFedereatedSaml(string templateAuthenticateToOffice365WithFederatedSamlResponse)
        {
            
            Guard.ThrowIf<ArgumentNullException>(templateAuthenticateToOffice365WithFederatedSamlResponse == null, "template");

            
            var request = ((HttpWebRequest)WebRequest.Create("https://login.microsoftonline.com/rst2.srf")).AddBody(templateAuthenticateToOffice365WithFederatedSamlResponse, useUTF8: true);

            request.Method = "POST";
            request.ContentType = "application/soap+xml; charset=utf-8";
            request.UserAgent = string.Empty;
            /*
            using (var stream = request.GetRequestStream())
            {
                
                using (BinaryWriter a = new BinaryWriter(stream, ))
                {
                    
                    a.Write(templateAuthenticateToOffice365WithFederatedSamlResponse);
                }
            }
            
            */

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
