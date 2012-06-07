using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Activities.Presentation.PropertyEditing;
using Sora.GameEngine.Cirrus.Design.Packages;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorContentFile : EditorContentObject
    {
        public EditorContentFile(EditorApplication editor, string relativePath, string basePath, string currentPath)
            : base(editor, relativePath, basePath, currentPath)
        {
            try
            {
                IsValid = File.Exists(currentPath);
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
                IsValid = false;
            } 
            
            var properties = GetContentInfo(false);

            if (properties != null)
            {
                processor = properties.Processor;
                importer = properties.Importer;
                buildAction = properties.BuildAction;
            }

            ResolveDefaultProcessorAndImporter();

        }

        public XmlCirrusContentInfo GetContentInfo(bool canCreate)
        {
            return Editor.CurrentPackage.GetItemDescriptor(RelativePath, canCreate);
        }

        public void CommitContentInfo()
        {
            var info = GetContentInfo(true);

            info.Processor = Processor;
            info.Importer = Importer;
            info.BuildAction = BuildAction;
        }

        private string processor = "";

        public string Processor
        {
            get { return processor; }
            set
            {
                processor = value;
                CommitContentInfo();
                RaisePropertyChanged("Processor");
            }
        }

        private string importer = "";

        public string Importer
        {
            get { return importer; }
            set
            {
                importer = value;
                CommitContentInfo();
                RaisePropertyChanged("Importer");
            }
        }

        private XmlBuildAction buildAction = XmlBuildAction.Compile;

        public XmlBuildAction BuildAction
        {
            get { return buildAction; }
            set
            {
                buildAction = value;
                CommitContentInfo();
                RaisePropertyChanged("BuildAction");
            }
        }


        public void ResolveDefaultProcessorAndImporter()
        {
            var xnaTypes = Editor.PackageContainer[0].AvailableXNATypes;
            IEnumerable<XNAContentImporterDescription> xnaImporters = null;

            if (String.IsNullOrEmpty(importer))
            {
                var extention = Path.GetExtension(CurrentPath);
                if (!String.IsNullOrEmpty(extention))
                {
                    xnaImporters = GetImporters(xnaTypes);

                    var defaultImporter = (from importer_obj in xnaImporters where importer_obj.FileExtensions.Contains(extention, StringComparer.OrdinalIgnoreCase) select importer_obj).FirstOrDefault();

                    if (defaultImporter != null)
                        importer = defaultImporter.Name;
                }
            }

            if (String.IsNullOrEmpty(processor))
            {
                if (xnaImporters == null)
                    xnaImporters = GetImporters(xnaTypes);

                var currentImporter = (from importer_obj in xnaImporters where importer_obj.Name == importer select importer_obj).FirstOrDefault();

                if (currentImporter != null)
                {
                    /* Special cases */
                    if (currentImporter.Name == "TextureImporter")
                        processor = "TextureProcessor";
                    else
                    {
                        /* General case */
                        processor = currentImporter.DefaultProcessor;
                    }
                }
            }

            RaisePropertyChanged("Processor");
            RaisePropertyChanged("Importer");
        }

        private static IEnumerable<XNAContentImporterDescription> GetImporters(IEnumerable<XNAAssemblyDescription> xnaTypes)
        {
            return (from xnaType in xnaTypes select xnaType.Importers).SelectMany((lists) => from entry in lists select entry);
        }

        public object GetTypedProperty(string name, Type type, object defaultValue)
        {
            try
            {
                var info = Editor.CurrentPackage.GetItemDescriptor(RelativePath, false);
                if (info == null)
                    return defaultValue;
                else
                {
                    var property = info.GetProperty(name, false);
                    if (property == null)
                        return defaultValue;
                    else
                    {
                        if (type.IsEnum)
                            return Enum.Parse(type, property.Value);
                        else
                            return Convert.ChangeType(property.Value, type);
                    }
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SetTypedProperty(string name, object value)
        {
            var info = Editor.CurrentPackage.GetItemDescriptor(RelativePath, true);
            var property = info.GetProperty(name, true);

            property.Value = Convert.ToString(value);

            CommitContentInfo();
        }
    }
}
