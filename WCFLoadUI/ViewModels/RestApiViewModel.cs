#region File Information/History
// <copyright file="RestApiViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Common;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using Test = WCFLoad.TestEngine;
using Common.Infrastructure.Entities;

namespace WCFLoadUI.ViewModels
{
    public class RestApiViewModel : BaseViewModel
    {


        #region private properties

        private const string WindowTitleDefault = "Add Rest API";
        private string _windowTitle = WindowTitleDefault;
        private List<RestMethod> _restMethodList;
        private RestMethod _doubleClickSelectedRestMethod;
        private RestMethod _selectedRestMethod;
        private bool _isPayloadVisible;
        private bool _isExisting;
        private string _existingGuid;
        private readonly string[] _names = new string[] { @"text/plain", @"application/json", @"application/xml", @"text/html" };
        private bool _isTestEnabled;
        private string _selectedContentItem;

        #endregion

        #region public properties
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                NotifyOfPropertyChange(() => WindowTitle);
            }
        }

        public List<RestMethod> RestMethodList
        {
            get { return _restMethodList; }
            set
            {
                _restMethodList = value;
                NotifyOfPropertyChange(() => RestMethodList);
            }
        }

        public RestMethod SelectedRestMethod
        {
            get { return _selectedRestMethod; }
            set
            {
                _selectedRestMethod = value;
                NotifyOfPropertyChange(() => SelectedRestMethod);
            }
        }

        public RestMethod DoubleClickSelectedRestMethod
        {
            get { return _doubleClickSelectedRestMethod; }
            set
            {
                _doubleClickSelectedRestMethod = value;
                NotifyOfPropertyChange(() => DoubleClickSelectedRestMethod);
            }
        }

        public bool IsPayloadVisible
        {
            get { return _isPayloadVisible; }
            set
            {
                _isPayloadVisible = value;
                NotifyOfPropertyChange(() => IsPayloadVisible);
            }
        }

        public string SelectedContentItem
        {
            get { return _selectedContentItem; }
            set
            {
                if (value == null)
                    return;
                _selectedContentItem = value;
                SelectedRestMethod.ContentType = _selectedContentItem;
                NotifyOfPropertyChange(() => SelectedContentItem);
            }
        }

        public string[] Names
        {
            get
            {
                return _names;
            }
        }

        public bool IsExisting
        {
            get { return _isExisting; }
            set
            {
                _isExisting = value;
                NotifyOfPropertyChange(() => IsExisting);
                NotifyOfPropertyChange(() => IsNew);
            }
        }
        public bool IsNew
        {
            get { return !_isExisting; }
        }

        public bool IsTestEnabled
        {
            get { return _isTestEnabled; }
            set
            {
                _isTestEnabled = value;
                NotifyOfPropertyChange(() => IsTestEnabled);
            }
        }

        #endregion

        #region window events
        /// <summary>
        /// On View loaded event of caliburn
        /// </summary>
        /// <param name="view"></param>
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            //Load rest API table
            SelectedRestMethod = new RestMethod();
            SelectedContentItem = SelectedRestMethod.ContentType;
            RestMethodList = Test.TestPackage.RestMethods;
        }
        #endregion

        #region List events

        public void MethodSelected()
        {
            SelectedRestMethod = DoubleClickSelectedRestMethod;
            if (SelectedRestMethod != null && !string.IsNullOrEmpty(SelectedRestMethod.Url))
            {
                //_isExisting = true;
                IsExisting = true;
                _existingGuid = SelectedRestMethod.Guid;
                SelectedContentItem = SelectedRestMethod.ContentType;
                IsTestEnabled = true;
                NotifyOfPropertyChange(() => SelectedRestMethod);
            }
            else
            {
                SelectedRestMethod = new RestMethod();
                SelectedContentItem = SelectedRestMethod.ContentType;
            }
        }
        #endregion

        #region radio button events

        public void RadioChecked(RoutedEventArgs eventArgs)
        {
            var requestTypeSelected = ((RadioButton)eventArgs.Source).Content as string;
            if (requestTypeSelected != null)
            {
                SelectedRestMethod.Type = (RequestType)Enum.Parse(typeof(RequestType), requestTypeSelected);

                switch (SelectedRestMethod.Type)
                {
                    case RequestType.Get:
                    case RequestType.Head:
                        IsPayloadVisible = false;
                        break;
                    default:
                        IsPayloadVisible = true;
                        break;
                }
            }
        }
        #endregion

        #region button events



        public void SavePayload()
        {

            if (!IsExisting)
            //if (!_isExisting)
            {
                Test.TestPackage.RestMethods.Add(SelectedRestMethod);
            }
            else
            {
                Test.TestPackage.RestMethods.RemoveAll(r => r.Guid == _existingGuid);
                Test.TestPackage.RestMethods.Add(SelectedRestMethod);
            }
            RestMethodList = null;
            RestMethodList = Test.TestPackage.RestMethods;
            //string s  = Environment.NewLine;
            ResetView();
        }

        public void CancelPayload()
        {
            ResetView();
        }

        public void DeletePayload()
        {
            if (IsExisting)
            {
                if ((from sc in Test.TestPackage.Scenarios
                     from so in sc.ScenarioOrder
                     where so.MethodGuid == _existingGuid
                     select 1).Any())
                {
                    MessageBox.Show("Value is used in a scenario. Please remove it from scenario first.");
                    return;
                }

                Test.TestPackage.RestMethods.RemoveAll(r => r.Guid == _existingGuid);
            }
            RestMethodList = null;
            RestMethodList = Test.TestPackage.RestMethods;
            ResetView();
        }
        private void ResetView()
        {
            IsExisting = false;
            //_isExisting = false;
            _existingGuid = string.Empty;
            SelectedRestMethod = new RestMethod();
            SelectedContentItem = SelectedRestMethod.ContentType;
            NotifyOfPropertyChange(() => RestMethodList);
        }

        public void TestRestApi()
        {
            RestMethodResponse response = Test.InvokeRestApi(SelectedRestMethod, false);
            Controller.ShowRestApiResultView(response.StatusCode, response.Response, SelectedRestMethod.Guid);
        }

        #endregion

        #region grid events
        public void DeleteFromHeaderList(object d)
        {
            KeyValue val = d as KeyValue;
            if (val != null)
            {
                List<KeyValue> temp = SelectedRestMethod.Headers.FindAll(h => h.Key != val.Key && h.Value != val.Value).ToList();
                SelectedRestMethod.Headers = temp;
                NotifyOfPropertyChange(() => SelectedRestMethod.Headers);
                NotifyOfPropertyChange(() => SelectedRestMethod);
            }
        }

        public void DeleteFromPayloadList(object d)
        {
            KeyValue val = d as KeyValue;
            if (val != null)
            {
                List<KeyValue> temp = SelectedRestMethod.PayloadValues.FindAll(h => h.Key != val.Key && h.Value != val.Value).ToList();
                SelectedRestMethod.PayloadValues = temp;
                NotifyOfPropertyChange(() => SelectedRestMethod.Headers);
                NotifyOfPropertyChange(() => SelectedRestMethod);
            }

        }
        public void AddNewHeader()
        {
            List<KeyValue> temp = SelectedRestMethod.Headers.ToList();
            temp.Add(new KeyValue());
            SelectedRestMethod.Headers = temp;
            NotifyOfPropertyChange(() => SelectedRestMethod.Headers);
            NotifyOfPropertyChange(() => SelectedRestMethod);
        }

        public void AddNewValue()
        {
            List<KeyValue> temp = SelectedRestMethod.PayloadValues.ToList();
            temp.Add(new KeyValue());
            SelectedRestMethod.PayloadValues = temp;
            NotifyOfPropertyChange(() => SelectedRestMethod.PayloadValues);
            NotifyOfPropertyChange(() => SelectedRestMethod);
        }
        #endregion

    }
}
