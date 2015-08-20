#region File Information/History
// <copyright file="TestHelper.cs" project="WCFLoad" >
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
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace WCFLoad.Helper
{
    public static class TestHelper
    {
        /// <summary>
        /// Converts value to enum type and if value is empty return default enum object
        /// </summary>
        /// <param name="enumType">Enum Type</param>
        /// <param name="value">value to check</param>
        /// <returns>Enum object</returns>
        public static object GetDefaultEnumValue(Type enumType, string value)
        {
            Type typeToCast = Nullable.GetUnderlyingType(enumType) ?? enumType;

            var newValue = value == "" ? Activator.CreateInstance(typeToCast) : Enum.ToObject(enumType, Convert.ToInt32(value));

            return newValue;
        }

        /// <summary>
        /// Converts value to type and if value is empty return default type object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="value">value to check</param>
        /// <returns>type object</returns>
        public static object GetSafeValue(Type type, string value)
        {
            Type typeToCast = Nullable.GetUnderlyingType(type) ?? type;
            object newValue;

            //Check for empty strrings and return default value for a type
            if (value == "" && typeToCast == typeof(string))
            {
                //ok to have empty string for string type
                newValue = value;
                return newValue;
            }
            else if (value == "" && typeToCast != typeof(string))
            {
                if (typeToCast.IsValueType)
                {
                    newValue = Activator.CreateInstance(typeToCast);
                    return newValue;
                }
                else
                {
                    return null;
                }
            }
            object safeValue = string.IsNullOrEmpty(value) ? null : Convert.ChangeType(value, typeToCast);
            return safeValue;
        }

        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <param name="obj">object to serialize</param>
        /// <returns>serialized object</returns>
        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// deserialize the xml string to type
        /// </summary>
        /// <param name="xml">xml to deserialize</param>
        /// <param name="toType">type</param>
        /// <returns>returns object of type <see cref="toType">toType</see>/></returns>
        public static object Deserialize(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(toType);
                return deserializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// Checks for special value and resolve the special value to its actual meaning
        /// </summary>
        /// <param name="val">value to check</param>
        /// <returns></returns>
        public static string CheckForSpecialValue(string val)
        {
            return SpecialValue.ResolveSpecialValue(val);
        }

        /// <summary>
        /// Encode attribute name
        /// </summary>
        /// <param name="attibuteName"></param>
        /// <returns></returns>
        public static string EncodeAttributeName(string attibuteName)
        {
            return attibuteName.Replace("[", "OPENING_BRACKET")
                                            .Replace("]", "CLOSING_BRACKET").Replace("`", "SPECIAL_CHAR");
        }

        /// <summary>
        /// Decode attribute name
        /// </summary>
        /// <param name="attibuteName"></param>
        /// <returns></returns>
        public static string DecodeAttributeName(string attibuteName)
        {
            return attibuteName.Replace("OPENING_BRACKET", "[")
                                            .Replace("CLOSING_BRACKET", "]").Replace("SPECIAL_CHAR", "`");
        }

        public static void GetArrayIndexesFromLinearIndex(List<int> arrayIndexes, int ir, List<int> indexes, int i)
        {
            int tot = 1;
            for (int ri = arrayIndexes.Count - 1; ri > ir; ri--)
            {
                tot = tot * arrayIndexes[ri];
            }

            if (arrayIndexes.Count - 1 == ir)
            {
                tot = arrayIndexes[arrayIndexes.Count - 1];
            }

            if (ir == arrayIndexes.Count - 1)
            {
                indexes.Add(i % tot);
            }
            else
            {
                indexes.Add(i / tot);
            }

            if (indexes[ir] >= arrayIndexes[ir])
            {
                indexes[ir] = indexes[ir] % arrayIndexes[ir];
            }
        }

    }
}
