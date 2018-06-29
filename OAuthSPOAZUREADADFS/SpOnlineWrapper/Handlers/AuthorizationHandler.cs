using System.Net;

namespace SpOnlineWrapper.Handlers
{
    internal class AuthorizationHandler
    {
        private readonly string _baseUrl;
        private readonly XmlHandler _soapHandler;
        private readonly AuthRequestsHandler _authRequestHandler;

        public AuthorizationHandler(string baseUrl, string siteUrl)
        {
            _baseUrl = baseUrl;
            _soapHandler = new XmlHandler();
            _authRequestHandler = new AuthRequestsHandler(baseUrl, siteUrl);
        }

        public CookieCollection GetAuthorizationCookies(string userName, string password)
        {
            var template = _soapHandler.GetSamlBodyTemplate(userName, password, _baseUrl);
            template = XMLMinifierFactory.GetInstace.Minify(template);
            var samlResponse = _authRequestHandler.GetSamlResponse(template);

            string templateAuthenticateToOffice365WithFederatedSamlResponse = _soapHandler.GetSamlBodyTemplateAuthenticateWithFederatedSamlResponse(samlResponse);
            templateAuthenticateToOffice365WithFederatedSamlResponse = XMLMinifierFactory.GetInstace.Minify(templateAuthenticateToOffice365WithFederatedSamlResponse);

            samlResponse = _authRequestHandler.GetSamlResponseOffice365WithFedereatedSaml(templateAuthenticateToOffice365WithFederatedSamlResponse);
            var securityToken = _soapHandler.GetSecurityToken(samlResponse);

           

            var cookies = _authRequestHandler.GetAuthorizationCookies(securityToken);

            return cookies;
        }

        public string GetFormDigestValue(CookieCollection cookies)
        {
            var digest = _authRequestHandler.GetFormDigestResponse(cookies);
            var digestValue = _soapHandler.GetDigestValue(digest);

            return digestValue;
        }
    }
}
