using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Sora.GameEngine.Cirrus.Design.Info
{
    public class XmlCrInfoEntry
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }
    }
}
