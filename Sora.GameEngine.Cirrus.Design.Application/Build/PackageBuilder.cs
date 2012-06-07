using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Reflection;
using Sora.GameEngine.Cirrus.Design.Application.Editor;
using Sora.GameEngine.Cirrus.Design.Application.Helpers;

namespace Sora.GameEngine.Cirrus.Design.Application.Build
{
    public class PackageBuilder : INotifyPropertyChanged
    {
        public EditorApplication Editor { get; private set; }

        public PackageBuilder(EditorApplication editor)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");

            Editor = editor;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        #region Build

        #region Build Helpers

        protected virtual void Build_Message(string msg, string source = "", BuildMessageSeverity severity = BuildMessageSeverity.Information)
        {
            Editor.WrapAsyncAction(delegate
            {
                Debug.WriteLine(String.Format("[{0}] {1} -- {2}", severity, msg, source));

                BuildOutput.Add(new BuildMessage() { Message = msg, Source = source, Severity = severity, Order = BuildOutput.Count });
            });
        }

        internal void InternalBuild_Message(string msg, string source = "", BuildMessageSeverity severity = BuildMessageSeverity.Information)
        {
            Build_Message(msg, source, severity);
        }

        private bool Build_CheckConditions()
        {
            if (String.IsNullOrEmpty(Editor.CurrentPackagePath))
            {
                Build_Message("You have to save the package first before building it");
                return false;
            }

            return true;
        }

        private void Build_WrapBuildProcessTaskAsync(EditorApplication packageCopy, Func<bool> buildTask)
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
                        cachedReferencesAssemblies = Editor.Helper.GetXNAAssembliesDescriptors(packageCopy.CurrentPackage.XNAReferences);

                        XNAContextInit(packageCopy);

                        BuildSucceeded = buildTask();
                    }
                    catch (FatalBuildErrorException)
                    {
                        BuildSucceeded = false;
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
                    XNAContextDispose();

                    CanBuild = true;
                    Building = false;

                    cachedReferencesAssemblies = null;
                    cachedProcessors.Clear();
                    cachedImporters.Clear();
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
                Editor.SavePackage(tempStream);
                tempStream.Position = 0;

                return Editor.LoadAndCreatePackageFromStream(tempStream, Editor.CurrentPackagePath);
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
            set
            {
                buildSucceeded = value;
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
                    Build_WrapBuildProcessTaskAsync(packageCopy, delegate
                    {
                        return Build_ActionForAllFiles(packageCopy, (file) =>
                        {
                            Build_ProcessFile(packageCopy, file);
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
                    Build_WrapBuildProcessTaskAsync(packageCopy, delegate
                    {
                        return Build_ActionForAllFiles(packageCopy, (file) =>
                        {
                            Build_ProcessFile(packageCopy, file);
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
                    Build_WrapBuildProcessTaskAsync(packageCopy, delegate
                    {
                        return Build_ActionForAllFiles(packageCopy, (file) =>
                        {
                            Build_ProcessFile(packageCopy, file);
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

        #region Building operations

        #region XNA Cheat Sheet

        private static string WriteAsset(string assetname, object target, ContentProcessorContext context, bool compress = true)
        {
            var constructors = typeof(ContentCompiler).GetConstructors(
                                          BindingFlags.NonPublic | BindingFlags.Instance);
            var compileContent = typeof(ContentCompiler).GetMethod(
                                           "Compile",
                                          BindingFlags.NonPublic | BindingFlags.Instance);

            var compiler = constructors[0].Invoke(null) as ContentCompiler;
            string outputname = CleanPath(context.OutputDirectory + assetname + ".xnb");
            context.AddOutputFile(outputname);

            var dir = Path.GetDirectoryName(outputname);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var stream = new FileStream(outputname, FileMode.Create))
            {

                var param = new object[]   
                    {   
                        stream,      
                        target,       
                        context.TargetPlatform,  
                        context.TargetProfile, 
                        compress,
                        context.OutputDirectory,   
                        context.OutputDirectory   
                    };


                compileContent.Invoke(compiler, param);
            }

            return outputname;
        }

        private static string CleanPath(string p)
        {
            if (String.IsNullOrEmpty(p))
                return String.Empty;
            else
            {
                while (p.IndexOf(@"\\") >= 0)
                    p = p.Replace(@"\\", @"\");

                return p;
            }
        }

        #endregion

        private string GetSourcePathForFile(EditorApplication packageCopy, EditorContentFile file)
        {
            var contentBaseDirectory = GetContentBaseDirectory(packageCopy);

            var inputCompletePath = Path.Combine(contentBaseDirectory, file.RelativePath);

            var normalizedInputCompletePath = new FileInfo(inputCompletePath).FullName;

            return normalizedInputCompletePath;
        }

        private string GetContentBaseDirectory(EditorApplication packageCopy)
        {
            var rootDirectory = Path.GetDirectoryName(packageCopy.CurrentPackagePath);

            var contentBaseDirectory = Path.IsPathRooted(packageCopy.CurrentPackage.RootDirectory)
                ? packageCopy.CurrentPackage.RootDirectory
                : Path.Combine(rootDirectory, packageCopy.CurrentPackage.RootDirectory);
            return contentBaseDirectory;
        }

        private string GetOutputPathForFile(EditorApplication packageCopy, EditorContentFile file)
        {
            var rootDirectory = Path.GetDirectoryName(packageCopy.CurrentPackagePath);

            var outputBaseDirectory = Path.IsPathRooted(packageCopy.CurrentPackage.OutputDirectory)
                ? packageCopy.CurrentPackage.OutputDirectory
                : Path.Combine(rootDirectory, packageCopy.CurrentPackage.OutputDirectory);

            outputBaseDirectory += "\\" + CirrusContentManager.OutputDirectorySuffix;

            var outputCompletePath = Path.Combine(outputBaseDirectory, file.RelativePath);

            var normalizedOutputCompletePath = Path.ChangeExtension(new FileInfo(outputCompletePath).FullName, CirrusContentManager.ContentFileExtention);

            return normalizedOutputCompletePath;
        }

        private void Build_ProcessFile(EditorApplication packageCopy, EditorContentFile file)
        {
#warning TODO file caching depending on the build mode (using hash and so on)
            var src = GetSourcePathForFile(packageCopy, file);
            var dst = GetOutputPathForFile(packageCopy, file);

            switch (file.BuildAction)
            {
                default:
                case Sora.GameEngine.Cirrus.Design.Packages.XmlBuildAction.None:
                    {
                        Build_Message("Ignored", "ProcessFile");
                        break;
                    }
                case Sora.GameEngine.Cirrus.Design.Packages.XmlBuildAction.Compile:
                    {
                        var importer = file.Importer;
                        var processor = file.Processor;

                        if (String.IsNullOrEmpty(importer) && String.IsNullOrEmpty(processor))
                        {
                            Build_Message("No importer or processor specified, copying to output");
                            goto case Sora.GameEngine.Cirrus.Design.Packages.XmlBuildAction.CopyToOutput;
                        }
                        else
                        {
                            Build_Message(String.Format("Compiling {0} to {1}", src, dst), "ProcessFile");
                            XNAFileCompile(packageCopy, file, src, dst);
                        }
                        break;
                    }
                case Sora.GameEngine.Cirrus.Design.Packages.XmlBuildAction.CopyToOutput:
                    {
                        Build_Message(String.Format("Copying {0} to {1}", src, dst), "ProcessFile");

                        var outDirectory = Path.GetDirectoryName(dst);
                        if (!Directory.Exists(outDirectory))
                            Directory.CreateDirectory(outDirectory);

                        File.Copy(src, dst, true);
                        break;
                    }
            }
        }

        XNAAssemblyDescription[] cachedReferencesAssemblies;
        Dictionary<string, IContentImporter> cachedImporters = new Dictionary<string, IContentImporter>();
        Dictionary<string, IContentProcessor> cachedProcessors = new Dictionary<string, IContentProcessor>();

        private IEnumerable<XNAContentImporterDescription> AvailableImporters
        {
            get
            {
                foreach (var item in cachedReferencesAssemblies)
                {
                    foreach (var importer in item.Importers)
                        yield return importer;
                }
            }
        }

        private IContentImporter XNAGetImporterForFile(EditorApplication packageCopy, EditorContentFile file)
        {
            var importerName = file.Importer;

            if (String.IsNullOrEmpty(importerName))
            {
                Build_Message("Cannot import file because no importer is defined", "GetImporter", BuildMessageSeverity.Error);
                throw new FatalBuildErrorException();
            }
            else
            {
                IContentImporter result;

                /* First, look in the cache */
                if (cachedImporters.TryGetValue(importerName, out result))
                    return result;
                else
                {
                    /* Then, try finding the descriptor */
                    var match = (from importer in AvailableImporters where importer.Name == importerName select importer).ToArray();
                    if (match == null || match.Length < 1)
                    {
                        Build_Message(String.Format("Cannot find an importer matching the name {0}", importerName), "GetImporter", BuildMessageSeverity.Error);
                        throw new FatalBuildErrorException();
                    }
                    else if (match.Length > 1)
                    {
                        Build_Message(String.Format("Several importers found for the name {0}", importerName), "GetImporter", BuildMessageSeverity.Error);
                        throw new FatalBuildErrorException();
                    }
                    else
                    {
                        Build_Message(String.Format("Creating an instance of the importer {0}", importerName), "GetImporter");
                        var first_match_type = match.First().Type;

                        var constructor = first_match_type.GetConstructor(new Type[0]);
                        if (constructor == null)
                        {
                            Build_Message("Cannot find a parameterless constructor for the importer", "GetImporter", BuildMessageSeverity.Error);
                            throw new FatalBuildErrorException();
                        }
                        else
                        {
                            return cachedImporters[importerName] = (IContentImporter)constructor.Invoke(new object[0]);
                        }
                    }
                }
            }
        }

        private IEnumerable<XNAContentProcessorDescription> AvailableProcessors
        {
            get
            {
                foreach (var item in cachedReferencesAssemblies)
                {
                    foreach (var processor in item.Processors)
                        yield return processor;
                }
            }
        }

        private IContentProcessor XNAGetProcessorForFile(EditorApplication packageCopy, EditorContentFile file)
        {
            var processorName = file.Processor;

            if (String.IsNullOrEmpty(processorName))
            {
                Build_Message("Cannot process file because no processor is defined", "GetProcessor", BuildMessageSeverity.Error);
                throw new FatalBuildErrorException();
            }
            else
            {
                IContentProcessor result;

                /* First, look in the cache */
                if (cachedProcessors.TryGetValue(processorName, out result))
                    return result;
                else
                {
                    /* Then, try finding the descriptor */
                    var match = (from processor in AvailableProcessors where processor.Name == processorName select processor).ToArray();
                    if (match == null || match.Length < 1)
                    {
                        Build_Message(String.Format("Cannot find an processor matching the name {0}", processorName), "GetProcessor", BuildMessageSeverity.Error);
                        throw new FatalBuildErrorException();
                    }
                    else if (match.Length > 1)
                    {
                        Build_Message(String.Format("Several processors found for the name {0}", processorName), "GetProcessor", BuildMessageSeverity.Error);
                        throw new FatalBuildErrorException();
                    }
                    else
                    {
                        Build_Message(String.Format("Creating an instance of the processor {0}", processorName), "GetProcessor");
                        var first_match_type = match.First().Type;

                        var constructor = first_match_type.GetConstructor(new Type[0]);
                        if (constructor == null)
                        {
                            Build_Message("Cannot find a parameterless constructor for the processor", "GetProcessor", BuildMessageSeverity.Error);
                            throw new FatalBuildErrorException();
                        }
                        else
                        {
                            return cachedProcessors[processorName] = (IContentProcessor)constructor.Invoke(new object[0]);
                        }
                    }
                }
            }
        }

        internal BuildLogger XNALogger { get; private set; }
        internal ContentImporterContext XNAContentImporterContext { get; private set; }
        internal ContentProcessorContext XNAContentProcessorContext { get; private set; }

        internal string XNAIntermediateDirectory { get; private set; }
        internal string XNAOutputDirectory { get; private set; }

        private void XNAContextInit(EditorApplication packageCopy)
        {
            var rootDirectory = Path.GetDirectoryName(packageCopy.CurrentPackagePath);

            var outputBaseDirectory = Path.IsPathRooted(packageCopy.CurrentPackage.OutputDirectory)
                ? packageCopy.CurrentPackage.OutputDirectory
                : Path.Combine(rootDirectory, packageCopy.CurrentPackage.OutputDirectory);

            XNAOutputDirectory = Path.Combine(outputBaseDirectory, CirrusContentManager.OutputDirectorySuffix);
            XNAIntermediateDirectory = Path.Combine(outputBaseDirectory, "ContentIntermediate");

            XNALogger = new BuildLogger(this);
            XNAContentImporterContext = new CustomContentImporterContext(this);
        }

        private void XNAContextDispose()
        {
            XNAContentProcessorContext = null;
            XNAContentProcessorContext = null;
            xnaProcessedAbsoluteFilesPath.Clear();
            xnaPassDetectedDependency.Clear();
        }

        /// <summary>
        /// Complete list of processed files by the compilation process
        /// </summary>
        private List<string> xnaProcessedAbsoluteFilesPath = new List<string>();

        private List<string> xnaPassDetectedDependency = new List<string>();

        internal void XNAAddDependency(string fileName)
        {
            xnaPassDetectedDependency.Add(fileName);
        }

        private void XNAFileCompile(EditorApplication packageCopy, EditorContentFile file, string src, string dst)
        {
            xnaPassDetectedDependency.Clear();

            #region Process File Content

            if (xnaProcessedAbsoluteFilesPath.Contains(src, StringComparer.OrdinalIgnoreCase))
            {
                /* already processed */
                return;
            }
            else
            {
                xnaProcessedAbsoluteFilesPath.Add(src);

                var importer = XNAGetImporterForFile(packageCopy, file);

                var importedObj = importer.Import(file.CurrentPath, XNAContentImporterContext);
            }

            #endregion

            #region Process Added Dependencies

            if (xnaPassDetectedDependency.Count > 0)
            {
                var dependencyPath = xnaPassDetectedDependency[0];

                var newEditorFileForDependency = new EditorContentFile(Editor, dependencyPath,
                    GetContentBaseDirectory(packageCopy), Path.Combine(GetContentBaseDirectory(packageCopy), dependencyPath));

                xnaPassDetectedDependency.RemoveAt(0);

                Build_ProcessFile(packageCopy, newEditorFileForDependency);
            }

            #endregion
        }

        #endregion
    }
}
