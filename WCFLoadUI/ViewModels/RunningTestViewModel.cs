#region File Information/History
// <copyright file="RunningTestViewModel.cs" project="WCFLoadUI" >
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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WCFLoad;
using WCFLoad.Helper;
using WCFLoad.Tokens;
using WCFLoadUI.ApplicationController;
using WCFLoadUI.Base;
using WCFLoadUI.Distributed;
using WCFLoadUI.TypeToBind;

namespace WCFLoadUI.ViewModels
{
    public class RunningTestViewModel : BaseViewModel
    {
        #region private fields
        private TestResults _testResultSelected;
        private bool _canViewResult;
        private List<TestResults> _testResultList = new List<TestResults>();
        private Dictionary<string, int> _methodValuesForReport = new Dictionary<string, int>();
        private const string WindowTitleDefault = "Test Running...";
        private string _windowTitle = WindowTitleDefault;
        private int _computeCycle;
        private bool _isResultFileAvailable;
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

        public TestResults TestResultSelected
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

        public List<TestResults> TestResultList
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



        public bool CanViewStopTest
        {
            get
            {
                return !_canViewResult;
            }
        }


        public bool CanViewResult
        {
            get
            {
                return _canViewResult;
            }
            set
            {
                _canViewResult = value;

                NotifyOfPropertyChange(() => CanViewStopTest);
                NotifyOfPropertyChange(() => CanViewResult);
            }
        }
        #endregion

        #region events
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            CanViewResult = false;
            _computeCycle = 0;
            TestEngine.ClearResultHandler();
            TestEngine.RunResultUpdated += Test_RunResultUpdated;

        }

        public void StopTest()
        {
            //kill remote process if any
            Task.Factory.StartNew(Wmi.TerminateRemoteProcess);
            //cancel the current task
            CancellationTokens.CancelPerformanceCancellationTokenSource();
        }


        void Test_RunResultUpdated(object sender, EventArgs e)
        {
            if (_computeCycle % 100 == 0 || Controller.IsTestCompleted)
            {
                _computeCycle = 0;
                //avgs = GetAvg();
            }

            DataTable result = FbHelper.GetMethodLogStats();

            List<TestResults> tempTestResultList = new List<TestResults>();
            if (result.Rows.Count > 0)
            {
                tempTestResultList = (from DataRow dr in result.Rows
                                      select new TestResults
                                      {
                                          MethodName = Convert.ToString(dr["MethodName"]),
                                          Pass = Convert.ToInt32(dr["Pass"]),
                                          Fail = Convert.ToInt32(dr["Fail"]),
                                          Total = Convert.ToInt32(dr["Total"]),
                                          AvgTimeTaken = dr["AvgTimeTaken"] == DBNull.Value ? 0 : Convert.ToInt32(dr["AvgTimeTaken"]),
                                          MinTime = dr["MinTime"] == DBNull.Value ? 0 : Convert.ToInt32(dr["MinTime"]),
                                          Maxtime = dr["MaxTime"] == DBNull.Value ? 0 : Convert.ToInt32(dr["MaxTime"]),
                                          PreThread = Convert.ToInt32(dr["PreThread"])
                                      }).ToList();
            }
            TestResultList = tempTestResultList;
            if (Controller.IsTestCompleted)
            {
                Controller.IsTestCompleted = false;
                CanViewResult = true;
                //Test.ClearResultHandler();
                WindowTitle = "Test Completed";
            }
            _computeCycle++;
        }

        public void ViewResultFile()
        {
            string resultFileName = TestEngine.TestPackage.ResultFileName;
            if (!_isResultFileAvailable)
            {
                StreamWriter log = new StreamWriter(resultFileName, true);
                DataTable dtAllData = FbHelper.GetAllData();

                //dtAllData.Columns.Add("Merged");

                log.WriteLine("{0},{1},{2},{3},{4}", "DateTime", "MethodName", "TimeTaken", "Status", "Error");

                foreach (DataRow dr in dtAllData.Rows)
                {
                    log.WriteLine("{0},{1},{2},{3},{4}", Convert.ToString(dr["DateTime"]), Convert.ToString(dr["MethodName"]), Convert.ToString(dr["TimeTaken"]), Convert.ToString(dr["Status"]), Convert.ToString(dr["Error"]));
                }
                log.Flush();
                log.Close();
                _isResultFileAvailable = !_isResultFileAvailable;
            }
            Process p = new Process { StartInfo = { FileName = resultFileName } };
            p.Start();
        }

        public void TestResultClicked()
        {
            if (CanViewResult)
            {
                string methodToConsiderForGraph = TestResultSelected.MethodName;
                DataTable dtResult = FbHelper.GetMethodLogChartData(methodToConsiderForGraph);
                Dictionary<string, int> methodValues = new Dictionary<string, int>();
                foreach (DataRow dr in dtResult.Rows)
                {
                    if (!methodValues.ContainsKey(Convert.ToString(dr["DateTime"]))) //TimeTaken
                    {
                        methodValues.Add(Convert.ToString(dr["DateTime"]), Convert.ToInt32(dr["TimeTaken"]));
                    }
                    else
                    {
                        methodValues[Convert.ToString(dr["DateTime"])] = Convert.ToInt32((methodValues[Convert.ToString(dr["DateTime"])] + Convert.ToInt32(dr["TimeTaken"])) / 2);
                    }
                }

                MethodValuesForReport = methodValues;
            }
        }
        #endregion
    }
}
