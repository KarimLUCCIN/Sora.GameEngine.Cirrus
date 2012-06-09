using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sora.GameEngine.Cirrus.Design.Application.Helpers;
using System.Collections.ObjectModel;
using Sora.GameEngine.Cirrus.Design.Application.Editor;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    public class SearchBindingProvider : INotifyPropertyChanged
    {
        private EditorUIApplication editor;

        public EditorUIApplication Editor
        {
            get { return editor; }
        }

        private ObservableCollection<object> searchResult = new ObservableCollection<object>();

        public ObservableCollection<object> SearchResult
        {
            get { return searchResult; }
        }
        
        public SearchBindingProvider(EditorUIApplication editor)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");

            this.editor = editor;

            SearchAndStopCommand = new GenericCommand(_ =>
            {
                if (searching)
                    Stop();
                else
                    Search();
            });
        }

        private GenericCommand searchAndStopCommand;

        public GenericCommand SearchAndStopCommand
        {
            get { return searchAndStopCommand; }
            private set
            {
                searchAndStopCommand = value;
                RaisePropertyChanged("SearchAndStopCommand");
            }
        }


        private bool searching = false;

        public bool Searching
        {
            get { return searching; }
            set
            {
                searching = value;
                RaisePropertyChanged("Searching");
            }
        }

        private string searchString;

        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                RaisePropertyChanged("SearchString");
            }
        }
        
        public void Search()
        {
            Stop();

            if (!String.IsNullOrEmpty(searchString))
            {
                try
                {
                    searchRegex = new Regex(searchString, RegexOptions.IgnoreCase);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                searchResult.Clear();
                searchThreadParameter = searchString;
                Searching = true;
                searchStop = false;
                searchQueue.Clear();

                foreach (var rootElement in Editor.PackageContainer[0].Content)
                {
                    var objElement = rootElement as EditorContentObject;
                    if (objElement != null)
                        searchQueue.Enqueue(objElement);
                }

                searchThread = new Thread(SearchThreadMain);
                searchThread.IsBackground = true;
                searchThread.Start();
            }
        }

        public void Stop()
        {
            searchStop = true;
            while (searching)
                Thread.Sleep(1);
        }

        #region Searching Operations

        Thread searchThread = null;
        string searchThreadParameter = String.Empty;
        bool searchStop = false;
        Queue<EditorContentObject> searchQueue = new Queue<EditorContentObject>();
        Regex searchRegex;

        private void SearchThreadMain()
        {
            try
            {
                try
                {
                    while (searchQueue.Count > 0)
                    {
                        if (searchStop)
                            return;

                        var currentObj = searchQueue.Dequeue();

                        if (currentObj == null)
                            continue;

                        var as_dir = currentObj as EditorContentDirectory;
                        var as_file = currentObj as EditorContentFile;

                        if (as_dir != null)
                        {
                            if (searchRegex.IsMatch(as_dir.Title))
                            {
                                AppendSearchResult(as_dir);
                            }

                            foreach (var sub in as_dir.Content)
                            {
                                if (searchStop)
                                    return;

                                searchQueue.Enqueue(sub as EditorContentObject);
                            }
                        }
                        else if (as_file != null)
                        {
                            if (searchRegex.IsMatch(as_file.Title))
                            {
                                AppendSearchResult(as_file);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            finally
            {
                Searching = false;
            }
        }

        private void AppendSearchResult(EditorContentObject ed_obj)
        {
            Editor.Dispatcher.BeginInvoke((Action)delegate
            {
                var as_file = ed_obj as EditorContentFile;
                if (as_file != null)
                    SearchResult.Add(new EditorUIContentFile(as_file));
                else
                    SearchResult.Add(ed_obj);
            });
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
