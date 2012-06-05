using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.UI.EditorBindings.Helpers
{
    /// <summary>
    /// Provides custom properties that can be used in a PropertyGrid
    /// </summary>
    public class DictionaryCustomPropertiesProvider : ICustomTypeDescriptor
    {
        public Dictionary<string, CustomEditorValidatedPropertyBase> Properties { get; private set; }

        public DictionaryCustomPropertiesProvider()
        {
            Properties = new Dictionary<string, CustomEditorValidatedPropertyBase>();
        }

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return ((ICustomTypeDescriptor)this).GetEvents(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection((
                from property in Properties
                where property.Value != null
                select new CustomEntryPropertyDescriptor(property.Key, property.Value.EditorType, attributes)
                ).ToArray());
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion

        class CustomEntryPropertyDescriptor : PropertyDescriptor
        {
            Type editorType;

            public CustomEntryPropertyDescriptor(string name, Type editorType, Attribute[] attrs)
                : base(name, attrs)
            {
                this.editorType = editorType;
            }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override Type ComponentType
            {
                get { return typeof(DictionaryCustomPropertiesProvider); }
            }

            public override object GetValue(object component)
            {
                return ((DictionaryCustomPropertiesProvider)component).Properties[Name].Get();
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return editorType; }
            }

            public override void ResetValue(object component)
            {
                ((DictionaryCustomPropertiesProvider)component).Properties[Name].Reset();
            }

            public override void SetValue(object component, object value)
            {
                ((DictionaryCustomPropertiesProvider)component).Properties[Name].Set(value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }
    }
}
