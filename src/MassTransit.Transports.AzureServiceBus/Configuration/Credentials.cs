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
namespace MassTransit.Transports.AzureServiceBus.Configuration
{
    using System;


    /// <summary>
    ///     DTO with account details that is capable of building a service bus (MT-style)
    ///     from the credentials.
    /// </summary>
    public class Credentials
        : SharedAccessSignatureSettings
    {
        public Credentials(string keyName, string key, string ns, string application)
        {
            KeyName = keyName;
            Key = key;
            Namespace = ns;
            Application = application;
        }

        public string KeyName { get; private set; }
        public string Key { get; private set; }
        public string Namespace { get; private set; }
        public string Application { get; private set; }

        public Uri BuildUri(string application = null)
        {
            string uriString = string.Format("azure-sb://{0}:{1}@{2}/{3}", KeyName, Uri.EscapeDataString(Key), Namespace,
                application ?? Application);

            return new Uri(uriString);
        }

        public SharedAccessSignatureSettings WithApplication(string application)
        {
            return new Credentials(KeyName, Key, Namespace, application);
        }
    }
}