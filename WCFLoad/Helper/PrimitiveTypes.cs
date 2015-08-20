#region File Information/History
// <copyright file="PrimitiveTypes.cs" project="WCFLoad" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Linq;

namespace WCFLoad.Helper
{
    public static class PrimitiveTypes
    {
        public static readonly Type[] List;

        static PrimitiveTypes()
        {
            var types = new[]
                          {
                              typeof (Enum),
                              typeof (String),
                              typeof (Char),
                              typeof (Guid),

                              typeof (Boolean),
                              typeof (Byte),
                              typeof (Int16),
                              typeof (Int32),
                              typeof (Int64),
                              typeof (Single),
                              typeof (Double),
                              typeof (Decimal),

                              typeof (SByte),
                              typeof (UInt16),
                              typeof (UInt32),
                              typeof (UInt64),

                              typeof (DateTime),
                              typeof (DateTimeOffset),
                              typeof (TimeSpan),
                          };


            var nullTypes = from t in types
                            where t.IsValueType
                            select typeof(Nullable<>).MakeGenericType(t);

            List = types.Concat(nullTypes).ToArray();
        }

        public static Type GetTypeByName(string typeName)
        {
            switch (typeName.ToLower())
            {
                case "system.string": return typeof(string);
                case "system.int32": return typeof(int);
                case "system.boolean":
                case "system.bool": return typeof(bool);
                case "system.datetime": return typeof(DateTime);
                case "system.int64": return typeof(Int64);
                case "system.long": return typeof(long);
                case "system.double": return typeof(double);
                case "system.decimal": return typeof(decimal);
                case "system.char": return typeof(char);
                case "system.guid": return typeof(Guid);
                case "system.single": return typeof(Single);
                case "system.sbyte": return typeof(sbyte);
                case "system.uint16": return typeof(UInt16);
                case "system.uint32": return typeof(UInt32);
                case "system.uint64": return typeof(UInt64);
                case "system.timespan": return typeof(TimeSpan);
            }
            return null;
        }

        public static bool Test(string typeFullName)
        {
            if (List.Select(l => l.FullName).ToList().Contains(typeFullName))
                return true;
            return false;
        }

        public static bool Test(Type type)
        {
            if (List.Any(x => x.IsAssignableFrom(type)))
                return true;

            bool isNullable = Nullable.GetUnderlyingType(type) != null;

            if(isNullable
                && List.Any(x => x.IsAssignableFrom(Nullable.GetUnderlyingType(type))))
                return true;

            var nut = Nullable.GetUnderlyingType(type);
            return nut != null && nut.IsEnum;
        }
    }
}
