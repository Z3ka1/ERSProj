using System;
using Common;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace ReadingDevice
{
    public class ReadingDevice : IReadingDevice
    {
        public int Id { get; set; }
        public double Temperature { get; set; }

        //TODO Promeniti generisanje IDa ovako nece raditi
        public ReadingDevice()
        {
            Id = Common.Globals.GeneratorIdReadingDevice;
            //Setuje temperaturu na random vrednost izmedju 15 i 25
            Temperature = 15 + (new Random()).NextDouble() * 10;
            Common.Globals.GeneratorIdReadingDevice++;
        }

        //Salje temperaturu regulatoru
        //Metoda ce se verovatno menjati (ukoliko moze bez Client/Server)
        public void sendTemperature()
        {
            //Konektovanje na server (Regulator)
            TcpClient client = new TcpClient("localhost", Common.Constants.PortDeviceRegulator);
            NetworkStream stream = client.GetStream();

            //Slanje poruke sa temperaturom regulatoru
            string message = Id.ToString() + "," + Temperature.ToString();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);


            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Log($"ReadingDevice ID={Id} je poslao temperaturu: {Temperature}", w);
            }


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


        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine($"  :  {logMessage}");
            w.WriteLine("-------------------------------");
        }

        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            //ReadingDevice rd = new ReadingDevice();

            //rd.sendTemperature();


            Console.WriteLine("Test");

            Console.ReadLine();
        }
    }
}

