// Copyright 2012 Henrik Feldt
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
    using System;
    using Microsoft.ServiceBus.Messaging;


    class MessageSenderImpl : MessageSender
    {
        readonly QueueClient _queueClient;
        readonly TopicClient _sender;
        bool _disposed;

        public MessageSenderImpl(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public MessageSenderImpl(TopicClient sender)
        {
            _sender = sender;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IAsyncResult BeginSend(BrokeredMessage message, AsyncCallback callback, object state)
        {
            return _queueClient != null
                       ? _queueClient.BeginSend(message, callback, state)
                       : _sender.BeginSend(message, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            if (_queueClient != null)
                _queueClient.EndSend(result);
            else
                _sender.EndSend(result);
        }

        protected virtual void Dispose(bool managed)
        {
            if (!managed || _disposed)
                return;

            try
            {
                if (_queueClient != null && !_queueClient.IsClosed)
                    _queueClient.Close();

                if (_sender != null && !_sender.IsClosed)
                    _sender.Close();
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}