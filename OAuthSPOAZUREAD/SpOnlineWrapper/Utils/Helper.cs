using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpOnlineWrapper.Utils
{
    public class Helper
    {
        public static string GetFieldName(string fieldTitle, Client client, string guid)
        {
            var hashTable = new Hashtable()
            {
                { fieldTitle, "" }
            }.ConvertToCreateFieldHashtable(client, guid);
            var keys = new List<object>(hashTable.Keys.Cast<object>());

            return keys.FirstOrDefault(k => k.ToString() != "__metadata").ToString();
        }
    }
}
