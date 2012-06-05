using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Activities.Presentation.PropertyEditing;

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
        }

        private string processor = "";

        public string Processor
        {
            get { return processor; }
            set
            {
                processor = value;
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
                RaisePropertyChanged("Importer");
            }
        }

        public void ResolveDefaultProcessorAndImporter()
        {
            var xnaTypes = Editor.PackageContainer[0].AvailableXNATypes;
            IEnumerable<XNAContentImporterDescription> xnaImporters = null;

            if (String.IsNullOrEmpty(Importer))
            {
                var extention = Path.GetExtension(CurrentPath);
                if (!String.IsNullOrEmpty(extention))
                {
                    xnaImporters = GetImporters(xnaTypes);

                    var defaultImporter = (from importer in xnaImporters where importer.FileExtensions.Contains(extention, StringComparer.OrdinalIgnoreCase) select importer).FirstOrDefault();

                    if (defaultImporter != null)
                        Importer = defaultImporter.Name;
                }
            }

            if (String.IsNullOrEmpty(Processor))
            {
                if (xnaImporters == null)
                xnaImporters = GetImporters(xnaTypes);

                var currentImporter = (from importer in xnaImporters where importer.Name.Equals(Importer) select importer).FirstOrDefault();

                if (currentImporter != null)
                    Processor = currentImporter.DefaultProcessor;
            }
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
        }
    }
}
