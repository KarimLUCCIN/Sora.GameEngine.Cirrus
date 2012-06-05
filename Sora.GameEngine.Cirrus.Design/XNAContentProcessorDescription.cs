using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design
{
    [Serializable]
    public class XNAContentProcessorDescription
    {
        public XNAContentProcessorDescription()
        {
            Properties = new Dictionary<string, Type>();
        }

        public string DisplayName { get; set; }

        public object TypeId { get; set; }

        public string Name { get; set; }

        public Dictionary<string, Type> Properties { get; private set; }

        public override string ToString()
        {
            return "Processor: " + Name;
        }
    }
}
