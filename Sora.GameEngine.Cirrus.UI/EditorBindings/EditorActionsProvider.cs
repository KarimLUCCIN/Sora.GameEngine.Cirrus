using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sora.GameEngine.Cirrus.Design.Application;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using Sora.GameEngine.Cirrus.Design.Packages;
using Sora.GameEngine.Cirrus.Design.Application.Helpers;
using Sora.GameEngine.Cirrus.Design.Application.Editor;
using Microsoft.Win32;
using System.Windows;
using Sora.GameEngine.Cirrus.Design;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    public class EditorActionsProvider
    {
        private EditorUIApplication editorApplication;

        public EditorActionsProvider(EditorUIApplication editorApplication)
        {
            if (editorApplication == null)
                throw new ArgumentNullException("editorApplication");

            this.editorApplication = editorApplication;
        }

        public void Load(ObservableCollection<GenericCommand> targetCollection)
        {
            targetCollection.Add(new GenericCommand(AppendXNAProcessorReference) { DisplayName = "Append XNA Content Processor Reference" });
            targetCollection.Add(new GenericCommand(AppendPackageReference) { DisplayName = "Append Package Reference" });

            targetCollection.Add(new GenericCommand(
               (p) => OpenInWindowsExplorer(editorApplication.SelectionForProperties.First((item) => IsItemModifiableWithPropertiesForFileSystem(item))),
                (p) => editorApplication.SelectionForProperties.FirstOrDefault((item) => IsItemModifiableWithPropertiesForFileSystem(item)) != null) { DisplayName = "Open in Windows Explorer" });

            targetCollection.Add(new GenericCommand(
                ResetProperties,
                (prop) => editorApplication.SelectionForProperties.All((item) => IsItemModifiableWithPropertiesForFileSystem(item))) { DisplayName = "Reset Properties" });

            targetCollection.Add(new GenericCommand(
                DeleteElements,
                (p) => editorApplication.SelectionForProperties.All((item) => CanDeleteItem(item))) { DisplayName = "Remove Reference" });


            targetCollection.Add(new GenericCommand(_ => editorApplication.Refresh()) { DisplayName = "Refresh View" });
        }

        #region Utils

        private void ResetProperties(object p)
        {
            bool hadDirectory = false;

            foreach (var item in editorApplication.SelectionForProperties)
            {
                var contentFile = item as EditorUIContentFile;
                var contentDirectory = item as EditorContentDirectory;

                if (contentFile != null)
                {
                    contentFile.ResetProperties();
                }
                else if (contentDirectory != null)
                {
                    hadDirectory = true;
                    ResetPropertiesRecursive(contentDirectory);
                }
            }

            if (hadDirectory)
                editorApplication.Refresh();
        }

        private void ResetPropertiesRecursive(EditorContentDirectory rootDirectory)
        {
            try
            {
                foreach (var element in rootDirectory.Content)
                {
                    var contentFile = element as EditorContentFile;
                    var contentDirectory = element as EditorContentDirectory;

                    if (contentFile != null)
                    {
                        contentFile.RemoveContentInfo();
                    }
                    else if (contentDirectory != null)
                    {
                        ResetPropertiesRecursive(contentDirectory);
                    }
                }
            }
            catch { }
        }

        private void DeleteElements(object p)
        {
            foreach (var item in editorApplication.SelectionForProperties)
            {
                if (item is XmlCirrusXNAReference)
                    editorApplication.CurrentPackage.XNAReferences.Remove((XmlCirrusXNAReference)item);
                else if (item is XmlCirrusPackageReference)
                    editorApplication.CurrentPackage.CirrusReferences.Remove((XmlCirrusPackageReference)item);
            }
        }

        private void OpenInWindowsExplorer(object p)
        {
            var basePath = Path.Combine(Path.GetDirectoryName(editorApplication.CurrentPackagePath), editorApplication.CurrentPackage.RootDirectory);

            var contentDir = p as EditorContentDirectory;
            var contentFile = p as EditorContentFile;
            var contentUIFile = p as EditorUIContentFile;

            if (contentDir != null)
                Process.Start("explorer", "\"" + Path.Combine(basePath, contentDir.RelativePath) + "\"");
            else if (contentFile != null)
                Process.Start("explorer", "\"" + Path.Combine(basePath, Path.GetDirectoryName(contentFile.RelativePath)) + "\"");
            else if (contentUIFile != null)
                Process.Start("explorer", "\"" + Path.Combine(basePath, Path.GetDirectoryName(contentUIFile.EdFile.RelativePath)) + "\"");

        }

        #endregion

        #region Validations

        private bool CanDeleteItem(object item)
        {
            return item is XmlCirrusXNAReference
                || item is XmlCirrusPackageReference;
        }

        private bool IsItemModifiableWithPropertiesForFileSystem(object item)
        {
            return
                !String.IsNullOrEmpty(editorApplication.CurrentPackagePath) &&
                (item is EditorContentObject ||
                item is EditorUIContentFile);
        }

        #endregion

        #region References

        private bool ProjectIsSavedRequiredmentCheck()
        {
            if (String.IsNullOrEmpty(editorApplication.CurrentPackagePath))
            {
                MessageBox.Show("Please save the package before executing this operation", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
                return true;
        }

        private void AppendXNAProcessorReference(object parameter)
        {
            if (ProjectIsSavedRequiredmentCheck())
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "Content Pipeline Extention Assembly (*.dll)|*.dll";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == true)
                {
                    var xnaReferences = editorApplication.CurrentPackage.XNAReferences;

                    foreach (var fileName in ofd.FileNames)
                    {
                        /* Adding the reference only if not present */
                        if (xnaReferences.FirstOrDefault((p) => fileName.Equals(editorApplication.Builder.ParseReferencePath(p.Reference), StringComparison.OrdinalIgnoreCase)) == null)
                        {
                            string referencePath;

                            try
                            {
                                referencePath = CirrusDesignHelper.RelativePath(Path.GetDirectoryName(editorApplication.CurrentPackagePath), fileName);
                            }
                            catch
                            {
                                referencePath = fileName;
                            }

                            xnaReferences.Add(new XmlCirrusXNAReference() { Valid = true, Reference = referencePath });
                        }
                    }
                }
            }
        }

        private void AppendPackageReference(object parameter)
        {
            if (ProjectIsSavedRequiredmentCheck())
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = CirrusDesignHelper.CirrusPackageDialogFilter;
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == true)
                {
                    var cirrusReferences = editorApplication.CurrentPackage.CirrusReferences;

                    foreach (var fileName in ofd.FileNames)
                    {
                        /* Adding the reference only if not present */
                        if (cirrusReferences.FirstOrDefault((p) => fileName.Equals(editorApplication.Builder.ParseReferencePath(p.Reference), StringComparison.OrdinalIgnoreCase)) == null)
                        {
                            string referencePath;

                            try
                            {
                                referencePath = CirrusDesignHelper.RelativePath(Path.GetDirectoryName(editorApplication.CurrentPackagePath), fileName);
                            }
                            catch
                            {
                                referencePath = fileName;
                            }

                            cirrusReferences.Add(new XmlCirrusPackageReference() { Valid = true, Reference = referencePath });
                        }
                    }
                }
            }
        }

        #endregion
    }
}
