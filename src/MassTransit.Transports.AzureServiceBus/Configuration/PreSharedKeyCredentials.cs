﻿// Copyright 2012 Henrik Feldt
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
    using Util;


    /// <summary>
    /// Implementors of this interface should know how to create a uri based on the credentials supplied.
    /// </summary>
    public interface PreSharedKeyCredentials
    {
        /// <summary>
        /// Gets the issuer name as specified by Azure.
        /// </summary>
        string KeyName { get; }

        /// <summary>
        /// Gets the base64-encoded key for the service as specified by Azure.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the namespace for the service as specified by Azure.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// What application is under authorization?
        /// </summary>
        string Application { get; }

        /// <summary>
        /// Builds a URI with the passed application (or by default the Application property).
        /// </summary>
        /// <param name="application">If you wish to have another application</param>
        /// <returns>The corresponding uri</returns>
        [NotNull]
        Uri BuildUri(string application = null);

        /// <summary>
        /// Create a new credential item based on the passed application
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        PreSharedKeyCredentials WithApplication([NotNull] string application);
    }
}