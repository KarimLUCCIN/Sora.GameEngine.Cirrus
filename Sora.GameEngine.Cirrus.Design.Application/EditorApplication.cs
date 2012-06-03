using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sora.GameEngine.Cirrus.Design.Packages;
using System.Collections.ObjectModel;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorApplication : INotifyPropertyChanged
    {
        public CirrusDesignHelper Helper { get; private set; }

        public EditorApplication()
        {
            InitializeObjects();

            currentPackagePath = String.Empty;
            currentPackage = new XmlCirrusPackage();

            InitializeNewPackage();

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

        #region Package management

        private string currentPackagePath;

        public string CurrentPackagePath
        {
            get { return currentPackagePath; }
            private set
            {
                currentPackagePath = value;
                RaisePropertyChanged("CurrentPackagePath");
            }
        }

        private XmlCirrusPackage currentPackage;

        public XmlCirrusPackage CurrentPackage
        {
            get { return currentPackage; }
            private set
            {
                currentPackage = value;
                RaisePropertyChanged("CurrentPackage");
            }
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

        /// <summary>
        /// Initialize common properties for a new package
        /// </summary>
        public void InitializeNewPackage()
        {
            currentPackage.Name = "Untitled";
            currentPackage.RootDirectory = String.Empty;

            var asm_list = (from asm in Helper.GetDefaultXNAReferences() select asm).Distinct().ToArray();

            foreach (var asm in asm_list)
                currentPackage.XNAReferences.Add(asm);
        }

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
    }
}
