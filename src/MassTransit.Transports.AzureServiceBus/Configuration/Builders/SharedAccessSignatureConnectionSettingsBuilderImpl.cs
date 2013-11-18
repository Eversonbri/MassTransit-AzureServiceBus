// Copyright 2012 Henrik Feldt, Chris Patterson, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports.AzureServiceBus.Configuration.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;


    public class SharedAccessSignatureConnectionSettingsBuilderImpl :
        SharedAccessSignatureConnectionSettingsBuilder
    {
        readonly string _key;
        readonly string _keyName;
        readonly string _namespace;

        public SharedAccessSignatureConnectionSettingsBuilderImpl(string ns, string keyName, string key)
        {
            _namespace = ns;
            _keyName = keyName;
            _key = key;
        }

        public string Namespace
        {
            get { return _namespace; }
        }

        /// <summary>
        ///     Build the connection settings, which are used to create a connection to the Azure ServiceBus
        /// </summary>
        /// <returns></returns>
        public IConnectionSettings Build()
        {
//            GetClientProperties(connectionSettings.ClientProperties);

            return new SharedAccessSignatureConnectionSettings(_keyName, _key);
        }

        static void GetClientProperties(IDictionary<string, string> properties)
        {
            if (properties.ContainsKey("client_api"))
                return;

            properties.Add("client_api", "MassTransit");
            properties.Add("masstransit_version", typeof(IServiceBus).Assembly.GetName().Version.ToString());
            properties.Add("net_version", Environment.Version.ToString());
            properties.Add("hostname", Environment.MachineName);
            properties.Add("connected", DateTimeOffset.Now.ToString("R"));
            properties.Add("process_id", Process.GetCurrentProcess().Id.ToString());
            properties.Add("process_name", Process.GetCurrentProcess().ProcessName);

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                AssemblyName assemblyName = entryAssembly.GetName();
                properties.Add("entry_assembly", assemblyName.Name);
            }
        }
    }
}