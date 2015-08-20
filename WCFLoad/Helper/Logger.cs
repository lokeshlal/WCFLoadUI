#region File Information/History
// <copyright file="Logger.cs" project="WCFLoad" >
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
using System.Threading.Tasks;

namespace WCFLoad.Helper
{
    /// <summary>
    /// Logging class
    /// </summary>
    public class Logger
    {
        #region private properties
        private static readonly object LockObj = new object();
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static List<string> _logs = new List<string>();
        #endregion

        #region private methods
        /// <summary>
        /// Write performance run logs to file
        /// </summary>
        private static void WriteLogsToFile()
        {
            StreamWriter log = new StreamWriter(Test.TestPackage.ResultFileName, true);
            // ReSharper disable once InconsistentlySynchronizedField
            log.Write(string.Join(Environment.NewLine, _logs.ToArray()));
            //Clear inmemory logs
            // ReSharper disable once InconsistentlySynchronizedField
            _logs.Clear();
            //flush all logs
            log.Flush();
            //close stream
            log.Close();
        }
        #endregion

        #region public methods
        /// <summary>
        /// Public method to write performance run logs to file
        /// </summary>
        public static void WriteLogs()
        {
            Task.Run(() =>
            {
                WriteLogsToFile();
            }
            );
        }

        /// <summary>
        /// Add log message to log queue
        /// </summary>
        /// <param name="message">log message</param>
        public static void LogMessage(string message)
        {
            lock (LockObj)
            {
                _logs.Add(string.Format("{0} => {1}", DateTime.Now.ToLongTimeString(), message));
                if (_logs.Count > 500)
                {
                    WriteLogsToFile();
                }
            }
        }
        #endregion
    }
}
