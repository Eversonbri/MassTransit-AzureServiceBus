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
namespace MassTransit.Transports.AzureServiceBus
{
    using Microsoft.ServiceBus;


    /// <summary>
    ///     Settings for Azure Service Bus using a Token Provider
    /// </summary>
    public class SharedAccessSignatureConnectionSettings :
        IConnectionSettings
    {
        string _key;
        string _keyName;

        public SharedAccessSignatureConnectionSettings(string keyName, string key)
        {
            _keyName = keyName;
            _key = key;
        }

        public string KeyName
        {
            get { return _keyName; }
            set { _keyName = value; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        ///     Returns the token provider for the shared access signature
        /// </summary>
        public TokenProvider TokenProvider
        {
            get { return TokenProvider.CreateSharedAccessSignatureTokenProvider(_keyName, _key); }
        }
    }
}