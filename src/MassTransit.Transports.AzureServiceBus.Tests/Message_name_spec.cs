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
    using System;
    using NUnit.Framework;


    [TestFixture]
    public class The_azure_message_name_formatter
    {
        [SetUp]
        public void Setup()
        {
            _formatter = new AzureServiceBusMessageNameFormatter();
        }

        [Test]
        public void Should_handle_a_closed_double_generic()
        {
            Assert.AreEqual(_formatter.GetMessageName(typeof(NameDoubleGeneric<string, NameEasy>)).Name,
                "MassTransit.Transports.AzureServiceBus.Tests..NameDoubleGeneric--System..String....NameEasy--");
        }

        [Test]
        public void Should_handle_a_closed_double_generic_with_a_generic()
        {
            Assert.AreEqual(
                _formatter.GetMessageName(typeof(NameDoubleGeneric<NameGeneric<NameEasyToo>, NameEasy>)).Name,
                "MassTransit.Transports.AzureServiceBus.Tests..NameDoubleGeneric--NameGeneric--NameEasyToo--....NameEasy--");
        }

        [Test]
        public void Should_handle_a_closed_single_generic()
        {
            Assert.AreEqual(_formatter.GetMessageName(typeof(NameGeneric<string>)).Name,
                "MassTransit.Transports.AzureServiceBus.Tests..NameGeneric--System..String--");
        }

        [Test]
        public void Should_handle_an_interface_name()
        {
            Assert.AreEqual(_formatter.GetMessageName(typeof(NameEasyToo)).Name,
                "MassTransit.Transports.AzureServiceBus.Tests..NameEasyToo");
        }

        [Test]
        public void Should_handle_nested_classes()
        {
            Assert.AreEqual(_formatter.GetMessageName(typeof(Nested)).Name,
                "MassTransit.Transports.AzureServiceBus.Tests..The_azure_message_name_formatter-Nested");
        }

        [Test]
        public void Should_handle_regular_classes()
        {
            Assert.AreEqual(_formatter.GetMessageName(typeof(NameEasy)).Name,
                "MassTransit.Transports.AzureServiceBus.Tests..NameEasy");
        }

        [Test]
        public void Should_throw_an_exception_on_an_open_generic_class_name()
        {
            Assert.Throws<ArgumentException>(() => _formatter.GetMessageName(typeof(NameGeneric<>)));
        }

        AzureServiceBusMessageNameFormatter _formatter;


        class Nested
        {
        }
    }


    class NameEasy
    {
    }


    interface NameEasyToo
    {
    }


    class NameGeneric<T>
    {
    }


    class NameDoubleGeneric<T1, T2>
    {
    }
}