using System;
using Common;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Net;
using CentralHeater;
using System.IO;
using System.Threading;

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



        public TemperatureRegulator()
        {
            temperatures = new Dictionary<int, double>();
            dayTemperature = DEFAULT_DAY_TEMPERATURE;
            nightTemperature = DEFAULT_NIGHT_TEMPERATURE;
            unosPodataka();
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

                Console.WriteLine("PRIMLJEN");

                NetworkStream stream = client.GetStream();

                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string request = Encoding.UTF8.GetString(data, 0, bytes);

                //parts[0] sadrzi id uredjaja, parts[1] sadrzi temperaturu uredjaja
                string[] parts = request.Split(",");

                temperatures[Int32.Parse(parts[0])] = double.Parse(parts[1]);

                Console.WriteLine("Primljeno " + temperatures[Int32.Parse(parts[0])]);

                regulate();

                client.Close();
                Console.WriteLine("KONEKCIJA SA UREDJAJEM ZATVORENA");
            }

        }

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

            Console.WriteLine("IZRACUNAO AVG");

            // Provera da li je potrebno upaliti ili ugasiti peć
            int currentHour = DateTime.Now.Hour;
            Common.Enums.Command komanda = Enums.Command.Nothing;

            if (currentHour >= dnevniPocetak && currentHour < dnevniKraj)
            {
                //trenutno je dan
                if (avgTemp < dayTemperature)
                    komanda = Enums.Command.TurnOn;
                else if (avgTemp >= dayTemperature + 3)
                    komanda = Enums.Command.TurnOff;
            }
            else
            {
                //trenutno je noc
                if (avgTemp < nightTemperature)
                    komanda = Enums.Command.TurnOn;
                else if (avgTemp >= nightTemperature + 3)
                    komanda = Enums.Command.TurnOff;
            }

            sendCommand(komanda);
            Console.WriteLine("POSLAO PECI");

            if (komanda != Enums.Command.Nothing && temperatures.Count != 0)
            {
                Console.WriteLine("USAO U IF");
                foreach (var tmp in temperatures)
                {
                    Console.WriteLine("USAO U FOREACH");
                    sendMessageToRegulator(komanda, Common.Constants.PortRegulatorDevice + tmp.Key);
                    Console.WriteLine("GOTOV FOREACH");
                }
            }
            Console.WriteLine("REGULATE ZAVRSEN");
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

        public void unosPodataka()
        {
            string com;

            int od;     //Temperatura od koje pocinje dnevni rezim
            int doo;    //Temperatura do koje traje dnevni rezim
            int temperaturaDnevnog;     // Temperatura dnevnog rezima
            int temperaturaNocnog;      //Temperatura nocnog rezima



            while (true)
            {
                Console.WriteLine("Unesite od koliko sati pocinje dnevni rezim!");
                com = Console.ReadLine();
                if (Int32.TryParse(com, out od) != true)
                {
                    continue;
                }
                if (od <= 23 && od >= 0)
                {
                    break;
                }
            }

            dnevniPocetak = od;

            while (true)
            {
                Console.WriteLine("Unesite do koliko sati traje dnevni rezim!");
                com = Console.ReadLine();
                if (Int32.TryParse(com, out doo) != true)
                {
                    continue;
                }
                if (doo <= 23 && doo >= 0)
                {
                    break;
                }
            }

            dnevniKraj = doo;

            while (true)
            {
                Console.WriteLine("Unesite temperaturu dnevnog rezima!");
                com = Console.ReadLine();
                if (Int32.TryParse(com, out temperaturaDnevnog) != true)
                {
                    continue;
                }
                if (temperaturaDnevnog >= 0 && temperaturaDnevnog <= 35)
                {
                    break;
                }
            }

            SetDayTemperature(temperaturaDnevnog);

            while (true)
            {
                Console.WriteLine("Unesite temperaturu nocnog rezima!");
                com = Console.ReadLine();
                if (Int32.TryParse(com, out temperaturaNocnog) != true)
                {
                    continue;
                }
                if (temperaturaNocnog >= 0 && temperaturaNocnog <= 35)
                {
                    break;
                }
            }

            SetNightTemperature(temperaturaNocnog);

            Console.WriteLine("Izvrsavanje...");
        }



    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Regulator");

            TemperatureRegulator tr = new TemperatureRegulator();
            Thread t1 = new Thread(tr.receiveTemperature);

            t1.Start();



            Console.ReadLine();
        }
    }



}