#region License
// Copyright (c) 2014 Davide Gironi
//
// Please refer to LICENSE file for licensing information.
#endregion

using DG.DataConcurrencyHelper.Objects;
using System;
using System.Collections.Generic;
using System.Data;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.Linq;

namespace DG.DataConcurrencyHelper
{
    public class DGDataConcurrencyHelper
    {
        /// <summary>
        /// Default table prefix
        /// </summary>
        public const string DefaultTablePrefix = "dch_";

        /// <summary>
        /// private SqlConnection used by the instance
        /// </summary>
        private SqlConnection _sqlConnection = new SqlConnection();

        /// <summary>
        /// Status of the record
        /// </summary>
        public enum Status { Editing, Viewing, Unknown };

        /// <summary>
        /// Table prefix
        /// </summary>
        private string _tablePrefix = DefaultTablePrefix;

        /// <summary>
        /// Build a DataConcurrencyHelper
        /// </summary>
        public DGDataConcurrencyHelper()
        { }
        public DGDataConcurrencyHelper(SqlConnection sqlConnection, string tablePrefix)
        {
            _sqlConnection = sqlConnection;
            _tablePrefix = String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix;
        }
        public DGDataConcurrencyHelper(string sqlConnectionString, string tablePrefix)
        {
            _sqlConnection.ConnectionString = sqlConnectionString;
            _tablePrefix = String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix;
        }

        /// <summary>
        /// List all connection status
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tablePrefix"></param>
        /// <returns></returns>
        public static IList<ConcurrencyRecord> List(SqlConnection sqlConnection, string tablePrefix)
        {
            List<ConcurrencyRecord> ret = new List<ConcurrencyRecord>();

            if (!OpenSQLConnection(sqlConnection))
                return ret;

            try
            {
                //check for user in group
                SqlCommand sql_cm1 = null;
                SqlDataReader sql_rd1 = null;

                sql_cm1 = new SqlCommand();
                sql_cm1.Connection = sqlConnection;
                sql_cm1.CommandType = CommandType.Text;
                sql_cm1.CommandText = @"
SELECT
    concurrencyrecords_id,
    concurrencyrecords_status,
    concurrencyrecords_database,
    concurrencyrecords_table,
    concurrencyrecords_recordid,
    concurrencyrecords_application,
    concurrencyrecords_logusername,
    concurrencyrecords_datetime
FROM " + (String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix) + @"concurrencyrecords
ORDER BY concurrencyrecords_datetime DESC";
                sql_rd1 = sql_cm1.ExecuteReader();
                while (sql_rd1.Read())
                {
                    ret.Add(new ConcurrencyRecord()
                    {
                        Id = sql_rd1.GetInt32(0),
                        Status = (sql_rd1.GetString(1) == "E" ? Status.Editing :
                                    (sql_rd1.GetString(1) == "V" ? Status.Viewing : Status.Unknown)),
                        Database = sql_rd1.GetString(2),
                        Table = sql_rd1.GetString(3),
                        RecordId = sql_rd1.GetString(4),
                        Application = sql_rd1.GetString(5),
                        Logusername = sql_rd1.GetString(6),
                        Datetime = sql_rd1.GetDateTime(7)
                    });
                }
                sql_rd1.Close();

                sql_rd1.Dispose();
                sql_cm1.Dispose();
            }
            catch
            {
                sqlConnection.Close();
            }

            return ret;
        }
        public IList<ConcurrencyRecord> List()
        {
            return List(this._sqlConnection, this._tablePrefix);
        }

        /// <summary>
        /// Find a connection status
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ConcurrencyRecord Find(SqlConnection sqlConnection, string tablePrefix, int id)
        {
            ConcurrencyRecord ret = null;

            if (!OpenSQLConnection(sqlConnection))
                return ret;

            try
            {
                //check for user in group
                SqlCommand sql_cm1 = null;
                SqlDataReader sql_rd1 = null;

                sql_cm1 = new SqlCommand();
                sql_cm1.Connection = sqlConnection;
                sql_cm1.CommandType = CommandType.Text;
                sql_cm1.CommandText = @"
SELECT
    concurrencyrecords_id,
    concurrencyrecords_status,
    concurrencyrecords_database,
    concurrencyrecords_table,
    concurrencyrecords_recordid,
    concurrencyrecords_application,
    concurrencyrecords_logusername,
    concurrencyrecords_datetime
FROM " + (String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix) + @"concurrencyrecords
WHERE concurrencyrecords_id = @concurrencyrecords_id";
                sql_cm1.Parameters.Add("@concurrencyrecords_id", SqlDbType.Int).Value = id;
                sql_rd1 = sql_cm1.ExecuteReader();
                if (sql_rd1.Read())
                {
                    ret = new ConcurrencyRecord()
                    {
                        Id = sql_rd1.GetInt32(0),
                        Status = (sql_rd1.GetString(1) == "E" ? Status.Editing :
                                    (sql_rd1.GetString(1) == "V" ? Status.Viewing : Status.Unknown)),
                        Database = sql_rd1.GetString(2),
                        Table = sql_rd1.GetString(3),
                        RecordId = sql_rd1.GetString(4),
                        Application = sql_rd1.GetString(5),
                        Logusername = sql_rd1.GetString(6),
                        Datetime = sql_rd1.GetDateTime(7)
                    };
                }
                sql_rd1.Close();

                sql_rd1.Dispose();
                sql_cm1.Dispose();
            }
            catch
            {
                sqlConnection.Close();
            }

            return ret;
        }
        public ConcurrencyRecord Find(int id)
        {
            return Find(this._sqlConnection, this._tablePrefix, id);
        }

        /// <summary>
        /// Find a connection status
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="database"></param>
        /// <param name="table"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static ConcurrencyRecord Find(SqlConnection sqlConnection, string tablePrefix, string database, string table, string recordId)
        {
            ConcurrencyRecord ret = null;

            if (!OpenSQLConnection(sqlConnection))
                return ret;

            try
            {
                //check for user in group
                SqlCommand sql_cm1 = null;
                SqlDataReader sql_rd1 = null;

                sql_cm1 = new SqlCommand();
                sql_cm1.Connection = sqlConnection;
                sql_cm1.CommandType = CommandType.Text;
                sql_cm1.CommandText = @"
SELECT
    concurrencyrecords_id,
    concurrencyrecords_status,
    concurrencyrecords_database,
    concurrencyrecords_table,
    concurrencyrecords_recordid,
    concurrencyrecords_application,
    concurrencyrecords_logusername,
    concurrencyrecords_datetime
FROM " + (String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix) + @"concurrencyrecords
WHERE
    concurrencyrecords_database = @concurrencyrecords_database AND
    concurrencyrecords_table = @concurrencyrecords_table AND
    concurrencyrecords_recordid = @concurrencyrecords_recordid";
                sql_cm1.Parameters.Add("@concurrencyrecords_database", SqlDbType.VarChar, 256).Value = database;
                sql_cm1.Parameters.Add("@concurrencyrecords_table", SqlDbType.VarChar, 256).Value = table;
                sql_cm1.Parameters.Add("@concurrencyrecords_recordid", SqlDbType.VarChar, 1024).Value = recordId;
                sql_rd1 = sql_cm1.ExecuteReader();
                if (sql_rd1.Read())
                {
                    ret = new ConcurrencyRecord()
                    {
                        Id = sql_rd1.GetInt32(0),
                        Status = (sql_rd1.GetString(1) == "E" ? Status.Editing :
                                    (sql_rd1.GetString(1) == "V" ? Status.Viewing : Status.Unknown)),
                        Database = sql_rd1.GetString(2),
                        Table = sql_rd1.GetString(3),
                        RecordId = sql_rd1.GetString(4),
                        Application = sql_rd1.GetString(5),
                        Logusername = sql_rd1.GetString(6),
                        Datetime = sql_rd1.GetDateTime(7)
                    };
                }
                sql_rd1.Close();

                sql_rd1.Dispose();
                sql_cm1.Dispose();
            }
            catch
            {
                sqlConnection.Close();
            }

            return ret;
        }
        public ConcurrencyRecord Find(string database, string table, string recordId)
        {
            return Find(this._sqlConnection, this._tablePrefix, database, table, recordId);
        }

        /// <summary>
        /// Purge connection status older than selected hours
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="hours"></param>
        /// <returns></returns>
        public static int Purge(SqlConnection sqlConnection, string tablePrefix, int hours)
        {
            int ret = 0;

            if (!OpenSQLConnection(sqlConnection))
                return ret;

            try
            {
                //check for user in group
                SqlCommand sql_cm1 = null;

                sql_cm1 = new SqlCommand();
                sql_cm1.Connection = sqlConnection;
                sql_cm1.CommandType = CommandType.Text;
                sql_cm1.CommandText = @"
DELETE FROM " + (String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix) + @"concurrencyrecords
WHERE concurrencyrecords_datetime < DATEADD(hour, @hour, GETDATE());";
                sql_cm1.Parameters.Add("@hour", SqlDbType.Int).Value = -hours;
                ret = sql_cm1.ExecuteNonQuery();

                sql_cm1.Dispose();
            }
            catch
            {
                sqlConnection.Close();
            }

            return ret;
        }
        public int PurgeConnectionsStatus(int hours)
        {
            return Purge(this._sqlConnection, this._tablePrefix, hours);
        }

        /// <summary>
        /// Get the status of a connection record
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="database"></param>
        /// <param name="table"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static Nullable<Status> GetStatus(SqlConnection sqlConnection, string tablePrefix, string database, string table, string recordId)
        {
            Nullable<Status> ret = null;

            if (!OpenSQLConnection(sqlConnection))
                return ret;

            try
            {
                //check for user in group
                SqlCommand sql_cm1 = null;
                SqlDataReader sql_rd1 = null;

                sql_cm1 = new SqlCommand();
                sql_cm1.Connection = sqlConnection;
                sql_cm1.CommandType = CommandType.Text;
                sql_cm1.CommandText = @"
SELECT concurrencyrecords_status
FROM " + (String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix) + @"concurrencyrecords
WHERE
    concurrencyrecords_database = @concurrencyrecords_database AND
    concurrencyrecords_table = @concurrencyrecords_table AND
    concurrencyrecords_recordid = @concurrencyrecords_recordid";
                sql_cm1.Parameters.Add("@concurrencyrecords_database", SqlDbType.VarChar, 256).Value = database;
                sql_cm1.Parameters.Add("@concurrencyrecords_table", SqlDbType.VarChar, 256).Value = table;
                sql_cm1.Parameters.Add("@concurrencyrecords_recordid", SqlDbType.VarChar, 1024).Value = recordId;
                sql_rd1 = sql_cm1.ExecuteReader();
                if (sql_rd1.Read())
                {
                    if (sql_rd1.GetString(0) == "E")
                        ret = Status.Editing;
                }
                sql_rd1.Close();

                sql_rd1.Dispose();
                sql_cm1.Dispose();
            }
            catch
            {
                sqlConnection.Close();
            }

            return ret;
        }
        public Nullable<Status> GetStatus(string database, string table, string recordId)
        {
            return GetStatus(this._sqlConnection, this._tablePrefix, database, table, recordId);
        }

        /// <summary>
        /// Set the status of a connection record
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="database"></param>
        /// <param name="table"></param>
        /// <param name="recordId"></param>
        /// <param name="application"></param>
        /// <param name="logUsername"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool SetStatus(SqlConnection sqlConnection, string tablePrefix, string database, string table, string recordId, string application, string logUsername, Status status)
        {
            bool ret = false;

            Nullable<Status> actualAction = GetStatus(sqlConnection, tablePrefix, database, table, recordId);
            if (actualAction != null)
            {
                if ((Status)actualAction == Status.Editing)
                    return ret;
            }

            if (!OpenSQLConnection(sqlConnection))
                return ret;

            try
            {
                //check for user in group
                SqlCommand sql_cm1 = null;

                sql_cm1 = new SqlCommand();
                sql_cm1.Connection = sqlConnection;
                sql_cm1.CommandType = CommandType.Text;
                sql_cm1.CommandText = @"
INSERT INTO " + (String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix) + @"concurrencyrecords(
    concurrencyrecords_status,
    concurrencyrecords_database,
    concurrencyrecords_table,
    concurrencyrecords_recordid,
    concurrencyrecords_application,
    concurrencyrecords_logusername,
    concurrencyrecords_datetime)
VALUES(
    @concurrencyrecords_status,
    @concurrencyrecords_database,
    @concurrencyrecords_table,
    @concurrencyrecords_recordid,
    @concurrencyrecords_application,
    @concurrencyrecords_logusername,
    GETDATE()
)";
                switch (status)
                {
                    case Status.Editing:
                        sql_cm1.Parameters.Add("@concurrencyrecords_status", SqlDbType.Char).Value = 'E';
                        break;
                    case Status.Viewing:
                    default:
                        sql_cm1.Parameters.Add("@concurrencyrecords_status", SqlDbType.Char).Value = 'V';
                        break;
                }
                sql_cm1.Parameters.Add("@concurrencyrecords_database", SqlDbType.VarChar, 256).Value = database;
                sql_cm1.Parameters.Add("@concurrencyrecords_table", SqlDbType.VarChar, 256).Value = table;
                sql_cm1.Parameters.Add("@concurrencyrecords_recordid", SqlDbType.VarChar, 1024).Value = recordId;
                sql_cm1.Parameters.Add("@concurrencyrecords_application", SqlDbType.VarChar, 128).Value = application;
                sql_cm1.Parameters.Add("@concurrencyrecords_logusername", SqlDbType.VarChar, 64).Value = logUsername;
                if (sql_cm1.ExecuteNonQuery() != 0)
                {
                    ret = true;
                }

                sql_cm1.Dispose();
            }
            catch
            {
                sqlConnection.Close();
            }

            return ret;
        }
        public bool SetStatus(string database, string table, string recordId, string application, string logUsername, Status status)
        {
            return SetStatus(this._sqlConnection, this._tablePrefix, database, table, recordId, application, logUsername, status);
        }

        /// <summary>
        /// Reset a connection status, remove it
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="database"></param>
        /// <param name="table"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static bool ResetStatus(SqlConnection sqlConnection, string tablePrefix, string database, string table, string recordId)
        {
            bool ret = false;

            if (!OpenSQLConnection(sqlConnection))
                return ret;

            try
            {
                //check for user in group
                SqlCommand sql_cm1 = null;

                sql_cm1 = new SqlCommand();
                sql_cm1.Connection = sqlConnection;
                sql_cm1.CommandType = CommandType.Text;
                sql_cm1.CommandText = @"
DELETE FROM " + (String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix) + @"concurrencyrecords
WHERE
    concurrencyrecords_database = @concurrencyrecords_database AND
    concurrencyrecords_table = @concurrencyrecords_table AND
    concurrencyrecords_recordid = @concurrencyrecords_recordid";
                sql_cm1.Parameters.Add("@concurrencyrecords_database", SqlDbType.VarChar, 256).Value = database;
                sql_cm1.Parameters.Add("@concurrencyrecords_table", SqlDbType.VarChar, 256).Value = table;
                sql_cm1.Parameters.Add("@concurrencyrecords_recordid", SqlDbType.VarChar, 1024).Value = recordId;
                if (sql_cm1.ExecuteNonQuery() != 0)
                {
                    ret = true;
                }

                sql_cm1.Dispose();
            }
            catch
            {
                sqlConnection.Close();
            }

            return ret;
        }
        public bool ResetStatus(string database, string table, string recordId)
        {
            return ResetStatus(this._sqlConnection, this._tablePrefix, database, table, recordId);
        }

        /// <summary>
        /// Remove a connection record
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Remove(SqlConnection sqlConnection, string tablePrefix, int id)
        {
            bool ret = false;

            if (!OpenSQLConnection(sqlConnection))
                return ret;

            try
            {
                //check for user in group
                SqlCommand sql_cm1 = null;

                sql_cm1 = new SqlCommand();
                sql_cm1.Connection = sqlConnection;
                sql_cm1.CommandType = CommandType.Text;
                sql_cm1.CommandText = @"
DELETE FROM " + (String.IsNullOrEmpty(tablePrefix) ? "" : tablePrefix) + @"concurrencyrecords
WHERE
    concurrencyrecords_id = @concurrencyrecords_id";
                sql_cm1.Parameters.Add("@concurrencyrecords_id", SqlDbType.Int).Value = id;
                if (sql_cm1.ExecuteNonQuery() != 0)
                {
                    ret = true;
                }

                sql_cm1.Dispose();
            }
            catch
            {
                sqlConnection.Close();
            }

            return ret;
        }
        public bool Remove(int id)
        {
            return Remove(this._sqlConnection, this._tablePrefix, id);
        }

        /// <summary>
        /// Convert a dictionary of keypairs to a Json string
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string KeyPairsDictionaryToJson(IDictionary<string, object> dictionary)
        {
            int i = 0;
            return "{ " + string.Join(", ", dictionary.Select(d => string.Format((int.TryParse(d.Value.ToString(), out i) ? "\"{0}\": {1}" : "\"{0}\": \"{1}\""), d.Key, d.Value))) + " }";
        }

        /// <summary>
        /// Open an SqlConnection
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        private static bool OpenSQLConnection(SqlConnection sqlConnection)
        {
            bool ret = false;

            try
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State == ConnectionState.Closed || sqlConnection.State == ConnectionState.Broken)
                        sqlConnection.Open();

                    if (sqlConnection.State == ConnectionState.Open)
                    {
                        return true;
                    }
                }
            }
            catch { }

            return ret;
        }

    }
}
