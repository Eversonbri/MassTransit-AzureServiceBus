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
namespace MassTransit.Transports.AzureServiceBus.Tests
{
    using Framework;
    using Magnum.TestFramework;
    using NUnit.Framework;
    using Testing;


    [Description("This test verifies that one can use the testing framework with Azure Service Bus infrastructure"), Explicit]
    [TestFixture]
    public class Handler_test_factory_contract
    {
        HandlerTest<A> _test;

        [TestFixtureSetUp]
        public void Setup()
        {
            _test = TestFactory.ForHandler<A>()
                .New(x =>
                    {
                        // this does not work because subscription notifications are not present with Azure
                        x.UseAzureServiceBusBusScenario(new AccountDetails());

                        x.Send(new A());
                        x.Send(new B());
                    });

            _test.Execute();
        }

        [TearDown]
        public void Teardown()
        {
            _test.Dispose();
            _test = null;
        }

        [Test]
        public void Should_have_received_a_message_of_type_a()
        {
            _test.Received.Any<A>().ShouldBeTrue();
        }

        [Test]
        public void Should_have_skipped_a_message_of_type_b()
        {
            _test.Skipped.Any<B>().ShouldBeTrue();
        }

        [Test]
        public void Should_not_have_skipped_a_message_of_type_a()
        {
            _test.Skipped.Any<A>().ShouldBeFalse();
        }

        [Test]
        public void Should_have_sent_a_message_of_type_a()
        {
            _test.Sent.Any<A>().ShouldBeTrue();
        }

        [Test]
        public void Should_have_sent_a_message_of_type_b()
        {
            _test.Sent.Any<B>().ShouldBeTrue();
        }

        [Test]
        public void Should_support_a_simple_handler()
        {
            _test.Handler.Received.Any().ShouldBeTrue();
        }


        class A
        {
        }


        class B
        {
        }
    }
}