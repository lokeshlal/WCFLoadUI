#region File Information/History
// <copyright file="BoolToColorConverter.cs" project="WCFLoadUI" >
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
using System.Windows.Data;

namespace WCFLoadUI.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {

                if (System.Convert.ToBoolean(value) == false)
                {
                    return "Red";
                }
                else
                {
                    return "Green";
                }
            }

            return "Red";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
