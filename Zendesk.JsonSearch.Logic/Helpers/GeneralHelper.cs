using System;
using System.Collections.Generic;
using System.Text;

namespace Zendesk.JsonSearch.Logic.Helpers
{
    public static class GeneralHelper
    {
        public static object TryChangeType(object value, Type conversionType)
        {
            try
            {
                return Convert.ChangeType(value, conversionType);
            }
            catch
            {
                return value;
            }           
        }
    }
}
