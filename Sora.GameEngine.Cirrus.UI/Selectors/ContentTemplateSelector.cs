using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Sora.GameEngine.Cirrus.Design.Application;

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
        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var obj = item as EditorContentObject;
            if (obj == null)
                return null;
            else
            {
                DataTemplate result = null;

                if (!obj.IsValid)
                    result = errorTemplate;
                else if (obj is EditorContentFile)
                    result = fileTemplate;
                else
                    result = directoryTemplate;

                var hierarchy = result as HierarchicalDataTemplate;
                if (hierarchy != null && hierarchy.ItemTemplateSelector == null)
                    hierarchy.ItemTemplateSelector = this;

                return result;
            }
        }
    }
}
