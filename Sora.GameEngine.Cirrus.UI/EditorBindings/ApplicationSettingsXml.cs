using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    [XmlRoot("cirrus.application.ui.settings")]
    public class ApplicationSettingsXml
    {
        private static XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettingsXml));

        public static XmlSerializer Serializer
        {
            get { return serializer; }
        }

        public ApplicationSettingsXml()
        {
            RecentFiles = new ObservableCollection<string>();
        }

        public static int MaxRecentsCount
        {
            get { return 10; }
        }

        [XmlArray("recents")]
        [XmlArrayItem("mru")]
        public ObservableCollection<string> RecentFiles { get; set; }

        public void CapRecents()
        {
            while (RecentFiles.Count > MaxRecentsCount)
                RecentFiles.RemoveAt(MaxRecentsCount);
        }

        public void AppendToRecent(string recentPath)
        {
            if (!String.IsNullOrEmpty(recentPath))
            {
                string existingEntry;

                do
                {
                    existingEntry = RecentFiles.FirstOrDefault(p => recentPath.Equals(p, StringComparison.OrdinalIgnoreCase));
                    if (existingEntry != null)
                        RecentFiles.Remove(existingEntry);
                } while (existingEntry != null);

                RecentFiles.Insert(0, recentPath);

                CapRecents();
            }
        }
    }
}
