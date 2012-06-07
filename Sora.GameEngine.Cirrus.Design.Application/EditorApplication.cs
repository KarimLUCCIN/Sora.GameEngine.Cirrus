﻿using System;
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

        public PackageBuilder Builder { get; private set; }

        public EditorApplication()
        {
            Builder = new PackageBuilder(this);

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

        internal EditorApplication LoadAndCreatePackageFromStream(Stream stream, string associatedFile)
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

        internal void SavePackage(Stream stream)
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

        /// <summary>
        /// When called from a different thread, caller should redirect to the correct thread
        /// </summary>
        /// <param name="a"></param>
        public virtual void WrapAsyncAction(Action action)
        {
            if (action != null)
                action();
        }

        #endregion
    }
}
