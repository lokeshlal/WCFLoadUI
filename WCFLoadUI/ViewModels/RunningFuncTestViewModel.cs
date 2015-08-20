#region File Information/History
// <copyright file="RunningFuncTestViewModel.cs" project="WCFLoadUI" >
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
using System.Collections.ObjectModel;
using System.Windows;
using WCFLoad;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{
    public class RunningFuncTestViewModel : BaseViewModel
    {
        #region private fields
        private ObservableCollection<FunctionalTestResults> _testResultList = new ObservableCollection<FunctionalTestResults>();
        private Dictionary<string, int> _methodValuesForReport = new Dictionary<string, int>();
        private FunctionalTestResults _testResultSelected = new FunctionalTestResults();
        private bool _canViewResult;
        private const string WindowTitleDefault = "Test Running...";
        private string _windowTitle = WindowTitleDefault;
        #endregion

        #region public properties
        public bool CanViewResult
        {
            get
            {
                return _canViewResult;
            }
            set
            {
                _canViewResult = value;
                NotifyOfPropertyChange(() => CanViewResult);
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

        public Dictionary<string, int> MethodValuesForReport
        {
            get
            {
                return _methodValuesForReport;
            }
            set
            {
                _methodValuesForReport = value;
                NotifyOfPropertyChange(() => MethodValuesForReport);
            }
        }

        public FunctionalTestResults TestResultSelected
        {
            get
            {
                return _testResultSelected;
            }
            set
            {
                _testResultSelected = value;
                NotifyOfPropertyChange(() => TestResultSelected);
            }
        }

        public ObservableCollection<FunctionalTestResults> TestResultList
        {
            get
            {
                return _testResultList;
            }
            set
            {
                _testResultList = value;
                NotifyOfPropertyChange(() => TestResultList);
            }
        }
        #endregion

        #region events
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            CanViewResult = false;
            Test.ClearResultHandler();
            TestResultList = new ObservableCollection<FunctionalTestResults>();
            Controller.FunctionalTestUpdated += Controller_FunctionalTestUpdated;
            Controller.InvokeAllFunctionTest();
        }

        void Controller_FunctionalTestUpdated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TestResultList = new ObservableCollection<FunctionalTestResults>(Controller.FunctionalTestResults);
                if (Controller.IsFunctionalTestCompleted)
                {
                    _canViewResult = true;
                    WindowTitle = "Test Completed";
                }
            });
        }

        public void TestResultClicked()
        {
            if(TestResultSelected != null)
            {
                string actual = TestResultSelected.Actual;
                string expected = TestResultSelected.Expected;
                bool passfail = TestResultSelected.PassFailStatus;
                string input = TestResultSelected.Input;

                Controller.ShowFunctionResultDetailViewDialog(actual, expected, passfail, input);
            }
        }
        #endregion
    }
}
