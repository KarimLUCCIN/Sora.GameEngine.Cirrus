using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sora.GameEngine.Cirrus.Design.Application;
using Sora.GameEngine.Cirrus.UI.EditorBindings.Editors;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    public class EditorUIContentFile : INotifyPropertyChanged
    {
        [Browsable(false)]
        public EditorContentFile EdFile { get; private set; }
        
        public EditorUIContentFile(EditorContentFile file)
        {
            EdFile = file;
            processorProperty = new CustomProcessorEditorDataEntry()
            {
                ProcessorValue = file.Processor,
                XNATypes = file.Editor.PackageContainer[0].AvailableXNATypes
            };

            processorProperty.PropertyChanged += delegate
            {
                EdFile.Processor = processorProperty == null ? null : processorProperty.ProcessorValue;
            };


            importerProperty = new CustomImporterEditorDataEntry()
            {
                ImporterValue = file.Importer,
                XNATypes = file.Editor.PackageContainer[0].AvailableXNATypes
            };

            importerProperty.PropertyChanged += delegate
            {
                EdFile.Importer = importerProperty == null ? null : importerProperty.ImporterValue;
            };
        }

        public string Title
        {
            get { return EdFile.Title; }
        }

        CustomImporterEditorDataEntry importerProperty;

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
