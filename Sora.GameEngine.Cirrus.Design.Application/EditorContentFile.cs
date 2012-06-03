using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Activities.Presentation.PropertyEditing;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class EditorContentFile : EditorContentObject
    {
        public EditorContentFile(EditorApplication editor, string basePath, string currentPath)
            : base(editor, basePath, currentPath)
        {
            try
            {
                IsValid = File.Exists(currentPath);
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
                IsValid = false;
            }
        }

        private string processor = "";

        [Category("Compiling")]
        [Editor(typeof(ContentFileProcessorEditor), typeof(System.Activities.Presentation.PropertyEditing.PropertyValueEditor))]
        public string Processor
        {
            get { return processor; }
            set
            {
                processor = value;
                RaisePropertyChanged("Processor");
            }
        }
    }
}
