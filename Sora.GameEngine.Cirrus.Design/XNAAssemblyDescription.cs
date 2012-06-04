using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design
{
    [Serializable]
    public class XNAAssemblyDescription
    {
        public XNAAssemblyDescription()
        {
            Importers = new List<XNAContentImporterDescription>();
            Processors = new List<XNAContentProcessorDescription>();
        }

        public string ReferenceName { get; set; }

        public bool Valid { get; set; }

        public List<XNAContentImporterDescription> Importers { get; private set; }

        public List<XNAContentProcessorDescription> Processors { get; private set; }
    }
}
