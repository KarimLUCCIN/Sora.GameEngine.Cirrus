using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Markup;
using System.Windows;
using System.Activities.Presentation.PropertyEditing;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    public class ContentFileProcessorEditor : ExtendedPropertyValueEditor
    {
        public ContentFileProcessorEditor()
        {
#warning TODO
//            // Template for normal view
//            string template1 = @"
//                <DataTemplate
//                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
//                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
//                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'>
//                    <DockPanel LastChildFill='True'>
//                        <pe:EditModeSwitchButton TargetEditMode='ExtendedPopup' Name='EditButton' 
//                        DockPanel.Dock='Right' />
//                        <TextBlock Text='{Binding Value}' Margin='2,0,0,0' VerticalAlignment='Center'/>
//                    </DockPanel>
//                </DataTemplate>";

//            // Template for extended view. Shown when dropdown button is pressed.
//            string template2 = @"
//                <DataTemplate
//                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
//                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
//                    xmlns:sys='clr-namespace:System;assembly=mscorlib'
//                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'
//                    xmlns:wpg='clr-namespace:WpfPropertyGrid_Demo;assembly=WpfPropertyGrid_Demo'>
//                    <DataTemplate.Resources>
//                        <ObjectDataProvider MethodName='GetValues' ObjectType='{x:Type sys:Enum}' x:Key='ContinentEnumValues'>
//                            <ObjectDataProvider.MethodParameters>
//                                <x:Type TypeName='wpg:Continent' />
//                            </ObjectDataProvider.MethodParameters>
//                        </ObjectDataProvider>
//                        <wpg:FilteredCountriesConverter x:Key='CountriesConverter' />
//                    </DataTemplate.Resources>
//                    <StackPanel Orientation='Vertical' Background='White'>
//                        <ComboBox Width='120' x:Name='ComboContinents' 
//                            ItemsSource='{Binding Source={StaticResource ContinentEnumValues}}' 
//                            SelectedItem='{Binding Path=Value.Contin,Mode=OneTime}' />
//                        <ComboBox Width='120' SelectedItem='{Binding Path=Value, Mode=TwoWay, UpdateSourceTrigger=LostFocus}' 
//                            IsSynchronizedWithCurrentItem='True'>
//                            <ComboBox.ItemsSource>
//                                <MultiBinding Converter='{StaticResource CountriesConverter}'>
//                                    <Binding ElementName='ComboContinents' Path='SelectedItem' />
//                                </MultiBinding>                   
//                            </ComboBox.ItemsSource>
//                       </ComboBox>
//                    </StackPanel>
//                </DataTemplate>";

//            // Load templates
//            using (var sr = new MemoryStream(Encoding.UTF8.GetBytes(template1)))
//            {
//                this.InlineEditorTemplate = XamlReader.Load(sr) as DataTemplate;
//            }
//            using (var sr = new MemoryStream(Encoding.UTF8.GetBytes(template2)))
//            {
//                this.ExtendedEditorTemplate = XamlReader.Load(sr) as DataTemplate;
//            }
        }
    }
}
