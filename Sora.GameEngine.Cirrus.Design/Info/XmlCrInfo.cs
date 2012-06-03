using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Sora.GameEngine.Cirrus.Design.Info
{
    [XmlRoot("cirrus.info")]
    public class XmlCrInfo
    {
        public XmlCrInfo()
        {
            Entries = new List<XmlCrInfoEntry>();
            Compile = true;
        }

        [XmlAttribute("compile")]
        public bool Compile { get; set; }

        [XmlAttribute("importer")]
        public string Importer { get; set; }

        [XmlAttribute("processor")]
        public string Processor { get; set; }

        [XmlArray("entries")]
        [XmlAttribute("property", typeof(XmlCrInfoEntry))]
        public List<XmlCrInfoEntry> Entries { get; set; }
    }
}
