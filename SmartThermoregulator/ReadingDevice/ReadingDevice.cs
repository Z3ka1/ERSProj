using System;
using Common;
using System.Net.Sockets;
using System.Text;

namespace ReadingDevice
{
    public class ReadingDevice : IReadingDevice
    {
        public int Id { get; set; }
        public double Temperature { get; set; }

        public ReadingDevice()
        {
            Id = Common.Globals.GeneratorIdReadingDevice;
            //Setuje temperaturu na random vrednost izmedju 15 i 25
            Temperature = 15 + (new Random()).NextDouble() * 10;
            Common.Globals.GeneratorIdReadingDevice++;
        }

        //Salje temperaturu regulatoru
        public void sendTemperature()
        {
            //Konektovanje na server (Regulator)
            TcpClient client = new TcpClient("localhost", Common.Constants.PortDeviceRegulator);
            NetworkStream stream = client.GetStream();

            //Slanje poruke sa temperaturom regulatoru
            string message = Temperature.ToString();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);



            client.Close();
        }

        //Povecava temperaturu ukoliko je grejac ukljucen
        public void raiseTemperature()
        {
            Temperature += Common.Constants.ReadingDeviceTempChange;
        }

        public override string ToString()
        {
                                                                    //Da bi se temp ispisala na 2 decimale
            string s = "Id: " + Id + " Temperature: " + Temperature.ToString("0.00"); 
            return s;
        }
    }
}

