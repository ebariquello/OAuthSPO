using SpOnlineWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteSPOnlineToken
{
    class Program
    {
        static void Main(string[] args)
        {
            Client c = new Client("eduardo.bariquello@pibariquelloteste.onmicrosoft.com", "pass@word1", "https://pibariquelloteste.sharepoint.com/sites/dev/");


            var response = c.Get("/_api/Web/Lists/GetByTitle('ListaTeste')/items");
        }
    }
}
