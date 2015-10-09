#region File Information/History
// <copyright file="FunctionalTestResults.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using Common;
using Common.Infrastructure.Entities;

namespace WCFLoadUI.TypeToBind
{
    /// <summary>
    /// To display the functional test results
    /// </summary>
    public class FunctionalTestResults
    {
        public string MethodName { get; set; }
        public string FunctionTestNumber { get; set; }
        public Value Value { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public string Input { get; set; }
        public Status Status { get; set; }
        public bool PassFailStatus { get; set; }
    }

    /// <summary>
    /// Status of functional test run result
    /// </summary>
    public enum Status
    {
        NotStarted,
        Started,
        Completed
    }
}
