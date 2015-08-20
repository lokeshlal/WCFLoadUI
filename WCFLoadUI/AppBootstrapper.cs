#region File Information/History
// <copyright file="AppBootstrapper.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using WCFLoadUI.ViewModels;
using System.Diagnostics;
using System.Text;

namespace WCFLoadUI
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e != null && e.Exception != null)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "WCFLoadUI";

                    StringBuilder errorInformation = new StringBuilder();
                    errorInformation.AppendLine(string.Format("{0}{1}{2}", e.Exception.Message, Environment.NewLine,
                        e.Exception.StackTrace));
                    var innerException = e.Exception.InnerException;
                    while (innerException != null)
                    {
                        errorInformation.AppendLine(string.Format("{0}{1}{2}", innerException.Message, Environment.NewLine,
                            innerException.StackTrace));
                        innerException = innerException.InnerException;
                    }

                    eventLog.WriteEntry(errorInformation.ToString()
                        , EventLogEntryType.Error
                        , 101
                        , 1);
                }
                MessageBox.Show("Error occured. Please check event viewer for more details");
                e.Handled = true;
            }
            //base.OnUnhandledException(sender, e);
        }
    }
}
