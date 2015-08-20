#region File Information/History
// <copyright file="ArrayItem.cs" project="WCFLoadUI" >
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
    public class ArrayItem
    {
        public string Index { get; set; }
        public string DisplayIndex { get; set; }

        public string DisplayIndexWithText
        {
            get { return string.Format("Index {0}", DisplayIndex); }
        }
    }
}
