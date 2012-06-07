using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sora.GameEngine.Cirrus.Design.Packages;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorApplication : INotifyPropertyChanged
    {
        public CirrusDesignHelper Helper { get; private set; }

        public EditorApplication()
        {
            InitializeNew();
        }

        #region Loading and Saving Package

        public void LoadPackage(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var package = LoadPackage(stream);

                CurrentPackagePath = fileName;
                CurrentPackage = package;

                Refresh();
            }
        }

        private XmlCirrusPackage LoadPackage(Stream stream)
        {
            return (XmlCirrusPackage)XmlCirrusPackage.Serializer.Deserialize(stream);
        }

        private static EditorApplication LoadAndCreatePackageFromStream(Stream stream, string associatedFile)
        {
            var app = new EditorApplication();
            
            var x_package = app.LoadPackage(stream);
            
            app.CurrentPackage = x_package;
            app.CurrentPackagePath = associatedFile;

            app.Refresh();

            return app;
        }

        public void SavePackage(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                SavePackage(stream);
            }
        }

        private void SavePackage(Stream stream)
        {
            XmlCirrusPackage.Serializer.Serialize(stream, CurrentPackage);
        }

        public void InitializeNew()
        {
            InitializeObjects();

            currentPackagePath = String.Empty;
            currentPackage = new XmlCirrusPackage();

            LoadDefaultPackageProperties();

            PackageContainer[0].RefreshContent();
        }

        private void InitializeObjects()
        {
            Helper = new CirrusDesignHelper();

            PackageReferences = new EditorBaseBoundObject[]{
                new EditorXNAReferencesObject(this),
                new EditorPackageReferencesObject(this)};

            PackageContainer = new[] { new EditorPackageContainerObject(this) };
        }

        /// <summary>
        /// Initialize common properties for a new package
        /// </summary>
        public void LoadDefaultPackageProperties()
        {
            currentPackage.Name = "Untitled";
            currentPackage.RootDirectory = String.Empty;

            var asm_list = (from asm in Helper.GetDefaultXNAReferences() select asm).Distinct().ToArray();

            foreach (var asm in asm_list)
                currentPackage.XNAReferences.Add(asm);
        }

        #endregion

        #region Selection UI Helper

        object[] selectionForProperties = new object[0];

        /// <summary>
        /// Collection used to select properties for the properties editor
        /// </summary>
        public object[] SelectionForProperties
        {
            get
            {
                if (selectionForProperties == null)
                    return selectionForProperties = new object[0];
                else
                    return selectionForProperties;
            }
            set
            {
                if (selectionForProperties != value)
                {
                    selectionForProperties = value;
                    RaisePropertyChanged("SelectionForProperties");
                }
            }
        }

        #endregion

        #region Package management

        private string currentPackagePath;

        public string CurrentPackagePath
        {
            get { return currentPackagePath; }
            set
            {
                currentPackagePath = value;
                RaisePropertyChanged("CurrentPackagePath");
            }
        }

        private XmlCirrusPackage currentPackage;

        public XmlCirrusPackage CurrentPackage
        {
            get { return currentPackage; }
            set
            {
                currentPackage = value;
                RaisePropertyChanged("CurrentPackage");
            }
        }

        public void Refresh()
        {
            SelectionForProperties = new object[0];

            InitializeObjects();
            PackageContainer[0].RefreshContent();

            RaisePropertyChanged("CurrentPackage");
            RaisePropertyChanged("PackageContainer");
        }

        private EditorPackageContainerObject[] packageContainer;

        public EditorPackageContainerObject[] PackageContainer
        {
            get { return packageContainer; }
            private set
            {
                packageContainer = value;
                RaisePropertyChanged("PackageContainer");
            }
        }

        #endregion

        #region References

        public EditorBaseBoundObject[] PackageReferences { get; private set; }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        #region Generic UI Helpers

        public event EventHandler RefreshPropertiesViewRequested;

        public void RefreshPropertiesView()
        {
            if (RefreshPropertiesViewRequested != null)
                RefreshPropertiesViewRequested(this, EventArgs.Empty);
        }

        #endregion

        #region Build

        #region Build Helpers

        protected virtual void Build_Message(string msg, string source = "", BuildMessageSeverity severity = BuildMessageSeverity.Information)
        {
            Debug.WriteLine(String.Format("[{0}] {1} -- {2}", severity, msg, source));

            BuildOutput.Add(new BuildMessage() { Message = msg, Source = source, Severity = severity, Order = BuildOutput.Count });
        }

        private bool Build_CheckConditions()
        {
            if (String.IsNullOrEmpty(CurrentPackagePath))
            {
                Build_Message("You have to save the package first before building it");
                return false;
            }

            return true;
        }

        private void Build_WrapBuildProcessTaskAsync(Func<bool> buildTask)
        {
            Building = true;
            CanBuild = false;
            cancellationPending = false;
            BuildSucceeded = null;

            Task.Factory.StartNew(delegate
            {

                try
                {
                    try
                    {
                        BuildSucceeded = buildTask();
                    }
                    catch (Exception ex)
                    {
                        Build_Message(ex.ToString(), "BuildTask", BuildMessageSeverity.Error);
                        BuildSucceeded = false;
                    }

                    if (BuildSucceeded == true)
                        Build_Message("Compilation success", "BuildTask", BuildMessageSeverity.Information);
                    else
                        Build_Message("Compilation failed", "BuildTask", BuildMessageSeverity.Error);
                }
                finally
                {
                    CanBuild = true;
                    Building = false;
                }
            });
        }

        /// <summary>
        /// To prevent altering of a package while being built, we work on a copy
        /// </summary>
        /// <returns></returns>
        private EditorApplication Build_GetPackageCopy()
        {
            Build_Message("Preparing package for build");

            using (var tempStream = new MemoryStream())
            {
                SavePackage(tempStream);
                tempStream.Position = 0;

                return LoadAndCreatePackageFromStream(tempStream, CurrentPackagePath);
            }
        }

        #endregion

        #region Build Enumerators

        private bool Build_ActionForAllFiles(EditorApplication buildPackageApplication, Func<EditorContentFile, bool> action)
        {
            if (action != null)
            {
                /* we process files level by level to ensure that lower level files will be evaluated after all
                 * high level files
                 */
                var enumeratorQueue = new Queue<EditorContentObject>();

                /* first, we add the root files */
                enumeratorQueue.Enqueue(from obj in buildPackageApplication.PackageContainer[0].Content where obj is EditorContentFile select (EditorContentFile)obj);

                /* then, root directories */
                enumeratorQueue.Enqueue(from obj in buildPackageApplication.PackageContainer[0].Content where obj is EditorContentDirectory select (EditorContentDirectory)obj);

                /* and here we go */
                while (enumeratorQueue.Count > 0)
                {
                    var current = enumeratorQueue.Dequeue();
                    var as_file = current as EditorContentFile;
                    var as_directory = current as EditorContentDirectory;

                    if (as_directory != null)
                    {
                        Build_Message(String.Format("Directory - {0}", as_directory.RelativePath), "Build_EnumFiles", BuildMessageSeverity.Information);

                        enumeratorQueue.Enqueue(from element in as_directory.Content select (EditorContentObject)element);

                        if (!as_directory.IsValid)
                            Build_Message(String.Format("Directory {0} was marked as invalid", as_directory.RelativePath), "Build_EnumFiles", BuildMessageSeverity.Warning);
                    }
                    else if (as_file != null)
                    {
                        Build_Message(String.Format("File - {0}", as_file.RelativePath), "Build_EnumFiles", BuildMessageSeverity.Information);

                        if (cancellationPending)
                        {
                            Build_Message("Compilation aborted", "Build_EnumFiles", BuildMessageSeverity.Error);
                            return false;
                        }

                        if (!as_file.IsValid)
                            Build_Message(String.Format("File {0} was marked as invalid", as_file.RelativePath), "Build_EnumFiles", BuildMessageSeverity.Warning);
                        else
                        {
                            var process_result = action(as_file);

                            if (!process_result)
                            {
                                Build_Message(String.Format("File {0} aborted the compilation", as_file.RelativePath), "Build_EnumFiles", BuildMessageSeverity.Error);
                                return false;
                            }
                        }
                    }

                    if (cancellationPending)
                    {
                        Build_Message("Compilation aborted", "Build_EnumFiles", BuildMessageSeverity.Error);
                        return false;
                    }
                }

                return true;
            }
            else
                return true;
        }

        #endregion

        private bool canBuild = true;

        public bool CanBuild
        {
            get { return canBuild; }
            set
            {
                canBuild = value;
                RaisePropertyChanged("CanBuild");
                RaisePropertyChanged("Building");
            }
        }

        private bool building = false;

        public bool Building
        {
            get { return building; }
            set
            {
                building = value;
                RaisePropertyChanged("CanBuild");
                RaisePropertyChanged("Building");
            }
        }

        private ObservableCollection<BuildMessage> buildOutput = new ObservableCollection<BuildMessage>();

        public ObservableCollection<BuildMessage> BuildOutput
        {
            get { return buildOutput; }
            private set
            {
                buildOutput = value;
                RaisePropertyChanged("BuildOutput");
            }
        }

        private bool? buildSucceeded;

        public bool? BuildSucceeded
        {
            get { return buildSucceeded; }
            set { buildSucceeded = value;
            RaisePropertyChanged("BuildSucceeded");
            }
        }


        public void ActionBuild()
        {
            if (CanBuild)
            {
                BuildOutput.Clear();
                if (Build_CheckConditions())
                {
                    var packageCopy = Build_GetPackageCopy();
                    Build_WrapBuildProcessTaskAsync(delegate
                    {
                        return Build_ActionForAllFiles(packageCopy, (file) =>
                        {
                            Build_Message(file.RelativePath);
                            return true;
                        });
                    });
                }
            }
        }

        public void ActionBuildAll()
        {
            if (CanBuild)
            {
                BuildOutput.Clear();
                if (Build_CheckConditions())
                {
                    var packageCopy = Build_GetPackageCopy();
                    Build_WrapBuildProcessTaskAsync(delegate
                    {
                        return Build_ActionForAllFiles(packageCopy, (file) =>
                        {
                            Build_Message(file.RelativePath);
                            return true;
                        });
                    });
                }
            }
        }

        public void ActionRebuildAll()
        {
            if (CanBuild)
            {
                BuildOutput.Clear();
                if (Build_CheckConditions())
                {
                    var packageCopy = Build_GetPackageCopy();
                    Build_WrapBuildProcessTaskAsync(delegate
                    {
                        return Build_ActionForAllFiles(packageCopy, (file) =>
                        {
                            Build_Message(file.RelativePath);
                            return true;
                        });
                    });
                }
            }
        }

        bool cancellationPending = false;

        public void ActionCancelBuild()
        {
            cancellationPending = true;
        }


        #endregion
    }
}
