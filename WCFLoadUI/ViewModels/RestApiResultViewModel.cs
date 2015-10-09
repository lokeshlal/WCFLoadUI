#region File Information/History
// <copyright file="RestApiResultViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using WCFLoadUI.Base;

namespace WCFLoadUI.ViewModels
{
    class RestApiResultViewModel : BaseViewModel
    {
        #region private fields
        private const string WindowTitleDefault = "Enter Service Url";
        private string _windowTitle = WindowTitleDefault;
        private string _status = string.Empty;
        private string _response = string.Empty;
        private string _guid;
        private bool _isAddedToFunctionalTestCase;
        #endregion

        #region

        public RestApiResultViewModel(string status, string response, string guid)
        {
            Status = status;
            Response = response;
            Guid = guid;
            _isAddedToFunctionalTestCase =
                WCFLoad.TestEngine.TestPackage.RestMethods.Find(r => r.Guid == guid).IsAddedToFunctional;
        }
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

        public string Status
        {
            get { return _status; }
            set
            {
                _status = string.Format("Status:  {0}", value);
                NotifyOfPropertyChange(() => Status);
            }
        }

        public string Response
        {
            get { return _response; }
            set
            {
                _response = value;
                NotifyOfPropertyChange(() => Response);
            }
        }

        public bool IsAddButtonEnabled
        {
            get { return !_isAddedToFunctionalTestCase; }
        }

        public bool IsRemoveButtonEnabled
        {
            get { return _isAddedToFunctionalTestCase; }
        }

        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        #endregion

        #region button events

        public void AddFunctional()
        {
            WCFLoad.TestEngine.TestPackage.RestMethods.Find(r => r.Guid == Guid).IsAddedToFunctional = true;
            WCFLoad.TestEngine.TestPackage.RestMethods.Find(r => r.Guid == Guid).MethodOutput = Response;
            _isAddedToFunctionalTestCase = !_isAddedToFunctionalTestCase;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            NotifyOfPropertyChange(() => IsAddButtonEnabled);
            NotifyOfPropertyChange(() => IsRemoveButtonEnabled);
        }

        public void RemoveFunctional()
        {
            WCFLoad.TestEngine.TestPackage.RestMethods.Find(r => r.Guid == Guid).IsAddedToFunctional = false;
            WCFLoad.TestEngine.TestPackage.RestMethods.Find(r => r.Guid == Guid).MethodOutput = string.Empty;
            _isAddedToFunctionalTestCase = !_isAddedToFunctionalTestCase;
            UpdateButtons();
        }

        #endregion
    }
}
