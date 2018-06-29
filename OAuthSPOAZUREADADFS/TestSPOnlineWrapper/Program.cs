using SpOnlineWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSPOnlineWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Client c = new Client("sp_mribeiro@uolinc.com", "Senha_20", "https://uolinc-d637c39cd9faa4.sharepoint.com/sites/PagSeguroMaquinas/PagSeguroMaquinasApp2");
            
            
            var response = c.Get("_api/Web/Lists/GetByTitle('Solicitacoes')/items");


            string body = "{ '__metadata': { 'type': 'SP.Data.SolicitacoesListItem' }, 'Title': 'Fucionou! autenticado'} ";
            c.Post("_api/Web/Lists/GetByTitle('Solicitacoes')/items", SpOnlineWrapper.Enums.XHttpMethod.CREATE, body);
        }
    }
}
