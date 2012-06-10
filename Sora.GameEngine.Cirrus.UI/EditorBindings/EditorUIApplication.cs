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
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Sora.GameEngine.Cirrus.Design.Application.Helpers;
using Sora.GameEngine.Cirrus.Design.Application.Editor;
using Sora.GameEngine.Cirrus.Design.Application.Build;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    public class EditorUIApplication : EditorApplication
    {
        private MainWindow mainWindow;
        private EditorActionsProvider actionsProvider;

        Dispatcher dispatcher;

        public Dispatcher Dispatcher
        {
            get { return dispatcher; }
        }

        private ApplicationSettingsXml settings;

        public ApplicationSettingsXml Settings
        {
            get { return settings; }
        }

        public SearchBindingProvider Search { get; private set; }

        public EditorUIApplication(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            dispatcher = mainWindow.Dispatcher;

            Search = new SearchBindingProvider(this);

            NewFile = new GenericCommand((p) => ActionNewFile(p));

            OpenFile = new GenericCommand((p) => ActionOpenFile(p));

            SaveAsFile = new GenericCommand((p) => ActionSaveAsFile(p));
            SaveFile = new GenericCommand((p) => ActionSaveFile(p));

            Quit = new GenericCommand((p) => ActionQuit(p));

            About = new GenericCommand((p) => ActionAbout(p));

            LoadContextCommands();

            Builder.PropertyChanged += new PropertyChangedEventHandler(Builder_PropertyChanged);

            Build = new GenericCommand((p) => { Builder.ActionBuild(); }, (p) => Builder.CanBuild);
            Rebuild = new GenericCommand((p) => { Builder.ActionRebuildAll(); }, (p) => Builder.CanBuild);
            CancelBuild = new GenericCommand((p) => { Builder.ActionCancelBuild(); }, (p) => !Builder.CanBuild);

            OpenRecent = new GenericCommand(ActionOpenRecent);

            LoadApplicationSettings();

            Build_OutputCommands = new ObservableCollection<GenericCommand>();
            Build_OutputCommands.Add(new GenericCommand(Build_Action_CopySelection, (item) => SelectedBuildMessage != null) { DisplayName = "Copy Selection" });
            Build_OutputCommands.Add(new GenericCommand(Build_Action_CopyAll) { DisplayName = "Copy All" });
            Build_OutputCommands.Add(new GenericCommand(Build_Action_SaveAll) { DisplayName = "Save to file" });

            SelectedBuildMessage = new BuildMessage();
            Builder.BuildOutput.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(BuildOutput_CollectionChanged);
        }

        #region Build Helpers

        public ObservableCollection<GenericCommand> Build_OutputCommands { get; private set; }

        private BuildMessage selectedBuildMessage;

        public BuildMessage SelectedBuildMessage
        {
            get { return selectedBuildMessage; }
            set
            {
                selectedBuildMessage = value;

                foreach (var command in Build_OutputCommands)
                    command.Refresh();

                RaisePropertyChanged("SelectedBuildMessage");
            }
        }

        public void Build_Action_CopySelection(object parameter)
        {
            if (selectedBuildMessage != null)
                Clipboard.SetText(selectedBuildMessage.ToString());
        }

        public void Build_Action_CopyAll(object parameter)
        {
            if (Builder.BuildOutput.Count > 0)
                Clipboard.SetText(String.Concat(from message in Builder.BuildOutput select (message.ToString() + "\r\n")));
        }

        public void Build_Action_SaveAll(object parameter)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt";
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var stream = new FileStream(sfd.Filter, FileMode.Create))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            foreach (var message in Builder.BuildOutput)
                            {
                                writer.WriteLine(message.ToString());
                            }

                            writer.Flush();
                        }
                        stream.Flush();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void BuildOutput_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        SelectedBuildMessage = null;
                        break;
                    }
            }
        }

        #endregion

        #region Application Settings
        public GenericCommand OpenRecent { get; private set; }
        public void ActionOpenRecent(object paramater)
        {
            string recentPath = paramater as string;

            if (!String.IsNullOrEmpty(recentPath))
            {
                if (ActionClose())
                {
                    UserOpenFile(recentPath);
                }
            }
        }

        public void LoadApplicationSettings()
        {
            settings = new ApplicationSettingsXml();
            try
            {
                var settingsFile = UISettingsLocation;

                if (File.Exists(settingsFile))
                {
                    using (var stream = new FileStream(settingsFile, FileMode.Open, FileAccess.Read))
                    {
                        settings = (ApplicationSettingsXml)ApplicationSettingsXml.Serializer.Deserialize(stream);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);

                settings = new ApplicationSettingsXml();
            }
        }

        public void SaveApplicationSettings()
        {
            settings.CapRecents();

            try
            {
                var settingsFile = UISettingsLocation;

                using (var stream = new FileStream(settingsFile, FileMode.Create))
                {
                    ApplicationSettingsXml.Serializer.Serialize(stream, settings);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        #endregion

        void Builder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var property = e.PropertyName;

            if (String.IsNullOrEmpty(property) || "CanBuild".Equals(property))
                RefreshBuildCommands();
        }

        #region Build Commands

        public GenericCommand Build { get; private set; }
        public GenericCommand Rebuild { get; private set; }
        public GenericCommand CancelBuild { get; private set; }

        private void RefreshBuildCommands()
        {
            mainWindow.Dispatcher.Invoke((Action)delegate
            {
                Build.Refresh();
                Rebuild.Refresh();
                CancelBuild.Refresh();
            });
        }

        #endregion

        #region Helper

        public override void Refresh()
        {
            Search.SearchResult.Clear();

            base.Refresh();
        }

        public override void WrapAsyncAction(Action action)
        {
            if (action != null && dispatcher != null)
            {
                dispatcher.Invoke(action);
            }
        }

        protected override void RaisePropertyChanged(string property)
        {
            if (dispatcher != null)
            {
                if (dispatcher.Thread != System.Threading.Thread.CurrentThread)
                    dispatcher.BeginInvoke((Action)delegate
                    {
                        RaisePropertyChanged(property);
                    });
                else
                {
                    base.RaisePropertyChanged(property);

                    if (String.IsNullOrEmpty(property) || "CurrentPackagePath".Equals(property))
                        RaisePropertyChanged("Title");
                }
            }
        }

        #endregion

        #region Title

        public string Title
        {
            get
            {
                return String.Format("{0} - Cirrus Package Editor", String.IsNullOrEmpty(CurrentPackagePath) ? "Untitled" : Path.GetFileNameWithoutExtension(CurrentPackagePath));
            }
        }

        #endregion

        #region Menu
        public bool ActionClose(object parameter = null)
        {
            if (Builder.Building)
            {
                MessageBox.Show("A build is in progress. Please wait for it to complete or cancel the build process", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                Search.Stop();

                switch (MessageBox.Show("Do you want to save your package before closing ?", "Closing", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning))
                {
                    default:
                    case MessageBoxResult.Cancel:
                        return false;
                    case MessageBoxResult.No:
                        return true;
                    case MessageBoxResult.Yes:
                        return ActionSaveFile();
                }
            }
        }

        public GenericCommand About { get; private set; }
        public void ActionAbout(object parameter = null)
        {
            var aboutDlg = new AboutWindow();
            aboutDlg.ShowDialog();
        }

        public GenericCommand NewFile { get; private set; }
        public void ActionNewFile(object parameter = null)
        {
            if (ActionClose())
                InitializeNew();
        }

        public GenericCommand OpenFile { get; private set; }
        public void ActionOpenFile(object parameter = null)
        {
            if (ActionClose())
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = CirrusDesignHelper.CirrusPackageDialogFilter;
                if (ofd.ShowDialog() == true)
                {
                    string fileName = ofd.FileName;

                    UserOpenFile(fileName);
                }
            }
        }

        public bool UserOpenFile(string fileName)
        {
            try
            {
                LoadPackage(fileName);


                settings.AppendToRecent(fileName);
                SaveApplicationSettings();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }

        public GenericCommand SaveFile { get; private set; }
        public bool ActionSaveFile(object parameter = null)
        {
            if (!String.IsNullOrEmpty(CurrentPackagePath))
            {
                try
                {
                    if (SavePackage(CurrentPackagePath))
                    {
                        settings.AppendToRecent(CurrentPackagePath);
                        SaveApplicationSettings();

                        return true;
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return ActionSaveAsFile();
                }
            }
            else
                return ActionSaveAsFile();
        }

        public GenericCommand SaveAsFile { get; private set; }
        public bool ActionSaveAsFile(object parameter = null)
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
                bool succeded = ActionSaveFile();

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
        public bool ActionQuit(object parameter = null)
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
                var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sora", "Cirrus", "Editor.UI.Settings.xml");
                var settingsDirectory = Path.GetDirectoryName(settingsPath);

                var dirInfo = new DirectoryInfo(settingsDirectory);

                if (!dirInfo.Exists)
                    dirInfo.Create();

                return settingsPath;
            }
        }
        
        public string UILayoutLocation
        {
            get
            {
                var layoutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sora", "Cirrus", "Editor.UI.Config.xml");
                var layoutDirectory = Path.GetDirectoryName(layoutPath);

                var dirInfo = new DirectoryInfo(layoutDirectory);

                if (!dirInfo.Exists)
                    dirInfo.Create();

                return layoutPath;
            }
        }

        #endregion

        #region Context Commands

        private void LoadContextCommands()
        {
            actionsProvider = new EditorActionsProvider(this);
            actionsProvider.Load(contextCommands);
        }

        private ObservableCollection<GenericCommand> contextCommands = new ObservableCollection<GenericCommand>();

        public ObservableCollection<GenericCommand> ContextCommands
        {
            get { return contextCommands; }
            private set { contextCommands = value; }
        }

        public void RefreshContextCommands()
        {
            foreach (var command in contextCommands)
                command.Refresh();

            RaisePropertyChanged("ContextCommands");
        }

        #endregion
    }
}
