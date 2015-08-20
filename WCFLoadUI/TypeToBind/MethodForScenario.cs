#region File Information/History
// <copyright file="MethodForScenario.cs" project="WCFLoadUI" >
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
    public class MethodForScenario
    {
        public string MethodName { get; set; }
        public string MethodGuid { get; set; }
        public string AssemblyGuid { get; set; }
        public bool IsRest { get; set; }

        public string DisplayName
        {
            get
            {
                if (IsRest)
                {
                    return string.Format("{0} - {1}", MethodName, MethodGuid);
                }
                else
                {
                    return string.Format("{0} - {1} - {2}", MethodName, MethodGuid, AssemblyGuid);
                }
            }
        }
    }
}
