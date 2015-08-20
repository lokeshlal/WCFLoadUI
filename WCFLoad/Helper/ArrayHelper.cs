#region File Information/History
// <copyright file="ArrayHelper.cs" project="WCFLoad" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;

namespace WCFLoad.Helper
{
    public static class ArrayHelper
    {
        // ReSharper disable once RedundantAssignment
        public static object GetJaggedArray(int dimensions, Type arrayBaseType, Array aOo)
        {
            object d = null;
            switch (dimensions)
            {
                case 2:
                    d = typeof(ArrayHelper).GetMethod("ConvertToJaggedArray2").MakeGenericMethod(arrayBaseType).Invoke(null, new object[] { aOo });
                    break;
                case 3:
                    d = typeof(ArrayHelper).GetMethod("ConvertToJaggedArray3").MakeGenericMethod(arrayBaseType).Invoke(null, new object[] { aOo });
                    break;
                case 4:
                    d = typeof(ArrayHelper).GetMethod("ConvertToJaggedArray4").MakeGenericMethod(arrayBaseType).Invoke(null, new object[] { aOo });
                    break;
                case 5:
                    d = typeof(ArrayHelper).GetMethod("ConvertToJaggedArray5").MakeGenericMethod(arrayBaseType).Invoke(null, new object[] { aOo });
                    break;
                default:
                    d = aOo;
                    break;
            }
            return d;
        }

        #region convert array to jagged array
        public static T[][] ConvertToJaggedArray2<T>(T[,] multiArray)
        {
            int firstElement = multiArray.GetLength(0);
            int secondElement = multiArray.GetLength(1);

            T[][] jaggedArray = new T[firstElement][];

            for (int c = 0; c < firstElement; c++)
            {
                jaggedArray[c] = new T[secondElement];
                for (int r = 0; r < secondElement; r++)
                {
                    jaggedArray[c][r] = multiArray[c, r];
                }
            }
            return jaggedArray;
        }

        public static T[][][] ConvertToJaggedArray3<T>(T[, ,] multiArray)
        {
            int firstElement = multiArray.GetLength(0);
            int secondElement = multiArray.GetLength(1);
            int thirdElement = multiArray.GetLength(2);

            T[][][] jaggedArray = new T[firstElement][][];

            for (int c = 0; c < firstElement; c++)
            {
                jaggedArray[c] = new T[secondElement][];
                for (int r = 0; r < secondElement; r++)
                {
                    jaggedArray[c][r] = new T[thirdElement];
                    for (int p = 0; p < thirdElement; p++)
                    {
                        jaggedArray[c][r][p] = multiArray[c, r, p];
                    }
                }
            }
            return jaggedArray;
        }

        public static T[][][][] ConvertToJaggedArray4<T>(T[, , ,] multiArray)
        {
            int firstElement = multiArray.GetLength(0);
            int secondElement = multiArray.GetLength(1);
            int thirdElement = multiArray.GetLength(2);
            int fourthElement = multiArray.GetLength(3);

            T[][][][] jaggedArray = new T[firstElement][][][];

            for (int c = 0; c < firstElement; c++)
            {
                jaggedArray[c] = new T[secondElement][][];
                for (int r = 0; r < secondElement; r++)
                {
                    jaggedArray[c][r] = new T[thirdElement][];
                    for (int p = 0; p < thirdElement; p++)
                    {
                        jaggedArray[c][r][p] = new T[fourthElement];
                        for (int q = 0; q < fourthElement; q++)
                        {
                            jaggedArray[c][r][p][q] = multiArray[c, r, p, q];
                        }
                    }
                }
            }
            return jaggedArray;
        }

        public static T[][][][][] ConvertToJaggedArray5<T>(T[, , , ,] multiArray)
        {
            int firstElement = multiArray.GetLength(0);
            int secondElement = multiArray.GetLength(1);
            int thirdElement = multiArray.GetLength(2);
            int fourthElement = multiArray.GetLength(3);
            int fifthElement = multiArray.GetLength(4);

            T[][][][][] jaggedArray = new T[firstElement][][][][];

            for (int c = 0; c < firstElement; c++)
            {
                jaggedArray[c] = new T[secondElement][][][];
                for (int r = 0; r < secondElement; r++)
                {
                    jaggedArray[c][r] = new T[thirdElement][][];
                    for (int p = 0; p < thirdElement; p++)
                    {
                        jaggedArray[c][r][p] = new T[fourthElement][];
                        for (int q = 0; q < fourthElement; q++)
                        {
                            jaggedArray[c][r][p][q] = new T[fifthElement];
                            for (int s = 0; s < fifthElement; s++)
                            {
                                jaggedArray[c][r][p][q][s] = multiArray[c, r, p, q, s];
                            }
                        }
                    }
                }
            }
            return jaggedArray;
        }
        #endregion
    }
}
