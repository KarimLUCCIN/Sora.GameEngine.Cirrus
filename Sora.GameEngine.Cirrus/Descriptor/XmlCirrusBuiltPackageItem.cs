using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Sora.GameEngine.Cirrus.Descriptor
{
    public class XmlCirrusBuiltPackageItem
    {
        [XmlAttribute("path")]
        public string RelativeItemPath { get; set; }

        [XmlElementAttribute(ElementName="modification", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime SourceModificationDate { get; set; }

        [XmlAttribute("hash")]
        public string HashString { get; set; }

        [XmlAttribute("type")]
        public string TypeString { get; set; }
    }
}
