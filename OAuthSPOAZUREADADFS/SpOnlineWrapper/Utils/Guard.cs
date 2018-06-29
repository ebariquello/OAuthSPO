using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpOnlineWrapper.Utils
{
    internal static class Guard
    {
        private static string WrapperExceptionMessage = "Wrapper exception occured. Please see inner exception for details.";

        public static void ThrowIf<T>(bool assertion, string msg) where T : Exception
        {
            if (!assertion)
                return;

            var innerException = (T)Activator.CreateInstance(typeof(T), new object[] { msg });
            var exception = (WrapperException)Activator.CreateInstance(typeof(WrapperException), new object[] { WrapperExceptionMessage, innerException });

            throw exception;
        }
    }
}
