using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sora.GameEngine.Cirrus.Design.Packages;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.Diagnostics;
using System.Reflection;

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
            yield return new XmlCirrusXNAReference() { Reference = typeof(ContentImporterAttribute).Assembly.FullName };
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

        private static object class_sync = new object();

        /// <summary>
        /// For each full assembly name, the descriptors associated (allowing fast reload)
        /// </summary>
        private static Dictionary<string, XNAAssemblyDescription> xnaAssemblies = new Dictionary<string, XNAAssemblyDescription>();

        public void InvalidateXNAAssembliesCache()
        {
            lock (class_sync)
            {
                xnaAssemblies.Clear();
            }
        }

        /// <summary>
        /// Get a list of object describing xna references
        /// </summary>
        /// <param name="references"></param>
        /// <returns></returns>
        public XNAAssemblyDescription[] GetXNAAssembliesDescriptors(IEnumerable<XmlCirrusXNAReference> references)
        {
            if (references == null)
                throw new ArgumentNullException("references");
            else
            {
                lock (class_sync)
                {
                    var result = new List<XNAAssemblyDescription>();

                    foreach (var reference in references)
                    {
                        string finalAssemblyName;

                        if (TryLoadAssemblyReferences(reference, out finalAssemblyName))
                        {
                            XNAAssemblyDescription description;

                            if (xnaAssemblies.TryGetValue(finalAssemblyName, out description))
                            {
                                result.Add(description);
                                reference.Valid = false;
                            }
                            else
                                reference.Valid = true;
                        }
                        else
                            reference.Valid = false;
                    }

                    return result.ToArray();
                }
            }
        }

        /// <summary>
        /// Try to load an assembly description from the cache or load in from the assembly if required
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="finalAssemblyName"></param>
        /// <returns></returns>
        private static bool TryLoadAssemblyReferences(XmlCirrusXNAReference reference, out string finalAssemblyName)
        {
            finalAssemblyName = null;

            if (String.IsNullOrEmpty(reference.Reference))
                return false;
            else
            {
                try
                {
                    XNAAssemblyDescription xnaDescription;

                    using (var isolatedLoadingContext = new Isolated<XNAAssemblyDescriptionLoader>())
                    {
                        xnaDescription = isolatedLoadingContext.Value.LoadDescription(reference.Reference);
                    }

                    if (xnaDescription.Valid)
                    {
                        finalAssemblyName = xnaDescription.ReferenceName;
                        xnaAssemblies[finalAssemblyName] = xnaDescription;

                        return true;
                    }
                    else
                        return false;
                }
                catch
                {
                    if (!String.IsNullOrEmpty(finalAssemblyName))
                        xnaAssemblies.Remove(finalAssemblyName);

                    return false;
                }
            }
        }
    }
}
