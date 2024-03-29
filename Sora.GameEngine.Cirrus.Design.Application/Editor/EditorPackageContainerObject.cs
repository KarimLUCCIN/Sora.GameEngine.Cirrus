﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Sora.GameEngine.Cirrus.Design.Packages;

namespace Sora.GameEngine.Cirrus.Design.Application.Editor
{
    public class EditorPackageContainerObject : EditorBaseBoundObject
    {
        public EditorPackageContainerObject(EditorApplication editor)
            : base(editor)
        {
            AvailableXNATypes = new ObservableCollection<XNAAssemblyDescription>();
        }

        private ObservableCollection<XNAAssemblyDescription> availableXNATypes;

        public ObservableCollection<XNAAssemblyDescription> AvailableXNATypes
        {
            get { return availableXNATypes; }
            private set
            {
                availableXNATypes = value;
                RaisePropertyChanged("AvailableXNATypes");
            }
        }


        /// <summary>
        /// Reload the content of the package
        /// </summary>
        public void RefreshContent()
        {
            AttachPackageEvents();

            string baseDirectory = String.IsNullOrEmpty(Editor.CurrentPackagePath)
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : Path.GetDirectoryName(Editor.CurrentPackagePath);

            Content = new object[] { 
                new EditorXNAReferencesObject(Editor),
                new EditorPackageReferencesObject(Editor),
                new EditorContentDirectory(Editor, "", baseDirectory, Editor.Helper.ResolveDirectory(baseDirectory, RootDirectory)) 
            };

            LoadXNADescriptorsForPackage();
        }

        private void LoadXNADescriptorsForPackage()
        {
            /* resolve references path */
            var parsedReferences = from xnaReference in Editor.CurrentPackage.XNAReferences select new XmlCirrusXNAReference() { Reference = Editor.Builder.ParseReferencePath(xnaReference.Reference) };

            /* then, load types */
            LoadXNATypes(Editor.Helper.GetXNAAssembliesDescriptors(parsedReferences));
        }

        private void LoadXNATypes(XNAAssemblyDescription[] xnaAssemblies)
        {
            AvailableXNATypes.Clear();
            foreach (var item in xnaAssemblies)
                AvailableXNATypes.Add(item);

            RaisePropertyChanged("AvailableXNATypes");
        }

        private bool packageEventsAttached = false;
        private void AttachPackageEvents()
        {
            if (!packageEventsAttached)
            {
                packageEventsAttached = true;

                Editor.CurrentPackage.XNAReferences.CollectionChanged += delegate
                {
                    LoadXNADescriptorsForPackage();
                };
            }
        }

        private object[] content;

        [Browsable(false)]
        public object[] Content
        {
            get { return content; }
            private set
            {
                content = value;
                RaisePropertyChanged("Content");
            }
        }

        public string ContentDirectorySuffix
        {
            get { return Editor.CurrentPackage.ContentDirectorySuffix; }
            set
            {
                Editor.CurrentPackage.ContentDirectorySuffix = value;
                RaisePropertyChanged("ContentDirectorySuffix");
            }
        }

        public string Name
        {
            get { return Editor.CurrentPackage.Name; }
            set
            {
                Editor.CurrentPackage.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string Title
        {
            get { return Editor.CurrentPackage.Title; }
            set
            {
                Editor.CurrentPackage.Title = value;
                RaisePropertyChanged("Title");
            }
        }

        public string Description
        {
            get { return Editor.CurrentPackage.Description; }
            set
            {
                Editor.CurrentPackage.Description = value;
                RaisePropertyChanged("Description");
            }
        }

        /// <summary>
        /// Root path of the package. Everything inside this directory will be available.
        /// </summary>
        /// <remarks>The root can be a path relative to this package directory</remarks>
        [Description("Root path of the package. Everything inside this directory will be available (ignoring only filtered elements)")]
        public string RootDirectory
        {
            get { return Editor.CurrentPackage.RootDirectory; }
            set
            {
                Editor.CurrentPackage.RootDirectory = value;
                RaisePropertyChanged("RootDirectory");

                RefreshContent();
            }
        }
        
        [Description("Starting from RootDirectory, the relative directory in which content importers and processors will be run")]
        public string BuildRootRelativeDirectory
        {
            get { return Editor.CurrentPackage.BuildRootRelativeDirectory; }
            set
            {
                Editor.CurrentPackage.BuildRootRelativeDirectory = value;
                RaisePropertyChanged("BuildRootRelativeDirectory");
            }
        }

        /// <summary>
        /// Directory where the built content is placed
        /// </summary>
        /// <remarks>The output can be a path relative to this package directory</remarks>
        public string OutputDirectory
        {
            get { return Editor.CurrentPackage.OutputDirectory; }
            set
            {
                Editor.CurrentPackage.OutputDirectory = value;
                RaisePropertyChanged("OutputDirectory");
            }
        }
    }
}
