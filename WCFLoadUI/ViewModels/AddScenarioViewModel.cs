#region File Information/History
// <copyright file="AddScenarioViewModel.cs" project="WCFLoadUI" >
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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Common;
using WCFLoadUI.Base;
using Test = WCFLoad.Test;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{
    public class AddScenarioViewModel : BaseViewModel
    {
        #region private fields
        private const string WindowTitleDefault = "Add Scenarios";
        private string _windowTitle = WindowTitleDefault;
        List<string> _scenarios = new List<string>();
        private string _doubleClickSelectedScenarioName = string.Empty;
        private string _selectedScenarioName = string.Empty;
        List<MethodForScenario> _availableMethods = new List<MethodForScenario>();
        MethodForScenario _selectedAvailableMethodName = new MethodForScenario();
        List<MethodForScenario> _selectedMethods = new List<MethodForScenario>();
        MethodForScenario _selectedSelectedMethodName = new MethodForScenario();
        int _selectedSelectedMethodIndex;
        int _intervalBetweenScenarios = 5000;
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

        public int IntervalBetweenScenarios
        {
            get
            {
                return _intervalBetweenScenarios;
            }
            set
            {
                _intervalBetweenScenarios = value;
                NotifyOfPropertyChange(() => IntervalBetweenScenarios);
            }
        }

        public int SelectedSelectedMethodIndex
        {
            get
            {
                return _selectedSelectedMethodIndex;
            }
            set
            {
                _selectedSelectedMethodIndex = value;
                NotifyOfPropertyChange(() => SelectedSelectedMethodIndex);
            }
        }

        public MethodForScenario SelectedSelectedMethodName
        {
            get
            {
                return _selectedSelectedMethodName;
            }
            set
            {
                _selectedSelectedMethodName = value;
                NotifyOfPropertyChange(() => SelectedSelectedMethodName);
            }
        }
        public List<MethodForScenario> SelectedMethods
        {
            get
            {
                return _selectedMethods;
            }
            set
            {
                _selectedMethods = value;
                NotifyOfPropertyChange(() => SelectedMethods);
            }
        }

        public MethodForScenario SelectedAvailableMethodName
        {
            get
            {
                return _selectedAvailableMethodName;
            }
            set
            {
                _selectedAvailableMethodName = value;
                NotifyOfPropertyChange(() => SelectedAvailableMethodName);
            }
        }
        public List<MethodForScenario> AvailableMethods
        {
            get
            {
                return _availableMethods;
            }
            set
            {
                _availableMethods = value;
                NotifyOfPropertyChange(() => AvailableMethods);
            }
        }

        public string SelectedScenarioName
        {
            get
            {
                return _selectedScenarioName;
            }
            set
            {
                _selectedScenarioName = value;
                NotifyOfPropertyChange(() => _selectedScenarioName);
            }
        }
        public List<string> Scenarios
        {
            get
            {
                return _scenarios;
            }
            set
            {
                _scenarios = value;
                NotifyOfPropertyChange(() => Scenarios);
            }
        }

        public string DoubleClickSelectedScenarioName
        {
            get { return _doubleClickSelectedScenarioName; }
            set
            {
                _doubleClickSelectedScenarioName = value;
                NotifyOfPropertyChange(() => DoubleClickSelectedScenarioName);
            }
        }

        #endregion

        #region view events
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            LoadScenarios();
        }
        #endregion

        #region private methods
        private void LoadScenarios()
        {
            //load scenarios
            List<string> scenariosList = new List<string>();
            if (Test.TestPackage.Scenarios != null
                && Test.TestPackage.Scenarios.Count > 0)
            {
                for (int i = 0; i < Test.TestPackage.Scenarios.Count; i++)
                {
                    scenariosList.Add(string.Format("Scenario {0}", i + 1));
                }

                IntervalBetweenScenarios = Test.TestPackage.IntervalBetweenScenarios;
            }
            Scenarios = scenariosList;
        }
        private void LoadAvailableMethods()
        {
            if (!string.IsNullOrEmpty(SelectedScenarioName))
            {
                SelectedMethodsInScenario();

                List<MethodForScenario> availableMethodsListWithValues =
                    (from suite in Test.TestPackage.Suites
                     from test in suite.Tests
                     from value in test.Service.Values.ValueList
                     select new MethodForScenario()
                     {
                         MethodName = test.Service.MethodName,
                         MethodGuid = value.Guid,
                         AssemblyGuid = suite.Guid,
                         IsRest = false
                     }).ToList();

                List<MethodForScenario> availableRestMethodsListWithValues =
                    (from restMethods in Test.TestPackage.RestMethods
                     select new MethodForScenario()
                     {
                         MethodName = restMethods.Url,
                         MethodGuid = restMethods.Guid,
                         AssemblyGuid = string.Empty,
                         IsRest = true
                     }).ToList();

                //Add rest methods
                availableMethodsListWithValues.AddRange(availableRestMethodsListWithValues);

                AvailableMethods = availableMethodsListWithValues;
            }
            else
            {
                AvailableMethods = new List<MethodForScenario>();
            }
        }
        private void SelectedMethodsInScenario()
        {
            if (!string.IsNullOrEmpty(SelectedScenarioName))
            {
                int index = Convert.ToInt32(SelectedScenarioName.Replace("Scenario ", string.Empty)) - 1;
                var sc = Test.TestPackage.Scenarios.ElementAt(index);
                sc.ScenarioOrder = sc.ScenarioOrder.OrderBy(o => o.Order).ToList();

                List<MethodForScenario> selectedMethodsList = sc.ScenarioOrder.Select(c =>
                    new MethodForScenario()
                    {
                        MethodName = c.MethodName,
                        MethodGuid = c.MethodGuid,
                        AssemblyGuid = c.AssemblyGuid,
                        IsRest = c.IsRest
                    }
                    ).ToList();

                SelectedMethods = selectedMethodsList;
            }
            else
            {
                SelectedMethods = new List<MethodForScenario>();
            }
        }
        #endregion

        #region list view event
        public void ScenarioSelected()
        {
            SelectedScenarioName = DoubleClickSelectedScenarioName;

            LoadAvailableMethods();
        }
        #endregion

        #region button events
        public void DeleteScenario()
        {
            if (!string.IsNullOrEmpty(SelectedScenarioName))
            {
                int index = Convert.ToInt32(SelectedScenarioName.Replace("Scenario ", string.Empty)) - 1;
                if (Test.TestPackage.Scenarios.Count > 0)
                {
                    Test.TestPackage.Scenarios.RemoveAt(index);
                }
                LoadScenarios();
                SelectedMethods = new List<MethodForScenario>();
                AvailableMethods = new List<MethodForScenario>();
            }

        }
        public void AddScenario()
        {
            var newscenario = new Scenario();
            Test.TestPackage.Scenarios.Add(newscenario);
            LoadScenarios();
        }
        public void AddToSelected()
        {
            if (!string.IsNullOrEmpty(SelectedScenarioName))
            {
                int scenarioIndex = Convert.ToInt32(SelectedScenarioName.Replace("Scenario ", string.Empty)) - 1;

                if (SelectedAvailableMethodName != null && !string.IsNullOrEmpty(SelectedAvailableMethodName.MethodName))
                {
                    List<MethodForScenario> tempselectedMethods = _selectedMethods.ToList();

                    List<MethodForScenario> tempavailableMethods = _availableMethods.ToList();
                    tempselectedMethods.Add(SelectedAvailableMethodName);

                    SetScenarioOrder(scenarioIndex, tempselectedMethods);

                    SelectedMethods = tempselectedMethods;
                    AvailableMethods = tempavailableMethods;
                }
            }
        }
        private static void SetScenarioOrder(int scenarioIndex, List<MethodForScenario> tempselectedMethods)
        {
            List<ScenarioOrder> scOrder = tempselectedMethods.Select((t, si) => new ScenarioOrder()
            {
                MethodName = t.MethodName,
                MethodGuid = t.MethodGuid,
                AssemblyGuid = t.AssemblyGuid,
                IsRest =  t.IsRest,
                Order = si
            }).ToList();

            Test.TestPackage.Scenarios.ElementAt(scenarioIndex).ScenarioOrder = scOrder;
        }
        public void RemoveFromSelected()
        {
            if (!string.IsNullOrEmpty(SelectedScenarioName))
            {
                int scenarioIndex = Convert.ToInt32(SelectedScenarioName.Replace("Scenario ", string.Empty)) - 1;

                if (SelectedSelectedMethodName != null && !string.IsNullOrEmpty(SelectedSelectedMethodName.MethodName) && SelectedSelectedMethodIndex > -1)
                {
                    List<MethodForScenario> tempselectedMethods = _selectedMethods.ToList();

                    tempselectedMethods.RemoveAt(SelectedSelectedMethodIndex);

                    SetScenarioOrder(scenarioIndex, tempselectedMethods);

                    SelectedMethods = tempselectedMethods;
                }
            }
        }
        public void MoveSelectedUp()
        {

            if (!string.IsNullOrEmpty(SelectedScenarioName))
            {
                int scenarioIndex = Convert.ToInt32(SelectedScenarioName.Replace("Scenario ", string.Empty)) - 1;

                if (SelectedSelectedMethodName != null && !string.IsNullOrEmpty(SelectedSelectedMethodName.MethodName) && SelectedSelectedMethodIndex > -1)
                {
                    if (SelectedSelectedMethodIndex > 0)
                    {
                        List<MethodForScenario> tempselectedMethods = _selectedMethods.ToList();

                        var old = tempselectedMethods[SelectedSelectedMethodIndex - 1];
                        tempselectedMethods[SelectedSelectedMethodIndex - 1] = tempselectedMethods[SelectedSelectedMethodIndex];
                        tempselectedMethods[SelectedSelectedMethodIndex] = old;

                        SetScenarioOrder(scenarioIndex, tempselectedMethods);

                        SelectedMethods = tempselectedMethods;
                    }
                }
            }
        }
        public void MoveSelectedDown()
        {
            if (!string.IsNullOrEmpty(SelectedScenarioName))
            {
                int scenarioIndex = Convert.ToInt32(SelectedScenarioName.Replace("Scenario ", string.Empty)) - 1;

                if (SelectedSelectedMethodName != null && !string.IsNullOrEmpty(SelectedSelectedMethodName.MethodName) && SelectedSelectedMethodIndex > -1)
                {
                    if (SelectedSelectedMethodIndex < SelectedMethods.Count - 1)
                    {
                        List<MethodForScenario> tempselectedMethods = _selectedMethods.ToList();

                        var old = tempselectedMethods[SelectedSelectedMethodIndex + 1];
                        tempselectedMethods[SelectedSelectedMethodIndex + 1] = tempselectedMethods[SelectedSelectedMethodIndex];
                        tempselectedMethods[SelectedSelectedMethodIndex] = old;

                        SetScenarioOrder(scenarioIndex, tempselectedMethods);

                        SelectedMethods = tempselectedMethods;
                    }
                }
            }
        }
        public void SaveScenarios()
        {
            var emptyScenarios = from es in Test.TestPackage.Scenarios
                                 where es.ScenarioOrder.Count == 0
                                 select es;
            var enumerable = emptyScenarios as Scenario[] ?? emptyScenarios.ToArray();
            if (enumerable.Count() == Test.TestPackage.Scenarios.Count)
            {
                //delete scenario node
                Test.TestPackage.Scenarios = new List<Scenario>();
                Test.TestPackage.IntervalBetweenScenarios = 0;
                ExecuteCancelCommand();
            }
            else if (enumerable.Any())
            {
                MessageBox.Show("Cannot have empty scnearios");
            }
            else
            {
                ExecuteCancelCommand();
            }
        }
        public void SaveScenarios_(CancelEventArgs args)
        {
            Test.TestPackage.IntervalBetweenScenarios = IntervalBetweenScenarios;
            //Save scenarios
            var emptyScenarios = from es in Test.TestPackage.Scenarios
                                 where es.ScenarioOrder.Count == 0
                                 select es;

            var enumerable = emptyScenarios as Scenario[] ?? emptyScenarios.ToArray();
            if (enumerable.Count() == Test.TestPackage.Scenarios.Count)
            {
                //delete scenario node
                Test.TestPackage.Scenarios = new List<Scenario>();
                Test.TestPackage.IntervalBetweenScenarios = 0;
            }

            if (enumerable.Any())
            {
                args.Cancel = true;
                MessageBox.Show("Cannot have empty scnearios");
            }
            else
            {
            }
        }
        public void CancelScenarios()
        {
            ExecuteCancelCommand();
        }
        public void OnClose(CancelEventArgs args)
        {
            SaveScenarios_(args);
        }
        #endregion

    }
}
