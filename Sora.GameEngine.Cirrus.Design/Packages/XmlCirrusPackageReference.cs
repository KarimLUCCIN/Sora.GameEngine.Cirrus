using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Sora.GameEngine.Cirrus.Design.Packages
{
    public class XmlCirrusPackageReference
    {
        /// <summary>
        /// Can be an absolute or relative path from the current package directory.
        /// Child packages inherits references from parent ones.
        /// </summary>
        [XmlAttribute("path")]
        public string Reference { get; set; }

        public override string ToString()
        {
            return "Package: " + Reference;
        }
    }
}
