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
        private ReadingDevice.ReadingDevice rd;

        [Test]
        [TestCase()]
        public void ReadingDeviceConstructor()
        {
            rd = new ReadingDevice.ReadingDevice();

            Assert.AreEqual(rd.Id, Globals.GeneratorIdReadingDevice - 1);
            Assert.AreEqual((rd.Temperature >= 15 && rd.Temperature <= 25), true);
        }

        //Ovo verovatno ne radi dok se ne implementira server
        [Test]
        public void TestSendTemperature()
        {
            var mockTcpClient = new Mock<TcpClient>();
            //rd.sendTemperature(mockTcpClient.Object);
            mockTcpClient.Verify(x => x.Connect("localhost", Common.Constants.PortDeviceRegulator));
            mockTcpClient.Verify(x => x.GetStream());
        }

        [Test]
        public void TestRaiseTemperature()
        {
            rd = new ReadingDevice.ReadingDevice();
            double initialTemp = rd.Temperature;
            rd.HeaterIsOn = true;
            rd.regulateTemperature();
            Assert.AreEqual(initialTemp + Common.Constants.ReadingDeviceTempChange, rd.Temperature);
        }

        [Test]
        public void TestToString()
        {
            rd = new ReadingDevice.ReadingDevice();
            string expected = "Id: " + rd.Id + " Temperature: " + rd.Temperature.ToString("0.00");
            Assert.AreEqual(expected, rd.ToString());
        }

    }
}

