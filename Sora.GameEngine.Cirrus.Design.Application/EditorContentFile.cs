using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorContentFile : EditorContentObject
    {
        public EditorContentFile(EditorApplication editor, string basePath, string currentPath)
            :base(editor, basePath, currentPath)
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
    }
}
