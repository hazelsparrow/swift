using System;

namespace Swift
{
    public class DbParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public DbParameter(string name, object value)
        {
            if (!name.StartsWith("@"))
                name = "@" + name;

            this.Name = name;
            if (value != null && value != DBNull.Value)
                this.Value = value;
            else
                this.Value = DBNull.Value;
        }

        public static DbParameter Null(string name)
        {
            return new DbParameter(name, DBNull.Value); 
        }

        public static DbParameter Integer(string name, bool? value)
        {
            if (value == null) return new DbParameter(name, DBNull.Value);

            return new DbParameter(name, Convert.ToInt32(value.Value));
        }

        /// <summary>
        /// Returns a new db parameter with a string representation of the given date in the unambiguous unseparated format: 
        /// yyyyMMdd
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DbParameter DateTime(string name, DateTime? value)
        {
            if (value == null)
                return DbParameter.Null(name);

            return new DbParameter(name, value.Value.ToString("yyyyMMdd"));
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Value);
        }
    }
}
