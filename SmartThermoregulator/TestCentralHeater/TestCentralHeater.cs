using System;
using Common;
using CentralHeater;
using NUnit.Framework;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace TestCentralHeater
{
    [TestFixture]
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
            TimeSpan expected = TimeSpan.FromSeconds(10);

            _centralHeater.TurnOn();
            Thread.Sleep(10000); // Sleep 10 seconds
            _centralHeater.TurnOff();
            TimeSpan actual = _centralHeater.GetRunTime();

            Assert.That(actual, Is.EqualTo(expected).Within(TimeSpan.FromMilliseconds(100)));

        }

        [Test]
        public void TestReceiveCommand_TurnOn()
        {
            var client = new TcpClient();

            Task.Factory.StartNew(() => _centralHeater.receiveCommand());
            Thread.Sleep(500); // ceka se listener na metodi receiveCommand()
            client.Connect("localhost", Constants.PortRegulatorHeater);

            var stream = client.GetStream();
            var message = Encoding.UTF8.GetBytes("TurnOn");

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se promena podataka

            Assert.AreEqual(Common.Enums.Command.TurnOn.ToString(), message);
        }

        [Test]
        public void TestReceiveCommand_TurnOn_Mock()
        {
            var mockRegulator = new Mock<TemperatureRegulator.TemperatureRegulator>();
            _centralHeater.isOn = false;
            bool expected = true;

            Task.Factory.StartNew(() => _centralHeater.receiveCommand());
            Thread.Sleep(500);  //ceka se listener

            mockRegulator.Object.sendCommand(Enums.Command.TurnOn);


            Assert.AreEqual(expected, _centralHeater.isOn);
        }

        [Test]
        public void TestReceiveCommand_TurnOff()
        {
            var client = new TcpClient();

            Task.Factory.StartNew(() => _centralHeater.receiveCommand());
            Thread.Sleep(500); // ceka se listener na metodi receiveCommand()
            client.Connect("localhost", Constants.PortRegulatorHeater);

            var stream = client.GetStream();
            var message = Encoding.UTF8.GetBytes("TurnOff");

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se promena podataka

            Assert.AreEqual(Common.Enums.Command.TurnOff.ToString(), message);
        }


        [Test]
        public void TestReceiveCommand_TurnOff_Mock()
        {
            var mockRegulator = new Mock<TemperatureRegulator.TemperatureRegulator>();
            _centralHeater.isOn = true;
            bool expected = false;

            Task.Factory.StartNew(() => _centralHeater.receiveCommand());
            Thread.Sleep(500);  //ceka se listener

            mockRegulator.Object.sendCommand(Enums.Command.TurnOff);


            Assert.AreEqual(expected, _centralHeater.isOn);
        }

        [Test]
        public void TestWaitNewDevice_HeaterOn()
        {
            _centralHeater.isOn = true;
            var client = new TcpClient();

            Task.Factory.StartNew(() => _centralHeater.waitNewDevice());
            Thread.Sleep(500);  // ceka se listener na metodi waitNewDevice()
            client.Connect("localhost", Constants.PortHeaterDevice);

            var stream = client.GetStream();

            var message = Encoding.UTF8.GetBytes(_centralHeater.isOn.ToString());

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se slanje

            Assert.AreEqual(_centralHeater.isOn.ToString(), message);
        }

        [Test]
        public void TestWaitNewDevice_HeaterOn_Mock()
        {
            var mockDevice = new Mock<ReadingDevice.ReadingDevice>();
            _centralHeater.isOn = true;
            bool expected = _centralHeater.isOn;

            Task.Factory.StartNew(() => _centralHeater.waitNewDevice());
            Thread.Sleep(500);  //ceka se listener
            mockDevice.Object.getHeaterState();
            Thread.Sleep(500);  //ceka se slanje

            Assert.AreEqual(expected, mockDevice.Object.HeaterIsOn);
        }


        [Test]
        public void TestWaitNewDevice_HeaterOff()
        {
            _centralHeater.isOn = false;
            var client = new TcpClient();

            Task.Factory.StartNew(() => _centralHeater.waitNewDevice());
            Thread.Sleep(500);  // ceka se listener na metodi waitNewDevice()
            client.Connect("localhost", Constants.PortHeaterDevice);

            var stream = client.GetStream();

            var message = Encoding.UTF8.GetBytes(_centralHeater.isOn.ToString());

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se slanje

            Assert.AreEqual(_centralHeater.isOn.ToString(), message);
        }

        [Test]
        public void TestWaitNewDevice_HeaterOff_Mock()
        {
            var mockDevice = new Mock<ReadingDevice.ReadingDevice>();
            _centralHeater.isOn = false;
            bool expected = _centralHeater.isOn;

            Task.Factory.StartNew(() => _centralHeater.waitNewDevice());
            Thread.Sleep(500);  //ceka se listener
            mockDevice.Object.getHeaterState();
            Thread.Sleep(500);  //ceka se slanje

            Assert.AreEqual(expected, mockDevice.Object.HeaterIsOn);
        }

        //test kada nisu jednaki, heater upaljen salje se da je ugasen
        [Test]
        public void TestWaitNewDevice_HeaterOn_SendingOff()
        {
            _centralHeater.isOn = true;
            var client = new TcpClient();

            Task.Factory.StartNew(() => _centralHeater.waitNewDevice());
            Thread.Sleep(500);  // ceka se listener na metodi waitNewDevice()
            client.Connect("localhost", Constants.PortHeaterDevice);

            var stream = client.GetStream();

            var message = Encoding.UTF8.GetBytes((!_centralHeater.isOn).ToString());

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se slanje

            Assert.AreNotEqual(_centralHeater.isOn.ToString(), message);
        }


    }
}

