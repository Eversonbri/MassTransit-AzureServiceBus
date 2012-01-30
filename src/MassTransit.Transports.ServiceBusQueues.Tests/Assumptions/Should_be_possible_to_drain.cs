﻿// Copyright 2011 Henrik Feldt
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

using Magnum.Extensions;
using Magnum.TestFramework;
using MassTransit.Transports.ServiceBusQueues.Internal;

namespace MassTransit.Transports.ServiceBusQueues.Tests.Assumptions
{
	public class Should_be_possible_to_drain
	{
		Topic topic;

		[Given]
		public void nsm_mf_and_topic()
		{
			var tp = ConfigFactory.CreateTokenProvider();
			var mf = ConfigFactory.CreateMessagingFactory(tp);
			var nm = ConfigFactory.CreateNamespaceManager(mf, tp);
			topic = nm.TryCreateTopic(mf, "non-existing").Result;
			topic.ShouldNotBeNull();
		}

		[Then]
		public void I_can_drain_the_topic()
		{
			topic.DrainBestEffort(3.Seconds());
		}
	}
}