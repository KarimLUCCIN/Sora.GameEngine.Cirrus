using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Sora.GameEngine.Cirrus.Design;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings.Editors
{
    public class CustomProcessorEditorDataEntry : INotifyPropertyChanged
    {
        private string processorValue;

        public string ProcessorValue
        {
            get { return processorValue; }
            set
            {
                processorValue = value;
                RaisePropertyChanged("ProcessorValue");
            }
        }

        private ObservableCollection<XNAAssemblyDescription> xnaTypes;

        public ObservableCollection<XNAAssemblyDescription> XNATypes
        {
            get { return xnaTypes; }
            set
            {
                xnaTypes = value;
                RaisePropertyChanged("XNATypes");
                RaisePropertyChanged("AvailableProcessors");
            }
        }

        public IEnumerable<string> AvailableProcessors
        {
            get
            {
                if(xnaTypes == null)
                    yield break;

                foreach (var type in xnaTypes)
                {
                    foreach (var processor in type.Processors)
                        yield return processor.Name;
                }
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
