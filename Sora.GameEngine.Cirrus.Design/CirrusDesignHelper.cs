using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sora.GameEngine.Cirrus.Design.Packages;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.Diagnostics;

namespace Sora.GameEngine.Cirrus.Design
{
    public class CirrusDesignHelper
    {
        /// <summary>
        /// Extention for the attached information file
        /// </summary>
        public static string CirrusPropertiesFileExt
        {
            get { return ".crinfo"; }
        }

        public IEnumerable<XmlCirrusXNAReference> GetDefaultXNAReferences()
        {
            yield return new XmlCirrusXNAReference() { Reference = typeof(Mp3Importer).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(EffectImporter).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(FbxImporter).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(TextureImporter).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(WmvImporter).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(XImporter).Assembly.FullName };
        }

        public string ResolveDirectory(string baseDirectory, string directory)
        {
            if (String.IsNullOrEmpty(directory))
                return baseDirectory;
            else
            {
                try
                {
                    if (!Path.IsPathRooted(directory))
                        return Path.GetFullPath(baseDirectory + "\\" + directory);
                    else
                        return directory;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    return directory;
                }
            }
        }
    }
}
