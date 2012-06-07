using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design.Application.Build
{
    [Serializable]
    public class FatalBuildErrorException : Exception
    {
        public FatalBuildErrorException() { }
        public FatalBuildErrorException(string message) : base(message) { }
        public FatalBuildErrorException(string message, Exception inner) : base(message, inner) { }
        protected FatalBuildErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
