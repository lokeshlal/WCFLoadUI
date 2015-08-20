#region File Information/History
// <copyright file="BooleanVisibilityValueConverter.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WCFLoadUI.Converters
{
    public class BooleanVisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {

                if (value is Boolean)
                {
                    if (((bool)value))
                        return Visibility.Visible;
                    return Visibility.Collapsed;
                }

                if (value is Visibility)
                {
                    if ((Visibility)value == Visibility.Visible)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
