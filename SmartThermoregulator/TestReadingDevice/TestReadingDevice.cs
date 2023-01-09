using System;
using Common;
using ReadingDevice;
using NUnit.Framework;
using System.Net.Sockets;
using Moq;

namespace TestReadingDevice
{ 
    [TestFixture]
    public class TestReadingDevice
    {
        ReadingDevice.ReadingDevice device;

        [SetUp]
        public void SetUp()
        {
            device = new ReadingDevice.ReadingDevice();
        }

        [Test]
        public void TestReceiveStateHeater()
        {
            //Recimo da smo dobili da je grejac upaljen
            device.HeaterIsOn = true;

            Assert.IsTrue(device.HeaterIsOn);
        }

        [Test]
        public void TestGetHeaterState()
        {
            //Recimo da smo dobili da je grejac upaljen
            device.HeaterIsOn = true;

            Assert.IsTrue(device.HeaterIsOn);
        }

    }
}

