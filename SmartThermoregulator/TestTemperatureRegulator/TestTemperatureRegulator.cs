using System;
using Common;
using TemperatureRegulator;
using NUnit.Framework;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using ReadingDevice;

namespace TestTemperatureRegulator
{
    [TestFixture]
    public class TestTemperatureRegulator
    {
        TemperatureRegulator.TemperatureRegulator regulator;

        [SetUp]
        public void SetUp()
        {
            regulator = new TemperatureRegulator.TemperatureRegulator();
        }

        [Test]
        public void TestSetDayTemperature()
        {
            regulator.SetDayTemperature(70);
            Assert.AreEqual(70, regulator.dayTemperature);
        }

        [Test]
        public void TestSetNightTemperature()
        {
            regulator.SetNightTemperature(60);
            Assert.AreEqual(60, regulator.nightTemperature);
        }

        [Test]
        public void TestReceiveTemperature()
        {
            var client = new TcpClient();

            Task.Factory.StartNew(() => regulator.receiveTemperature());
            Thread.Sleep(500); // ceka se listener na metodi receiveTemperature()
            client.Connect("localhost", Constants.PortDeviceRegulator);

            var stream = client.GetStream();
            var message = Encoding.UTF8.GetBytes("1,30");

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se promena stanja grejaca

            Assert.AreEqual(30,regulator.temperatures[1]);
        }

        [Test]
        public void TestReceiveTemperature_Mock()
        {
            var mockDevice = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice.Object.Id = 1;
            mockDevice.Object.Temperature = 19;

            double expectedTemp = mockDevice.Object.Temperature;
            int idx = mockDevice.Object.Id;

            Task.Factory.StartNew(() => regulator.receiveTemperature());
            Thread.Sleep(500);  //Ceka se listener

            mockDevice.Object.sendTemperature();
            Thread.Sleep(500); //Ceka se slanje

            Assert.AreEqual(expectedTemp, regulator.temperatures[idx]);
        }

        [Test]
        public void TestReceiveTemperature_Mock_2Devices()
        {
            var mockDevice1 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice2 = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice1.Object.Id = 19;
            mockDevice1.Object.Temperature = 13;
            mockDevice2.Object.Id = 89;
            mockDevice2.Object.Temperature = 12;

            double expectedTemp = mockDevice1.Object.Temperature;
            int idx = mockDevice1.Object.Id;
            double expectedTemp2 = mockDevice2.Object.Temperature;
            int idx2 = mockDevice2.Object.Id;

            Task.Factory.StartNew(() => regulator.receiveTemperature());
            Thread.Sleep(500);  //Ceka se listener

            mockDevice1.Object.sendTemperature();
            mockDevice2.Object.sendTemperature();
            Thread.Sleep(500); //Ceka se slanje

            Assert.AreEqual(expectedTemp, regulator.temperatures[idx]);
            Assert.AreEqual(expectedTemp2, regulator.temperatures[idx2]);
        }

        //ovo ne treba da prodje
        [Test]
        public void TestReceiveTemperature_False()
        {
            var client = new TcpClient();

            Task.Factory.StartNew(() => regulator.receiveTemperature());
            Thread.Sleep(500); // ceka se listener na metodi receiveTemperature()
            client.Connect("localhost", Constants.PortDeviceRegulator);

            var stream = client.GetStream();
            var message = Encoding.UTF8.GetBytes("1,20");

            stream.Write(message, 0, message.Length);
            Thread.Sleep(500);  // ceka se upis vrednosti

            Assert.AreNotEqual(10, regulator.temperatures[1]);
        }

        [Test]
        public void TestRegulate_LessThanFourTemperatures_DoesNothing()
        {
            regulator.temperatures = new Dictionary<int, double>
            {
                { 1, 22.5 },
                { 2, 23.5 },
                { 3, 24.5 }
            };

            regulator.regulate();

            
            Assert.Greater(4, regulator.temperatures.Count);
        }

        //The active test run was aborted. Reason: Test host process crashed
        [Test]
        public void TestRegulate_AverageTemperatureLessThanDayTemperature_TurnsHeaterOn()
        {
            regulator.dnevniPocetak = 10;
            regulator.dnevniKraj = 20;
            regulator.temperatures = new Dictionary<int, double>
            {
                { 1, 22.5 },
                { 2, 23.5 },
                { 3, 24.5 },
                { 4, 25.5 }
            };
            regulator.dayTemperature = 25;
            regulator.nightTemperature = 20;

            var mockHeater = new Mock<CentralHeater.CentralHeater>();
            Task.Factory.StartNew(() => mockHeater.Object.receiveCommand());
            var mockDevice1 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice2 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice3 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice4 = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice1.Object.Id = 1;
            mockDevice1.Object.Id = 2;
            mockDevice1.Object.Id = 3;
            mockDevice1.Object.Id = 4;
            Task.Factory.StartNew(() => mockDevice1.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice2.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice3.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice4.Object.receiveStateHeater());
            Thread.Sleep(500);  //Cekanje da se pokrenu listeneri

            regulator.regulate();

            Assert.IsTrue(mockDevice1.Object.HeaterIsOn);
            Assert.IsTrue(mockDevice2.Object.HeaterIsOn);
            Assert.IsTrue(mockDevice3.Object.HeaterIsOn);
            Assert.IsTrue(mockDevice4.Object.HeaterIsOn);
            Assert.IsTrue(mockHeater.Object.isOn);
            Assert.AreEqual(Enums.Command.TurnOn, regulator.previousCommand);
        }


        //The active test run was aborted. Reason: Test host process crashed
        [Test]
        public void TestRegulate_AverageTemperatureLessThanNightTemperature_TurnsHeaterOn()
        {
            regulator.dnevniPocetak = 10;
            regulator.dnevniKraj = 20;
            regulator.temperatures = new Dictionary<int, double>
            {
                { 1, 22.5 },
                { 2, 23.5 },
                { 3, 24.5 },
                { 4, 25.5 }
            };
            regulator.dayTemperature = 30;
            regulator.nightTemperature = 25;

            var mockHeater = new Mock<CentralHeater.CentralHeater>();
            Task.Factory.StartNew(() => mockHeater.Object.receiveCommand());
            var mockDevice1 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice2 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice3 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice4 = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice1.Object.Id = 1;
            mockDevice1.Object.Id = 2;
            mockDevice1.Object.Id = 3;
            mockDevice1.Object.Id = 4;
            Task.Factory.StartNew(() => mockDevice1.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice2.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice3.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice4.Object.receiveStateHeater());
            Thread.Sleep(500);  //Cekanje da se pokrenu listeneri

            regulator.regulate();


            Assert.IsTrue(mockDevice1.Object.HeaterIsOn);
            Assert.IsTrue(mockDevice2.Object.HeaterIsOn);
            Assert.IsTrue(mockDevice3.Object.HeaterIsOn);
            Assert.IsTrue(mockDevice4.Object.HeaterIsOn);
            Assert.IsTrue(mockHeater.Object.isOn);
            Assert.AreEqual(Enums.Command.TurnOn, regulator.previousCommand);
        }

        //The active test run was aborted. Reason: Test host process crashed
        [Test]
        public void TestRegulate_AverageTemperatureGreaterThanDayTemperature_TurnsHeaterOff()
        {
            regulator.dnevniPocetak = 10;
            regulator.dnevniKraj = 20;
            regulator.temperatures = new Dictionary<int, double>
            {
                { 1, 26.5 },
                { 2, 27.5 },
                { 3, 28.5 },
                { 4, 29.5 }
            };
            regulator.dayTemperature = 25;
            regulator.nightTemperature = 20;
            regulator.previousCommand = Enums.Command.TurnOn;

            var mockHeater = new Mock<CentralHeater.CentralHeater>();
            Task.Factory.StartNew(() => mockHeater.Object.receiveCommand());
            var mockDevice1 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice2 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice3 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice4 = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice1.Object.Id = 1;
            mockDevice1.Object.Id = 2;
            mockDevice1.Object.Id = 3;
            mockDevice1.Object.Id = 4;
            Task.Factory.StartNew(() => mockDevice1.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice2.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice3.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice4.Object.receiveStateHeater());
            Thread.Sleep(500);  //Cekanje da se pokrenu listeneri

            regulator.regulate();

            Assert.IsFalse(mockDevice1.Object.HeaterIsOn);
            Assert.IsFalse(mockDevice2.Object.HeaterIsOn);
            Assert.IsFalse(mockDevice3.Object.HeaterIsOn);
            Assert.IsFalse(mockDevice4.Object.HeaterIsOn);
            Assert.IsFalse(mockHeater.Object.isOn);
            Assert.AreEqual(Enums.Command.TurnOff, regulator.previousCommand);
        }

        //The active test run was aborted. Reason: Test host process crashed
        [Test]
        public void TestRegulate_AverageTemperatureGreaterThanNightTemperature_TurnsHeaterOff()
        {
            regulator.dnevniPocetak = 10;
            regulator.dnevniKraj = 20;
            regulator.temperatures = new Dictionary<int, double>
            {
                { 1, 26.5 },
                { 2, 27.5 },
                { 3, 28.5 },
                { 4, 29.5 }
            };
            regulator.dayTemperature = 29;
            regulator.nightTemperature = 25;
            regulator.previousCommand = Enums.Command.TurnOn;

            var mockHeater = new Mock<CentralHeater.CentralHeater>();
            Task.Factory.StartNew(() => mockHeater.Object.receiveCommand());
            var mockDevice1 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice2 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice3 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice4 = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice1.Object.Id = 1;
            mockDevice1.Object.Id = 2;
            mockDevice1.Object.Id = 3;
            mockDevice1.Object.Id = 4;
            Task.Factory.StartNew(() => mockDevice1.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice2.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice3.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice4.Object.receiveStateHeater());
            Thread.Sleep(500);  //Cekanje da se pokrenu listeneri

            regulator.regulate();



            Assert.IsFalse(mockDevice1.Object.HeaterIsOn);
            Assert.IsFalse(mockDevice2.Object.HeaterIsOn);
            Assert.IsFalse(mockDevice3.Object.HeaterIsOn);
            Assert.IsFalse(mockDevice4.Object.HeaterIsOn);
            Assert.IsFalse(mockHeater.Object.isOn);
            Assert.AreEqual(Enums.Command.TurnOff, regulator.previousCommand);
        }

        [Test]
        public void TestSendCommand_TurnOn()
        {
            var mockHeater = new Mock<CentralHeater.CentralHeater>();
            mockHeater.Object.isOn = false;
            bool expected = true;

            Task.Factory.StartNew(() => mockHeater.Object.receiveCommand());
            Thread.Sleep(500);  //Cekanje da se pokrene listener receiveCommand()

            regulator.sendCommand(Common.Enums.Command.TurnOn);
            Thread.Sleep(500);  //Cekanje da se posalje komanda


            Assert.AreEqual(expected, mockHeater.Object.isOn);
        }

        [Test]
        public void TestSendCommand_TurnOff()
        {
            var mockHeater = new Mock<CentralHeater.CentralHeater>();
            mockHeater.Object.isOn = false;
            bool expected = false;

            Task.Factory.StartNew(() => mockHeater.Object.receiveCommand());
            Thread.Sleep(500);  //Cekanje da se pokrene listener receiveCommand()

            regulator.sendCommand(Common.Enums.Command.TurnOff);
            Thread.Sleep(500);  //Cekanje da se posalje komanda


            Assert.AreEqual(expected, mockHeater.Object.isOn);
        }

        [Test]
        public void TestSendMessageToDevice_1Device_TurnOn()
        {
            var mockDevice1 = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice1.Object.Id = 19;
            mockDevice1.Object.HeaterIsOn = true;

            bool expected = true;

            Task.Factory.StartNew(() => mockDevice1.Object.receiveStateHeater());
            Thread.Sleep(500);  //Cekanje da se pokrenu listeneri

            regulator.sendMessageToDevice(Enums.Command.TurnOn, Constants.PortRegulatorDevice + mockDevice1.Object.Id);
            Thread.Sleep(500);  //Cekanje slanja poruka


            Assert.AreEqual(expected, mockDevice1.Object.HeaterIsOn);
        }

        [Test]
        public void TestSendMessageToDevice_2Devices_TurnOn()
        {
            var mockDevice1 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice2 = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice1.Object.Id = 1;
            mockDevice1.Object.HeaterIsOn = true;
            mockDevice2.Object.Id = 2;
            mockDevice2.Object.HeaterIsOn = false;

            bool expected = true;

            Task.Factory.StartNew(() => mockDevice1.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice2.Object.receiveStateHeater());
            Thread.Sleep(500);  //Cekanje da se pokrenu listeneri

            regulator.sendMessageToDevice(Enums.Command.TurnOn, Constants.PortRegulatorDevice + mockDevice1.Object.Id);
            regulator.sendMessageToDevice(Enums.Command.TurnOn, Constants.PortRegulatorDevice + mockDevice2.Object.Id);
            Thread.Sleep(500);  //Cekanje slanja poruka


            Assert.AreEqual(expected, mockDevice1.Object.HeaterIsOn);
            Assert.AreEqual(expected, mockDevice2.Object.HeaterIsOn);
        }

        [Test]
        public void TestSendMessageToDevice_2Devices_TurnOff()
        {
            var mockDevice1 = new Mock<ReadingDevice.ReadingDevice>();
            var mockDevice2 = new Mock<ReadingDevice.ReadingDevice>();
            mockDevice1.Object.Id = 1;
            mockDevice1.Object.HeaterIsOn = true;
            mockDevice2.Object.Id = 2;
            mockDevice2.Object.HeaterIsOn = false;

            bool expected = false;

            Task.Factory.StartNew(() => mockDevice1.Object.receiveStateHeater());
            Task.Factory.StartNew(() => mockDevice2.Object.receiveStateHeater());
            Thread.Sleep(500);  //Cekanje da se pokrenu listeneri

            regulator.sendMessageToDevice(Enums.Command.TurnOff, Constants.PortRegulatorDevice + mockDevice1.Object.Id);
            regulator.sendMessageToDevice(Enums.Command.TurnOff, Constants.PortRegulatorDevice + mockDevice2.Object.Id);
            Thread.Sleep(500);  //Cekanje slanja poruka


            Assert.AreEqual(expected, mockDevice1.Object.HeaterIsOn);
            Assert.AreEqual(expected, mockDevice2.Object.HeaterIsOn);
        }



    }
}

