﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Sora.GameEngine.Cirrus.Design
{
    public class XmlCirrusContentInfo
    {
        public XmlCirrusContentInfo()
        {
            Entries = new List<XmlCirrusContentInfoEntry>();
            Compile = true;
        }

        /// <summary>
        /// Relative path to the file associated with this info inside the package
        /// </summary>
        [XmlAttribute("path")]
        public string RelativePath { get; set; }

        [XmlAttribute("compile")]
        public bool Compile { get; set; }

        [XmlAttribute("importer")]
        public string Importer { get; set; }

        [XmlAttribute("processor")]
        public string Processor { get; set; }

        [XmlArray("entries")]
        [XmlArrayItem("property", typeof(XmlCirrusContentInfoEntry))]
        public List<XmlCirrusContentInfoEntry> Entries { get; set; }
    }
}