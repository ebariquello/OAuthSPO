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
                        
            var templateText = string.Format(Resources.SamlTemplate, userName, password, baseUrl);

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
