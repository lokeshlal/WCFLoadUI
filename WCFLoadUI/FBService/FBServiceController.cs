#region File Information/History
// <copyright file="FBServiceController.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Common;
using WCFLoad.FbServiceClient;
using WCFLoadUI.Common;
using Common.FBServiceContracts;

namespace WCFLoadUI.FBService
{
    /// <summary>
    /// Class to manage life span of FBService
    /// </summary>
    public class FbServiceController
    {
        private static ServiceHost _host;
        private static bool _isStarted;
        private static int _serviceStopTries;
        /// <summary>
        /// Start the FB service
        /// </summary>
        public static void StartService()
        {
            if (_host == null)
            {
                var binding = new NetTcpBinding
                {
                    Security = new NetTcpSecurity
                    {
                        Mode = SecurityMode.None,
                        Message = new MessageSecurityOverTcp
                        {
                            ClientCredentialType = MessageCredentialType.None
                        }
                    }
                };

                binding.MaxConnections = 9999999;
                binding.MaxReceivedMessageSize = Int32.MaxValue;
                binding.MaxBufferSize = Int32.MaxValue;
                binding.MaxBufferPoolSize = Int32.MaxValue;
                binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
                binding.SendTimeout = new TimeSpan(0, 0, 1, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 0, 1, 0);
                binding.OpenTimeout = new TimeSpan(0, 0, 1, 0);


                Uri tcpBaseAddress = new Uri("net.tcp://localhost:9090/fbservice");
                _host = new ServiceHost(typeof(FbService), tcpBaseAddress);
                _host.AddServiceEndpoint(typeof(IFbService), binding, "");
                _host.Open();
                _isStarted = !_isStarted;
                _serviceStopTries = 0;
            }
        }

        /// <summary>
        /// Stop the FBService
        /// </summary>
        public static void StopService()
        {
            if (_isStarted)
            {
                Stop();
            }
        }

        private static void Stop()
        {
            Task.Factory.StartNew(() =>
            {
                //if all clients finished or 3 tries has already been done...
                //i.e. a maximum of 90 seconds wait before finally closing the service
                if (ApplicationData.TotalClientsStarted == 0 || _serviceStopTries > 3)
                {
                    _host.Close();
                    _isStarted = !_isStarted;
                    _host = null;
                    Task.Factory.StartNew(WCFLoad.TestEngine.CallRunResultUpdatedEvent);
                }
                else
                {
                    //wait for 30 seconds before retrying
                    Thread.Sleep(30000);
                    _serviceStopTries++;
                    Stop();
                }
            });
        }
    }
}
