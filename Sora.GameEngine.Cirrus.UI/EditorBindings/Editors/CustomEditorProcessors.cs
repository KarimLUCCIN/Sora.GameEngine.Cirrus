using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings.Editors
{
    public class CustomEditorProcessors : ExtendedPropertyValueEditor
    {
        public CustomEditorProcessors()
        {
            var dictionary = new ResourceDictionary();
            dictionary.Source = new Uri("/CirrusUI;component/EditorBindings/Editors/EditorBindingsCustomEditors.xaml", UriKind.Relative);

            InlineEditorTemplate = (DataTemplate)dictionary["ProcessorInlineEditor"];
            ExtendedEditorTemplate = (DataTemplate)dictionary["ProcessorExtendedEditor"];
        }
    }
}
