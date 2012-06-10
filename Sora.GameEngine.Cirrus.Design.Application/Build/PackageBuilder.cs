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
using Microsoft.Xna.Framework.Content.Pipeline.Tasks;
using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework.Graphics;

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

        private Action<string, string, BuildMessageSeverity> build_message_redirection = null;

        protected virtual void Build_Message(string msg, string source = "", BuildMessageSeverity severity = BuildMessageSeverity.Information)
        {
            if (build_message_redirection != null)
                build_message_redirection(msg, source, severity);
            else
            {
                Editor.WrapAsyncAction(delegate
                {
                    Debug.WriteLine(String.Format("[{0}] {1} -- {2}", severity, msg, source));

                    BuildOutput.Add(new BuildMessage() { Message = msg, Source = source, Severity = severity, Order = BuildOutput.Count });
                });
            }
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

        private void Build_WrapBuildProcessTaskAsync(EditorApplication packageCopy, string contextId, Func<bool> buildTask)
        {
            Building = true;
            CanBuild = false;
            cancellationPending = false;
            BuildSucceeded = null;

            Editor.Status = "Building ...";

            System.Threading.Tasks.Task.Factory.StartNew(delegate
            {
                try
                {
                    try
                    {
                        XNAContextInit(packageCopy, contextId);

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
                        Build_Message("Build success", "BuildTask", BuildMessageSeverity.Information);
                    else
                        Build_Message("Build failed", "BuildTask", BuildMessageSeverity.Error);
                }
                finally
                {
                    XNAContextDispose();

                    CanBuild = true;
                    Building = false;

                    Editor.Status = BuildSucceeded == true ? "Build success" : "Build failed";
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
                /* We process files using depth traversal to ensure that lower level files will be evaluated before all
                 * high level files
                 * 
                 * This allows to build Levels depending on sub components placed below in the graph
                 */
                var enumeratorStack = new Stack<EditorContentObject>();

                /* we add the root files */
                enumeratorStack.Push(from obj in buildPackageApplication.PackageContainer[0].Content where obj is EditorContentFile select (EditorContentFile)obj);

                /* root directories */
                enumeratorStack.Push(from obj in buildPackageApplication.PackageContainer[0].Content where obj is EditorContentDirectory select (EditorContentDirectory)obj);


                /* and here we go */
                while (enumeratorStack.Count > 0)
                {
                    var current = enumeratorStack.Pop();
                    var as_file = current as EditorContentFile;
                    var as_directory = current as EditorContentDirectory;

                    if (as_directory != null)
                    {
                        Build_Message(String.Format("Directory - {0}", as_directory.RelativePath), "Build_EnumFiles", BuildMessageSeverity.Information);
                        
                        /* sub files */
                        enumeratorStack.Push(from element in as_directory.Content where element is EditorContentFile select (EditorContentObject)element);

                        /* sub directories */
                        enumeratorStack.Push(from element in as_directory.Content where element is EditorContentDirectory orderby ((EditorContentDirectory)element).BuildOrder select (EditorContentObject)element);


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

        /// <summary>
        /// Build changed content
        /// </summary>
        /// <param name="contextId">contextId is used to isolate sub build configurations of the same package</param>
        /// <param name="filesFilter">Filter that can be used to filter out of the build process files from the main package AND from it dependencies</param>
        public void ActionBuild(string contextId = null, Predicate<EditorContentFile> filesFilter = null)
        {
            if (CanBuild)
            {
                BuildOutput.Clear();
                if (Build_CheckConditions())
                {
                    var packageCopy = Build_GetPackageCopy();
                    Build_WrapBuildProcessTaskAsync(packageCopy, contextId, delegate
                    {
                        return Build_Execute(packageCopy, contextId, false, packageCopy.CurrentPackage.CompressContent, filesFilter);
                    });
                }
            }
        }

        /// <summary>
        /// Rebuild content
        /// </summary>
        /// <param name="contextId">contextId is used to isolate sub build configurations of the same package</param>
        /// <param name="filesFilter">Filter that can be used to filter out of the build process files from the main package AND from it dependencies</param>
        public void ActionRebuildAll(string contextId = null, Predicate<EditorContentFile> filesFilter = null)
        {
            if (CanBuild)
            {
                BuildOutput.Clear();
                if (Build_CheckConditions())
                {
                    var packageCopy = Build_GetPackageCopy();
                    Build_WrapBuildProcessTaskAsync(packageCopy, contextId, delegate
                    {
                        return Build_Execute(packageCopy, contextId, true, packageCopy.CurrentPackage.CompressContent, filesFilter);
                    });
                }
            }
        }

        internal bool cancellationPending = false;

        public void ActionCancelBuild()
        {
            cancellationPending = true;

            if (Building)
                Editor.Status = "Cancelling ...";
        }


        #endregion

        #region Helpers

        public string ParseReferencePath(string referenceName)
        {
            return ParseReferencePath(Editor, referenceName);
        }

        private string ParseReferencePath(EditorApplication packageCopy, string referenceName)
        {
            /* check to see if the reference is a local file */
            try
            {
                var fi = new FileInfo(Path.Combine(Path.GetDirectoryName(packageCopy.CurrentPackagePath), referenceName));
                if (fi.Exists)
                    return fi.FullName;
            }
            catch { }

            /* nope */
            return referenceName;
        }

        #endregion

        #region Building operations

        private bool Build_Execute(EditorApplication packageCopy, string contextId, bool rebuild, bool compress, Predicate<EditorContentFile> filesFilter = null, List<string> callTree = null, List<string> builtPackages = null)
        {
            /* callTree is used to identify circular references */
            if (callTree == null)
                callTree = new List<string>();

            /* builtPackages is used to skip the recompilation of an already processed package reference */
            if (builtPackages == null)
                builtPackages = new List<string>();

            callTree.Add(packageCopy.CurrentPackagePath);

            /* first, processes referenced package */
            foreach (var packageReference in packageCopy.CurrentPackage.CirrusReferences)
            {
                if (!String.IsNullOrEmpty(packageReference.Reference))
                {
                    if (packageReference.Build)
                    {
                        var referencePath = ParseReferencePath(packageCopy, packageReference.Reference);

                        if (!ProcessPackageReference(packageCopy, referencePath, contextId, rebuild, compress, filesFilter, callTree, builtPackages))
                            return false;
                    }
                    else
                    {
                        Build_Message(String.Format("--- Ignore --- The package {0} has been ignored because not marked for Build", packageReference.Reference),
                            "PackageReferenceCondition", BuildMessageSeverity.Information);
                    }
                }
            }

            /* then process the current package*/
            var decodedAssets = new List<XNACirrusAsset>();

            bool success = Build_ActionForAllFiles(packageCopy, (file) =>
            {
                if (filesFilter == null || filesFilter(file))
                {
                    Build_ProcessFile(decodedAssets, packageCopy, file);
                }
                else
                    Build_Message("-- Skipped by filter");

                return true;
            });

            if (success)
            {
                var build = new BuildContent();
                // BuildEngine is used by TaskLoggingHelper, so an implementation must be provided  
                //  
                build.BuildEngine = new BuildEngine(this);
                build.RebuildAll = rebuild;
                build.CompressContent = compress;
                build.TargetProfile = GraphicsProfile.HiDef.ToString();
                build.BuildConfiguration = "Debug";
                build.TargetPlatform = TargetPlatform.Windows.ToString();

                build.OutputDirectory = XNAOutputDirectory;

                if (decodedAssets.Count > 0)
                {
                    success = XNA_Build_Execute(packageCopy, build, decodedAssets);
                }

                builtPackages.Add(packageCopy.CurrentPackagePath);
            }

            return success;
        }

        private bool Build_Sync_Execute(string contextId, bool rebuild, bool compress, Predicate<EditorContentFile> filesFilter, List<string> callTree, List<string> builtPackages)
        {
            var packageCopy = Build_GetPackageCopy();

            XNAContextInit(packageCopy, contextId);
            try
            {
                return Build_Execute(packageCopy, contextId, rebuild, compress, filesFilter, callTree, builtPackages);
            }
            finally
            {
                XNAContextDispose();
            }
        }

        private bool ProcessPackageReference(EditorApplication packageCopy, string referencePath, string contextId, bool rebuild, bool compress, Predicate<EditorContentFile> filesFilter, List<string> callTree, List<string> builtPackages)
        {
            if (cancellationPending)
                return false;

            Build_Message(String.Format("--- Begin --- Referenced Package : {0}", referencePath), "PackageReferenceResolve", BuildMessageSeverity.Information);

            if (callTree.Contains(referencePath))
            {
                Build_Message("Circular reference identified, aborting", "PackageReferenceResolve", BuildMessageSeverity.Error);
                return false;
            }
            else
            {
                if (builtPackages.Contains(referencePath))
                {
                    Build_Message("--- Package already built. Skipping ...", "PackageReferenceResolve", BuildMessageSeverity.Information);
                    return true;
                }
                else
                {
                    var newCallTree = new List<string>();
                    newCallTree.AddRange(callTree);

                    EditorApplication referencedPackage;

                    using (var packageReferenceStream = new FileStream(referencePath, FileMode.Open, FileAccess.Read))
                    {
                        referencedPackage = Editor.LoadAndCreatePackageFromStream(packageReferenceStream, referencePath);
                    }

                    referencedPackage.CurrentPackage.OutputDirectory = GetOutputBaseDirectory(packageCopy);
                    referencedPackage.Builder.build_message_redirection = (msg, src, severity) =>
                    {
                        Build_Message(msg, src, severity);
                        referencedPackage.Builder.cancellationPending = cancellationPending;
                    };

                    var success = referencedPackage.Builder.Build_Sync_Execute(contextId, rebuild, compress, filesFilter, newCallTree, builtPackages);

                    if (success)
                    {
                        Build_Message(String.Format("--- End Success --- Referenced Package : {0}", referencePath), "PackageReferenceResolve", BuildMessageSeverity.Information);
                        return true;
                    }
                    else
                    {
                        Build_Message(String.Format("--- End Failed --- Referenced Package : {0}", referencePath), "PackageReferenceResolve", BuildMessageSeverity.Information);
                        return false;
                    }
                }
            }
        }

        private bool XNA_Build_Execute(EditorApplication packageCopy, BuildContent build, IList<XNACirrusAsset> sourceAssets)
        {
            // the RootDirectory must contain the sourceFile to avoid an "%0" from being appended to the   
            // output file name  
            //  
            var computedRootDirectory = String.IsNullOrEmpty(packageCopy.CurrentPackage.BuildRootRelativeDirectory)
            ? GetContentBaseDirectory(packageCopy)
            : Path.Combine(GetContentBaseDirectory(packageCopy), packageCopy.CurrentPackage.BuildRootRelativeDirectory);

            Environment.CurrentDirectory = build.RootDirectory = computedRootDirectory;

            build.IntermediateDirectory = XNAIntermediateDirectory;
            build.LoggerRootDirectory = null;
            build.SourceAssets = (from sourceAsset in sourceAssets select sourceAsset.TaskItem).ToArray();

            for (int i = 0; i < sourceAssets.Count; ++i)
            {
                build.SourceAssets[i] = sourceAssets[i].TaskItem;
            }

            //const string xnaVersion = ", Version=2.0.0.0, PublicKeyToken=6d5c3888ef60e27d";  
            // TODO: Why is "Culture" required?  
            //const string xnaVersion = ", Version=3.0.0.0, Culture=neutral, PublicKeyToken=6d5c3888ef60e27d";

            // Don't append .dll??? if loading from the GAC  
            build.PipelineAssemblies = (from xnaReference in packageCopy.CurrentPackage.XNAReferences select new TaskItem(ParseReferencePath(packageCopy, xnaReference.Reference))).ToArray();
            
            try
            {
                return build.Execute();
            }
            catch (Exception e)
            {
                Build_Message(e.Message, e.Source, BuildMessageSeverity.Error);
                return false;
            }
        }

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

        private string GetSafeContentDirectorySuffix(EditorApplication packageCopy)
        {
            string baseSuffix = packageCopy.CurrentPackage.ContentDirectorySuffix;

            if (String.IsNullOrEmpty(baseSuffix))
                baseSuffix = "Content";

            return baseSuffix;
        }

        private string GetOutputPathForFile(EditorApplication packageCopy, EditorContentFile file)
        {
            var rootDirectory = Path.GetDirectoryName(packageCopy.CurrentPackagePath);

            var outputBaseDirectory = Path.IsPathRooted(packageCopy.CurrentPackage.OutputDirectory)
                ? packageCopy.CurrentPackage.OutputDirectory
                : Path.Combine(rootDirectory, packageCopy.CurrentPackage.OutputDirectory);

            outputBaseDirectory += "\\" + GetSafeContentDirectorySuffix(packageCopy);

            var outputCompletePath = Path.Combine(outputBaseDirectory, file.RelativePath);

            var normalizedOutputCompletePath = Path.ChangeExtension(new FileInfo(outputCompletePath).FullName, CirrusContentManager.ContentFileExtention);

            return normalizedOutputCompletePath;
        }

        private void Build_ProcessFile(List<XNACirrusAsset> decodedAssets, EditorApplication packageCopy, EditorContentFile file)
        {
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
                            //Build_Message(String.Format("Compiling {0} to {1}", src, dst), "ProcessFile");
                            //XNAFileCompile(packageCopy, file, src, dst);
                            decodedAssets.Add(new XNACirrusAsset(packageCopy, file));
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

        internal BuildLogger XNALogger { get; private set; }

        internal string XNAIntermediateDirectory { get; private set; }
        internal string XNAOutputDirectory { get; private set; }

        private string AbsoluteDirectoryConvert(string directory)
        {
            return new DirectoryInfo(directory).FullName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageCopy"></param>
        /// <param name="contextId">contextId is used to isolate sub build configurations of the same package</param>
        private void XNAContextInit(EditorApplication packageCopy, string contextId)
        {
            var outputBaseDirectory = GetOutputBaseDirectory(packageCopy);

            XNAOutputDirectory = AbsoluteDirectoryConvert(Path.Combine(outputBaseDirectory, GetSafeContentDirectorySuffix(packageCopy)));

            /* each content has to have an independant intermediate directory, so we generate an unique key for them (approximately) */
            var xnaIntermediateSubDir = Path.Combine("obj", GenerateContentSubDirectory(packageCopy.CurrentPackagePath, contextId));
            XNAIntermediateDirectory = AbsoluteDirectoryConvert(Path.Combine(outputBaseDirectory, xnaIntermediateSubDir));

            if (!String.IsNullOrEmpty(contextId))
            {
                /* Because XNA always clear the content before compiling if it doesn't match its file, 
                 * we force a full rebuild when the context is not null (ie. SingleItemBuild)
                 */
                try
                {
                    var xnaContentXmlPath = Path.Combine(XNAIntermediateDirectory, "ContentPipeline.xml");

                    if (File.Exists(xnaContentXmlPath))
                        File.Delete(xnaContentXmlPath);
                }
                catch { }
            }

            XNALogger = new BuildLogger(this);
        }

        public string GetMD5Hash(string input)
        {
            var md5prod = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var input_str = System.Text.Encoding.UTF8.GetBytes(input);

            input_str = md5prod.ComputeHash(input_str);
            var result = new System.Text.StringBuilder();

            foreach (var b in input_str)
            {
                result.Append(b.ToString("x2").ToLower());
            }

            return result.ToString();
        }

        private string GenerateContentSubDirectory(string path, string contextId)
        {
            if (String.IsNullOrEmpty(contextId))
                contextId = String.Empty;
            else
                contextId = "." + contextId;

            return String.Format("{0}_{1}{2}", GetMD5Hash(path).Substring(0,8), Path.GetFileNameWithoutExtension(path), contextId);
        }

        private static string GetOutputBaseDirectory(EditorApplication packageCopy)
        {
            var rootDirectory = Path.GetDirectoryName(packageCopy.CurrentPackagePath);

            var outputBaseDirectory = Path.IsPathRooted(packageCopy.CurrentPackage.OutputDirectory)
                ? packageCopy.CurrentPackage.OutputDirectory
                : Path.Combine(rootDirectory, packageCopy.CurrentPackage.OutputDirectory);
            return outputBaseDirectory;
        }

        private void XNAContextDispose()
        {

        }

        #endregion
    }
}
