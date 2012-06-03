using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Sora.GameEngine.Cirrus.Descriptor
{
    /// <summary>
    /// Xml file generated during compilation that describes what is actually embedded inside the package
    /// </summary>
    [XmlRoot("cirrus.built.package")]
    public class XmlCirrusBuiltPackageDescriptor
    {
        public XmlCirrusBuiltPackageDescriptor()
        {
            Items = new List<XmlCirrusBuiltPackageItem>();
        }

        [XmlAttribute("source")]
        public string Source{get;set;}

        [XmlAttribute("description")]
        public string Description{get;set;}

        /// <summary>
        /// Flattened list of embeded items
        /// </summary>
        [XmlArray("items")]
        [XmlArrayItem("item", typeof(XmlCirrusBuiltPackageItem))]
        public List<XmlCirrusBuiltPackageItem> Items { get; set; }
    }
}
