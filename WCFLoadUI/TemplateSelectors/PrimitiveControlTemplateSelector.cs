#region File Information/History
// <copyright file="PrimitiveControlTemplateSelector.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System.Windows;
using System.Windows.Controls;
using WCFLoadUI.ViewModels;

namespace WCFLoadUI.TemplateSelectors
{
    /// <summary>
    /// Template selector for Primitive control
    /// </summary>
    public class PrimitiveControlTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate BooleanTemplate { get; set; }
        public DataTemplate IntegerTemplate { get; set; }
        public DataTemplate DoubleTemplate { get; set; }
        public DataTemplate LongTemplate { get; set; }
        public DataTemplate DecimalTemplate { get; set; }
        public DataTemplate CharTemplate { get; set; }
        public DataTemplate DateTimeTemplate { get; set; }
        public DataTemplate EnumTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
                      DependencyObject container)
        {
            var i = item as PrimitiveControlViewModel;
            if (i != null)
            {
                if (i.FieldType.IsEnum)
                {
                    return EnumTemplate;// IntegerTemplate;
                }
                else
                {
                    switch (i.FieldType.Name.ToLower())
                    {
                        case "int32":
                            return IntegerTemplate;
                        case "string":
                            return StringTemplate;
                        case "boolean":
                        case "bool":
                            //set default bool value
                            if (string.IsNullOrEmpty(i.FieldValue))
                            {
                                i.FieldValue = "false";
                            }
                            return BooleanTemplate;
                        case "datetime":
                            return DateTimeTemplate;
                        case "int64":
                        case "long":
                            return LongTemplate;
                        case "double":
                            return DoubleTemplate;
                        case "decimal":
                            return DecimalTemplate;
                        case "char":
                            return CharTemplate;
                    }
                }
            }
            return StringTemplate;
        }
    }
}
