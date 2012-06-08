using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.Design.Packages
{
    public class XmlCirrusPackageReference : INotifyPropertyChanged
    {
        /// <summary>
        /// Can be an absolute or relative path from the current package directory.
        /// Child packages inherits references from parent ones.
        /// </summary>
        [XmlAttribute("path")]
        public string Reference { get; set; }

        public override string ToString()
        {
            return "Package: " + Reference;
        }

        public XmlCirrusPackageReference()
        {
            Build = true;
        }

        #region INotifyPropertyChanged Members

        private bool build;

        public bool Build
        {
            get { return build; }
            set
            {
                build = value;
                RaisePropertyChanged("Build");
            }
        }
        
        private bool valid = true;

        /// <summary>
        /// Indicates if the reference is valid or not
        /// </summary>
        /// <remarks>Used in the designer but not serialized</remarks>
        [XmlIgnore]
        public bool Valid
        {
            get { return valid; }
            set
            {
                valid = value;
                RaisePropertyChanged("Valid");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
