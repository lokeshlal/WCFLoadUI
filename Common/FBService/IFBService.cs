#region File Information/History
// <copyright file="IFBService.cs" project="Common" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System.ServiceModel;

namespace Common.FBService
{
    [ServiceContract]
    public interface IFbService
    {
        [OperationContract]
        void InsertNewMethodLog(string methodName, string token);

        [OperationContract]
        void UpdateMethodLog(string token, string methodName, MethodStatus status, long? timeTaken = null, string error = null);

        [OperationContract]
        Package GetTestPackage();

        [OperationContract]
        void Done();

    }
}
