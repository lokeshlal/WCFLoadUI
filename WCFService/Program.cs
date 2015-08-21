#region File Information/History
// <copyright file="Program.cs" project="WCFService" >
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using FBServiceClient;
using WCFLoad.Helper;
using WCFLoad.Tokens;
using Test = WCFLoad.Test;

namespace WCFService
{
    class Program
    {
        static void Main(string[] args)
        {
            string ipAddress = args[0];
            string port = args[1];

            ServiceClient.ServerIpAddress = ipAddress;
            ServiceClient.ServerPort = port;

            Test.Clear();

            Test.TestPackage = ServiceClient.GetTestPackage();

            //generate proxy
            foreach (var suite in Test.TestPackage.Suites)
            {
                Test.GenerateProxyAssembly(suite.Wsdl, suite.Guid);
            }

            //run test
            Task runTest = new Task(RunTest);
            runTest.Start();

            runTest.Wait();

            //signal server of completion
            ServiceClient.Done();
        }

        public static void RunTest()
        {
            CancellationTokens.ResetPerformanceCancellationToken();
            CancellationTokens.GetPerformanceCancellationToken();

            Test.MethodLevelResults = new ConcurrentDictionary<string, MethodLogs>();
            Test.MethodLevelReqStatus = new ConcurrentDictionary<string, MethodLogs>();
            Test.ClearCache();

            Test.PerformanceRunToken = CancellationTokens.GetPerformanceCancellationToken();

            if (!string.IsNullOrEmpty(Test.TestPackage.ResultFileName))
            {
                if (File.Exists(Test.TestPackage.ResultFileName))
                {
                    File.Delete(Test.TestPackage.ResultFileName);
                }
            }

            int scenariosCount = Test.TestPackage.Scenarios.Count;

            int duration = Test.TestPackage.Duration;
            int clients = Test.TestPackage.Clients;

            Stopwatch testDurationTimer = new Stopwatch();

            testDurationTimer.Start();

            int requestSent = 0;

            int delayRangeStart = Test.TestPackage.DelayRangeStart;
            int delayRangeEnd = Test.TestPackage.DelayRangeEnd;

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

                            if (Test.TestPackage.Suites.Count == 0)
                                executeWcf = false;

                            if (executeWcf || Test.TestPackage.RestMethods.Count == 0)
                            {
                                int suiteNumberToExecute = r.Next(0, Test.TestPackage.Suites.Count - 1);
                                int testNumberToExecute = r.Next(0,
                                    Test.TestPackage.Suites[suiteNumberToExecute].Tests.Count - 1);
                                Common.Test testToExecute =
                                    Test.TestPackage.Suites[suiteNumberToExecute].Tests[testNumberToExecute];
                                Test.InvokeTest(testToExecute, Test.TestPackage.Suites[suiteNumberToExecute].Guid);
                                requestSent++;
                            }
                            else
                            {
                                int suiteNumberToExecute = r.Next(0, Test.TestPackage.RestMethods.Count - 1);
                                Task.Factory.StartNew(() =>
                                {
                                    Test.InvokeRestApi(
                                        Test.TestPackage.RestMethods[suiteNumberToExecute], true);
                                });
                                requestSent++;
                            }
                            Thread.Sleep(r.Next(delayRangeStart, delayRangeEnd));
                        }
                        else
                        {
                            if (currentScenario == -1)
                            {
                                Random r = new Random();
                                currentScenario = r.Next(0, scenariosCount - 1);
                            }
                            var scen = Test.TestPackage.Scenarios[currentScenario];

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
                                var tts = from suite in Test.TestPackage.Suites
                                          from tt in suite.Tests
                                          where tt.Service.MethodName == methodName
                                                && ss.AssemblyGuid == suite.Guid
                                          select tt;
                                Common.Test testToExecute = tts.ElementAt(0);

                                var valueToUse = (from v in testToExecute.Service.Values.ValueList
                                                  where v.Guid == ss.MethodGuid
                                                  select v).First();

                                Test.InvokeTest(testToExecute, ss.AssemblyGuid, valueToUse);
                            }
                            else
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    Test.InvokeRestApi(
                                        Test.TestPackage.RestMethods.Find(r => r.Guid == ss.MethodGuid), true);
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
                                Thread.Sleep(Test.TestPackage.IntervalBetweenScenarios);
                            }
                        }
                    }
                }, CancellationTokens.GetPerformanceCancellationToken());
                clientTasks[clientIndex].Start();
                clientsSpawned++;
            }

            try
            {
                Task.WaitAll(clientTasks);
                SpinWait.SpinUntil(() => (requestSent == Test.TotalResponsesRecieved), 60000);
            }
            catch (AggregateException ae)
            {
                Logger.LogMessage("Aggregaate Exception Occured : " + ae.Message);
                foreach (Exception ex in ae.InnerExceptions)
                {
                    Logger.LogMessage(string.Format("Aggregate exception : {0} {1} {2}", ex.Message, Environment.NewLine, ex.StackTrace));
                }
            }
        }
    }
}
