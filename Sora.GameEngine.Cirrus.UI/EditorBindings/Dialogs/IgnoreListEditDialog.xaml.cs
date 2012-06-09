using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Sora.GameEngine.Cirrus.Design.Application.Editor;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings.Dialogs
{
    /// <summary>
    /// Interaction logic for IgnoreListEditDialog.xaml
    /// </summary>
    public partial class IgnoreListEditDialog : Window, INotifyPropertyChanged
    {
        #region Global Properties

        public EditorApplication Editor { get; private set; }

        private string localIgnoreString;

        public string LocalIgnoreString
        {
            get { return localIgnoreString; }
            set
            {
                localIgnoreString = value;
                RaisePropertyChanged("LocalIgnoreString");
            }
        }

        #endregion

        #region Constructor

        public IgnoreListEditDialog(EditorApplication editor)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");

            Editor = editor;
            localIgnoreString = editor.CurrentPackage.IgnoreString;

            DataContext = this;

            InitializeComponent();
        }

        #endregion

        #region UI Functions

        private bool UserValidateInput()
        {
            if (String.IsNullOrEmpty(localIgnoreString))
            {
                LocalIgnoreString = String.Empty;
                return true;
            }
            else
            {
                var entries = localIgnoreString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var entry in entries)
                {
                    try
                    {
                        var tempRegex = new Regex(entry);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(String.Format("Invalid input regex : {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                return true;
            }
        }

        public string TestQueryButtonString
        {
            get { return testing ? "Stop" : "Test Query"; }
        }

        private void btnTestQuery_Click(object sender, RoutedEventArgs e)
        {
            if (UserValidateInput())
            {
                StartTestQuery();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopTestQuery();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (UserValidateInput())
            {
                Editor.CurrentPackage.IgnoreString = localIgnoreString;
                Editor.Refresh();
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Testing

        private bool stopTesting = false;

        private bool testing;

        public bool Testing
        {
            get { return testing; }
            set
            {
                testing = value;
                RaisePropertyChanged("Testing");
                RaisePropertyChanged("TestQueryButtonString");
            }
        }

        private void StartTestQuery()
        {
            StopTestQuery();
            Testing = true;
            stopTesting = false;
            TestingResult.Clear();

            testRegexes = (from string_entry in localIgnoreString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                           select new Regex(string_entry, RegexOptions.IgnoreCase)).ToArray();

            testQueue.Clear();
            foreach (var rootElement in Editor.PackageContainer[0].Content)
            {
                var objElement = rootElement as EditorContentObject;
                if (objElement != null)
                    testQueue.Enqueue(objElement);
            }

            var testThread = new Thread(TestThreadMain);
            testThread.IsBackground = true;
            testThread.Start();
        }

        private void StopTestQuery()
        {
            stopTesting = true;
            while (testing)
                Thread.Sleep(1);
        }

        private Regex[] testRegexes = new Regex[0];
        private Queue<EditorContentObject> testQueue = new Queue<EditorContentObject>();

        private void TestThreadMain()
        {
            try
            {
                try
                {
                    while (testQueue.Count > 0)
                    {
                        if (stopTesting)
                            return;

                        var currentObj = testQueue.Dequeue();

                        if (currentObj == null)
                            continue;

                        var as_dir = currentObj as EditorContentDirectory;
                        var as_file = currentObj as EditorContentFile;

                        if (as_dir != null)
                        {
                            foreach (var sub in as_dir.UnfilteredContent)
                            {
                                if (stopTesting)
                                    return;

                                testQueue.Enqueue(sub as EditorContentObject);
                            }
                        }
                        else if (as_file != null)
                        {
                            foreach (var testRegex in testRegexes)
                            {
                                if (testRegex.IsMatch(as_file.RelativePath))
                                {
                                    Dispatcher.BeginInvoke((Action)delegate
                                    {
                                        TestingResult.Add(as_file);
                                    });
                                    break;
                                }
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
                Testing = false;
            }
        }

        private ObservableCollection<EditorContentFile> testingResult = new ObservableCollection<EditorContentFile>();

        public ObservableCollection<EditorContentFile> TestingResult
        {
            get { return testingResult; }
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
