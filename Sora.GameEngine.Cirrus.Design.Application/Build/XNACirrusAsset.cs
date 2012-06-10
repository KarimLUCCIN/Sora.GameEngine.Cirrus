using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using System.IO;
using Sora.GameEngine.Cirrus.Design.Application.Editor;

namespace Sora.GameEngine.Cirrus.Design.Application.Build
{
    internal class XNACirrusAsset
    {
        private TaskItem taskItem;

        public TaskItem TaskItem
        {
            get { return taskItem; }
        }

        public XNACirrusAsset(EditorApplication editorPackage, EditorContentFile contentFile)
        {
            var pathToAsset = contentFile.CurrentPath;
            var processorName = contentFile.Processor;
            var importerName = contentFile.Importer;

            Dictionary<string, object> metaData = new Dictionary<string, object>();
            metaData.Add("Processor", processorName);

            // The importer can be inferred by XNA's BuildContent class  
            //  
            if (!string.IsNullOrEmpty(importerName))
                metaData.Add("Importer", importerName);

            // "Name" will only work if the model is in the RootDirectory or any RootDirectory subdirectory  
            // a %0 will be appended the the sourceFile name if a "Name" isn't in the metadata  
            // .xnb will be appended to the name   
            //  
            metaData.Add("Name", Path.GetFileNameWithoutExtension(pathToAsset));

            var contentInfo = contentFile.GetContentInfo(false);
            if (contentInfo != null)
            {
                foreach (var entry in contentInfo.Entries)
                {
                    // Constructor a metadata entry key string of the form ProcessorParameters_NameOfKey  
                    metaData.Add(ProcessorParameterPrefix + entry.Name, entry.Value);
                }
            }

            this.taskItem = new TaskItem(pathToAsset, metaData);
        }

        private static string ProcessorParameterPrefix
        {
            get { return "ProcessorParameters_"; }
        }
    }
}
