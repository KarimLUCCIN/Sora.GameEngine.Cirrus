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
        public static string CirrusPackageExtention
        {
            get { return ".crpackage"; }
        }

        public static string CirrusPackageDialogFilter
        {
            get { return "Cirrus Package (*.crpackage)|*.crpackage"; }
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

                    /*
                     * Using a different AppDomain is beautiful and so on, but there are a lot of problems
                     * when trying to marshal back editable types for the processors as they are not available
                     * in the UI context.
                     * 
                     * So, we are going the old way, everything in the same AppDomain ...
                     * */
                    //using (var isolatedLoadingContext = new Isolated<XNAAssemblyDescriptionLoader>())
                    //{
                    //    xnaDescription = isolatedLoadingContext.Value.LoadDescription(reference.Reference);
                    //}
                    var referencesLoader = new XNAAssemblyDescriptionLoader();
                    xnaDescription = referencesLoader.LoadDescription(reference.Reference);

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
        
        //from http://www.iandevlin.com/blog/2010/01/csharp/generating-a-relative-path-in-csharp
        public static string RelativePath(string absPath, string relTo)
        {
            string[] absDirs = absPath.Split('\\');
            string[] relDirs = relTo.Split('\\');

            // Get the shortest of the two paths
            int len = absDirs.Length < relDirs.Length ? absDirs.Length :
            relDirs.Length;

            // Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            // Find common root
            for (index = 0; index < len; index++)
            {
                if (absDirs[index] == relDirs[index]) lastCommonRoot = index;
                else break;
            }

            // If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
            {
                throw new ArgumentException("Paths do not have a common base");
            }

            // Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            // Add on the ..
            for (index = lastCommonRoot + 1; index < absDirs.Length; index++)
            {
                if (absDirs[index].Length > 0) relativePath.Append("..\\");
            }

            // Add on the folders
            for (index = lastCommonRoot + 1; index < relDirs.Length - 1; index++)
            {
                relativePath.Append(relDirs[index] + "\\");
            }
            relativePath.Append(relDirs[relDirs.Length - 1]);

            return relativePath.ToString();
        }
    }
}
