﻿#region File Information/History
// <copyright file="DateTimeTextFieldVisibilityValueConverter.cs" project="WCFLoadUI" >
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
    /// <summary>
    /// Converter to show DateTime control or Textbox on the basis of value of field
    /// </summary>
    public class DateTimeTextFieldVisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {

                if (System.Convert.ToString(value) == "{DateTime.Now}")
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
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
