#region File Information/History
// <copyright file="WMI.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using WCFLoad;
using WCFLoadUI.Common;

namespace WCFLoadUI.Distributed
{
    /// <summary>
    /// Class to provide WMI features
    /// </summary>
    public class Wmi
    {
        //Lock object
        private static readonly object LockObj = new object();

        /// <summary>
        /// Enable net tcp port sharing service
        /// </summary>
        public static void StartNetTcpPortSharing()
        {
            string MethodName = "ChangeStartMode";
            ManagementPath path = new ManagementPath
            {
                Server = ".",
                NamespacePath = @"root\CIMV2",
                RelativePath =
                    string.Format(CultureInfo.InvariantCulture, "Win32_Service.Name='{0}'", "NetTcpPortSharing")
            };

            using (ManagementObject serviceObject = new ManagementObject(path))
            {
                ManagementBaseObject inputParameters = serviceObject.GetMethodParameters(MethodName);
                inputParameters["startmode"] = "Automatic";
                serviceObject.InvokeMethod(MethodName, inputParameters, null);
            }
        }

        /// <summary>
        /// create client agents on nodes and start agents using WMI
        /// </summary>
        public static void CreateClientAgents()
        {
            Parallel.ForEach(TestEngine.TestPackage.Nodes.NodeList, node =>
            {
                for (int i = 0; i < TestEngine.TestPackage.Nodes.NoOfClientsPerNode; i++)
                {
                    string clientNumber = i.ToString();
                    //copy exe and other files to client location and start the exe
                    string remoteServerName = node;
                    if (!Directory.Exists(@"\\" + remoteServerName + @"\C$\WCFLoadUI" + clientNumber))
                    {
                        Directory.CreateDirectory(@"\\" + remoteServerName + @"\C$\WCFLoadUI" + clientNumber);
                    }

                    foreach (string newPath in Directory.GetFiles(Environment.CurrentDirectory + @"\WCFService", "*.*",
                        SearchOption.AllDirectories))
                        File.Copy(newPath,
                            newPath.Replace(Environment.CurrentDirectory + @"\WCFService",
                                @"\\" + remoteServerName + @"\C$\WCFLoadUI" + clientNumber), true);

                    ConnectionOptions connectionOptions = new ConnectionOptions();

                    ManagementScope scope =
                        new ManagementScope(
                            @"\\" + remoteServerName + "." + Domain.GetComputerDomain().Name + @"\root\CIMV2",
                            connectionOptions);
                    scope.Connect();

                    ManagementPath p = new ManagementPath("Win32_Process");
                    ObjectGetOptions objectGetOptions = new ObjectGetOptions();

                    ManagementClass classInstance = new ManagementClass(scope, p, objectGetOptions);

                    ManagementBaseObject inParams = classInstance.GetMethodParameters("Create");

                    inParams["CommandLine"] = @"C:\WCFLoadUI"  + clientNumber + @"\WCFService.exe " + Environment.MachineName + " 9090";
                    inParams["CurrentDirectory"] = @"C:\WCFLoadUI" + clientNumber;
                    classInstance.InvokeMethod("Create", inParams, null);
                }
            });
        }

        public static void TerminateRemoteProcess()
        {
            Parallel.ForEach(TestEngine.TestPackage.Nodes.NodeList, node =>
            {
                string remoteServerName = node;
                ConnectionOptions connectionOptions = new ConnectionOptions();
                ManagementScope scope = new ManagementScope(@"\\" + remoteServerName + "." + Domain.GetComputerDomain().Name + @"\root\CIMV2", connectionOptions);
                scope.Connect();

                ObjectQuery theQuery = new ObjectQuery("SELECT * FROM Win32_Process WHERE Name='WCFService.exe'");

                ManagementObjectSearcher theSearcher = new ManagementObjectSearcher(scope, theQuery);
                ManagementObjectCollection theCollection = theSearcher.Get();
                foreach (var o in theCollection)
                {
                    var theCurObject = (ManagementObject)o;
                    theCurObject.InvokeMethod("Terminate", null);
                    lock (LockObj)
                    {
                        if (ApplicationData.TotalClientsStarted > 0)
                        {
                            ApplicationData.TotalClientsStarted--;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Open firewall port 9090 for FBService
        /// </summary>
        public static void OpenPortOnFireWall()
        {
            Type netFwMngrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(netFwMngrType);

            Type portType = Type.GetTypeFromCLSID(new Guid("{0CA545C6-37AD-4A6C-BF92-9F7610067EF5}"));
            INetFwOpenPort port = (INetFwOpenPort)Activator.CreateInstance(portType);


            port.Port = 9090;
            port.Name = "WCFLoadUI";
            port.Enabled = true;

            var ports = mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

            ports.Add(port);
        }
    }
}
