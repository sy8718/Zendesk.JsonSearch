using System;
using System.Collections.Generic;
using System.Text;

namespace Zendesk.JsonSearch.Models.Helpers
{
    /// <summary>
    /// This class is helper class to convert different types into string type
    /// </summary>
    public static class FormatHelper
    {       
        public static string ConvertToString(this List<string> array)
        {
            return ConvertToString(array.ToArray());
        }

        public static string ConvertToString(this string[] array)
        {
            if (array.Length == 0) return null;
            return $"[{string.Join(",", array)}]";
        }

        public static string ConvertToString(this object obj)
        {
            if (obj is List<string>) return (obj as List<string>).ConvertToString();
            if (obj is string[]) return (obj as string[]).ConvertToString();
            return obj.ToString();
        }
    }
}
