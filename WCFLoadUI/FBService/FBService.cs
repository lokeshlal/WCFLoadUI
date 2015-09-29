#region File Information/History
// <copyright file="FBService.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System.Threading.Tasks;
using Common;
using Common.FBService;
using WCFLoad.Helper;
using Test = WCFLoad.Test;
using WCFLoadUI.Common;

namespace WCFLoadUI.FBService
{
    public class FbService : IFbService
    {
        /// <summary>
        /// Inserts a new method log
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="token"></param>
        public void InsertNewMethodLog(string methodName, string token)
        {
            Task.Factory.StartNew(() => FbHelper.InsertNewMethodLog(methodName, token)).ConfigureAwait(false);
        }

        /// <summary>
        /// updates a new method log, in an append only manner
        /// </summary>
        /// <param name="token"></param>
        /// <param name="methodName"></param>
        /// <param name="status"></param>
        /// <param name="timeTaken"></param>
        /// <param name="error"></param>
        public void UpdateMethodLog(string token, string methodName, MethodStatus status, long? timeTaken = null, string error = null)
        {
            Task.Factory.StartNew(() => FbHelper.UpdateMethodLog(token, methodName, status, timeTaken, error)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the test suite to run
        /// </summary>
        /// <returns></returns>
        public Package GetTestPackage()
        {
            //Add client to started clients collection
            ApplicationData.TotalClientsStarted++;
            //serialize not working on ServiceEndpoint
            //commented below code and now reduce no of clients in agent
            Package testPackage = (Package)TestHelper.Deserialize(TestHelper.Serialize(Test.TestPackage), Test.TestPackage.GetType());
            //testPackage.Clients = testPackage.Clients / testPackage.Nodes.NodeList.Count;
            return testPackage; //Test.TestPackage;
        }

        /// <summary>
        /// notify of completion
        /// </summary>
        public void Done()
        {
            //Remove client from started client collection
            ApplicationData.TotalClientsStarted--;
            Task.Factory.StartNew(Test.CallRunResultUpdatedEvent);
        }
    }
}
