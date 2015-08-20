#region File Information/History
// <copyright file="SpecialValue.cs" project="WCFLoad" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WCFLoad.Helper
{
    public class SpecialValue
    {
        static readonly Dictionary<string, string> SpecialValues = new Dictionary<string, string>();
        static string _copiedValue = string.Empty;

        public static string CopiedValue
        {
            get
            {
                return _copiedValue;
            }
            set
            {
                _copiedValue = value;
            }
        }

        public static List<string> GetSpecialValueList()
        {
            PopulateSpecialValueArray();
            return SpecialValues.Keys.ToList();
        }

        private static void PopulateSpecialValueArray()
        {
            if (SpecialValues.Count == 0)
            {
                SpecialValues.Add("Current DateTime", "{DateTime.Now}");
                SpecialValues.Add("Empty String", "{String.Empty}");
                SpecialValues.Add("Boolean True", "true");
                SpecialValues.Add("Boolean False", "false");
                SpecialValues.Add("DateTime Format", DateTime.Now.ToString(CultureInfo.CurrentCulture));
            }
        }

        public static string ResolveSpecialValue(string val)
        {
            if (val.StartsWith("{") && val.EndsWith("}"))
            {
                val = val.Replace("{", "").Replace("}", "");
                if (val == "DateTime.Now")
                {
                    val = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                }
                else if (val == "String.Empty")
                {
                    val = string.Empty;
                }
            }
            return val;
        }

        public static string GetValueAgainstSpecialValue(string specialValueStr)
        {
            PopulateSpecialValueArray();
            if (string.IsNullOrEmpty(specialValueStr))
                return string.Empty;
            return SpecialValues[specialValueStr];
        }
    }
}
