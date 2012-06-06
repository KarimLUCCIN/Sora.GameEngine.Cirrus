using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sora.GameEngine.Cirrus.Design.Application;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;

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
            targetCollection.Add(new GenericCommand(
               (p) => OpenInWindowsExplorer(editorApplication.SelectionForProperties.First((item) => CanItemBeOpennedInWindowsExplorer(item))),
                (p) => editorApplication.SelectionForProperties.FirstOrDefault((item) => CanItemBeOpennedInWindowsExplorer(item)) != null) { DisplayName = "Open in Windows Explorer" });
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

        private bool CanItemBeOpennedInWindowsExplorer(object item)
        {
            return
                !String.IsNullOrEmpty(editorApplication.CurrentPackagePath) &&
                (item is EditorContentObject ||
                item is EditorUIContentFile);
        }
    }
}
