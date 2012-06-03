using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorContentDirectory : EditorContentObject
    {        
        public EditorContentDirectory(EditorApplication editor, string basePath, string currentPath)
            :base(editor, basePath, currentPath)
        {
            try
            {
                IsValid = Directory.Exists(currentPath);
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
                IsValid = false;
            }
        }

        public IEnumerable<EditorBaseBoundObject> Content
        {
            get
            {
                if (IsValid)
                {
                    DirectoryInfo directory;

                    try
                    {
                        directory = new DirectoryInfo(CurrentPath);
                    }
                    catch (Exception ex)
                    {
                        directory = null;
                        ErrorString = ex.ToString();

                        IsValid = false;
                    }

                    if (directory != null)
                    {
                        foreach (var sub_dir in directory.GetDirectories())
                            yield return new EditorContentDirectory(Editor, BasePath, sub_dir.FullName);

                        foreach (var sub_file in directory.GetFiles())
                        {
                            if (!CirrusDesignHelper.CirrusPropertiesFileExt.Equals(sub_file.Extension, StringComparison.OrdinalIgnoreCase))
                                yield return new EditorContentFile(Editor, BasePath, sub_file.FullName);
                        }
                    }
                }
            }
        }
    }
}
