using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Sora.GameEngine.Cirrus.Design.Packages;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorXNAReferencesObject : EditorBaseBoundObject
    {
        public string Title { get; private set; }

        public ObservableCollection<XmlCirrusXNAReference> Content
        {
            get { return Editor.CurrentPackage.XNAReferences; }
        }

        public EditorXNAReferencesObject(EditorApplication editor)
            :base(editor)
        {
            Title = "XNA References";
        }
    }
}
