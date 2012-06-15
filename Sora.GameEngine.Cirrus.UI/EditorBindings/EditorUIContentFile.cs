using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sora.GameEngine.Cirrus.Design.Application;
using Sora.GameEngine.Cirrus.UI.EditorBindings.Editors;
using Sora.GameEngine.Cirrus.UI.EditorBindings.Helpers;
using Sora.GameEngine.Cirrus.Design;
using Sora.GameEngine.Cirrus.Design.Packages;
using Sora.GameEngine.Cirrus.Design.Application.Editor;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    public class EditorUIContentFile : INotifyPropertyChanged
    {
        [Browsable(false)]
        public EditorContentFile EdFile { get; private set; }
        
        public EditorUIContentFile(EditorContentFile file)
        {
            EdFile = file;

            CreateSpecialProperties();

            /* Processor properties */
            RefreshProcessorProperties(false);
        }

        private void CreateSpecialProperties()
        {
            /* Processor */
            processorProperty = new CustomProcessorEditorDataEntry()
            {
                ProcessorValue = EdFile.Processor,
                XNATypes = EdFile.Editor.PackageContainer[0].AvailableXNATypes
            };

            processorProperty.PropertyChanged += delegate
            {
                EdFile.Processor = processorProperty == null ? null : processorProperty.ProcessorValue;

                RefreshProcessorProperties(true);
            };


            /* Importer */
            importerProperty = new CustomImporterEditorDataEntry()
            {
                ImporterValue = EdFile.Importer,
                XNATypes = EdFile.Editor.PackageContainer[0].AvailableXNATypes
            };

            importerProperty.PropertyChanged += delegate
            {
                EdFile.Importer = importerProperty == null ? null : importerProperty.ImporterValue;
            };
        }

        public void ResetProperties()
        {
            BuildAction = XmlBuildAction.Compile;
            var info = EdFile.GetContentInfo(false);
            if (info != null)
                info.Entries.Clear();

            EdFile.Processor = EdFile.Importer = null;

            EdFile.ResolveDefaultProcessorAndImporter();

            CreateSpecialProperties();
            RefreshProcessorProperties(true);

            RaisePropertyChanged("Processor");
            RaisePropertyChanged("Importer");
            RaisePropertyChanged("BuildAction");
        }

        public string Title
        {
            get { return EdFile.Title; }
        }

        public XmlBuildAction BuildAction
        {
            get { return EdFile.BuildAction; }
            set { EdFile.BuildAction = value; }
        }

        CustomImporterEditorDataEntry importerProperty;

        [Category("Importer")]
        [Editor(typeof(CustomEditorImporters), typeof(System.Activities.Presentation.PropertyEditing.PropertyValueEditor))]
        public CustomImporterEditorDataEntry Importer
        {
            get { return importerProperty; }
            set
            {
                importerProperty = value;
                EdFile.Importer = importerProperty == null ? null : importerProperty.ImporterValue;

                if (importerProperty != null)
                {
                    importerProperty.PropertyChanged += delegate
                    {
                        EdFile.Importer = importerProperty == null ? null : importerProperty.ImporterValue;
                    };
                }

                RaisePropertyChanged("Importer");
            }
        }

        CustomProcessorEditorDataEntry processorProperty;

        [Category("Processor")]
        [Editor(typeof(CustomEditorProcessors), typeof(System.Activities.Presentation.PropertyEditing.PropertyValueEditor))]
        public CustomProcessorEditorDataEntry Processor
        {
            get { return processorProperty; }
            set
            {
                processorProperty = value;
                EdFile.Processor = processorProperty == null ? null : processorProperty.ProcessorValue;

                if (processorProperty != null)
                {
                    processorProperty.PropertyChanged += delegate
                    {
                        EdFile.Processor = processorProperty == null ? null : processorProperty.ProcessorValue;
                    };
                }

                RaisePropertyChanged("Processor");
            }
        }

        private DictionaryCustomPropertiesProvider processorProperties;

        [Category("Processor")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Browsable(true)]
        public DictionaryCustomPropertiesProvider ProcessorProperties
        {
            get { return processorProperties; }
            private set
            {
                processorProperties = value;
                RaisePropertyChanged("ProcessorProperties");
            }
        }

        public void RefreshProcessorProperties(bool invalidateView)
        {
            processorProperties = new DictionaryCustomPropertiesProvider();

            var selectedProcessor = (
                from processor in Processor.AvailableProcessorsDescriptions
                where Processor.ProcessorValue == processor.Name
                select processor
                    ).FirstOrDefault();

            if (selectedProcessor != null)
            {
                foreach (var property in selectedProcessor.Properties)
                {
                    CreateDescriptorForProperty(property);
                }
            }

            RaisePropertyChanged("ProcessorProperties");

            /* epic cheat */
            EdFile.Editor.RefreshPropertiesView();
        }

        private void CreateDescriptorForProperty(KeyValuePair<string, XNAContentProcessorPropertyDescriptor> property)
        {
            var descriptor = property.Value;
            var defaultValue = EdFile.GetTypedProperty(property.Key, descriptor.Type, descriptor.DefaultValue);

            if (descriptor.Type.IsEnum)
            {
                processorProperties.Properties[property.Key] = new CustomEditorValidatedProperty<object, object>(
                    defaultValue,
                    (a) => a,
                    (b) => b,
                    (v) =>
                    {
                        EdFile.SetTypedProperty(property.Key, v);
                    },
                    descriptor.Type);
            }
            else
            {
                processorProperties.Properties[property.Key] = new CustomEditorValidatedProperty<object, object>(
                    defaultValue,
                    (a) => a,
                    (b) => b,
                    (v) =>
                    {
                        EdFile.SetTypedProperty(property.Key, v);
                    },
                    descriptor.Type);
            }
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
