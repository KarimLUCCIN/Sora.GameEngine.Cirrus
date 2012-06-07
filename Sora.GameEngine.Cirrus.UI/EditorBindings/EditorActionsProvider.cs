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
            targetCollection.Add(new GenericCommand(AppendXNAReference) { DisplayName = "Append XNA Reference" });
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
        }

        private void ResetProperties(object p)
        {

        }

        private bool CanDeleteItem(object item)
        {
            return item is XmlCirrusXNAReference
                || item is XmlCirrusPackageReference;
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

        private void AppendXNAReference(object p)
        {

        }

        private void AppendPackageReference(object p)
        {

        }

        private void OpenInWindowsExplorer(object p)
        {
            var basePath = Path.Combine(Path.GetDirectoryName( editorApplication.CurrentPackagePath), editorApplication.CurrentPackage.RootDirectory);

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

        private bool IsItemModifiableWithPropertiesForFileSystem(object item)
        {
            return
                !String.IsNullOrEmpty(editorApplication.CurrentPackagePath) &&
                (item is EditorContentObject ||
                item is EditorUIContentFile);
        }
    }
}
