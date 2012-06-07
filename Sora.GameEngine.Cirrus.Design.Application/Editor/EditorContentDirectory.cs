using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace Sora.GameEngine.Cirrus.Design.Application.Editor
{
    public class EditorContentDirectory : EditorContentObject
    {        
        public EditorContentDirectory(EditorApplication editor, string relativePath, string basePath, string currentPath)
            :base(editor, relativePath, basePath, currentPath)
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
                        DirectoryInfo[] dirs = null;
                        try
                        {
                            dirs = directory.GetDirectories();
                        }
                        catch { IsValid = false; }

                        if (dirs != null)
                        {
                            foreach (var sub_dir in dirs)
                            {
                                EditorContentDirectory contentDir = null;
                                try
                                {
                                    contentDir = new EditorContentDirectory(Editor, Path.Combine(RelativePath, sub_dir.Name), BasePath, sub_dir.FullName);
                                }
                                catch { }

                                if (contentDir != null)
                                    yield return contentDir;
                            }
                        }

                        FileInfo[] files = null;
                        try
                        {
                            files = directory.GetFiles();
                        }
                        catch { IsValid = false; }

                        if (files != null)
                        {
                            foreach (var sub_file in files)
                            {
                                if (!CirrusDesignHelper.CirrusPackageExtention.Equals(sub_file.Extension, StringComparison.OrdinalIgnoreCase))
                                {
                                    EditorContentFile contentFile = null;

                                    try
                                    {
                                        contentFile = new EditorContentFile(Editor, Path.Combine(RelativePath, sub_file.Name), BasePath, sub_file.FullName);
                                    }
                                    catch { }

                                    if (contentFile != null)
                                        yield return contentFile;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
