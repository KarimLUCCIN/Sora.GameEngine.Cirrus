using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.Design.Application.Editor
{
    public class EditorBaseBoundObject : INotifyPropertyChanged
    {
        [Browsable(false)]
        public EditorApplication Editor { get; private set; }

        public EditorBaseBoundObject(EditorApplication editor)
        {
            Editor = editor;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
