using System;
using Common;
using CentralHeater;
using NUnit.Framework;
using System.Threading;

namespace TestCentralHeater
{
    public class TestCentralHeater
    {
        private CentralHeater.CentralHeater _centralHeater;

        [SetUp]
        public void SetUp()
        {
            _centralHeater = new CentralHeater.CentralHeater();
        }

        [Test]
        public void TestTurnOn()
        {
            bool expected = true;

            _centralHeater.TurnOn();
            bool actual = _centralHeater.IsOn();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestTurnOff()
        {
            bool expected = false;

            _centralHeater.TurnOn();
            _centralHeater.TurnOff();
            bool actual = _centralHeater.IsOn();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void IsOn_ShouldReturnCorrectValueForIsOn()
        {
            _centralHeater.isOn = true;

            Assert.IsTrue(_centralHeater.IsOn());

            _centralHeater.isOn = false;
            Assert.IsFalse(_centralHeater.IsOn());
        }


        [Test]
        public void TestGetRunTime()
        {
            TimeSpan expected = TimeSpan.FromSeconds(30);

            _centralHeater.TurnOn();
            Thread.Sleep(30000); // Sleep 30 seconds
            _centralHeater.TurnOff();
            TimeSpan actual = _centralHeater.GetRunTime();

            Assert.That(actual, Is.EqualTo(expected).Within(TimeSpan.FromMilliseconds(100)));

        }
    }
}

