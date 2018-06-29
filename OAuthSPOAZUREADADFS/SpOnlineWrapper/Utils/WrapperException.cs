using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpOnlineWrapper.Utils
{
    [Serializable]
    internal class WrapperException : Exception
    {
        public WrapperException() { }
        public WrapperException(string message) : base(message) { }
        public WrapperException(string message, Exception inner) : base(message, inner) { }
        protected WrapperException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
