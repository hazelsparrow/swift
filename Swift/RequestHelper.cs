using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Swift
{
    public static class RequestHelper
    {
        /// <summary>
        /// Using reflection, gets an object from HttpContext.Current.Request.
        /// Every public property that has ContextAttribute will be initialized
        /// using RequestHelper.GetFromContext([contextAttribute.Name], [contextAttribute.DefaultValue]) function.
        /// contextAttribute.DefaultValue can be omitted, in which case the default value will be default(TProperty)
        /// where TProperty is the type of the property.
        /// defaultObject values will be used and will override the contextAttribute.defaultValues if the value
        /// from context is null.
        /// Example:
        /// class Example 
        /// {
        ///     [Context("test", 5)]
        ///     public int Foo { get; set; }
        /// }
        /// Example e = RequestHelper.GetObjectFromContext();
        /// current url: /?test=10
        /// e.Foo will be 10
        /// current url: /
        /// e.Foo will be 5
        /// e = RequestHelper.GetObjectFromContext(new Example() { Foo = 22; });
        /// current url: /
        /// e.Foo will be 22
        /// current url: /?test=10
        /// e.Foo will be 10
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultObject"></param>
        /// <returns></returns>
        public static T GetObjectFromContext<T>(T defaultObject = null)
            where T : class, new()
        {
            return (T)GetObjectFromContext(typeof(T), defaultObject);
        }

        public static object GetObjectFromContext(Type type, object defaultObject = null)
        {
            var obj = Activator.CreateInstance(type);

            foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!property.CanWrite)
                    continue;

                var name = property.Name;

                var attr = (ContextAttribute)property.GetCustomAttributes(typeof(ContextAttribute), false).FirstOrDefault();
                if (attr != null)
                    name = attr.ParameterName;

                object defaultValue = null;
                if (defaultObject != null)
                    defaultValue = property.GetValue(defaultObject);
                else if (attr != null && attr.DefaultValue != null)
                    defaultValue = attr.DefaultValue;
                else
                {
                    if (property.PropertyType.IsValueType)
                        defaultValue = Activator.CreateInstance(property.PropertyType);
                    else
                        defaultValue = null;
                }

                var newValue = GetFromContext(name, property.PropertyType, defaultValue);

                if (property.PropertyType.IsNullableEnum() && newValue != null)
                {
                    // if nullable enum, manually convert it to enum
                    var enumType = Nullable.GetUnderlyingType(property.PropertyType);
                    newValue = Enum.ToObject(enumType, newValue);
                }
                else if (property.PropertyType.IsEnum)
                {
                    newValue = Enum.ToObject(property.PropertyType, newValue); // newValue cannot be null because enumType is not nullable
                }

                property.SetValue(obj, newValue);
            }

            return obj;
        }

        public static T GetFromContext<T>(string key, T defaultValue = default(T))
        {
            return (T)GetFromContext(key, typeof(T), defaultValue);
        }

        private static CultureInfo culture = new CultureInfo("en-CA");

        public static object GetFromContext(string key, Type type, object defaultValue = null)
        {
            var valueFromContext = HttpContext.Current.Request[key];
            if (string.IsNullOrEmpty(valueFromContext))
                return defaultValue;

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var splitted = valueFromContext.Split(',');

                var array = Array.CreateInstance(elementType, splitted.Length);

                for (int i = 0; i < splitted.Length; i++)
                {
                    array.SetValue(GetSingleFromContext(splitted[i], elementType), i);
                }

                return array;
            }
            else if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            {
                var elementType = type.GetGenericArguments()[0];
                var splitted = valueFromContext.Split(',');

                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = (IList)Activator.CreateInstance(listType);

                for (int i = 0; i < splitted.Length; i++)
                {
                    list.Add(GetSingleFromContext(splitted[i], elementType));
                }

                return list;
            }
            else
                return GetSingleFromContext(valueFromContext, type);
        }

        /// <summary>
        /// Returns a single object from context (Type is not supposed to be IEnumerable).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static object GetSingleFromContext(string valueFromContext, Type type)
        {
            if (type.IsNullableEnum() || type.IsEnum)
            {
                // for enums, convert the value to int
                return Convert.ToInt32(valueFromContext);
            }
            else if (type.IsNullableBoolean())
            {
                // for nullable boolean, convert directly to bool
                return valueFromContext == "1";
            }
            else if (type.IsGuidOrNullableGuid())
            {
                Guid guid;
                if (Guid.TryParse(valueFromContext, out guid))
                    return guid;
                else
                {
                    return Guid.Empty;
                }
            }
            else if (Nullable.GetUnderlyingType(type) != null)
            {
                // for other nullable types, convert to underlying type
                return Convert.ChangeType(valueFromContext, Nullable.GetUnderlyingType(type),
                    culture);
            }
            else
            {
                return Convert.ChangeType(valueFromContext, type, culture);
            }
        }

        private static bool IsGuidOrNullableGuid(this Type t)
        {
            return t == typeof(Guid) || Nullable.GetUnderlyingType(t) == typeof(Guid);
        }

        public static bool IsNullableEnum(this Type t)
        {
            Type u = Nullable.GetUnderlyingType(t);
            return (u != null) && u.IsEnum;
        }

        public static bool IsNullableBoolean(this Type t)
        {
            Type u = Nullable.GetUnderlyingType(t);
            return (u != null) && u.Name == "Boolean";
        }
    }
}