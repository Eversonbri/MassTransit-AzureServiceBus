using System;
using MassTransit.Transports.AzureServiceBus.Tests.Framework;
using Microsoft.ServiceBus;
using NUnit.Framework;

namespace MassTransit.Transports.AzureServiceBus.Tests
{
	[TestFixture]
	public class Interop_topic_spec
	{
		NamespaceManager nm;
		AzureServiceBusMessageNameFormatter _formatter;

		[SetUp]
		public void theres_a_namespace_manager_available()
		{
			var mf = TestConfigFactory.CreateMessagingFactory();
			nm = TestConfigFactory.CreateNamespaceManager(mf);

            _formatter = new AzureServiceBusMessageNameFormatter();
        }

	
		[Test]
		[TestCase(typeof(NameEasyToo))]
		[TestCase(typeof(Nested))]
		[TestCase(typeof(NameEasy))]
		[TestCase(typeof(NameGeneric<string>))]
		[TestCase(typeof(NameDoubleGeneric<string, NameEasy>))]
		[TestCase(typeof(NameDoubleGeneric<NameGeneric<double>, NameEasy>))]
		public void app_fabric_service_bus_accepts_these_names(Type messageType)
		{
			var mname = _formatter.GetMessageName(messageType);
			try
			{
				nm.CreateTopic(mname.Name);
			}
			finally
			{
				try
				{
					if (nm.TopicExists(mname.Name))
						nm.DeleteTopic(mname.Name);
				}
				catch
				{
				}
			}
		}

		class Nested
		{
		}
	}
}