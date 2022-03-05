using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Zendesk.JsonSearch.Logic.Consts;

namespace Zendesk.JsonSearch.Logic.Helpers
{
    internal static class ConfigHelper
    {
        internal static IConfiguration InitialiseConfig(string configFile)
        {
            return new ConfigurationBuilder()
               .AddJsonFile(configFile, optional: true, reloadOnChange: true)
               .Build();
        }     
    }
}
