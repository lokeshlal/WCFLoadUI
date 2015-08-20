#region File Information/History
// <copyright file="EnumBinding.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;

namespace WCFLoadUI.TypeToBind
{
    /// <summary>
    /// Type to bind for enum combobox control
    /// </summary>
    [Serializable]
    public class EnumBinding
    {
        #region private fields
        private int _id;
        private string _name;
        #endregion

        #region public properties
        /// <summary>
        /// Integer value of enum
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Name of enum value
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        #endregion
    }
}
