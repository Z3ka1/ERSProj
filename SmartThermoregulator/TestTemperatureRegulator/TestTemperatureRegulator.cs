using System;
using Common;
using TemperatureRegulator;
using NUnit.Framework;

namespace TestTemperatureRegulator
{
    public class TestTemperatureRegulator
    {
        TemperatureRegulator.TemperatureRegulator regulator;

        [SetUp]
        public void SetUp()
        {
            regulator = new TemperatureRegulator.TemperatureRegulator();
        }

        //[Test]
        //public void TestSetDayTemperature()
        //{
        //    regulator.SetDayTemperature(70);
        //    Assert.AreEqual(70, regulator.getDayTemperature);
        //}

        //[Test]
        //public void TestSetNightTemperature()
        //{
        //    regulator.SetNightTemperature(60);
        //    Assert.AreEqual(60, regulator.nightTemperature);
        //}

        [Test]
        public void TestGetTemperature()
        {
            regulator.SetDayTemperature(70);
            regulator.SetNightTemperature(60);

            Assert.AreEqual(70, regulator.GetTemperature(12));
            Assert.AreEqual(60, regulator.GetTemperature(23));
        }

        //[Test]
        //public void TestReceiveTemperature()
        //{
        //    // simulate receiving temperature from a device
        //    int deviceId = 123;
        //    double temperature = 72.5;

        //    // add the temperature reading to the regulator's dictionary
        //    regulator.temperatures.Add(deviceId, temperature);

        //    // check that the temperature was added to the dictionary
        //    Assert.AreEqual(temperature, regulator.temperatures[deviceId]);
        //}

        //[Test]
        //public void TestSendCommandToDevice()
        //{
        //    // simulate sending a command to a device
        //    int deviceId = 123;
        //    Enums.Command command = Enums.Command.TurnOn;

        //    // add the command to the regulator's nextCommand field
        //    regulator.nextCommand = command;

        //    // check that the command was added to the nextCommand field
        //    Assert.AreEqual(command, regulator.nextCommand);
        //}

    }
}

