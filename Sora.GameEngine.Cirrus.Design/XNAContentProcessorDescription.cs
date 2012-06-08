using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design
{
    [Serializable]
    public class XNAContentProcessorPropertyDescriptor
    {
        public Type Type { get; set; }
     
        public object DefaultValue { get; set; }
    }

    [Serializable]
    public class XNAContentProcessorDescription
    {
        public XNAContentProcessorDescription()
        {
            Properties = new Dictionary<string, XNAContentProcessorPropertyDescriptor>();
        }

        public string DisplayName { get; set; }

        public object TypeId { get; set; }

        public string Name { get; set; }

        public Dictionary<string, XNAContentProcessorPropertyDescriptor> Properties { get; private set; }

        public override string ToString()
        {
            return "Processor: " + Name;
        }

        public string TypeName { get; set; }
    }
}
