using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Sora.GameEngine.Cirrus.Design.Application;
using Sora.GameEngine.Cirrus.UI.EditorBindings;
using Sora.GameEngine.Cirrus.Design.Application.Editor;

namespace Sora.GameEngine.Cirrus.UI.Converters
{
    public class EditorContentFileToUIContentFileConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var initialArray = value as IEnumerable<EditorBaseBoundObject>;

            if (initialArray == null)
                return null;
            else
            {
                return (
                    from entry in initialArray 
                    select 
                        entry is EditorContentFile 
                        ? (object)new EditorUIContentFile((EditorContentFile)entry)
                        : entry).ToList();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
