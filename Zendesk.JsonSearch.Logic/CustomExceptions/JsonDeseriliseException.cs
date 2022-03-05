//-----------------------------------------------------------------
// This is the custom exception to hold json deserilisation exceptions
//-----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Zendesk.JsonSearch.Logic.CustomExceptions
{
    public class JsonDeseriliseException : Exception
    {
        public JsonDeseriliseException(string message)
           : base(message)
        {
        }
    }
}
