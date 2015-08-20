#region File Information/History
// <copyright file="ServiceClient.cs" project="FBServiceClient" >
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
using Common;
using Common.FBService;

namespace FBServiceClient
{
    public class ServiceClient
    {
        public static string ServerIpAddress = string.Empty;
        public static string ServerPort = string.Empty;

        public static Package GetTestPackage()
        {
            Package returnObj;
            IFbService client = null;
            var myChannelFactory = GetClient();

            try
            {
                client = myChannelFactory.CreateChannel();
                returnObj = client.GetTestPackage();
                // ReSharper disable once SuspiciousTypeConversion.Global
                ((ICommunicationObject) client).Close();
            }
            catch (Exception ex)
            {
                if (client != null)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    ((ICommunicationObject)client).Abort();
                }

                throw new Exception("Failed during service call--", ex);
            }
            return returnObj;
        }

        private static ChannelFactory<IFbService> GetClient()
        {
            var myBinding = new NetTcpBinding
            {
                Security = new NetTcpSecurity
                {
                    Mode = SecurityMode.None,
                    Message = new MessageSecurityOverTcp {ClientCredentialType = MessageCredentialType.None}
                }
            };

            var myEndpoint =
                new EndpointAddress(string.Format("net.tcp://{0}:{1}/fbservice", ServerIpAddress, ServerPort));
            return new ChannelFactory<IFbService>(myBinding, myEndpoint);
        }

        public static void Done()
        {
            IFbService client = null;
            var myChannelFactory = GetClient();

            try
            {
                client = myChannelFactory.CreateChannel();
                client.Done();
                // ReSharper disable once SuspiciousTypeConversion.Global
                ((ICommunicationObject)client).Close();
            }
            catch
            {
                if (client != null)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    ((ICommunicationObject)client).Abort();
                }
            }
        }

        public static void InsertNewMethodLog(string methodName, string token)
        {
            IFbService client = null;
            var myChannelFactory = GetClient();

            try
            {
                client = myChannelFactory.CreateChannel();
                client.InsertNewMethodLog(methodName, token);
                // ReSharper disable once SuspiciousTypeConversion.Global
                ((ICommunicationObject)client).Close();
            }
            catch
            {
                if (client != null)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    ((ICommunicationObject)client).Abort();
                }
            }
        }

        public static void UpdateMethodLog(string token, string methodName, MethodStatus status, long? timeTaken = null,
            string error = null)
        {
            IFbService client = null;
            var myChannelFactory = GetClient();

            try
            {
                client = myChannelFactory.CreateChannel();
                client.UpdateMethodLog(token, methodName, status, timeTaken,
                    error + " - From " + Environment.MachineName);
                // ReSharper disable once SuspiciousTypeConversion.Global
                ((ICommunicationObject)client).Close();
            }
            catch
            {
                if (client != null)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    ((ICommunicationObject)client).Abort();
                }
            }
        }
    }
}