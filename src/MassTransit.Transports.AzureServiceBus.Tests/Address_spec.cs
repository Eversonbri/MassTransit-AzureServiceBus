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
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit.Configurators;
using MassTransit.Transports.AzureServiceBus.Tests.Framework;
using NUnit.Framework;

namespace MassTransit.Transports.AzureServiceBus.Tests
{
	[TestFixture]
	public class When_parsing_application_endpoint_uri
	{
		IAzureServiceBusEndpointAddress _address;

		[SetUp]
		public void a_servicebusqueues_address_is_given()
		{
			_address = AzureServiceBusEndpointAddress.Parse(
				TestDataFactory.ApplicationEndpoint);
		}

		[Test]
		public void messaging_factory_should_not_be_null()
		{
			//_address.MessagingFactoryFactory.ShouldNotBeNull();
		}

        [Test]
		public void token_provider_should_not_be_null()
		{
			Assert.IsNotNull(_address.ConnectionSettings);
		}

        [Test]
		public void namespace_manager_should_not_be_null()
		{
		//	_address.NamespaceManager.ShouldNotBeNull();
		}

        [Test]
		public void details_should_have_correct_app_name()
		{
			Assert.AreEqual("my-application", _address.QueueName);
		}

        [Test]
		public void details_should_have_correct_namespace()
		{
            Assert.AreEqual(AccountDetails.Namespace, _address.Namespace);
		}

//		[Then]
//		public void details_should_have_correct_shared_secret()
//		{
//			_address.PasswordSharedSecret
//				.ShouldEqual(AccountDetails.Key);
//		}
//
//		[Then]
//		public void details_should_have_correct_issuer()
//		{
//			_address.Details.UsernameIssuer
//				.ShouldEqual(AccountDetails.IssuerName);
//		}

        [Test]
		public void rebuilt_uri_should_be_correct()
		{
			var uriWithoutCreds = new Uri(string.Format("azure-sb://{0}/{1}", 
				AccountDetails.Namespace, "my-application"));

			Assert.AreEqual(_address.Uri,uriWithoutCreds);
		}
	}

	[TestFixture]
	public class When_giving_full_hostname_spec
	{
		IAzureServiceBusEndpointAddress _addressExtended;
		Uri _extended;
		Uri _normal;
		IAzureServiceBusEndpointAddress _address;

		[SetUp]
		public void a_servicebusqueues_address_is_given()
		{
			var extraHost = ".servicebus.windows.net";
			_extended = GetUri(extraHost);
			_normal = GetUri("");

			_addressExtended = AzureServiceBusEndpointAddress.Parse(_extended);
			_address = AzureServiceBusEndpointAddress.Parse(_normal);
		}

		Uri GetUri(string extraHost)
		{
			return new Uri(string.Format("azure-sb://owner:{0}@{1}{2}/my-application",
                Uri.EscapeDataString(AccountDetails.Key), AccountDetails.Namespace, extraHost));
        }
         
        [Test]
		public void address_is_subset_of_extended_address()
		{
            Assert.IsTrue(_addressExtended.Uri.Host.StartsWith(_address.Uri.Host));
		}
	}

	[TestFixture]
	public class When_giving_faulty_address_spec
	{
		Uri _faulty_app;
		Uri _missing_creds;

		[SetUp]
		public void two_bad_uris()
		{
			_faulty_app = new Uri(string.Format("azure-sb://owner:{0}@{1}/my-application/but_then_another_too",
                Uri.EscapeDataString(AccountDetails.Key), AccountDetails.Namespace));
            _missing_creds = new Uri(string.Format("azure-sb://owner-pass@lalala.servicebus.windows.net/app"));
		}

//		[Test]
//		public void something_after_app_name()
//		{
//			IEnumerable<ValidationResult> results;
//			IAzureServiceBusEndpointAddress address;
//			AzureServiceBusEndpointAddress.TryParse(_faulty_app, out address, out results)
//				.ShouldBeFalse("parse should have failed");
//
//			AssertGotKey("Application", address, results);
//		}
//
//		[Test]
//		public void missing_credentials()
//		{
//			IEnumerable<ValidationResult> results;
//			IAzureServiceBusEndpointAddress address;
//			AzureServiceBusEndpointAddress.TryParse(_missing_creds, out address, out results)
//				.ShouldBeFalse("parse should have failed");
//
//			AssertGotKey("UserInfo", address, results);
//		}

		static void AssertGotKey(string key, AzureServiceBusEndpointAddress address, IEnumerable<ValidationResult> results)
		{
//			results.ShouldNotBeNull();
//			address.ShouldBeNull();
//
//			results.Count().ShouldEqual(1);
//			results.First().Key.ShouldBeEqualTo(key);
		}
	}

	[TestFixture]
	public class When_creating_topic_address
	{
		// subjects
		IAzureServiceBusEndpointAddress _queueAddress;
		IAzureServiceBusEndpointAddress _topicAddress;
		
		// assertion data
		string _topicName;

		class A
		{
		}

		[SetUp]
		public void a_normal_address_and_its_topic_corresponding_address()
		{
			_queueAddress = AzureServiceBusEndpointAddress.Parse(
				TestDataFactory.ApplicationEndpoint);

			var formatter = new AzureServiceBusMessageNameFormatter();
			_topicName = formatter.GetMessageName(typeof (A)).ToString();

			_topicAddress = _queueAddress.ForTopic(_topicName);

			Assert.Throws<ArgumentNullException>(
				() => _queueAddress.ForTopic(null));
		}

        [Test]
		public void topic_address_has_same_password()
		{
			Assert.AreEqual(_queueAddress.Uri.UserInfo,_topicAddress.Uri.UserInfo);
		}

        [Test]
		public void topic_address_has_same_host()
		{
			Assert.AreEqual(_queueAddress.Uri.Host,_topicAddress.Uri.Host);
		}

//		[Then]
//		public void queue_address_has_queue_description()
//		{
//			_queueAddress.QueueDescription.ShouldNotBeNull();
//		}
//
//		[Then]
//		public void queue_address_hasnt_got_topic_description()
//		{
//			_queueAddress.TopicDescription.ShouldBeNull();
//		}
//
//		[Then]
//		public void topic_address_hasnt_got_queue_description()
//		{
//			_topicAddress.QueueDescription.ShouldBeNull();
//		}
//
//		[Then]
//		public void topic_address_got_topic_description()
//		{
//			_topicAddress.TopicDescription.ShouldNotBeNull();
//		}
//
//		[Then]
//		public void topic_address_contains_topic_name()
//		{
//			_topicAddress.TopicDescription.Path
//				.ShouldContain(_topicName);
//		}

        [Test]
		public void topic_address_uri_tells_its_topic()
		{
//			_topicAddress.Uri.PathAndQuery
//				.ShouldContain("topic=true");
		}
	}
}