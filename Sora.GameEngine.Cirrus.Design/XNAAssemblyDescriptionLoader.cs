using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using System.IO;

namespace Sora.GameEngine.Cirrus.Design
{
    /// <summary>
    /// Proxy loader for XNA references. Allow loading and unloading XNA assemblies in an isolated AppDomain
    /// </summary>
    internal class XNAAssemblyDescriptionLoader : MarshalByRefObject
    {
        public XNAAssemblyDescriptionLoader()
        {

        }

        static List<string> searchPaths = new List<string>();

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyname = args.Name.Split(',')[0];

            foreach (var searchPath in searchPaths)
            {
                var assemblyFileName = Path.Combine(searchPath, assemblyname + ".dll");

                if (File.Exists(assemblyFileName))
                    return Assembly.LoadFrom(assemblyFileName);
            }

            return null;
        }

        static XNAAssemblyDescriptionLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        public XNAAssemblyDescription LoadDescription(string referenceName)
        {
            if (!searchPaths.Contains(Assembly.GetExecutingAssembly().Location))
                searchPaths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            if (String.IsNullOrEmpty(referenceName))
                return null;
            else
            {
                var description = new XNAAssemblyDescription() { ReferenceName = referenceName };
                description.Valid = true;

                try
                {
                    Assembly assembly;

                    if (File.Exists(description.ReferenceName))
                    {
                        if (!searchPaths.Contains(Path.GetDirectoryName(description.ReferenceName)))
                            searchPaths.Add(Path.GetDirectoryName(description.ReferenceName));

                        assembly = Assembly.LoadFrom(description.ReferenceName);
                    }
                    else
                        assembly = Assembly.LoadWithPartialName(description.ReferenceName);

                    if (assembly != null)
                    {
                        description.ReferenceName = File.Exists(description.ReferenceName) ? description.ReferenceName : assembly.FullName;

                        foreach (var type in assembly.GetTypes())
                        {
                            bool isImporter = false;
                            bool isProcessor = false;

                            ContentImporterAttribute importerAtt = null;
                            ContentProcessorAttribute processorAtt = null;

                            foreach (var att in type.GetCustomAttributes(true))
                            {
                                if (typeof(ContentImporterAttribute).IsAssignableFrom(att.GetType()))
                                {
                                    isImporter = true;
                                    importerAtt = (ContentImporterAttribute)att;
                                    break;
                                }
                                else if (typeof(ContentProcessorAttribute).IsAssignableFrom(att.GetType()))
                                {
                                    isProcessor = true;
                                    processorAtt = (ContentProcessorAttribute)att;
                                    break;
                                }
                            }

                            if (isImporter)
                            {
                                description.Importers.Add(new XNAContentImporterDescription()
                                {
                                    Name = type.Name,
                                    FileExtensions = importerAtt.FileExtensions,
                                    DefaultProcessor = importerAtt.DefaultProcessor,
                                    DisplayName = importerAtt.DisplayName,
                                    TypeId = importerAtt.TypeId,
                                    TypeName = type.FullName
                                });
                            }
                            else if (isProcessor)
                            {
                                var processorDescription = new XNAContentProcessorDescription()
                                {
                                    Name = type.Name,
                                    DisplayName = processorAtt.DisplayName,
                                    TypeId = processorAtt.TypeId,
                                    TypeName = type.FullName
                                };

                                /* Loading available properties */
                                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                {
                                    if (property.CanWrite && property.CanRead)
                                    {
                                        var displayProperty = true;

                                        object defaultValue = null;

                                        foreach (var att in property.GetCustomAttributes(true))
                                        {
                                            if (typeof(BrowsableAttribute).IsAssignableFrom(att.GetType()))
                                            {
                                                var b_att = (BrowsableAttribute)att;

                                                if (!b_att.Browsable)
                                                {
                                                    displayProperty = false;
                                                    break;
                                                }
                                            }
                                            else if (typeof(DefaultValueAttribute).IsAssignableFrom(att.GetType()))
                                            {
                                                defaultValue = ((DefaultValueAttribute)att).Value;
                                            }
                                        }

                                        if (displayProperty)
                                        {
                                            processorDescription.Properties[property.Name] =
                                                new XNAContentProcessorPropertyDescriptor() { Type = property.PropertyType, DefaultValue = defaultValue };
                                        }
                                    }
                                }

                                description.Processors.Add(processorDescription);
                            }
                        }

                        description.Valid = true;
                    }
                }
                catch
                {
                    description.Valid = false;
                }

                return description;
            }
        }
    }
}
