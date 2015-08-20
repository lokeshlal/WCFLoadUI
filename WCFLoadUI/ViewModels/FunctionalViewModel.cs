#region File Information/History
// <copyright file="FunctionalViewModel.cs" project="WCFLoadUI" >
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
using System.Linq;
using System.Reflection;
using System.Windows;
using Common;
using WCFLoad.Helper;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.Helpers;
using WCFLoadUI.TypeToBind;
using Test = WCFLoad.Test;
// ReSharper disable PossibleMultipleEnumeration

namespace WCFLoadUI.ViewModels
{
    public class FunctionalViewModel : BaseViewModel
    {
        #region private fields
        private const string WindowTitleDefault = "Functional Test";
        private ObservableCollection<PrimitiveControlViewModel> _methodFields = new ObservableCollection<PrimitiveControlViewModel>();

        private string _windowTitle = WindowTitleDefault;
        private List<string> _methodsNames = new List<string>();
        private List<string> _valuesNames = new List<string>();
        private string _doubleClickSelectedMethodValueName;
        private string _selectedMethodValueName;
        private string _doubleClickSelectedMethodName;
        private string _selectedMethodName;
        private ControlView _selectedControlViewBindingObject = new ControlView();
        private ObservableCollection<ControlView> _controlViewBindingObject = new ObservableCollection<ControlView>();
        private bool _isExisting;
        private int _existingIndex = -1;
        private string _existingGuid = string.Empty;

        private bool _isValueGridPopulated;

        private string _selectedSpecialValue = string.Empty;

        private string _methodOutput = string.Empty;
        private string _methodInput = string.Empty;
        private string _currentServiceGuid;

        private List<string> _serviceUrlList;
        private string _serviceUrlSelected;

        #endregion

        #region public properties

        public bool IsValueGridPopulated
        {
            get { return _isValueGridPopulated; }
            set
            {
                _isValueGridPopulated = value;
                NotifyOfPropertyChange(() => IsValueGridPopulated);
            }
        }

        public string MethodOutput
        {
            get
            {
                return _methodOutput;
            }
            set
            {
                _methodOutput = value;
                NotifyOfPropertyChange(() => MethodOutput);
            }
        }

        public string MethodInput
        {
            get
            {
                return _methodInput;
            }
            set
            {
                _methodInput = value;
                NotifyOfPropertyChange(() => MethodInput);
            }
        }

        public ControlView SelectedControlViewBindingObject
        {
            get
            {
                return _selectedControlViewBindingObject;
            }
            set
            {
                _selectedControlViewBindingObject = value;
                NotifyOfPropertyChange(() => SelectedControlViewBindingObject);
            }

        }
        public string SelectedSpecialValue
        {
            get
            {
                return _selectedSpecialValue;
            }
            set
            {
                _selectedSpecialValue = value;
                NotifyOfPropertyChange(() => SelectedSpecialValue);
            }
        }

        public string SelectedMethodName
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedMethodName))
                    return string.Empty;
                return _selectedMethodName;
            }
            set
            {
                _selectedMethodName = value;
                NotifyOfPropertyChange(() => SelectedMethodName);
            }
        }

        public string DoubleClickSelectedMethodName
        {
            get
            {
                return _doubleClickSelectedMethodName;
            }
            set
            {
                _doubleClickSelectedMethodName = value;
                NotifyOfPropertyChange(() => DoubleClickSelectedMethodName);
            }

        }


        public string SelectedMethodValueName
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedMethodValueName))
                    return string.Empty;
                return _selectedMethodValueName;
            }
            set
            {
                _selectedMethodValueName = value;
                NotifyOfPropertyChange(() => SelectedMethodValueName);
            }
        }

        public string DoubleClickSelectedMethodValueName
        {
            get
            {
                if (string.IsNullOrEmpty(_doubleClickSelectedMethodValueName))
                    return string.Empty;
                return _doubleClickSelectedMethodValueName;
            }
            set
            {
                _doubleClickSelectedMethodValueName = value;
                NotifyOfPropertyChange(() => DoubleClickSelectedMethodValueName);
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

        public List<string> SpecialValueList
        {
            get
            {
                return SpecialValue.GetSpecialValueList();
            }
        }

        public List<string> FunctionalTestCases
        {
            get
            {
                return _valuesNames;
            }
            set
            {
                _valuesNames = value;
                NotifyOfPropertyChange(() => FunctionalTestCases);
            }
        }

        public List<string> ServiceMethods
        {
            get
            {
                return _methodsNames;
            }
            set
            {
                _methodsNames = value;
                NotifyOfPropertyChange(() => ServiceMethods);
            }
        }

        public bool CanDisplayDeleteValues
        {
            get
            {
                return _isExisting;
            }
        }

        public ObservableCollection<PrimitiveControlViewModel> MethodFields
        {
            get
            {
                return _methodFields;
            }
            set
            {
                _methodFields = value;
                NotifyOfPropertyChange(() => MethodFields);
            }
        }

        public ObservableCollection<ControlView> ControlViewBindingObject
        {
            get
            {
                return _controlViewBindingObject;
            }
            set
            {
                _controlViewBindingObject = value;
                NotifyOfPropertyChange(() => ControlViewBindingObject);
            }
        }


        public string CurrentServiceGuid
        {
            get { return _currentServiceGuid; }
            set
            {
                _currentServiceGuid = value;
                NotifyOfPropertyChange(() => CurrentServiceGuid);
            }
        }
        public List<string> ServiceUrlList
        {
            get { return _serviceUrlList; }
            set
            {
                _serviceUrlList = value;
                NotifyOfPropertyChange(() => ServiceUrlList);
            }
        }

        public string ServiceUrlSelected
        {
            get { return _serviceUrlSelected; }
            set
            {
                _serviceUrlSelected = value;
                NotifyOfPropertyChange(() => ServiceUrlSelected);
            }
        }

        #endregion
        #region view events
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            CurrentServiceGuid = Test.TestPackage.Suites[0].Guid;
            ServiceMethods = Controller.GetServiceMethods(CurrentServiceGuid);
            SetServiceUrl(CurrentServiceGuid);
        }

        #endregion

        #region list events

        private void SetServiceUrl(string guid)
        {
            ServiceUrlList = Test.TestPackage.Suites.Select(s => s.ServiceUrl).ToList();
            ServiceUrlSelected = Test.TestPackage.Suites.Find(s => s.Guid == guid).ServiceUrl;
        }

        public void ServiceChanged()
        {
            //ServiceUrlSelected
            var suite = Test.TestPackage.Suites.Find(s => s.ServiceUrl.ToLower() == ServiceUrlSelected.ToLower());
            var guid = suite.Guid;
            CurrentServiceGuid = guid;
            var sm = Test.GetAllServiceMethods(guid);
            ServiceMethods = sm;
            SetServiceUrl(guid);
            ResetForm();
        }
        private void ResetForm()
        {
            FunctionalTestCases = new List<string>();
            ControlViewBindingObject = new ObservableCollection<ControlView>();
            MethodOutput = string.Empty;
            SelectedMethodName = string.Empty;
            DoubleClickSelectedMethodName = String.Empty;
            DoubleClickSelectedMethodValueName = string.Empty;
            SelectedMethodValueName = String.Empty;
            IsValueGridPopulated = false;
            _existingGuid = string.Empty;
            _existingIndex = -1;
            _isExisting = false;
        }


        public void ValueSelected()
        {
            string itemPrepandString = "Functional Test ";

            //Capture the value selected
            SelectedMethodValueName = DoubleClickSelectedMethodValueName;

            if (string.IsNullOrEmpty(SelectedMethodValueName))
                return;
            //int selectedIndex = Convert.ToInt32(SelectedMethodValueName.Replace(itemPrepandString, string.Empty)) - 1;
            string selectedMethodGuid = Convert.ToString(SelectedMethodValueName.Replace(itemPrepandString, string.Empty));

            var testNode = (from s in Test.TestPackage.Suites
                            from test in s.FunctionalTests
                            where test.Service.MethodName == SelectedMethodName
                            && s.Guid == CurrentServiceGuid
                            select test).ToList()[0];

            Value selectedValueObj = testNode.Service.Values.ValueList.Find(v => v.Guid == selectedMethodGuid); //[selectedIndex];

            ParameterInfo[] parameters = Controller.GetMethodParameters(SelectedMethodName, CurrentServiceGuid);
            _controlViewBindingObject.Clear();
            _isExisting = true;
            //_existingIndex = selectedIndex;
            _existingGuid = selectedMethodGuid;

            IsValueGridPopulated = true;

            #region method output
            MethodOutput = !string.IsNullOrEmpty(selectedValueObj.MethodOutput) ? selectedValueObj.MethodOutput : string.Empty;
            #endregion

            ControlsHelper.SetCollectionOnValue(selectedValueObj, parameters, _controlViewBindingObject, CurrentServiceGuid);

            NotifyOfPropertyChange(() => ControlViewBindingObject);
            NotifyOfPropertyChange(() => CanDisplayDeleteValues);


        }


        public void MethodSelected()
        {
            //Caapture the selected method name
            SelectedMethodName = DoubleClickSelectedMethodName;

            if (string.IsNullOrEmpty(_selectedMethodName))
            {
                var tempcontrolViewBindingObject = new ObservableCollection<ControlView>();
                ControlViewBindingObject = tempcontrolViewBindingObject;

                List<string> tempvalues = new List<string>();
                FunctionalTestCases = tempvalues;

                return;
            }
            ParameterInfo[] parameters = Controller.GetMethodParameters(_selectedMethodName, CurrentServiceGuid);
            //string s = selectedMethodName;
            //now how to access the selected row after the double click event?
            _controlViewBindingObject.Clear();
            _isExisting = false;
            _existingIndex = -1;
            _existingGuid = String.Empty;

            MethodOutput = string.Empty;

            ControlsHelper.SetEmptyCollectionOnMethodSelect(parameters, _controlViewBindingObject, CurrentServiceGuid);

            NotifyOfPropertyChange(() => ControlViewBindingObject);

            var testNodes = (from s in Test.TestPackage.Suites
                             from test in s.FunctionalTests
                             where test.Service.MethodName == SelectedMethodName
                             && s.Guid == CurrentServiceGuid
                             select test).ToList();


            int maxValueNodes = 0;
            if (testNodes.Count == 1)
            {
                maxValueNodes = testNodes[0].Service.Values.ValueList.Count;
            }
            List<string> values = new List<string>();
            for (int i = 0; i < maxValueNodes; i++)
            {
                //values.Add(string.Format("Functional Test {0}", i + 1));
                values.Add(string.Format("Functional Test {0}", testNodes.ToList()[0].Service.Values.ValueList[i].Guid));
            }
            FunctionalTestCases = values;
            IsValueGridPopulated = true;

            NotifyOfPropertyChange(() => CanDisplayDeleteValues);

        }
        #endregion

        #region Button Events


        public void SaveFuncTest()
        {

        }


        public void SaveValues()
        {
            try
            {
                var valueNode = ControlsHelper.GetParameterValueFromControlViewList(ControlViewBindingObject);
                //set method output
                valueNode.MethodOutput = MethodOutput;

                global::Common.Test newTest = new global::Common.Test();
                var testNodes = (from s in Test.TestPackage.Suites
                                 from test in s.FunctionalTests
                                 where test.Service.MethodName == SelectedMethodName
                                 select test);


                int maxValueNodes;
                if (!_isExisting)
                {
                    valueNode.Guid = Guid.NewGuid().ToString();

                    if (testNodes.Count() == 1)
                    {
                        testNodes.ToList()[0].Service.Values.ValueList.Add(valueNode);
                        maxValueNodes = testNodes.ToList()[0].Service.Values.ValueList.Count;
                    }
                    else
                    {
                        newTest.Service = new Service
                        {
                            MethodName = SelectedMethodName,
                            IsAsync = false,
                            Values = new Values { ValueList = new List<Value> { valueNode } }
                        };

                        Test.TestPackage.Suites.Find(s => s.Guid == CurrentServiceGuid).FunctionalTests.Add(newTest);
                        maxValueNodes = 1;
                    }
                }
                else
                {
                    valueNode.Guid = testNodes.ToList()[0].Service.Values.ValueList.Find(v => v.Guid == _existingGuid).Guid;

                    _existingIndex = testNodes.ToList()[0].Service.Values.ValueList.IndexOf(
                                            testNodes.ToList()[0].Service.Values.ValueList.Find(v => v.Guid == _existingGuid));

                    if (_existingIndex > -1)
                    {
                        testNodes.ToList()[0].Service.Values.ValueList[_existingIndex] = valueNode;
                    }
                    maxValueNodes = testNodes.ToList()[0].Service.Values.ValueList.Count;
                }
                _isExisting = false;
                _existingIndex = -1;
                _existingGuid = string.Empty;
                List<string> values = new List<string>();
                for (int i = 0; i < maxValueNodes; i++)
                {
                    values.Add(string.Format("Functional Test {0}", testNodes.ToList()[0].Service.Values.ValueList[i].Guid));
                    //values.Add(string.Format("Functional Test {0}", i + 1));
                }
                FunctionalTestCases = values;

                MethodSelected();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in SaveValues Method");
            }
            finally
            {
                NotifyOfPropertyChange(() => CanDisplayDeleteValues);
            }
        }

        public void DeleteValues()
        {
            try
            {
                if (!_isExisting) return;

                var testNodes = (from s in Test.TestPackage.Suites
                                from test in s.FunctionalTests
                                 where test.Service.MethodName == SelectedMethodName
                                 && s.Guid == CurrentServiceGuid
                                 select test);

                testNodes.ToList()[0].Service.Values.ValueList.Remove(
                    testNodes.ToList()[0].Service.Values.ValueList.Find(v => v.Guid == _existingGuid));

                var maxValueNodes = testNodes.ToList()[0].Service.Values.ValueList.Count;
                if (maxValueNodes == 0)
                {
                    Test.TestPackage.Suites.Find(s => s.Guid == CurrentServiceGuid)
                        .FunctionalTests.RemoveAll(t => t.Service.MethodName == _selectedMethodName);
                    //Test.Suite.FunctionalTests.RemoveAll(t => t.Service.MethodName == _selectedMethodName);
                }
                var values = new List<string>();
                for (int i = 0; i < maxValueNodes; i++)
                {
                    values.Add(string.Format("Functional Test {0}", testNodes.ToList()[0].Service.Values.ValueList[i].Guid));
                }
                FunctionalTestCases = values;

                MethodSelected();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                _isExisting = false;
                _existingIndex = -1;
                _existingGuid = String.Empty;
                NotifyOfPropertyChange(() => CanDisplayDeleteValues);
            }
        }


        public void AddSelectedValue()
        {
            SpecialValue.CopiedValue = SpecialValue.GetValueAgainstSpecialValue(SelectedSpecialValue);
        }

        public void ResetValues()
        {
            MethodSelected();
        }

        public void ShowOutput()
        {
            try
            {
                global::Common.Test newTest = new global::Common.Test();

                var valueNode = ControlsHelper.GetParameterValueFromControlViewList(ControlViewBindingObject);

                newTest.Service = new Service
                {
                    MethodName = SelectedMethodName,
                    IsAsync = false,
                    Values = new Values { ValueList = new List<Value> { valueNode } }
                };
                string input;
                var output = Controller.InvokeFunctionalTest(newTest, 0, CurrentServiceGuid, out input);
                MethodOutput = output;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured during execution of the method");
                MethodOutput = ex.Message;
            }
        }
        #endregion
    }
}
