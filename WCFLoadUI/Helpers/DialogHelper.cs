#region File Information/History
// <copyright file="DialogHelper.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using Caliburn.Micro;

namespace WCFLoadUI.Helpers
{
    /// <summary>
    /// Dialog helper class
    /// </summary>
    public class DialogHelper
    {
        /// <summary>
        /// Show dialog window
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        public static void ShowDialog<T>(params Object[] param) where T : class
        {
            var windowManager = new WindowManager();
            T viewModel = Activator.CreateInstance(typeof(T), param) as T;
            windowManager.ShowDialog(viewModel);
        }

        /// <summary>
        /// Show dialog window and calls a specific contructor of dialog
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object ShowDialog<T>(string propertyName, params Object[] param) where T : class
        {
            var windowManager = new WindowManager();
            T viewModel = Activator.CreateInstance(typeof(T), param) as T;
            var dialogResult = windowManager.ShowDialog(viewModel);
            if (dialogResult != null && dialogResult.Value)
            {
                if (viewModel != null) return viewModel.GetType().GetProperty(propertyName).GetValue(viewModel);
            }
            else
                return string.Empty;
            return null;
        }

    }
}
