using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    public class DockableContentDescriptor : INotifyPropertyChanged
    {
        private AvalonDock.DockableContent panel;

        public AvalonDock.DockableContent Panel
        {
            get { return panel; }
        }

        public string Title
        {
            get { return panel.Title; }
        }

        public Visibility Visibility
        {
            get { return (panel.IsVisible || panel.IsActiveContent) ? Visibility.Visible : System.Windows.Visibility.Hidden;}
        }

        public DockableContentDescriptor(AvalonDock.DockableContent panel)
        {
            this.panel = panel;
            panel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(panel_PropertyChanged);
        }

        void panel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e.PropertyName) 
                || e.PropertyName == "Visibility"
                || e.PropertyName == "IsVisible"
                || e.PropertyName == "IsActiveContent")
                RaisePropertyChanged("Visibility");
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
