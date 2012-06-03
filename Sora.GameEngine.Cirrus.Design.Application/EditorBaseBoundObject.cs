using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorBaseBoundObject
    {
        public EditorApplication Editor { get; private set; }

        public EditorBaseBoundObject(EditorApplication editor)
        {
            Editor = editor;
        }
    }
}
