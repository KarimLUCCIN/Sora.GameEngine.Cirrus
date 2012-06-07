using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design
{
    [Serializable]
    public class XNAContentImporterDescription
    {
        public string Name { get; set; }

        public IEnumerable<string> FileExtensions { get; set; }

        public string DefaultProcessor { get; set; }

        public string DisplayName { get; set; }

        public object TypeId { get; set; }

        public override string ToString()
        {
            return "Importer: " + Name;
        }

        public Type Type { get; set; }
    }
}
