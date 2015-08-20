#region File Information/History
// <copyright file="TestResults.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
namespace WCFLoadUI.TypeToBind
{
    /// <summary>
    /// To display the performance test results
    /// </summary>
    public class TestResults
    {
        public string MethodName { get; set; }
        public int Total { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public double AvgTimeTaken { get; set; }
        public long Maxtime { get; set; }
        public long MinTime { get; set; }
        public int PreThread { get; set; }
    }
}
