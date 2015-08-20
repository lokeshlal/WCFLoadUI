#region File Information/History
// <copyright file="TypeTemplateSelector.cs" project="WCFLoadUI" >
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
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.TemplateSelectors
{
    /// <summary>
    /// Template selector for complex control
    /// </summary>
    public class TypeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PrimitiveTemplate { get; set; }
        public DataTemplate ComplexTemplate { get; set; }
        public DataTemplate ListTemplate { get; set; }
        public DataTemplate DictionaryTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
                      DependencyObject container)
        {
            var i = item as ControlView;
            if (i != null)
            {
                switch (i.Type)
                {
                    case 0: return PrimitiveTemplate;
                    case 1: return ComplexTemplate;
                    case 2: return ListTemplate;
                    case 3: return DictionaryTemplate;
                }
                if (i.IsPrimitive)
                {
                    return PrimitiveTemplate;
                }
                else
                {
                    return ComplexTemplate;
                }
            }
            return PrimitiveTemplate;
        }
    }
}
