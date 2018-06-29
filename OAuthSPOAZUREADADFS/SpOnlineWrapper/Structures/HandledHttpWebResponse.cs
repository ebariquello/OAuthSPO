using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpOnlineWrapper.Structures
{
    public class HandledHttpWebResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
        public CookieCollection Cookies { get; set; }
        public WebHeaderCollection Headers { get; set; }
    }
}
