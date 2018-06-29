using SpOnlineWrapper.Structures;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace SpOnlineWrapper.Utils
{
    public static class Extensions
    {
        private static JavaScriptSerializer Serializer = new JavaScriptSerializer();

        private class ListItem
        {
            public string ListItemEntityTypeFullName { get; set; }
        }

        private class FieldCollection
        {
            public class Field
            {
                public string Title { get; set; }
                public string EntityPropertyName { get; set; }
            }

            public IEnumerable<Field> Value { get; set; }
        }

        public static Hashtable ConvertToCreateFieldHashtable(this Hashtable source, Client client, string listGuid)
        {
            var listItemEntityTypeFullName = GetListItemEntityTypeFullName(client, listGuid);
            var fieldCollection = GetFieldCollection(client, listGuid);
            var adjusted = AdjustHashTable(source, fieldCollection);            

            source.Add("__metadata", new Hashtable { { "type", listItemEntityTypeFullName } });

            return source;
        }

        public static Hashtable ConvertToCreateFolderHashTable(this Hashtable source, Client client, string listGuid)
        {
            var listItemEntityTypeFullName = GetListItemEntityTypeFullName(client, listGuid);

            source.Add("__metadata", new Hashtable { { "type", listItemEntityTypeFullName } });
            source.Add("FileSystemObjectType", 1);
            source.Add("ContentTypeId", "0x0120");

            return source;
        }

        internal static string GetDescription<T>(this T source)
        {
            var fieldInfo = source.GetType().GetField(source.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return source.ToString();
        }

        internal static HttpWebRequest AddBody(this HttpWebRequest request, object body, string contentType = "application/soap+xml; charset=utf-8", bool useUTF8 = true)
        {
            var toBeEncoded = body.ToString();

            //request.Headers["Host"] = "";

            var bytes = Encoding.UTF8.GetBytes(toBeEncoded);

            if (useUTF8)
            {
                bytes = Encoding.UTF8.GetBytes(toBeEncoded);
            }

            request.Method = "POST";
            request.UserAgent = string.Empty;
            request.ContentType = contentType;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            return request;
        }

        internal static HandledHttpWebResponse GetHandledResponse(this HttpWebRequest request)
        {
            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (!request.HaveResponse || response == null)
                        throw new WrapperException("The request has no response.");

                    return new HandledHttpWebResponse
                    {
                        StatusCode = response.StatusCode,
                        Content = ReadResponseStream(response),
                        Cookies = response.Cookies,
                        Headers = response.Headers
                    };
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    throw new WrapperException("The request has no response.");

                using (var response = ex.Response as HttpWebResponse)
                {
                    return new HandledHttpWebResponse
                    {
                        StatusCode = response.StatusCode,
                        Content = ReadResponseStream(response),
                        Cookies = response.Cookies,
                        Headers = response.Headers
                    };
                }
            }
        }

        private static string ReadResponseStream(HttpWebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        private static string GetListItemEntityTypeFullName(Client client, string listGuid)
        {
            var uri = string.Format("_api/web/lists(guid'{0}')?$select=ListItemEntityTypeFullName", listGuid);
            var response = client.Get(uri);
            var listItem = Serializer.Deserialize<ListItem>(response.Content);

            return listItem.ListItemEntityTypeFullName;
        }

        private static FieldCollection GetFieldCollection(Client client, string listGuid)
        {
            var uri = string.Format("_api/web/lists(guid'{0}')/fields?$select=EntityPropertyName,title&$filter=canbedeleted eq true", listGuid);
            var response = client.Get(uri);
            var collection = Serializer.Deserialize<FieldCollection>(response.Content);

            return collection;
        }

        private static Hashtable AdjustHashTable(Hashtable source, FieldCollection fieldCollection)
        {
            foreach (var key in new List<object>(source.Keys.Cast<object>()))
            {
                if (key.ToString() == "Title" || fieldCollection.Value.Any(v => v.EntityPropertyName.Equals(key.ToString(), System.StringComparison.CurrentCultureIgnoreCase)))
                    continue;

                var field = fieldCollection.Value.FirstOrDefault(v => v.Title.Equals(key.ToString(), System.StringComparison.CurrentCultureIgnoreCase));

                if (field == null)
                    throw new WrapperException("");

                source.Add(field.EntityPropertyName, source[key]);
                source.Remove(key);
            }

            return source;
        }
    }
}
