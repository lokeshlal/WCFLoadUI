#region File Information/History
// <copyright file="MainViewModel.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// 08/21/2015: Modified to add support for rest without wsdl added first
// </history>
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Common;
using Microsoft.Win32;
using WCFLoad.Helper;
using WCFLoad.Tokens;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.Common;
using WCFLoadUI.Helpers;
using WCFLoadUI.TypeToBind;
using Test = WCFLoad.Test;
// ReSharper disable PossibleMultipleEnumeration

namespace WCFLoadUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region private properties
        private const string WindowTitleDefault = "WCF Load UI";

        private string _windowTitle = WindowTitleDefault;
        private List<string> _methodsNames = new List<string>();
        private List<string> _valuesNames = new List<string>();
        private string _doubleClickSelectedMethodValueName;
        private string _selectedMethodValueName;
        private string _doubleClickSelectedMethodName;
        private string _selectedMethodName;
        private StackPanel _methodFieldPanel;
        private ControlView _selectedControlViewBindingObject = new ControlView();
        private ObservableCollection<ControlView> _controlViewBindingObject = new ObservableCollection<ControlView>();
        private bool _isExisting;
        private int _existingIndex = -1;
        private string _existingGuid = string.Empty;
        private int _noOfClients = 10;
        private int _duration = 10;
        private int _delayRangeStart = 5000;
        private int _delayRangeEnd = 5100;
        private List<string> _bindings = new List<string>();
        private string _selectedBinding;
        private string _resultFilePath = @"c:\perfresult.txt";
        private bool _canRun;
        private string _currentServiceGuid;
        private string _perfXmlfilePath = string.Empty;
        private string _selectedSpecialValue = string.Empty;

        private ObservableCollection<PrimitiveControlViewModel> _methodFields = new ObservableCollection<PrimitiveControlViewModel>();

        public bool IsMethodSelected;

        private bool _isTestLoadedOrStarted;
        private bool _isValueGridPopulated;

        private List<string> _serviceUrlList;
        private string _serviceUrlSelected;
        #endregion

        #region public properties
        public bool CanRunPerfTest
        {
            get
            {
                return _canRun;
            }
        }

        public bool CanDisplayDeleteValues
        {
            get
            {
                return _isExisting;
            }
        }


        public bool IsValueGridPopulated
        {
            get { return _isValueGridPopulated; }
            set
            {
                _isValueGridPopulated = value;
                NotifyOfPropertyChange(() => IsValueGridPopulated);
            }
        }

        public bool IsTestLoadedOrStarted
        {
            get { return _isTestLoadedOrStarted; }
            set
            {
                _isTestLoadedOrStarted = value;
                NotifyOfPropertyChange(() => IsTestLoadedOrStarted);
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

        public string ResultFilePath
        {
            get
            {
                return _resultFilePath;
            }
            set
            {
                _resultFilePath = value;
                NotifyOfPropertyChange(() => ResultFilePath);
            }
        }
        public string SelectedBinding
        {
            get
            {
                return _selectedBinding;
            }
            set
            {
                _selectedBinding = value;
                if (!string.IsNullOrEmpty(SelectedBinding))
                {
                    Test.TestPackage.Suites.Find(s => s.Guid == CurrentServiceGuid).BindingToTest = SelectedBinding;
                }
                NotifyOfPropertyChange(() => SelectedBinding);
            }
        }

        public string SelectedBindingToSave
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedBinding))
                    return string.Empty;
                return SelectedBinding; //.Replace("[", string.Empty).Replace("]", string.Empty).Split(new char[] { ',' })[0];
            }
        }

        public StackPanel MethodFieldPanel
        {
            get
            {
                return _methodFieldPanel;
            }
            set
            {
                _methodFieldPanel = value;
                NotifyOfPropertyChange(() => MethodFieldPanel);
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

        public List<string> ServiceMethodValues
        {
            get
            {
                return _valuesNames;
            }
            set
            {
                _valuesNames = value;
                NotifyOfPropertyChange(() => ServiceMethodValues);
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

        public int NoOfClients
        {
            get
            {
                return _noOfClients;
            }
            set
            {
                _noOfClients = value;
                NotifyOfPropertyChange(() => NoOfClients);
            }
        }

        public int Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                _duration = value;
                NotifyOfPropertyChange(() => Duration);
            }
        }

        public int DelayRangeStart
        {
            get
            {
                return _delayRangeStart;
            }
            set
            {
                _delayRangeStart = value;
                NotifyOfPropertyChange(() => DelayRangeStart);
            }
        }

        public int DelayRangeEnd
        {
            get
            {
                return _delayRangeEnd;
            }
            set
            {
                _delayRangeEnd = value;
                NotifyOfPropertyChange(() => DelayRangeEnd);
            }
        }

        public List<string> Bindings
        {
            get
            {
                return _bindings;
            }
            set
            {
                _bindings = value;
                NotifyOfPropertyChange(() => Bindings);
            }
        }

        public string CurrentServiceGuid
        {
            get { return _currentServiceGuid; }
            set
            {
                if (value == _currentServiceGuid) return;
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

        #region Menu Events
        /// <summary>
        /// Handles the exit menu option
        /// </summary>
        public void MenuExit()
        {
            ExecuteCancelCommand();
        }

        /// <summary>
        /// Handles the New Menu option click
        /// </summary>
        public void MenuNew()
        {
            string guid;
            var sm = Controller.ShowServiceUrlViewDialog(Constants.Serviceurl, out guid, true);
            if (sm != null)
            {
                ServiceMethods = sm;
                CurrentServiceGuid = guid;
                Bindings = Test.TestPackage.Suites.Find(s => s.Guid == guid).EndPoints.Keys.ToList();

                SetServiceUrl(guid);

                _perfXmlfilePath = string.Empty;
                _canRun = false;
                NotifyOfPropertyChange(() => CanRunPerfTest);

                ResetForm();
            }
            else if (guid == "AddARest")
            {
                //rest service add request
                CurrentServiceGuid = string.Empty;
                _perfXmlfilePath = string.Empty;
                _canRun = false;
                NotifyOfPropertyChange(() => CanRunPerfTest);

                ResetForm();
            }
        }

        private void SetServiceUrl(string guid)
        {
            ServiceUrlList = Test.TestPackage.Suites.Select(s => s.ServiceUrl).ToList();
            ServiceUrlSelected = Test.TestPackage.Suites.Find(s => s.Guid == guid).ServiceUrl;
        }

        public void MenuAddRestUrl()
        {
            Controller.ShowRestApiDialog();
        }

        public void MenuAddService()
        {
            string guid;
            var sm = Controller.ShowServiceUrlViewDialogForAdd(Constants.Serviceurl, out guid);
            if (sm != null)
            {
                ServiceMethods = sm;
                CurrentServiceGuid = guid;
                Bindings = Test.TestPackage.Suites.Find(s => s.Guid == guid).EndPoints.Keys.ToList();
                SetServiceUrl(guid);
                NotifyOfPropertyChange(() => CanRunPerfTest);
                ResetFormServiceAdded();
            }
        }


        private void ResetForm()
        {
            IsTestLoadedOrStarted = true;

            ServiceMethodValues = new List<string>();
            ControlViewBindingObject = new ObservableCollection<ControlView>();

            SelectedMethodName = string.Empty;
            DoubleClickSelectedMethodName = String.Empty;
            DoubleClickSelectedMethodValueName = string.Empty;
            SelectedMethodValueName = String.Empty;
            IsMethodSelected = false;
            IsValueGridPopulated = false;
            _existingGuid = string.Empty;
            _existingIndex = -1;
            _isExisting = false;
        }
        private void ResetFormServiceAdded()
        {
            ServiceMethodValues = new List<string>();
            ControlViewBindingObject = new ObservableCollection<ControlView>();

            SelectedMethodName = string.Empty;
            DoubleClickSelectedMethodName = String.Empty;
            DoubleClickSelectedMethodValueName = string.Empty;
            SelectedMethodValueName = String.Empty;
            IsMethodSelected = false;
            IsValueGridPopulated = false;
            _existingGuid = string.Empty;
            _existingIndex = -1;
            _isExisting = false;
        }


        /// <summary>
        /// Handles the Open Menu option click
        /// </summary>
        public void MenuOpen()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Xml Files (*.xml)|*.xml" };
            if (openFileDialog.ShowDialog() == true)
            {
                _perfXmlfilePath = openFileDialog.FileName;
                ServiceMethods = Controller.LoadXmlFileAndGetAllMethods(_perfXmlfilePath);
                NoOfClients = Test.TestPackage.Clients;
                Duration = Test.TestPackage.Duration;
                DelayRangeStart = Test.TestPackage.DelayRangeStart;
                DelayRangeEnd = Test.TestPackage.DelayRangeEnd;
                ResultFilePath = Test.TestPackage.ResultFileName;
                if (Test.TestPackage.Suites.Count > 0)
                {
                    CurrentServiceGuid = Test.TestPackage.Suites[0].Guid;
                    Bindings = Test.TestPackage.Suites[0].EndPoints.Keys.ToList();
                    SelectedBinding = Test.TestPackage.Suites[0].BindingToTest;
                    SetServiceUrl(CurrentServiceGuid);
                }
                _canRun = true;
                NotifyOfPropertyChange(() => CanRunPerfTest);

                ResetForm();
            }
        }


        public void MenuAddFunctional()
        {
            Controller.ShowFunctionalViewDialog();
        }

        public void MenuAddScenario()
        {
            Controller.ShowScenarioViewDialog();
        }

        public void MenuAddNodes()
        {
            Controller.ShowAddNotesDialog();
        }

        public void MenuRunFunctional()
        {
            Controller.ShowRunningFuncTestViewDialog();
        }

        public void MenuRunPerformance()
        {
            CancellationTokens.ResetPerformanceCancellationToken();
            CancellationTokens.GetPerformanceCancellationToken();

            //run program on server
            Task.Factory.StartNew(Controller.RunTest);
            Controller.ShowRunningTestViewDialog();
        }

        #endregion


        #region list events

        public void ServiceChanged()
        {
            //ServiceUrlSelected
            var suite = Test.TestPackage.Suites.Find(s => s.ServiceUrl.ToLower() == ServiceUrlSelected.ToLower());
            var guid = suite.Guid;
            CurrentServiceGuid = guid;
            SelectedBinding = suite.BindingToTest;
            var sm = Test.GetAllServiceMethods(guid);
            ServiceMethods = sm;
            Bindings = Test.TestPackage.Suites.Find(s => s.Guid == guid).EndPoints.Keys.ToList();
            SetServiceUrl(guid);
            NotifyOfPropertyChange(() => CanRunPerfTest);
            ResetFormServiceAdded();
        }

        public void ValueSelected()
        {
            string itemPrepandString = "Value Set ";

            //Capture the value selected
            SelectedMethodValueName = DoubleClickSelectedMethodValueName;

            if (string.IsNullOrEmpty(SelectedMethodValueName))
                return;
            string selectedMethodGuid = Convert.ToString(SelectedMethodValueName.Replace(itemPrepandString, string.Empty));

            var testNode = (from s in Test.TestPackage.Suites
                            from test in s.Tests
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
                ServiceMethodValues = tempvalues;

                return;
            }
            ParameterInfo[] parameters = Controller.GetMethodParameters(_selectedMethodName, CurrentServiceGuid);
            //string s = selectedMethodName;
            //now how to access the selected row after the double click event?
            _controlViewBindingObject.Clear();
            _isExisting = false;
            _existingIndex = -1;
            _existingGuid = String.Empty;


            ControlsHelper.SetEmptyCollectionOnMethodSelect(parameters, _controlViewBindingObject, CurrentServiceGuid);

            NotifyOfPropertyChange(() => ControlViewBindingObject);

            var testNodes = (from s in Test.TestPackage.Suites
                             from test in s.Tests
                             where test.Service.MethodName == SelectedMethodName
                             && s.Guid == CurrentServiceGuid
                             select test);


            int maxValueNodes = 0;
            if (testNodes.Count() == 1)
            {
                maxValueNodes = testNodes.ToList()[0].Service.Values.ValueList.Count;
            }
            List<string> values = new List<string>();
            for (int i = 0; i < maxValueNodes; i++)
            {
                //values.Add(string.Format("Value Set {0}", i + 1));
                values.Add(string.Format("Value Set {0}", testNodes.ToList()[0].Service.Values.ValueList[i].Guid));
            }
            ServiceMethodValues = values;

            IsValueGridPopulated = true;

            NotifyOfPropertyChange(() => CanDisplayDeleteValues);
        }

        #endregion

        #region Button Events

        public void RunFuncTest()
        {
            Controller.ShowRunningFuncTestViewDialog();
        }

        public void RunPerfTest()
        {
            CancellationTokens.ResetPerformanceCancellationToken();
            CancellationTokens.GetPerformanceCancellationToken();
            
            //run program on server
            Task.Factory.StartNew(Controller.RunTest);
            Controller.ShowRunningTestViewDialog();
        }

        public void SavePerfTest()
        {
            try
            {
                Test.TestPackage.DelayRangeStart = Convert.ToInt32(DelayRangeStart);
                Test.TestPackage.DelayRangeEnd = Convert.ToInt32(DelayRangeEnd);
                Test.TestPackage.Clients = Convert.ToInt32(NoOfClients);
                Test.TestPackage.Duration = Convert.ToInt32(Duration);
                //Test.Suite.BindingToTest = SelectedBindingToSave;
                Test.TestPackage.ResultFileName = ResultFilePath;

                //Create XML file
                //open file dialog
                if (string.IsNullOrEmpty(_perfXmlfilePath))
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Xml files (*.xml)|*.xml",
                        OverwritePrompt = true,
                        CreatePrompt = true,
                        AddExtension = true
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string newFileName = saveFileDialog.FileName;
                        if (File.Exists(newFileName))
                        {
                            File.Delete(newFileName);
                        }
                        _perfXmlfilePath = newFileName;
                    }
                }

                if (!string.IsNullOrEmpty(_perfXmlfilePath))
                {
                    XDocument xmlDoc = new XDocument();
                    XElement testPackage = new XElement("package");
                    //XElement testSuite = new XElement("testSuite");
                    xmlDoc.Add(testPackage);
                    //xmlDoc.Add(testSuite);
                    //xmlDoc.Root = new XElement("testSuite");
                    xmlDoc = ControlsHelper.GenerateTestSuiteXmlDocument(xmlDoc);
                    xmlDoc.Save(_perfXmlfilePath);
                    _canRun = true;
                    NotifyOfPropertyChange(() => CanRunPerfTest);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in SavePerfTest Method");
            }
            finally
            {
                NotifyOfPropertyChange(() => CanDisplayDeleteValues);
            }
        }


        public void SaveValues()
        {
            try
            {
                var valueNode = ControlsHelper.GetParameterValueFromControlViewList(ControlViewBindingObject);


                global::Common.Test newTest = new global::Common.Test();
                var testNodes = (from s in Test.TestPackage.Suites
                                 from test in s.Tests
                                 where test.Service.MethodName == SelectedMethodName
                                 && s.Guid == CurrentServiceGuid
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

                        Test.TestPackage.Suites.Find(s => s.Guid == CurrentServiceGuid).Tests.Add(newTest);
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
                    //values.Add(string.Format("Value Set {0}", i + 1));
                    values.Add(string.Format("Value Set {0}", testNodes.ToList()[0].Service.Values.ValueList[i].Guid));
                }
                ServiceMethodValues = values;

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

        public void RemoveService()
        {

        }

        public void DeleteValues()
        {
            bool isUsedInScenario = false;
            try
            {
                if (_isExisting)
                {
                    if ((from sc in Test.TestPackage.Scenarios
                         from so in sc.ScenarioOrder
                         where so.MethodGuid == _existingGuid
                         select 1).Any())
                    {
                        MessageBox.Show("Value is used in a scenario. Please remove it from scenario first.");
                        isUsedInScenario = true;
                        return;
                    }


                    var testNodes = (from s in Test.TestPackage.Suites
                                     from test in s.Tests
                                     where test.Service.MethodName == SelectedMethodName
                                     && s.Guid == CurrentServiceGuid
                                     select test);



                    testNodes.ToList()[0].Service.Values.ValueList.Remove(
                        testNodes.ToList()[0].Service.Values.ValueList.Find(v => v.Guid == _existingGuid));

                    //testNodes.ToList()[0].Service.Values.ValueList.RemoveAt(_existingIndex);

                    int maxValueNodes = testNodes.ToList()[0].Service.Values.ValueList.Count;
                    if (maxValueNodes == 0)
                    {
                        Test.TestPackage.Suites.Find(s => s.Guid == CurrentServiceGuid).Tests.RemoveAll(t => t.Service.MethodName == _selectedMethodName);
                    }
                    List<string> values = new List<string>();
                    for (int i = 0; i < maxValueNodes; i++)
                    {
                        //values.Add(string.Format("Value Set {0}", i + 1));
                        values.Add(string.Format("Value Set {0}", testNodes.ToList()[0].Service.Values.ValueList[i].Guid));
                    }
                    ServiceMethodValues = values;

                    MethodSelected();
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                if (!isUsedInScenario)
                {
                    _isExisting = false;
                    _existingIndex = -1;
                    _existingGuid = String.Empty;
                    NotifyOfPropertyChange(() => CanDisplayDeleteValues);
                }
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
        #endregion
    }
}
