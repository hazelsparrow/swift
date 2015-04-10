using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace Swift
{
    public enum CommandType
    {
        StoredProc,
        DynamicSql
    }

    public class SwiftDataAccess
    {
        private SwiftDataAccess(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public static SwiftDataAccess Create(string connectionString)
        {
            return new SwiftDataAccess(connectionString);
        }

        private SqlConnection connection = null;
        private SqlCommand command = null;

        public void Close()
        {
            connection.Close();
        }

        public void SetCommand(string commandText, CommandType commandType, params DbParameter[] parameters)
        {
            command = connection.CreateCommand();
            command.CommandTimeout = 180;

            if (commandType == CommandType.StoredProc)
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = commandText;
            }
            else if (commandType == CommandType.DynamicSql)
            {
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = commandText;
            }
            else
                throw new ArgumentException();

            foreach (var item in parameters)
            {
                command.Parameters.Add(new SqlParameter(item.Name, item.Value));
            }
        }

        public SwiftDataReader ExecuteReader()
        {
            SwiftDataAccess self = null;

            connection.Open();
            self = this;

            return new SwiftDataReader(command.ExecuteReader(), self);
        }

        public bool Open { get { return connection.State != System.Data.ConnectionState.Closed; } }

        public int ExecuteNonQuery()
        {
            connection.Open();
            int result = command.ExecuteNonQuery();
            connection.Close();
            return result;
        }

        public object ExecuteScalar()
        {
            connection.Open();
            object result = command.ExecuteScalar();
            connection.Close();
            return result;
        }
    }

    public interface ISwiftDataReader
    {
        object this[int index]
        {
            get;
        }

        bool Read();
        string GetString(string name);
        bool GetBoolean(string name);
        bool? GetNullableBoolean(string name);
        int GetInt32(string name);
        int? GetNullableInt32(string name);
        T? GetNullableEnum<T>(string name) where T : struct;
        T GetEnum<T>(string name) where T : struct;
        decimal GetDecimal(string name);
        decimal? GetNullableDecimal(string name);
        DateTime GetDateTime(string name);
        DateTime? GetNullableDateTime(string name);
        Guid GetGuid(string name);
        byte GetByte(string name);
        byte? GetNullableByte(string name);
        bool NextResult(); // advances to the next table in the result set
    }

    public class SwiftDataReader : ISwiftDataReader, IDisposable
    {
        private SqlDataReader reader = null;
        private SwiftDataAccess disposableParent = null;

        public bool NextResult()
        {
            return reader.NextResult();
        }

        private static bool HasColumn(SqlDataReader dr, string columnName)
        {
            return true;
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public SwiftDataReader(SqlDataReader reader, SwiftDataAccess disposableParent)
        {
            this.reader = reader;
            this.disposableParent = disposableParent;
        }

        public string GetString(string name)
        {
            if (IsNull(name)) return null;

            return reader.GetString(reader.GetOrdinal(name));
        }

        public string GetStringNullable(string name)
        {
            if (HasColumn(reader, name) && !IsNull(name))
                return GetString(name);
            return null;
        }

        public int GetInt32(string name)
        {
            return reader.GetInt32(reader.GetOrdinal(name));
        }

        public decimal GetDecimal(string name)
        {
            return reader.GetDecimal(reader.GetOrdinal(name));
        }

        public decimal? GetNullableDecimal(string name)
        {
            if (HasColumn(reader, name) && !IsNull(name))
                return GetDecimal(name);
            return null;
        }

        public int? GetNullableInt32(string name)
        {
            if (HasColumn(reader, name) && !IsNull(name))
                return GetInt32(name);
            return null;
        }

        public T? GetNullableEnum<T>(string name)
            where T : struct
        {
            object value = GetNullableInt32(name);
            if (value == null)
                return null;
            return (T)value;
        }

        public T GetEnum<T>(string name)
            where T : struct
        {
            object value = GetInt32(name);
            return (T)value;
        }

        public bool? GetNullableBoolean(string name)
        {
            if (HasColumn(reader, name) && !IsNull(name))
                return reader.GetBoolean(reader.GetOrdinal(name));
            return null;
        }

        public bool GetBoolean(string name)
        {
            return reader.GetBoolean(reader.GetOrdinal(name));
        }

        public DateTime GetDateTime(string name)
        {
            return reader.GetDateTime(reader.GetOrdinal(name));
        }

        public byte GetByte(string name)
        {
            return reader.GetByte(reader.GetOrdinal(name));
        }

        public byte? GetNullableByte(string name)
        {
            if (HasColumn(reader, name) && !IsNull(name))
                return GetByte(name);
            return null;
        }

        public DateTime? GetNullableDateTime(string name)
        {
            if (HasColumn(reader, name) && !IsNull(name))
                return GetDateTime(name);
            return null;
        }

        public Guid GetGuid(string name)
        {
            if (HasColumn(reader, name) && !IsNull(name))
                return reader.GetGuid(reader.GetOrdinal(name));
            return Guid.Empty;
        }

        private bool IsNull(string name)
        {
            return reader.IsDBNull(reader.GetOrdinal(name));
        }

        public bool Read()
        {
            return reader.Read();
        }

        public void Dispose()
        {
            reader.Dispose();
            if (disposableParent != null)
                disposableParent.Close();
        }

        public object this[int index]
        {
            get
            {
                return reader[index];
            }
        }
    }

}