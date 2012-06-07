using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.Design.Application.Build
{
    public class BuildMessage
    {
        private int order;

        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        private BuildMessageSeverity severity;

        public BuildMessageSeverity Severity
        {
            get { return severity; }
            set { severity = value; }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private string source;

        public string Source
        {
            get { return source; }
            set { source = value; }
        }
    }
}
