#region File Information/History
// <copyright file="Controller.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// 08/21/2015: Added support to run rest without wsdl project added first
// </history>
#endregion
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Common;
using FBServiceClient;
using WCFLoad.Helper;
using WCFLoad.Tokens;
using WCFLoadUI.Distributed;
using WCFLoadUI.FBService;
using WCFLoadUI.Helpers;
using WCFLoadUI.TypeToBind;
using WCFLoadUI.ViewModels;
using WCFLoadUI.Common;

namespace WCFLoadUI.ApplicationController
{
    public class Controller
    {
        public static bool IsTestCompleted;

        public static List<FunctionalTestResults> FunctionalTestResults { get; set; }
        public static bool IsFunctionalTestCompleted { get; set; }
        public static event EventHandler FunctionalTestUpdated;

        #region Dialog methods
        /// <summary>
        /// Displays service url view dialog
        /// </summary>
        public static void ShowServiceUrlViewDialog()
        {
            DialogHelper.ShowDialog<ServiceUrlViewModel>();
        }

        /// <summary>
        /// Displays service url view dialog
        /// </summary>
        public static void ShowServiceUrlViewDialog(bool isAddARestEnabled)
        {
            DialogHelper.ShowDialog<ServiceUrlViewModel>(isAddARestEnabled);
        }

        /// <summary>
        /// Displays scenarop dialog
        /// </summary>
        public static void ShowScenarioViewDialog()
        {
            DialogHelper.ShowDialog<AddScenarioViewModel>();
        }

        /// <summary>
        /// Displays functional test dialog
        /// </summary>
        public static void ShowFunctionalViewDialog()
        {
            DialogHelper.ShowDialog<FunctionalViewModel>();
        }

        public static void ShowRunningTestViewDialog()
        {
            DialogHelper.ShowDialog<RunningTestViewModel>();
        }

        public static void ShowRunningFuncTestViewDialog()
        {
            DialogHelper.ShowDialog<RunningFuncTestViewModel>();
        }

        public static void ShowFunctionResultDetailViewDialog(string actual, string expected, bool passfail, string input)
        {
            DialogHelper.ShowDialog<FunctionResultDetailViewModel>(new object[] { actual, expected, passfail, input });
        }

        //public static void ShowListControlGridViewDialog(ObservableCollection<List<ControlView>> properties,
        public static void ShowListControlGridViewDialog(ObservableCollection<ControlView> properties,
            BindingList<IntWrappper> arrayIndexes,
            Type fieldType,
            ControlView baseTypeProperties)
        {
            DialogHelper.ShowDialog<ListControlGridViewModel>(properties, arrayIndexes, fieldType, baseTypeProperties);
        }

        public static void ShowDictionaryControlGridViewDialog(ObservableCollection<ControlView> properties,
            ObservableCollection<ControlView> propertiesValue,
           int itemsCount,
           Type fieldType,
           Type fieldValueType,
           ControlView baseTypeProperties,
            ControlView baseValueTypeProperties)
        {
            DialogHelper.ShowDialog<DictionaryControlGridViewModel>(properties, propertiesValue, itemsCount, fieldType, fieldValueType, baseTypeProperties, baseValueTypeProperties);
        }

        //ListViewItemViewModel
        public static void ShowListViewItemViewDialog(ControlView properties)
        {
            DialogHelper.ShowDialog<ListViewItemViewModel>(properties);
        }

        /// <summary>
        /// Displays service url view dialog and returns property value and fetches method name list for service
        /// </summary>
        public static List<string> ShowServiceUrlViewDialog(string propertyName, out string guid, bool isAddARestEnabled = false)
        {
            string serviceUrl = Convert.ToString(DialogHelper.ShowDialog<ServiceUrlViewModel>(propertyName, isAddARestEnabled));
            if (!string.IsNullOrEmpty(serviceUrl))
            {
                if (serviceUrl == "AddARest")
                {
                    WCFLoad.Test.Clear();
                    guid = "AddARest";
                    return null;
                }
                else
                {
                    WCFLoad.Test.Clear();
                    Guid g = Guid.NewGuid();
                    guid = g.ToString();
                    WCFLoad.Test.GenerateProxyAssembly(serviceUrl, g.ToString(), true);
                    return WCFLoad.Test.GetAllServiceMethods(g.ToString());
                }
            }
            guid = string.Empty;
            return null;
        }


        /// <summary>
        /// Displays service url view dialog and returns property value and fetches method name list for service
        /// </summary>
        public static List<string> ShowServiceUrlViewDialogForAdd(string propertyName, out string guid, bool isAddARestEnabled = false)
        {
            string serviceUrl = Convert.ToString(DialogHelper.ShowDialog<ServiceUrlViewModel>(propertyName, isAddARestEnabled));
            if (!string.IsNullOrEmpty(serviceUrl))
            {
                if (WCFLoad.Test.TestPackage.Suites.FindAll(s => s.ServiceUrl.ToLower() == serviceUrl.ToLower()).Any())
                {
                    MessageBox.Show("Url already present in the project");
                }
                else
                {
                    Guid g = Guid.NewGuid();
                    guid = g.ToString();
                    WCFLoad.Test.GenerateProxyAssembly(serviceUrl, g.ToString(), true);
                    return WCFLoad.Test.GetAllServiceMethods(g.ToString());
                }
            }
            guid = string.Empty;
            return null;
        }

        public static void ShowRestApiDialog()
        {
            DialogHelper.ShowDialog<RestApiViewModel>();
        }

        public static void ShowAddNotesDialog()
        {
            DialogHelper.ShowDialog<AddNotesViewModel>();
        }
        public static void ShowRestApiResultView(HttpStatusCode statusCode, string response, string guid)
        {
            DialogHelper.ShowDialog<RestApiResultViewModel>(new object[] { statusCode.ToString(), response, guid });
        }
        #endregion

        /// <summary>
        /// To deep copy the objects
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="other">object</param>
        /// <returns>new copied object</returns>
        public static T DeepCopy<T>(T other)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, other);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// Gets all service methods for a service
        /// </summary>
        /// <returns></returns>
        public static List<string> GetServiceMethods(string guid)
        {
            return WCFLoad.Test.GetAllServiceMethods(guid);
        }

        /// <summary>
        /// Invoke a functional test case
        /// </summary>
        /// <param name="t">Test to execute</param>
        /// <param name="index">index of value to use</param>
        /// <param name="guid"></param>
        /// <param name="input">out, returns xml string of input parameters</param>
        /// <returns>returns actual output after execution</returns>
        public static string InvokeFunctionalTest(Test t, int index, string guid, out string input)
        {
            return WCFLoad.Test.InvokeFunctionalTest(t, index, guid, out input);
        }

        /// <summary>
        /// Runs all funtional test
        /// </summary>
        public static void InvokeAllFunctionTest()
        {
            FunctionalTestResults = new List<FunctionalTestResults>();
            IsFunctionalTestCompleted = false;
            Task.Factory.StartNew(() =>
            {
                int ii;
                foreach (var t in WCFLoad.Test.TestPackage.Suites.SelectMany(s => s.FunctionalTests))
                {
                    for (ii = 0; ii < t.Service.Values.ValueList.Count; ii++)
                    {
                        FunctionalTestResults.Add(new FunctionalTestResults
                        {
                            MethodName = t.Service.MethodName,
                            FunctionTestNumber = "Function Test " + (ii + 1),
                            Status = Status.NotStarted,
                            Value = t.Service.Values.ValueList[ii]
                        });
                    }
                }
                ii = 0;
                foreach (var t in WCFLoad.Test.TestPackage.RestMethods.FindAll(r => r.IsAddedToFunctional))
                {
                    FunctionalTestResults.Add(new FunctionalTestResults
                    {
                        MethodName = t.Url,
                        FunctionTestNumber = "Function Test " + (ii + 1),
                        Status = Status.NotStarted,
                        Value = new Value()
                    });
                    ii++;
                }

                if (FunctionalTestUpdated != null)
                {
                    FunctionalTestUpdated(null, null);
                }
                int ij;
                foreach (var s in WCFLoad.Test.TestPackage.Suites)
                {
                    foreach (var t in s.FunctionalTests)
                    {
                        for (ij = 0; ij < t.Service.Values.ValueList.Count; ij++)
                        {
                            var ftrObj = (from ftr in FunctionalTestResults
                                          where ftr.MethodName == t.Service.MethodName
                                                && ftr.FunctionTestNumber == "Function Test " + (ij + 1)
                                          select ftr).First();

                            ftrObj.Status = Status.Started;

                            if (FunctionalTestUpdated != null)
                            {
                                FunctionalTestUpdated(null, null);
                            }
                            string input;
                            string result = InvokeFunctionalTest(t, ij, s.Guid, out input);

                            bool passfailstatus = result == t.Service.Values.ValueList[ij].MethodOutput;

                            ftrObj.Status = Status.Completed;
                            ftrObj.Actual = result;
                            ftrObj.Input = input;
                            ftrObj.Expected = t.Service.Values.ValueList[ij].MethodOutput;
                            ftrObj.PassFailStatus = passfailstatus;

                            if (FunctionalTestUpdated != null)
                            {
                                FunctionalTestUpdated(null, null);
                            }
                        }
                    }
                }
                ij = 0;
                foreach (var t in WCFLoad.Test.TestPackage.RestMethods.FindAll(r => r.IsAddedToFunctional))
                {
                    var ftrObj = (from ftr in FunctionalTestResults
                                  where ftr.MethodName == t.Url
                                        && ftr.FunctionTestNumber == "Function Test " + (ij + 1)
                                  select ftr).First();

                    ftrObj.Status = Status.Started;

                    if (FunctionalTestUpdated != null)
                    {
                        FunctionalTestUpdated(null, null);
                    }
                    RestMethodResponse result = WCFLoad.Test.InvokeRestApi(t, false);

                    bool passfailstatus = result.Response == t.MethodOutput;

                    ftrObj.Status = Status.Completed;
                    ftrObj.Actual = result.Response;
                    ftrObj.Input = string.Empty;
                    ftrObj.Expected = t.MethodOutput;
                    ftrObj.PassFailStatus = passfailstatus;

                    if (FunctionalTestUpdated != null)
                    {
                        FunctionalTestUpdated(null, null);
                    }

                    ij++;
                }

                IsFunctionalTestCompleted = true;
                if (FunctionalTestUpdated != null)
                {
                    FunctionalTestUpdated(null, null);
                }
                FunctionalTestUpdated = null;
            });
        }

        /// <summary>
        /// Displays service url view dialog and returns property value and fetches method name list for service
        /// </summary>
        public static List<string> LoadXmlFileAndGetAllMethods(string filePath)
        {
            WCFLoad.Test.Clear();
            WCFLoad.Test.LoadTest(filePath);
            //Get guid of first service
            if (WCFLoad.Test.TestPackage.Suites.Count > 0)
                return WCFLoad.Test.GetAllServiceMethods(WCFLoad.Test.TestPackage.Suites[0].Guid);
            return new List<string>();
        }

        /// <summary> 
        /// Return service method parameter
        /// </summary>
        /// <param name="methodName">Method Name</param>
        /// <param name="guid"></param>
        /// <returns>ParameterInfo array</returns>
        public static ParameterInfo[] GetMethodParameters(string methodName, string guid)
        {
            return WCFLoad.Test.GetMethodParameters(methodName, guid);
        }

        public static Type GetBaseTypeFromCurrentAssembly(string typeName, string guid)
        {
            return WCFLoad.Test.GetBaseTypeFromCurrentAssembly(typeName, guid);
        }

        /// <summary>
        /// Start Net Tcp Port sharing service on server
        /// Open FBService port on firewall
        /// Create remove clients
        /// Execute clients via WMI
        /// </summary>
        private static void CreateClients()
        {
            Wmi.StartNetTcpPortSharing();
            Wmi.OpenPortOnFireWall();
            Wmi.CreateClientAgents();
        }

        /// <summary>
        /// Runs performance test
        /// </summary>
        public static void RunTest()
        {
            WCFLoad.Test.MethodLevelResults = new ConcurrentDictionary<string, MethodLogs>();
            WCFLoad.Test.MethodLevelReqStatus = new ConcurrentDictionary<string, MethodLogs>();
            WCFLoad.Test.ClearCache();

            //Reset total clients
            ApplicationData.TotalClientsStarted = 0;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            FbHelper.SetInitialValues(ipAddress.ToString(), true, Environment.CurrentDirectory + "\\firebird_filesmydb.fdb");
            FbHelper.CreateDataBase();

            FbServiceController.StartService();
            CreateClients();
            ServiceClient.ServerIpAddress = "localhost";
            ServiceClient.ServerPort = "9090";

            WCFLoad.Test.PerformanceRunToken = CancellationTokens.GetPerformanceCancellationToken();

            if (!string.IsNullOrEmpty(WCFLoad.Test.TestPackage.ResultFileName))
            {
                if (File.Exists(WCFLoad.Test.TestPackage.ResultFileName))
                {
                    File.Delete(WCFLoad.Test.TestPackage.ResultFileName);
                }
            }

            IsTestCompleted = false;

            int scenariosCount = WCFLoad.Test.TestPackage.Scenarios.Count;

            int duration = WCFLoad.Test.TestPackage.Duration;
            int clients = WCFLoad.Test.TestPackage.Clients;
            if (WCFLoad.Test.TestPackage.Nodes.NodeList.Count > 0)
            {
                clients = WCFLoad.Test.TestPackage.Clients / WCFLoad.Test.TestPackage.Nodes.NodeList.Count;
            }

            Stopwatch testDurationTimer = new Stopwatch();

            testDurationTimer.Start();

            int requestSent = 0;

            int delayRangeStart = WCFLoad.Test.TestPackage.DelayRangeStart;
            int delayRangeEnd = WCFLoad.Test.TestPackage.DelayRangeEnd;

            int clientsSpawned = 0;

            Task[] clientTasks = new Task[clients];

            while (clientsSpawned < clients)
            {
                int clientIndex = clientsSpawned;
                clientTasks[clientIndex] = new Task(() =>
                {
                    int currentScenario = -1;
                    int currentScenarioOrder = -1;
                    while (testDurationTimer.ElapsedMilliseconds <= (duration * 60 * 1000)
                        && !CancellationTokens.GetPerformanceCancellationToken().IsCancellationRequested)
                    {
                        if (scenariosCount == 0)
                        {
                            Random r = new Random();
                            bool executeWcf = r.Next(0, 1000) % 2 == 0;

                            if (WCFLoad.Test.TestPackage.Suites.Count == 0)
                                executeWcf = false;

                            if (executeWcf || WCFLoad.Test.TestPackage.RestMethods.Count == 0)
                            {
                                int suiteNumberToExecute = r.Next(0, WCFLoad.Test.TestPackage.Suites.Count - 1);
                                int testNumberToExecute = r.Next(0,
                                    WCFLoad.Test.TestPackage.Suites[suiteNumberToExecute].Tests.Count - 1);
                                var testToExecute =
                                    WCFLoad.Test.TestPackage.Suites[suiteNumberToExecute].Tests[testNumberToExecute];
                                WCFLoad.Test.InvokeTest(testToExecute,
                                    WCFLoad.Test.TestPackage.Suites[suiteNumberToExecute].Guid);
                                requestSent++;
                            }
                            else
                            {
                                int suiteNumberToExecute = r.Next(0, WCFLoad.Test.TestPackage.RestMethods.Count - 1);
                                Task.Factory.StartNew(() =>
                                {
                                    WCFLoad.Test.InvokeRestApi(
                                        WCFLoad.Test.TestPackage.RestMethods[suiteNumberToExecute], true);
                                });
                            }
                            requestSent++;

                            Thread.Sleep(r.Next(delayRangeStart, delayRangeEnd));
                        }
                        else
                        {
                            if (currentScenario == -1)
                            {
                                Random r = new Random();
                                currentScenario = r.Next(0, scenariosCount - 1);
                            }
                            var scen = WCFLoad.Test.TestPackage.Scenarios[currentScenario];

                            if (currentScenarioOrder == -1)
                            {
                                currentScenarioOrder = 0;
                            }
                            var order = currentScenarioOrder;
                            var ss = (from s in scen.ScenarioOrder
                                      where s.Order == order
                                      select s).First();

                            int totalMethodsInScenario = scen.ScenarioOrder.Count;

                            if (!ss.IsRest)
                            {
                                var methodName = ss.MethodName;

                                var tts = from suite in WCFLoad.Test.TestPackage.Suites
                                          from tt in suite.Tests
                                          where tt.Service.MethodName == methodName
                                                && ss.AssemblyGuid == suite.Guid
                                          select tt;
                                Test testToExecute = tts.ElementAt(0);

                                var valueToUse = (from v in testToExecute.Service.Values.ValueList
                                                  where v.Guid == ss.MethodGuid
                                                  select v).First();

                                WCFLoad.Test.InvokeTest(testToExecute, ss.AssemblyGuid, valueToUse);
                            }
                            else
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    WCFLoad.Test.InvokeRestApi(
                                        WCFLoad.Test.TestPackage.RestMethods.Find(r => r.Guid == ss.MethodGuid), true);
                                });
                            }
                            requestSent++;
                            Random r1 = new Random();
                            Thread.Sleep(r1.Next(delayRangeStart, delayRangeEnd));

                            currentScenarioOrder = currentScenarioOrder + 1;
                            if (totalMethodsInScenario == currentScenarioOrder)
                            {
                                currentScenarioOrder = -1;
                                currentScenario = -1;
                                Thread.Sleep(WCFLoad.Test.TestPackage.IntervalBetweenScenarios);
                            }
                        }
                    }
                }, CancellationTokens.GetPerformanceCancellationToken());
                clientTasks[clientIndex].Start();
                clientsSpawned++;
            }

            try
            {
                //Wait for all client threads to complete
                Task.WaitAll(clientTasks);
                SpinWait.SpinUntil(() => (requestSent == WCFLoad.Test.TotalResponsesRecieved), 60000);
            }
            catch (AggregateException ae)
            {
                Logger.LogMessage("Aggregaate Exception Occured : " + ae.Message);
                foreach (Exception ex in ae.InnerExceptions)
                {
                    Logger.LogMessage(string.Format("Aggregate exception : {0} {1} {2}", ex.Message, Environment.NewLine, ex.StackTrace));
                }
            }
            IsTestCompleted = true;
            WCFLoad.Test.CallRunResultUpdatedEvent();
            FbServiceController.StopService();
        }
    }
}
