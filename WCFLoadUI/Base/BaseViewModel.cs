#region File Information/History
// <copyright file="BaseViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System.Windows;
using Caliburn.Micro;

namespace WCFLoadUI.Base
{
    /// <summary>
    /// Base view model class
    /// </summary>
    public class BaseViewModel : ViewAware
    {
        protected Window DialogWindow;

        protected override void OnViewAttached(object view, object context)
        {
            DialogWindow = view as Window;
            base.OnViewAttached(view, context);
        }

        public override object GetView(object context = null)
        {
            return DialogWindow;
        }

        public void ExecuteCancelCommand()
        {
            DialogWindow.Close();
        }
    }
}
