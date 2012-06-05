using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Sora.GameEngine.Cirrus.Design;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings.Editors
{
    public class CustomImporterEditorDataEntry : INotifyPropertyChanged
    {
        private string importerValue;

        public string ImporterValue
        {
            get { return importerValue; }
            set
            {
                importerValue = value;
                RaisePropertyChanged("ImporterValue");
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
                RaisePropertyChanged("AvailableImporters");
            }
        }

        public IEnumerable<string> AvailableImporters
        {
            get
            {
                if(xnaTypes == null)
                    yield break;

                foreach (var type in xnaTypes)
                {
                    foreach (var importer in type.Importers)
                        yield return importer.Name;
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
