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

            CompressContent = true;
            ContentDirectorySuffix = "Content";
        }

        [XmlAttribute("contentsuffix")]
        public string ContentDirectorySuffix { get; set; }

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

        [XmlAttribute("compress")]
        public bool CompressContent { get; set; }

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

        public void RemoveItemDescriptor(string relativePath)
        {
            if (relativePath != null)
            {
                var existingItem = (from item in CirrusContentInfo where relativePath.Equals(item.RelativePath, StringComparison.OrdinalIgnoreCase) select item).FirstOrDefault();

                if (existingItem != null)
                {
                    CirrusContentInfo.Remove(existingItem);
                }
            }
        }

        /// <summary>
        /// Tries to get the description item associated to this path
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="canCreate">if false, null is returned if the item doesn't exists. Else, a new item is created with this path</param>
        /// <returns></returns>
        public XmlCirrusContentInfo GetItemDescriptor(string relativePath, bool canCreate)
        {
            if (relativePath == null)
                return null;
            else
            {
                var existingItem = (from item in CirrusContentInfo where relativePath.Equals(item.RelativePath, StringComparison.OrdinalIgnoreCase) select item).FirstOrDefault();

                if (existingItem == null && canCreate)
                {
                    existingItem = new XmlCirrusContentInfo() { RelativePath = relativePath };
                    CirrusContentInfo.Add(existingItem);
                }

                return existingItem;
            }
        }
    }
}
