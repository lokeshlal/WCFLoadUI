#region File Information/History
// <copyright file="FBHelper.cs" project="WCFLoad" >
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Lokesh Lal</author>
// <date>08/19/2015</date>
// <history>
// 08/19/2015: Created - Lokesh Lal
// </history>
#endregion
using System;
using System.Data;
using System.IO;
using Common;
using FirebirdSql.Data.FirebirdClient;



namespace WCFLoad.Helper
{
    /// <summary>
    /// Firebird helper class
    /// </summary>
    public class FbHelper
    {
        private static bool _isPropertiesSet;
        private static FbConnection _connection;


        public static string IpAddress = "172.16.2.34";
        public static bool IsServer;
        public static bool IsDbAlreadyCreated;
        public static string DbPath = string.Empty;

        public static void SetInitialValues(string iPAddress, bool isServer, string dBPath)
        {
            IpAddress = iPAddress;
            IsServer = isServer;
            DbPath = dBPath;
            _isPropertiesSet = true;
        }

        public static string GetConnectionString()
        {
            if (!_isPropertiesSet)
            {
                throw new Exception("Please set properties of FBHelper before making any call to database");
            }
            FbConnectionStringBuilder connString = new FbConnectionStringBuilder
            {
                Dialect = 3,
                ServerType = FbServerType.Embedded,
                UserID = "SYSDBA",
                Password = "12345",
                DataSource = IpAddress,
                Database = DbPath
            };

            return connString.ConnectionString;
        }

        public static FbConnection GetConnection()
        {
            return _connection ?? (_connection = new FbConnection(GetConnectionString()));
        }

        public static void Open()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public static void Close()
        {
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }

        public static void CreateDataBase()
        {
            //First run
            if (IsServer && !IsDbAlreadyCreated)
            {
                if (File.Exists(DbPath))
                    FbConnection.DropDatabase(GetConnectionString());

                FbConnection.CreateDatabase(GetConnectionString());

                FbConnection connection = GetConnection();

                Open();

                FbTransaction mtransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                FbCommand command = new FbCommand("create table MethodLogs (Token varchar(50), MethodName varchar(200), Status int, " +
                    "TimeTaken bigint, Datetime timestamp, EndDatetime timestamp, Error BLOB SUB_TYPE TEXT)", connection, mtransaction);
                command.ExecuteNonQuery();
                mtransaction.Commit();
                command.Dispose(); // Thus!
                mtransaction.Dispose();


                Close();

                IsDbAlreadyCreated = !IsDbAlreadyCreated;
            }
            //subsequent runs
            //do not recreate db, only refresh the database (drop and recreate MethodLogs table)
            else if (IsDbAlreadyCreated)
            {
                Close();

                FbConnection connection = GetConnection();

                Open();

                FbTransaction mtransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                FbCommand command = new FbCommand("drop table MethodLogs", connection, mtransaction);
                command.ExecuteNonQuery();
                mtransaction.Commit();
                command.Dispose(); // Thus!
                mtransaction.Dispose();


                mtransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                command = new FbCommand("create table MethodLogs (Token varchar(50), MethodName varchar(200), Status int, " +
                    "TimeTaken bigint, Datetime timestamp, EndDatetime timestamp, Error BLOB SUB_TYPE TEXT)", connection, mtransaction);
                command.ExecuteNonQuery();
                mtransaction.Commit();
                command.Dispose(); // Thus!
                mtransaction.Dispose();

                Close();
            }
        }

        public static void InsertNewMethodLog(string methodName, string token)
        {
            try
            {
                if (IsServer)
                {

                    FbConnection connection = GetConnection();
                    Open();

                    using (FbCommand insertData = connection.CreateCommand())
                    {
                        insertData.CommandText =
                            "insert into MethodLogs values (@token, @methodName, @status, NULL, @dt, NULL, NULL)";

                        insertData.Parameters.Clear();
                        insertData.Parameters.Add("@token", FbDbType.VarChar, 50).Value = token;
                        insertData.Parameters.Add("@methodName", FbDbType.VarChar, 200).Value = methodName;
                        insertData.Parameters.Add("@status", FbDbType.Integer).Value = -1;
                        insertData.Parameters.Add("@dt", FbDbType.TimeStamp).Value = DateTime.Now;
                        insertData.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                //eat exceptions of firebird. TODO
            }
        }

        public static void UpdateMethodLog(string token, string methodName, MethodStatus status, long? timeTaken = null, string error = null)
        {
            try
            {
                if (IsServer)
                {
                    FbConnection connection = GetConnection();
                    Open();

                    using (FbCommand insertData = connection.CreateCommand())
                    {
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (timeTaken != null)
                            insertData.CommandText = "insert into MethodLogs values (@token, @methodName, @status, @time, @dt, @edt";
                        else
                            insertData.CommandText = "insert into MethodLogs values (@token, @methodName, @status, NULL, @dt, @edt";

                        if (!string.IsNullOrEmpty(error))
                            insertData.CommandText += ", @err)";
                        else
                            insertData.CommandText += ", NULL)";

                        insertData.Parameters.Clear();

                        insertData.Parameters.Add("@token", FbDbType.VarChar, 50).Value = token;
                        insertData.Parameters.Add("@methodName", FbDbType.VarChar, 200).Value = methodName;
                        insertData.Parameters.Add("@status", FbDbType.Integer).Value = status;
                        if (timeTaken != null)
                        {
                            insertData.Parameters.Add("@time", FbDbType.BigInt).Value = timeTaken.Value;
                        }


                        insertData.Parameters.Add("@dt", FbDbType.TimeStamp).Value = DateTime.Now;
                        insertData.Parameters.Add("@edt", FbDbType.TimeStamp).Value = DateTime.Now;
                        if (!string.IsNullOrEmpty(error))
                        {
                            insertData.Parameters.Add("@err", FbDbType.Text).Value = error;
                        }

                        insertData.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                //eat exceptions of firebird. TODO
            }
        }

        /// <summary>
        /// Get data to display method stats
        /// </summary>
        /// <returns></returns>
        public static DataTable GetMethodLogStats()
        {
            try
            {
                string commandString = @"select m.MethodName,
                                (select count(1) from (select m1.MethodName from MethodLogs m1 where m1.MethodName = m.MethodName group by m1.MethodName, m1.Token Having Max(m1.Status) = 0) a) PreThread,
                                (select count(1) from (select m1.MethodName from MethodLogs m1 where m1.MethodName = m.MethodName group by m1.MethodName, m1.Token Having Max(m1.Status) = 1) a) Started,
                                (select count(1) from (select m1.MethodName from MethodLogs m1 where m1.MethodName = m.MethodName group by m1.MethodName, m1.Token Having Max(m1.Status) = 2) a) Pass,
                                (select count(1) from (select m1.MethodName from MethodLogs m1 where m1.MethodName = m.MethodName group by m1.MethodName, m1.Token Having Max(m1.Status) = 3) a) Fail,
                                (select count(1) from (select m1.MethodName from MethodLogs m1 where m1.MethodName = m.MethodName group by m1.MethodName, m1.Token Having Max(m1.Status) = 4) a) Error,
                                (select count(1) from (select m1.MethodName from MethodLogs m1 where m1.MethodName = m.MethodName group by m1.MethodName, m1.Token)) Total,
                                (select avg(TimeTaken) from MethodLogs m1 where m1.MethodName = m.MethodName and m1.Status = 2) AvgTimeTaken,
                                (select min(TimeTaken) from MethodLogs m1 where m1.MethodName = m.MethodName and m1.Status = 2) MinTime,
                                (select max(TimeTaken) from MethodLogs m1 where m1.MethodName = m.MethodName and m1.Status = 2) MaxTime
                                from MethodLogs m
                                Where m.Status != -1
                                group by m.MethodName";

                FbCommand command = new FbCommand(commandString, GetConnection());
                DataTable dt = new DataTable();
                FbDataAdapter adapter = new FbDataAdapter(command);
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception)
            {
                //eat exceptions of firebird. TODO
                return new DataTable();
            }
        }

        /// <summary>
        /// get data to print chart for a method
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns></returns>
        public static DataTable GetMethodLogChartData(string methodName)
        {
            string commandString = @"select m.DateTime, m.TimeTaken
                                from MethodLogs m
                                Where m.MethodName = @methodName
                                and m.Status = 2
                                order by m.Datetime asc";
            FbCommand command = new FbCommand(commandString, GetConnection());
            command.Parameters.Clear();
            command.Parameters.Add("@methodName", methodName);

            DataTable dt = new DataTable();
            FbDataAdapter adapter = new FbDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// Get data for final report
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAllData()
        {
            string commandString = @"
                            select m1.DateTime, m1.MethodName, coalesce(m1.TimeTaken,'') TimeTaken , decode( m1.Status,
                                    0, 'PreThread',
                                    1, 'Started',
                                    2, 'Pass',
                                    3, 'Fail',
                                    4, 'Error',
                                    'UnKnown') Status,
                                coalesce(m1.Error,'') Error
                            from 
                            MethodLogs m1 join
                            (select m.MethodName, m.Token, Max(m.Status) FinalStatus from MethodLogs m group by m.MethodName, m.Token) m2
                            on m1.Status = m2.FinalStatus
                            and m1.MethodName = m2.MethodName
                            and m1.Token = m2.Token
                            ";
            FbCommand command = new FbCommand(commandString, GetConnection());
            command.Parameters.Clear();

            DataTable dt = new DataTable();
            FbDataAdapter adapter = new FbDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
    }
}
