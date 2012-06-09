using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// Content of the current directory, considering the ignore filters defined at project level
        /// </summary>
        public IEnumerable<EditorBaseBoundObject> Content
        {
            get
            {
                if (IsValid)
                {
                    return EnumerateDirectoryContent(Editor.ignoreRegexFilters);
                }
                else 
                    return new EditorBaseBoundObject[0];
            }
        }

        /// <summary>
        /// Content of the current directory, including everything regardless of ignore filters
        /// </summary>
        public IEnumerable<EditorBaseBoundObject> UnfilteredContent
        {
            get
            {
                if (IsValid)
                {
                    return EnumerateDirectoryContent(null);
                }
                else
                    return new EditorBaseBoundObject[0];
            }
        }

        /// <summary>
        /// Enumerate the content of the directory excluding elements matching the specified regexes
        /// </summary>
        /// <param name="ignoreRegexes"></param>
        /// <returns></returns>
        private IEnumerable<EditorBaseBoundObject> EnumerateDirectoryContent(Regex[] ignoreRegexes)
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
                        string relativeDirPath = Path.Combine(RelativePath, sub_dir.Name);

                        bool ignoreDirectory = false;

                        if (ignoreRegexes != null)
                        {
                            /* test to see if the directory should be ignored */
                            foreach (var regex in ignoreRegexes)
                            {
                                if (regex.IsMatch(relativeDirPath))
                                {
                                    ignoreDirectory = true;
                                    break;
                                }
                            }
                        }

                        if (!ignoreDirectory)
                        {
                            EditorContentDirectory contentDir = null;

                            try
                            {
                                contentDir = new EditorContentDirectory(Editor, relativeDirPath, BasePath, sub_dir.FullName);
                            }
                            catch { }

                            if (contentDir != null)
                                yield return contentDir;
                        }
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
                            string relativeFilePath = Path.Combine(RelativePath, sub_file.Name);

                            bool ignoreFile = false;

                            if (ignoreRegexes != null)
                            {
                                /* test to see if the directory should be ignored */
                                foreach (var regex in ignoreRegexes)
                                {
                                    if (regex.IsMatch(relativeFilePath))
                                    {
                                        ignoreFile = true;
                                        break;
                                    }
                                }
                            }

                            if (!ignoreFile)
                            {
                                EditorContentFile contentFile = null;

                                try
                                {
                                    contentFile = new EditorContentFile(Editor, relativeFilePath, BasePath, sub_file.FullName);
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
