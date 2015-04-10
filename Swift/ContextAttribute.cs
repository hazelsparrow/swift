using System;

namespace Swift
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ContextAttribute : Attribute
    {
        public string ParameterName { get; set; }
        public object DefaultValue { get; set; }

        public ContextAttribute(string parameterName, object defaultValue = null)
        {
            ParameterName = parameterName;
            DefaultValue = defaultValue;
        }
    }
}