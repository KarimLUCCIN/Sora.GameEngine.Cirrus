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

        private void windowDockingManager_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void packageContentTree_SelectionChanged(object sender, RoutedEventArgs e)
        {
            editorApplication.SelectionForProperties = (from element in packageContentTree.SelectedItems select element.DataContext).ToArray();
        }
    }
}
