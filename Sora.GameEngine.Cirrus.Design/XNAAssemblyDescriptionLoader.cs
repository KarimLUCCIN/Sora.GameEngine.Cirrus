﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;

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

        public XNAAssemblyDescription LoadDescription(string referenceName)
        {
            if (String.IsNullOrEmpty(referenceName))
                return null;
            else
            {
                var description = new XNAAssemblyDescription() { ReferenceName = referenceName };
                description.Valid = true;

                try
                {
                    var assembly = Assembly.LoadWithPartialName(description.ReferenceName);

                    if (assembly != null)
                    {
                        description.ReferenceName = assembly.FullName;

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
                                    TypeId = importerAtt.TypeId
                                });
                            }
                            else if (isProcessor)
                            {
                                var processorDescription = new XNAContentProcessorDescription()
                                {
                                    Name = type.Name,
                                    DisplayName = processorAtt.DisplayName,
                                    TypeId = processorAtt.TypeId
                                };

                                /* Loading available properties */
                                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                                {
                                    if (property.CanWrite && property.CanRead)
                                    {
                                        var displayProperty = true;

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
                                        }

                                        if (displayProperty)
                                        {
                                            processorDescription.Properties[property.Name] = property.PropertyType;
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
