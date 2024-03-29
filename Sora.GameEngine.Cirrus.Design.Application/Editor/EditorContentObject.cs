﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace Sora.GameEngine.Cirrus.Design.Application.Editor
{
    public class EditorContentObject : EditorBaseBoundObject
    {
        [Browsable(false)]
        public string BasePath { get; private set; }

        [ReadOnly(true)]
        public string CurrentPath { get; private set; }

        private bool isValid;

        [ReadOnly(true)]
        public bool IsValid
        {
            get { return isValid; }
            set
            {
                isValid = value;
                RaisePropertyChanged("IsValid");
            }
        }

        [Browsable(false)]
        public string Title
        {
            get
            {
                return String.IsNullOrEmpty(CurrentPath) ? "{null}" : Path.GetFileName(CurrentPath);
            }
        }

        private string errorString;

        [ReadOnly(true)]
        public string ErrorString
        {
            get { return errorString; }
            set
            {
                errorString = value;
                RaisePropertyChanged("ErrorString");
            }
        }

        [Browsable(true)]
        [Category("Informations")]
        public string RelativePath { get; private set; }

        public EditorContentObject(EditorApplication editor, string relativePath, string basePath, string currentPath)
            :base(editor)
        {
            RelativePath = relativePath;
            BasePath = basePath;
            CurrentPath = currentPath;
        }
    }
}
