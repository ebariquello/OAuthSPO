using SpOnlineWrapper.Enums;
using SpOnlineWrapper.Handlers;
using SpOnlineWrapper.Structures;
using SpOnlineWrapper.Utils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SpOnlineWrapper
{
    public class Client : IDisposable
    {
        #region Fields

        private readonly string _baseUrl;
        private readonly string _siteUrl;
        private readonly string _userName;
        private readonly string _password;
        private readonly Task _initTask;
        private readonly AuthorizationHandler _handler;

        private CookieContainer _authorizationCookies;
        private string _formDigestValue;

        #endregion

        #region Constructor

        public Client(string userName, string password, string baseUrl)
        {
            var uri = new Uri(baseUrl);
            _baseUrl = string.Concat(uri.Scheme, "://", uri.Host);
            _siteUrl = uri.AbsolutePath;

            _userName = userName;
            _password = password;
            _handler = new AuthorizationHandler(_baseUrl, _siteUrl);
            _initTask = Task.Factory.StartNew(Authorize);
            Authorize();
        }

        #endregion

        #region Public

        public HandledHttpWebResponse Get(string url)
        {
            _initTask.Wait();

            var uri = string.Concat(_baseUrl, _siteUrl, "/", url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.CookieContainer = _authorizationCookies;
            request.Accept = "application/json;odata.metadata=none";
            request.Headers["OData-Version"] = "4.0";

            return request.GetHandledResponse();
        }

        public HandledHttpWebResponse Post(string url, XHttpMethod method, object body = null)
        {
            _initTask.Wait();

            var uri = string.Concat(_baseUrl, _siteUrl, "/", url);
            var request = (HttpWebRequest)WebRequest.Create(uri);

            request.Method = "POST";
            request.Accept = "application/json;odata.metadata=none";
            request.Headers["X-RequestDigest"] = _formDigestValue;
            request.CookieContainer = _authorizationCookies;

            if (method != XHttpMethod.CREATE)
            {
                request.Headers["X-HTTP-Method"] = method.GetDescription();
                request.Headers["If-Match"] = "*";
            }

            if (body != null)
            {
                request.AddBody(body, "application/json;odata=verbose");
            }
            else
            {
                request.ContentLength = 0;
            }

            return request.GetHandledResponse();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _initTask?.Dispose();
            }
        }

        #endregion

        #region Authorize Initialization

        private void Authorize()
        {
            try
            {
                var cookies = _handler.GetAuthorizationCookies(_userName, _password);

                _authorizationCookies = new CookieContainer();
                _authorizationCookies.Add(cookies);
                _formDigestValue = _handler.GetFormDigestValue(cookies);
            }
            catch (WrapperException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected exception occured. See inner exception for details.", ex);
            }
        }

        #endregion
    }
}
