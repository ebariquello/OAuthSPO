using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpOnlineWrapper.Enums
{
    public enum XHttpMethod
    {
        [Description("")]
        CREATE = 0,

        [Description("MERGE")]
        UPDATE = 1,

        [Description("DELETE")]
        DELETE = 2
    }
}
