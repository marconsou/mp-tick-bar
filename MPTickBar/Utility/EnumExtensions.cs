using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MPTickBar
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var genericEnumType = value.GetType();
            var memberInfo = genericEnumType.GetMember(value.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (_Attribs != null && _Attribs.Length > 0)
                    return ((DescriptionAttribute)_Attribs.ElementAt(0)).Description;
            }
            return value.ToString();
        }

        public static string[] GetNames<T>(this T value) where T : Enum
        {
            var method = typeof(EnumExtensions).GetMethod("GetDescription", BindingFlags.Public | BindingFlags.Static, null, new[] { value.GetType() }, null);
            var names = new List<string>();
            var values = Enum.GetValues(typeof(T));
            foreach (var item in values)
            {
                var description = (string)method.Invoke(null, new[] { item });
                names.Add(description);
            }
            return names.ToArray();
        }
    }
}