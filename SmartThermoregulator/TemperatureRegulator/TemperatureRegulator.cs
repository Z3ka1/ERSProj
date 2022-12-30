using System;
using Common;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Net;
using CentralHeater;
using System.IO;

namespace TemperatureRegulator
{
    public class TemperatureRegulator : ITemperatureRegulator
    {
        //TODO prebaciti u common Consts
        private const int DEFAULT_DAY_TEMPERATURE = 22;
        private const int DEFAULT_NIGHT_TEMPERATURE = 18;

        public Dictionary<int, string> readingDevices = new Dictionary<int, string>();         // Dictionary u kome cemo cuvati portove i ID-jeve ReadingDevices

        private int dayTemperature; //polje za cuvanje temperature za dnevni rezim
        private int nightTemperature;//polje za cuvanje temperature za nocni rezim


       
        public int dnevniPocetak { get; set; }
        public int dnevniKraj { get; set; }

        //Lista temperatura 
        private Dictionary<Int32, double> temperatures;



        private CentralHeater.CentralHeater centralHeater;
        public TemperatureRegulator()
        {
            this.centralHeater = new CentralHeater.CentralHeater();
        }

        public TemperatureRegulator(int dayHours)
        {
            temperatures = new Dictionary<int, double>();
            dayTemperature = DEFAULT_DAY_TEMPERATURE;
            nightTemperature = DEFAULT_NIGHT_TEMPERATURE;

            //ako je unet broj sati koji nije izmedju 0 i 24 imamo izuzetak
            if (dayHours < 0 || dayHours > 24)
            {
                throw new ArgumentOutOfRangeException("dayHours", "Day hours mora biti izmedju 0 i 24.");
            }
        }

        //metode za postavljanje temperature za odgovarajuci rezim
        public void SetDayTemperature(int temperature)
        {
            dayTemperature = temperature;
        }

        public void SetNightTemperature(int temperature)
        {
            nightTemperature = temperature;
        }

        //metoda koja vraca ocekivanu temperaturu u prosledjenom satu
        public int GetTemperature(int hour)
        {
            if (hour < 0 || hour > 24)
            {
                throw new ArgumentOutOfRangeException("hour", "Hour mora biti izmedju 0 i 24.");
            }

            if (hour < dayTemperature)
            {
                return dayTemperature;
            }
            else
            {
                return nightTemperature;
            }
        }

        //Prima temperaturu od uredjaja (ReadingDevice)
        public void receiveTemperature()
        {
            IPAddress localAddr = IPAddress.Parse(Constants.localIpAddress);
            TcpListener listener = new TcpListener(localAddr, Common.Constants.PortDeviceRegulator);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                NetworkStream stream = client.GetStream();

                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string request = Encoding.UTF8.GetString(data, 0, bytes);

                //parts[0] sadrzi id uredjaja, parts[1] sadrzi temperaturu uredjaja
                string[] parts = request.Split(",");

                temperatures[Int32.Parse(parts[0])] = double.Parse(parts[1]);

                regulate();

                client.Close();
            }

        }

        //TODO Dodati kad treba ugasiti pec
        public void regulate()
        {
            double avgTemp = 0;
            int numOfReadings = 0;
            foreach (var tmp in temperatures)
            {
                avgTemp += tmp.Value;
                numOfReadings++;
            }

            avgTemp = avgTemp / numOfReadings;

            // Provera da li je potrebno upaliti ili ugasiti peć
            int currentHour = DateTime.Now.Hour;
            Common.Enums.Command komanda = Enums.Command.Nothing;
            if(currentHour>=dnevniPocetak && currentHour<dnevniKraj)
            {
                //trenutno je dan
                if(avgTemp<dayTemperature)
                    komanda = Enums.Command.TurnOn;
                else if (avgTemp >= dayTemperature+3)
                    komanda = Enums.Command.TurnOff;
            }
            else
            {
                //trenutno je noc
                if(avgTemp<nightTemperature)
                    komanda = Enums.Command.TurnOn;
                else if (avgTemp >= nightTemperature+3)
                    komanda = Enums.Command.TurnOff;
            }

            sendCommand(komanda);
        }

        //Salje centralnoj peci znak da se ukljuci/iskljuci
        public void sendCommand(Common.Enums.Command komanda)
        {
            //Konektovanje na server (CentralHeater)
            TcpClient client = new TcpClient("localhost", Common.Constants.PortRegulatorHeater);
            NetworkStream stream = client.GetStream();

            //Slanje poruke sa komandom heateru
            string message = komanda.ToString();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            client.Close();
        }


        public void sendMessageToRegulator(Common.Enums.Command komanda, int port)
        {
            TcpClient client = new TcpClient("localhost", port);
            NetworkStream stream = client.GetStream();

            string message = "ugasena";
            if (komanda == Enums.Command.TurnOn)
                message = "upaljena";
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            client.Close();
        }



    }

    class Program
    {
        static void Main(string[] args)
        {
            //TemperatureRegulator tr = new TemperatureRegulator(10);

            //tr.receiveTemperature();


            Console.WriteLine("Test");
            Console.ReadLine();
        }
    }



}

