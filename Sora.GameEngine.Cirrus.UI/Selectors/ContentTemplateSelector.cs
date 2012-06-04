using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Sora.GameEngine.Cirrus.Design.Application;
using Sora.GameEngine.Cirrus.Design.Packages;

namespace Sora.GameEngine.Cirrus.UI.Selectors
{
    public class ContentTemplateSelector : DataTemplateSelector
    {
        private DataTemplate directoryTemplate;

        public DataTemplate DirectoryTemplate
        {
            get { return directoryTemplate; }
            set { directoryTemplate = value; }
        }

        private DataTemplate fileTemplate;

        public DataTemplate FileTemplate
        {
            get { return fileTemplate; }
            set { fileTemplate = value; }
        }

        private DataTemplate errorTemplate;

        public DataTemplate ErrorTemplate
        {
            get { return errorTemplate; }
            set { errorTemplate = value; }
        }

        private DataTemplate xnaReferenceTemplate;

        public DataTemplate XNAReferenceTemplate
        {
            get { return xnaReferenceTemplate; }
            set { xnaReferenceTemplate = value; }
        }

        private DataTemplate packageReferenceTemplate;

        public DataTemplate PackageReferenceTemplate
        {
            get { return packageReferenceTemplate; }
            set { packageReferenceTemplate = value; }
        }

        private DataTemplate defaultContainerTemplate;

        public DataTemplate DefaultContainerTemplate
        {
            get { return defaultContainerTemplate; }
            set { defaultContainerTemplate = value; }
        }
                        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate result = defaultContainerTemplate;

            var dir_obj = item as EditorContentObject;
            if (dir_obj == null)
            {
                if (item is XmlCirrusXNAReference)
                    result = xnaReferenceTemplate;
                else if (item is XmlCirrusPackageReference)
                    result = packageReferenceTemplate;
            }
            else
            {
                if (!dir_obj.IsValid)
                    result = errorTemplate;
                else if (dir_obj is EditorContentFile)
                    result = fileTemplate;
                else if (dir_obj is EditorContentDirectory)
                    result = directoryTemplate;
            }

            var hierarchy = result as HierarchicalDataTemplate;

            if (hierarchy != null && hierarchy.ItemTemplateSelector == null)
                hierarchy.ItemTemplateSelector = this;

            return result;
        }
    }
}
