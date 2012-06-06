using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sora.GameEngine.Cirrus.Design.Application;
using Microsoft.Win32;
using Sora.GameEngine.Cirrus.Design;
using System.Windows;
using System.Diagnostics;
using System.IO;
using Sora.GameEngine.Cirrus.Design.Packages;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    public class EditorUIApplication : EditorApplication
    {
        private MainWindow mainWindow;

        public EditorUIApplication(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            NewFile = new GenericCommand((p) => ActionNewFile(p));

            OpenFile = new GenericCommand((p) => ActionOpenFile(p));

            SaveAsFile = new GenericCommand((p) => ActionSaveAsFile(p));
            SaveFile = new GenericCommand((p) => ActionSaveFile(p));

            Quit = new GenericCommand((p) => ActionQuit(p));
        }

        #region Menu
        public GenericCommand NewFile { get; private set; }
        public void ActionNewFile(object parameter)
        {
            InitializeNew();
        }

        public GenericCommand OpenFile { get; private set; }
        public void ActionOpenFile(object parameter)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = CirrusDesignHelper.CirrusPackageDialogFilter;
            if (ofd.ShowDialog() == true)
            {
                string fileName = ofd.FileName;

                try
                {
                    LoadPackage(fileName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public GenericCommand SaveFile { get; private set; }
        public bool ActionSaveFile(object parameter)
        {
            if (!String.IsNullOrEmpty(CurrentPackagePath))
            {
                try
                {
                    SavePackage(CurrentPackagePath);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return ActionSaveAsFile(null);
                }
            }
            else
                return ActionSaveAsFile(null);
        }

        public GenericCommand SaveAsFile { get; private set; }
        public bool ActionSaveAsFile(object parameter)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = CirrusDesignHelper.CirrusPackageDialogFilter;
            if (sfd.ShowDialog() == true)
            {
                var oldRootPath = CurrentPackage.RootDirectory;

                if (!String.IsNullOrEmpty(CurrentPackagePath))
                {
                    /* The point is keeping a relative reference to the root path */
                    var absolutePath = (!String.IsNullOrEmpty(oldRootPath) && Path.IsPathRooted(oldRootPath))
                    ? oldRootPath
                    : Path.GetDirectoryName(CurrentPackagePath) + "\\" + CurrentPackage.RootDirectory;

                    var dirInfo = new DirectoryInfo(absolutePath);

                    if (dirInfo.Exists)
                    {
                        absolutePath = dirInfo.FullName;

                        try
                        {
                            var newTargetFolder = Path.GetDirectoryName(sfd.FileName);

                            CurrentPackage.RootDirectory = CirrusDesignHelper.RelativePath(newTargetFolder, absolutePath);
                        }
                        catch
                        {
                            /* keep it absolute */
                            CurrentPackage.RootDirectory = dirInfo.FullName;
                        }
                    }
                }

                var oldPackagePath = CurrentPackagePath;

                CurrentPackagePath = sfd.FileName;
                bool succeded = ActionSaveFile(null);

                if (!succeded)
                {
                    CurrentPackagePath = oldPackagePath;
                    CurrentPackage.RootDirectory = oldRootPath;

                    Refresh();

                    return false;
                }
                else
                    return true;
            }
            else
                return false;
        }

        public GenericCommand Quit { get; private set; }
        public bool ActionQuit(object parameter)
        {
            mainWindow.Close();
            return true;
        }
        #endregion

        #region Settings
        
        public string UISettingsLocation
        {
            get
            {
                var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sora", "Cirrus", "Editor.UI.config.xml");
                var settingsDirectory = Path.GetDirectoryName(settingsPath);

                var dirInfo = new DirectoryInfo(settingsDirectory);

                if (!dirInfo.Exists)
                    dirInfo.Create();

                return settingsPath;
            }
        }

        #endregion
    }
}
