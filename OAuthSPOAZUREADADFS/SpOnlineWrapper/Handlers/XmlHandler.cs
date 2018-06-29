using SpOnlineWrapper.Properties;
using SpOnlineWrapper.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SpOnlineWrapper.Handlers
{
    internal class XmlHandler
    {
        private const string DigestXmlNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        public string GetSamlBodyTemplate(string userName, string password, string baseUrl)
        {
            var isAnyNullOrEmplty = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(baseUrl);

            Guard.ThrowIf<Exception>(isAnyNullOrEmplty, "Username, password and BaseUrl cannot be empty.");

            string authUrl = "https://adfs.uolinc.com/adfs/services/trust/2005/usernamemixed";
            string messageID = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "");
            
            string created = DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
            string expires = DateTime.UtcNow.AddMinutes(10).ToString("o", System.Globalization.CultureInfo.InvariantCulture);


            string template = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:saml=\"urn:oasis:names:tc:SAML:1.0:assertion\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wssc=\"http://schemas.xmlsoap.org/ws/2005/02/sc\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\"><s:Header><wsa:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action><wsa:To s:mustUnderstand=\"1\">{0}</wsa:To><wsa:MessageID>{1}</wsa:MessageID><ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/Passport/SoapServices/PPCRL\" Id=\"PPAuthInfo\"><ps:HostingApp>Managed IDCRL</ps:HostingApp><ps:BinaryVersion>6</ps:BinaryVersion><ps:UIVersion>1</ps:UIVersion><ps:Cookies></ps:Cookies><ps:RequestParams>AQAAAAIAAABsYwQAAAAxMDMz</ps:RequestParams></ps:AuthInfo><wsse:Security><wsse:UsernameToken wsu:Id=\"user\"><wsse:Username>{2}</wsse:Username><wsse:Password>{3}</wsse:Password></wsse:UsernameToken><wsu:Timestamp Id=\"Timestamp\"><wsu:Created>{4}</wsu:Created><wsu:Expires>{5}</wsu:Expires></wsu:Timestamp></wsse:Security></s:Header><s:Body><wst:RequestSecurityToken Id=\"RST0\"><wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType><wsp:AppliesTo><wsa:EndpointReference>  <wsa:Address>urn:federation:MicrosoftOnline</wsa:Address></wsa:EndpointReference></wsp:AppliesTo><wst:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</wst:KeyType></wst:RequestSecurityToken></s:Body></s:Envelope>";

            var templateText = string.Format(template, authUrl, messageID, userName, password, created, expires);

            return templateText;
        }

        internal string GetSamlBodyTemplateAuthenticateWithFederatedSamlResponse(string samlResponse)
        {
            Guard.ThrowIf<ArgumentNullException>(samlResponse == null, "SamlReponse");

            var xTemplate = XDocument.Parse(samlResponse);
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("S", "http://www.w3.org/2003/05/soap-envelope");
            namespaceManager.AddNamespace("w", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            namespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");


            namespaceManager.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
            namespaceManager.AddNamespace("u", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            namespaceManager.AddNamespace("t", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            namespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:1.0:assertion");
            

            var assertionFederatedTokenResponse = xTemplate.XPathSelectElement("/S:Envelope/S:Body/t:RequestSecurityTokenResponse/t:RequestedSecurityToken/saml:Assertion", namespaceManager);

            //Guard.ThrowIf<Exception>(samlADFSFederatedTokenResponse), "SAML response parsed unsuccessfully. Security token not found.");

            string assertionFederatedTokenResponseValue = assertionFederatedTokenResponse.ToString();

            //assertionFederatedTokenResponseValue = assertionFederatedTokenResponse.ToString().Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);

            string template = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><S:Envelope xmlns:S=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\"><S:Header><wsa:Action S:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action><wsa:To S:mustUnderstand=\"1\">https://login.microsoftonline.com/rst2.srf</wsa:To><ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/LiveID/SoapServices/v1\" Id=\"PPAuthInfo\"><ps:BinaryVersion>5</ps:BinaryVersion><ps:HostingApp>Managed IDCRL</ps:HostingApp></ps:AuthInfo><wsse:Security>{0}</wsse:Security></S:Header><S:Body><wst:RequestSecurityToken xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\" Id=\"RST0\"><wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType><wsp:AppliesTo><wsa:EndpointReference><wsa:Address>sharepoint.com</wsa:Address></wsa:EndpointReference></wsp:AppliesTo><wsp:PolicyReference URI=\"MBI\"></wsp:PolicyReference></wst:RequestSecurityToken></S:Body></S:Envelope>";
            var templateText = string.Format(template, assertionFederatedTokenResponseValue);

            return templateText;


        }

        public string GetSecurityToken(string samlResponse)
        {
            Guard.ThrowIf<ArgumentNullException>(samlResponse == null, "SamlReponse");

            var xTemplate = XDocument.Parse(samlResponse);
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("S", "http://www.w3.org/2003/05/soap-envelope");
            namespaceManager.AddNamespace("wst", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            namespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            var binarySecurityToken = xTemplate.XPathSelectElement("/S:Envelope/S:Body/wst:RequestSecurityTokenResponse/wst:RequestedSecurityToken/wsse:BinarySecurityToken", namespaceManager);

            Guard.ThrowIf<Exception>(string.IsNullOrEmpty(binarySecurityToken.Value), "SAML response parsed unsuccessfully. Security token not found.");

            return binarySecurityToken.Value;
        }

        public string GetDigestValue(string digestResponse)
        {
            Guard.ThrowIf<ArgumentNullException>(digestResponse == null, "DigestReponse");

            var resultXml = XDocument.Parse(digestResponse);
            var descendants = resultXml.Descendants();
            var digest = descendants.FirstOrDefault(d => d.Name == XName.Get("FormDigestValue", DigestXmlNamespace));

            Guard.ThrowIf<Exception>(digest == null, "Digest is null");

            var value = digest.Value;
            
            Guard.ThrowIf<Exception>(value == null, "Digest value is null");

            return value;
        }
    }
}
