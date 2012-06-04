using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.Design.Packages
{
    public class XmlCirrusXNAReference : IComparable<XmlCirrusXNAReference>, INotifyPropertyChanged
    {
        /// <summary>
        /// Assembly path. Can be a gac reference or an assembly relative to the current package directory.
        /// Child packages inherits references from parent ones.
        /// </summary>
        [XmlAttribute("assembly")]
        public string Reference { get; set; }

        #region IComparable<XmlCirrusXNAReference> Members

        public int CompareTo(XmlCirrusXNAReference other)
        {
            if (other == null)
                return 1;
            else
                return String.CompareOrdinal(Reference, other.Reference);
        }

        #endregion

        public override string ToString()
        {
            return "XNA: " + Reference;
        }

        #region INotifyPropertyChanged Members

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
