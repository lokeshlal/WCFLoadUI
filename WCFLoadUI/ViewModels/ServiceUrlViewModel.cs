﻿#region File Information/History
// <copyright file="ServiceUrlViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// 08/21/2015: Added support to start a rest project without adding WSDL first
// </history>
#endregion
using WCFLoadUI.Base;

namespace WCFLoadUI.ViewModels
{
    public class ServiceUrlViewModel : BaseViewModel
    {
        #region private fields
        private const string WindowTitleDefault = "Enter Service Url";
        private string _windowTitle = WindowTitleDefault;
        private string _serviceUrl = string.Empty;
        private bool _setFocusOnTextBox;
        private bool _isAddARestEnabled;
        #endregion

        #region constructor
        public ServiceUrlViewModel() { }

        public ServiceUrlViewModel(bool isAddARestEnabled)
        {
            IsAddARestEnabled = isAddARestEnabled;
        }
        #endregion

        #region public properties
        private bool SetFocusOnTextBox
        {
            get
            {
                return _setFocusOnTextBox;
            }
            set
            {
                _setFocusOnTextBox = value;
                NotifyOfPropertyChange(() => SetFocusOnTextBox);
            }
        }
        public string ServiceUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_serviceUrl))
                    return string.Empty;
                return _serviceUrl;
            }
            set
            {
                _serviceUrl = value;
            }
        }

        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                NotifyOfPropertyChange(() => WindowTitle);
            }
        }

        public bool IsAddARestEnabled
        {
            get { return _isAddARestEnabled; }
            set
            {
                _isAddARestEnabled = value;
                NotifyOfPropertyChange(() => IsAddARestEnabled);
            }
        }

        #endregion

        #region View Events
        /// <summary>
        /// Add service url to create a new project
        /// </summary>
        public void AddServiceUrl()
        {
            DialogWindow.DialogResult = true;
            ExecuteCancelCommand();
        }

        public void AddARest()
        {
            ServiceUrl = "AddARest";
            DialogWindow.DialogResult = true;
            ExecuteCancelCommand();
        }


        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            SetFocusOnTextBox = true;
        }


        #endregion
    }
}
