#region File Information/History
// <copyright file="ServiceClient.cs" project="FBServiceClient" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal/Amit Choudhary</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// 09/20/2015: Revised - Amit Choudhary
// </history>
#endregion
using System;
using System.ServiceModel;
using Common;
using Common.FBServiceContracts;
using Common.Infrastructure.Entities;

namespace WCFLoad.FbServiceClient
{
    /// <summary>
    /// Class providing service client method invocation facility. 
    /// <remarks>
    /// TODO: Use IFbService as marker interface. Incase IFbservice method changes in future. 
    /// </remarks>
    /// </summary>
    public sealed class ServiceClient // : IFbService
    {
        /// <summary>
        /// Private constructor to prevent instance creation of this class.
        /// </summary>
        private ServiceClient() { }

        public static string ServerIpAddress = string.Empty;
        public static string ServerPort = string.Empty;

        public static Package GetTestPackage()
        {
            return InvokeWcfOperation<Package>((clientProxy) =>
            {
                return clientProxy.GetTestPackage();
            });
        }

        public static void Done()
        {
            InvokeWcfOperation((clientProxy) => {
                clientProxy.Done();
            });
        }

        public static void InsertNewMethodLog(string methodName, string token)
        {
            InvokeWcfOperation((clientProxy) => {
                clientProxy.InsertNewMethodLog(methodName, token);
            });
        }

        public static void UpdateMethodLog(string token, string methodName, MethodStatus status, long? timeTaken = null,
            string error = null)
        {
            InvokeWcfOperation((clientProxy) => 
            {
                clientProxy.UpdateMethodLog(token, methodName, status, timeTaken,
                    error + " - From " + Environment.MachineName);
            });
        }

        /// <summary>
        /// Invoke void operation on IFbService client.
        /// </summary>
        /// <param name="operation"></param>
        private static void InvokeWcfOperation(Action<IFbService> operation)
        {
            // invoke other overload to avoid duplication.
            InvokeWcfOperation<object>((clientProxy) =>
            {
                operation(clientProxy);
                return null;
            });
        }

        /// <summary>
        /// Invoke operation with returning values on IFbservice client.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <returns></returns>
        private static T InvokeWcfOperation<T>(Func<IFbService, T> operation) where T: class
        {
            T returnValue = null;
            IFbService client = null;

            try
            {
                client = GetClientFactory().CreateChannel();

                returnValue = operation(client);

                ((ICommunicationObject)client).Close();
            }
            catch(Exception ex)
            {
                // Log exception with injected logger. 
                // TODO: Open an interface for logging.
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Factory method for IFbService channel.
        /// </summary>
        /// <returns>Channel factory of type IFbService</returns>
        private static ChannelFactory<IFbService> GetClientFactory()
        {
            var myBinding = new NetTcpBinding
            {
                Security = new NetTcpSecurity
                {
                    Mode = SecurityMode.None,
                    Message = new MessageSecurityOverTcp { ClientCredentialType = MessageCredentialType.None }
                }
            };

            myBinding.MaxConnections = 1000;
            myBinding.MaxReceivedMessageSize = Int32.MaxValue;
            myBinding.MaxBufferSize = Int32.MaxValue;
            myBinding.MaxBufferPoolSize = Int32.MaxValue;
            myBinding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            myBinding.SendTimeout = new TimeSpan(0, 0, 1, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 0, 1, 0);
            myBinding.OpenTimeout = new TimeSpan(0, 0, 1, 0);

            var myEndpoint =
                new EndpointAddress(string.Format("net.tcp://{0}:{1}/fbservice", ServerIpAddress, ServerPort));

            return new ChannelFactory<IFbService>(myBinding, myEndpoint);
        }
    }
}