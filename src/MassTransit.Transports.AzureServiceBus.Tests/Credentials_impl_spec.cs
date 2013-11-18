using MassTransit.Transports.AzureServiceBus.Configuration;

namespace MassTransit.Transports.AzureServiceBus.Tests
{
    using NUnit.Framework;


    public class Loading_the_credentials
	{
		Credentials _subject;

		[SetUp]
		public void Setup()
		{
			_subject = new Credentials("owner", "key", "ns", "appx");
		}

		[Test]
		public void Should_have_correct_issuer_name()
		{
			Assert.AreEqual("owner", _subject.KeyName);
		}

        [Test]
		public void Should_have_correct_key()
		{
            Assert.AreEqual("key", _subject.Key);
		}

        [Test]
		public void Should_have_correct_ns()
		{
            Assert.AreEqual("ns", _subject.Namespace);
		}

        [Test]
		public void Should_have_correct_app()
		{
            Assert.AreEqual("appx", _subject.Application);
		}
	}
}