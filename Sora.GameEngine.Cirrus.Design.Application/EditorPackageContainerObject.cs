using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorPackageContainerObject : EditorBaseBoundObject
    {
        public EditorPackageContainerObject(EditorApplication editor)
            : base(editor)
        {
            AvailableXNATypes = new XNAAssemblyDescription[0];
        }

        private XNAAssemblyDescription[] availableXNATypes;

        public XNAAssemblyDescription[] AvailableXNATypes
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
                new EditorContentDirectory(Editor, baseDirectory, Editor.Helper.ResolveDirectory(baseDirectory, RootDirectory)) 
            };

            AvailableXNATypes = Editor.Helper.GetXNAAssembliesDescriptors(Editor.CurrentPackage.XNAReferences);
        }

        private bool packageEventsAttached = false;
        private void AttachPackageEvents()
        {
            if (!packageEventsAttached)
            {
                packageEventsAttached = true;

                Editor.CurrentPackage.XNAReferences.CollectionChanged += delegate
                {
                    AvailableXNATypes = Editor.Helper.GetXNAAssembliesDescriptors(Editor.CurrentPackage.XNAReferences);
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
