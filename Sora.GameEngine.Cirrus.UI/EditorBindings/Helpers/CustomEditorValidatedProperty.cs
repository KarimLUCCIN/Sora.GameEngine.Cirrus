using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings.Helpers
{
    /// <summary>
    /// Custom property that has an explicit converstion between string
    /// </summary>
    public abstract class CustomEditorValidatedPropertyBase
    {
        public CustomEditorValidatedPropertyBase(Type editorType)
        {
            if (editorType == null)
                throw new ArgumentNullException("editorType");

            EditorType = editorType;
        }

        public abstract void Reset();

        public abstract void Set(object value);

        public abstract object Get();

        public Type EditorType { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPropertyType">Base type of the property</typeparam>
    /// <typeparam name="TEditorType">Type that will be exposed to the PropertyGrid</typeparam>
    public class CustomEditorValidatedProperty<TPropertyType, TEditorType> : CustomEditorValidatedPropertyBase, INotifyPropertyChanged
    {
        Func<TPropertyType, TEditorType> convertToEditor;
        Func<TEditorType, TPropertyType> convertToProperty;

        private TPropertyType baseValue;

        public TPropertyType BaseValue
        {
            get { return baseValue; }
            set
            {
                baseValue = value;
                RaisePropertyChanged("BaseValue");
            }
        }
        
        public CustomEditorValidatedProperty(
            TPropertyType baseValue,
            Func<TPropertyType, TEditorType> convertToEditor,
            Func<TEditorType, TPropertyType> convertToProperty
            )
            : base(typeof(TEditorType))
        {
            if (convertToEditor == null)
                throw new ArgumentNullException("convertToEditor");

            if (convertToProperty == null)
                throw new ArgumentNullException("convertToProperty");

            this.convertToEditor = convertToEditor;
            this.convertToProperty = convertToProperty;
        }

        private TPropertyType value;

        public TPropertyType Value
        {
            get { return value; }
            set
            {
                this.value = value;
                RaisePropertyChanged("Value");
            }
        }

        public override void Reset()
        {
            Set(baseValue);
        }

        public override void Set(object value)
        {
            Value = convertToProperty((TEditorType)value);
        }

        public override object Get()
        {
            return convertToEditor(Value);
        }

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
