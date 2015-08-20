#region File Information/History
// <copyright file="FunctionalTestResultsToColorConverter.cs" project="WCFLoadUI" >
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
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.Converters
{
    public class FunctionalTestResultsToColorConverter : IValueConverter
    {
        public object Convert(object value1, Type targetType, object parameter, CultureInfo culture)
        {
            var value = value1 as FunctionalTestResults;
            if (value != null)
            {
                if (value.Status != Status.Completed)
                {
                    return "White";
                }
                if (System.Convert.ToBoolean(value.PassFailStatus) == false)
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
