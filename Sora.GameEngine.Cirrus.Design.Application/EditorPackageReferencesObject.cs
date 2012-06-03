using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Sora.GameEngine.Cirrus.Design.Packages;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorPackageReferencesObject : EditorBaseBoundObject
    {
        public string Title { get; private set; }

        public ObservableCollection<XmlCirrusPackageReference> Content
        {
            get { return Editor.CurrentPackage.CirrusReferences; }
        }

        public EditorPackageReferencesObject(EditorApplication editor)
            :base(editor)
        {
            Title = "Package References";
        }
    }
}
