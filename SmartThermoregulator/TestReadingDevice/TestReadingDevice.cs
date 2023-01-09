using System;
using Common;
using ReadingDevice;
using NUnit.Framework;
using System.Net.Sockets;
using Moq;
using System.Net.Http;
using System.Net;
using System.Text;
using NSubstitute;
using System.Threading.Tasks;
using System.Threading;
using TemperatureRegulator;

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
        public void TestGetHeaterState_Ugasena()
        {
            device.Id = 1;
            device.Temperature = 30;
            device.HeaterIsOn = true;

            var mockHeater = new Mock<CentralHeater.CentralHeater>();
            mockHeater.Object.isOn = false;

            Task.Factory.StartNew(() => mockHeater.Object.waitNewDevice());
            Thread.Sleep(500); // ceka se listener na metodi waitNewDevice()

            device.getHeaterState();
            Thread.Sleep(500); // ceka se povezivanje na grejac

            Assert.AreEqual(mockHeater.Object.isOn, device.HeaterIsOn);

        }

        [Test]
        public void TestGetHeaterState_Upaljena()
        {
            device.Id = 19;
            device.HeaterIsOn = true;

            var mockHeater = new Mock<CentralHeater.CentralHeater>();
            mockHeater.Object.isOn = true;

            Task.Factory.StartNew(() => mockHeater.Object.waitNewDevice());
            Thread.Sleep(500); // ceka se listener na metodi waitNewDevice()

            device.getHeaterState();
            Thread.Sleep(500); // ceka se povezivanje na grejac

            Assert.AreEqual(mockHeater.Object.isOn, device.HeaterIsOn);
        }

        [Test]
        public void TestSendTemperature()
        {
            device.Id = 1;
            device.Temperature = 30;

            var mockRegulator = new Mock<TemperatureRegulator.TemperatureRegulator>();

            Task.Factory.StartNew(() => mockRegulator.Object.receiveTemperature());
            Thread.Sleep(500); // ceka se listener na metodi receiveTemperature()

            device.sendTemperature();
            Thread.Sleep(500); // ceka se slanje temperature

            Assert.AreEqual(mockRegulator.Object.temperatures[device.Id], device.Temperature);
        }

        [Test]
        public void TestSendTemperature2()
        {
            device.Id = 13;
            device.Temperature = 12;

            var mockRegulator = new Mock<TemperatureRegulator.TemperatureRegulator>();

            Task.Factory.StartNew(() => mockRegulator.Object.receiveTemperature());
            Thread.Sleep(500); // ceka se listener na metodi receiveTemperature()

            device.sendTemperature();
            Thread.Sleep(500); // ceka se slanje temperature

            Assert.AreEqual(mockRegulator.Object.temperatures[device.Id], device.Temperature);
        }


        [Test]
        public void TestReceiveStateHeater_Ugasena()
        {
            device.Id = 4;
            var client = new TcpClient();

            Task.Factory.StartNew(() => device.receiveStateHeater());
            Thread.Sleep(500); // ceka se listener na metodi receiveStateHeater()
            client.Connect("localhost", Constants.PortRegulatorDevice + device.Id);

            var stream = client.GetStream();
            var message = Encoding.UTF8.GetBytes("ugasena");

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se promena stanja grejaca

            Assert.False(device.HeaterIsOn);
        }

        [Test]
        public void TestReceiveStateHeater_Ugasena_Mock()
        {
            device.Id = 3;
            device.HeaterIsOn = true;
            var mockRegulator = new Mock<TemperatureRegulator.TemperatureRegulator>();

            Task.Factory.StartNew(() => device.receiveStateHeater());
            Thread.Sleep(500);  //Ceka pokretanje listenera

            mockRegulator.Object.sendMessageToDevice(Enums.Command.TurnOff, Constants.PortRegulatorDevice + device.Id);
            Thread.Sleep(500);  //Ceka se slanje

            Assert.False(device.HeaterIsOn);
        }

        [Test]
        public void TestReceiveStateHeater_Upaljena()
        {
            device.Id = 4;
            var client = new TcpClient();

            Task.Factory.StartNew(() => device.receiveStateHeater());
            Thread.Sleep(500); // ceka se listener na metodi receiveStateHeater()
            client.Connect("localhost", Constants.PortRegulatorDevice + device.Id);

            var stream = client.GetStream();
            var message = Encoding.UTF8.GetBytes("upaljena");

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se promena stanja grejaca

            Assert.True(device.HeaterIsOn);
        }

        [Test]
        public void TestReceiveStateHeater_Upaljena_Mock()
        {
            device.Id = 3;
            device.HeaterIsOn = false;
            var mockRegulator = new Mock<TemperatureRegulator.TemperatureRegulator>();

            Task.Factory.StartNew(() => device.receiveStateHeater());
            Thread.Sleep(500);  //Ceka pokretanje listenera

            mockRegulator.Object.sendMessageToDevice(Enums.Command.TurnOn, Constants.PortRegulatorDevice + device.Id);
            Thread.Sleep(500);  //Ceka se slanje

            Assert.True(device.HeaterIsOn);
        }



        //NAPOMENA Koristi vreme menjanja temperature date u zadatku (3 minuta)
        //Za kraci test promeniti u Constants vrednost ReadingDeviceTempChangeTime
        [Test]
        public void TestRegulateTemperature_HeaterOn()
        {
            device.Id = 1;
            device.Temperature = 19.00;
            device.HeaterIsOn = true;

            double expectedTemp = device.Temperature + Common.Constants.ReadingDeviceTempChange;

            Task.Factory.StartNew(() => device.regulateTemperature());
            Thread.Sleep((Common.Constants.ReadingDeviceTempChangeTime + 1) * 1000); //Cekanje da se temp regulise

            Assert.That(device.Temperature, Is.EqualTo(expectedTemp).Within(0.0001));
        }

        //NAPOMENA Koristi vreme menjanja temperature date u zadatku (3 minuta)
        //Za kraci test promeniti u Constants vrednost ReadingDeviceTempChangeTime
        [Test]
        public void TestRegulateTemperature_HeaterOff()
        {
            device.Id = 32;
            device.Temperature = 24;
            device.HeaterIsOn = false;

            double expectedTemp = device.Temperature - Common.Constants.ReadingDeviceTempChange;

            Task.Factory.StartNew(() => device.regulateTemperature());
            Thread.Sleep((Common.Constants.ReadingDeviceTempChangeTime + 1) * 1000); //Cekanje da se temp regulise

            Assert.That(device.Temperature, Is.EqualTo(expectedTemp).Within(0.0001));
        }

        //NAPOMENA Koristi vreme menjanja temperature date u zadatku (3 minuta)
        //Za kraci test promeniti u Constants vrednost ReadingDeviceTempChangeTime
        [Test]
        public void TestRegulateTemperature_HeaterOn_5TimesTempChange()
        {
            device.Id = 8;
            device.Temperature = 15;
            device.HeaterIsOn = true;

            double expectedTemp = device.Temperature + Common.Constants.ReadingDeviceTempChange * 5;

            Task.Factory.StartNew(() => device.regulateTemperature());
            Thread.Sleep((5 * Common.Constants.ReadingDeviceTempChangeTime + 1) * 1000); //Cekanje da se temp regulise

            Assert.That(device.Temperature, Is.EqualTo(expectedTemp).Within(0.0001));
        }

    }
}

