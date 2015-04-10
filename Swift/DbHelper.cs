using System;
using System.Collections.Generic;

namespace Swift
{
    public class DbHelper
    {
        private static string DefaultConnectionName = null;

        public static void SetDefaultConnectionName(string connectionString)
        {
            DefaultConnectionName = connectionString; 
        }

        public static IEnumerable<T> GetList<T>(string connectionString, string commandText, params DbParameter[] parameters)
            where T : IPersistent, new()
        {
            var dataAccess = SwiftDataAccess.Create(connectionString);

            List<T> list = new List<T>();

            dataAccess.SetCommand(commandText, CommandType.StoredProc, parameters);

            using (var reader = dataAccess.ExecuteReader())
            {
                while (reader.Read())
                {
                    T item = new T();
                    item.Init(reader);

                    list.Add(item);
                }
            }

            return list;
        }

        public static IEnumerable<T> GetList<T>(string commandText, params DbParameter[] parameters)
            where T : IPersistent, new()
        {
            CheckDefaultConnection();
            return GetList<T>(DefaultConnectionName, commandText, parameters);
        }

        public static T GetItem<T>(string connectionString, string commandText, params DbParameter[] parameters)
            where T : IPersistent, new()
        {
            var dataAccess = SwiftDataAccess.Create(connectionString);

            dataAccess.SetCommand(commandText, CommandType.StoredProc, parameters);

            using (var reader = dataAccess.ExecuteReader())
            {
                if (reader.Read())
                {
                    T item = new T();
                    item.Init(reader);
                    return item;
                }
            }
            return default(T);
        }

        private static void CheckDefaultConnection()
        {
            if (DefaultConnectionName == null)
                throw new InvalidOperationException("DefaultConnectionName must be initialized before using this method. (Use DbHelper.SetDefaultConnectionName().)");
        }

        public static T GetItem<T>(string commandText, params DbParameter[] parameters)
            where T : IPersistent, new()
        {
            CheckDefaultConnection();
            return GetItem<T>(DefaultConnectionName, commandText, parameters);
        }

        /// <summary>
        /// Equivalent to calling ExecuteNonQuery() and checking the result; if result != 1 (no rows were 
        /// affected or more than one row affected), throws an exception.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        public static void UpdateSingle(string connectionString, string commandText, params DbParameter[] parameters)
        {
            var rowsAffected = ExecuteNonQuery(connectionString, commandText, parameters);
            if (rowsAffected != 1)
                throw new ApplicationException("Update operation was supposed to affect a single row but affected multiple or zero rows.");
        }

        public static void UpdateSingle(string commandText, params DbParameter[] parameters)
        {
            CheckDefaultConnection();
            UpdateSingle(DefaultConnectionName, commandText, parameters);
        }

        public static int ExecuteNonQuery(string connectionString, string commandText, params DbParameter[] parameters)
        {
            var dataAccess = SwiftDataAccess.Create(connectionString);
            dataAccess.SetCommand(commandText, CommandType.StoredProc, parameters);
            return dataAccess.ExecuteNonQuery();
        }

        public static int ExecuteNonQuery(string commandText, params DbParameter[] parameters)
        {
            CheckDefaultConnection();
            return ExecuteNonQuery(DefaultConnectionName, commandText, parameters);
        }

        public static SwiftDataReader ExecuteReader(string connectionString, string commandText, params DbParameter[] parameters)
        {
            var dataAccess = SwiftDataAccess.Create(connectionString);
            dataAccess.SetCommand(commandText, CommandType.StoredProc, parameters);
            return dataAccess.ExecuteReader();
        }

        public static SwiftDataReader ExecuteReader(string commandText, params DbParameter[] parameters)
        {
            CheckDefaultConnection();
            return ExecuteReader(DefaultConnectionName, commandText, parameters);
        }

        public static T ExecuteScalar<T>(string connectionString, string commandText, params DbParameter[] parameters)
        {
            var dataAccess = SwiftDataAccess.Create(connectionString);
            dataAccess.SetCommand(commandText, CommandType.StoredProc, parameters);
            var result = dataAccess.ExecuteScalar();
            return ConvertTo<T>(result);
        }

        public static T ExecuteScalar<T>(string commandText, params DbParameter[] parameters)
        {
            CheckDefaultConnection();
            return ExecuteScalar<T>(DefaultConnectionName, commandText, parameters);
        }

        private static T ConvertTo<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value) return default(T);

            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static IEnumerable<T> GetPrimitivesList<T>(string connectionString, string commandText, params DbParameter[] parameters)
        {
            var dataAccess = SwiftDataAccess.Create(connectionString);

            var list = new List<T>();

            dataAccess.SetCommand(commandText, CommandType.StoredProc, parameters);

            using (var reader = dataAccess.ExecuteReader())
            {
                while (reader.Read())
                {
                    T item = (T)reader[0];

                    list.Add(item);
                }
            }

            return list;
        }

        public static IEnumerable<T> GetPrimitivesList<T>(string commandText, params DbParameter[] parameters)
        {
            CheckDefaultConnection();
            return GetPrimitivesList<T>(DefaultConnectionName, commandText, parameters);
        }
    }
}
