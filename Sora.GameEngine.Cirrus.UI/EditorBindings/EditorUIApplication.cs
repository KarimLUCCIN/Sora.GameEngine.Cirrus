using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sora.GameEngine.Cirrus.Design.Application;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings
{
    public class EditorUIApplication : EditorApplication
    {
        private MainWindow mainWindow;

        public EditorUIApplication(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            Quit = new GenericCommand(ActionQuit);
        }

        #region Menu
        public GenericCommand NewFile { get; private set; }
        public GenericCommand OpenFile { get; private set; }

        public GenericCommand SaveFile { get; private set; }
        public GenericCommand SaveAsFile { get; private set; }

        public GenericCommand Quit { get; private set; }
        public void ActionQuit(object parameter)
        {
            mainWindow.Close();
        }
        #endregion
    }
}
