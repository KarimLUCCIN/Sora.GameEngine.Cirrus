using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design
{
    [Serializable]
    public class XNAContentProcessorDescription
    {
        public string DisplayName { get; set; }

        public object TypeId { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return "Processor: " + Name;
        }
    }
}
