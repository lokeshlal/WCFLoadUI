#region File Information/History
// <copyright file="PrimitiveTypeValueValidation.cs" project="WCFLoadUI" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;

namespace WCFLoadUI.Validations
{
    public class PrimitiveTypeValueValidation
    {
        public static bool IsValidValue(Type type, string value)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Convert.ChangeType(value, type);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
