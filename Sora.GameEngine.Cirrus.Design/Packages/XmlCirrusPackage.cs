using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Sora.GameEngine.Cirrus.Design.Packages
{
    [XmlRoot("cirrus.package")]
    public class XmlCirrusPackage
    {
        #region Serializer

        private static XmlSerializer serializer = new XmlSerializer(typeof(XmlCirrusPackage));

        public static XmlSerializer Serializer
        {
            get { return serializer; }
        }

        #endregion

        public XmlCirrusPackage()
        {
            XNAReferences = new ObservableCollection<XmlCirrusXNAReference>();
            CirrusReferences = new ObservableCollection<XmlCirrusPackageReference>();
            CirrusContentInfo = new ObservableCollection<XmlCirrusContentInfo>();
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        /// <summary>
        /// Root path of the package. Everything inside this directory will be available.
        /// </summary>
        /// <remarks>The root can be a path relative to this package directory</remarks>
        [XmlAttribute("root")]
        public string RootDirectory { get; set; }

        /// <summary>
        /// Directory where the built content is placed
        /// </summary>
        /// <remarks>The output can be a path relative to this package directory</remarks>
        [XmlAttribute("output")]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// List of references to the XNA content pipeline.
        /// Defaults on every references available for XNA core components.
        /// </summary>
        [XmlArray("xna")]
        [XmlArrayItem("reference", typeof(XmlCirrusXNAReference))]
        public ObservableCollection<XmlCirrusXNAReference> XNAReferences { get; set; }

        /// <summary>
        /// List of references to the other cirrus packages.
        /// </summary>
        [XmlArray("cirrus")]
        [XmlArrayItem("package", typeof(XmlCirrusPackageReference))]
        public ObservableCollection<XmlCirrusPackageReference> CirrusReferences { get; set; }

        /// <summary>
        /// List of informations related to files
        /// </summary>
        [XmlArray("info")]
        [XmlArrayItem("file", typeof(XmlCirrusContentInfo))]
        public ObservableCollection<XmlCirrusContentInfo> CirrusContentInfo { get; set; }
    }
}
