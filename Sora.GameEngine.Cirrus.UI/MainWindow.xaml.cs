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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sora.GameEngine.Cirrus.UI.EditorBindings;

namespace Sora.GameEngine.Cirrus.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public EditorUIApplication editorApplication;

        public MainWindow()
        {
            InitializeComponent();

            editorApplication = new EditorUIApplication(this);
            editorApplication.RefreshPropertiesViewRequested += delegate
            {
                var oldSelection = globalPropertyGrid.SelectedObjects;
                if (oldSelection != null)
                {
                    globalPropertyGrid.SelectedObjects = new object[0];
                    globalPropertyGrid.SelectedObjects = oldSelection;
                }
            };

            DataContext = editorApplication;

            editorApplication.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(editorApplication_PropertyChanged);
            globalPropertyGrid.SelectedObjects = editorApplication.SelectionForProperties;
        }

        void editorApplication_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e.PropertyName) || e.PropertyName == "SelectionForProperties")
            {
                globalPropertyGrid.SelectedObjects = editorApplication.SelectionForProperties;
                globalPropertyGrid.RefreshPropertyList();
            }
        }

        #region UI Layout Saving & Restoring

        private void windowDockingManager_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var uiSettingsLocation = editorApplication.UISettingsLocation;

                if (System.IO.File.Exists(uiSettingsLocation))
                    windowDockingManager.RestoreLayout(uiSettingsLocation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Impossible de restaurer le layout de l'affichage.\n{0}", ex), "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                windowDockingManager.SaveLayout(editorApplication.UISettingsLocation);
            }
            catch { }
        }

        #endregion

        private void packageContentTree_SelectionChanged(object sender, RoutedEventArgs e)
        {
            editorApplication.SelectionForProperties = (from element in packageContentTree.SelectedItems select element.DataContext).ToArray();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!editorApplication.ActionClose(null))
                e.Cancel = true;
        }
    }
}
