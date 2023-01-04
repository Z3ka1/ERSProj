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
        private int dayTemperature; //polje za cuvanje temperature za dnevni rezim
        private int nightTemperature;//polje za cuvanje temperature za nocni rezim

        Common.Enums.Command previousCommand;
        Common.Enums.Command nextCommand;

        public int dnevniPocetak { get; set; }
        public int dnevniKraj { get; set; }

        //Lista temperatura 
        private Dictionary<Int32, double> temperatures;



        public TemperatureRegulator()
        {
            temperatures = new Dictionary<int, double>();
            previousCommand = Enums.Command.TurnOff;
            nextCommand = Enums.Command.TurnOff;
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
                Console.WriteLine();
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

            Console.WriteLine("IZRACUNAO AVG = " + avgTemp);

            // Provera da li je potrebno upaliti ili ugasiti peć
            int currentHour = DateTime.Now.Hour;
            if (currentHour >= dnevniPocetak && currentHour < dnevniKraj)
            {
                //trenutno je dan
                if (avgTemp < dayTemperature)
                    nextCommand = Enums.Command.TurnOn;
                //Ne gasi se cim dostigne zeljenu temp zato sto bi se u tom slucaju stalno palila/gasila
                else if (avgTemp >= (dayTemperature + Common.Constants.TempRegulatorTempGap) && previousCommand != Enums.Command.TurnOff)
                    nextCommand = Enums.Command.TurnOff;
            }
            else
            {
                //trenutno je noc
                if (avgTemp < nightTemperature)
                    nextCommand = Enums.Command.TurnOn;
                else if (avgTemp >= (nightTemperature + Common.Constants.TempRegulatorTempGap) && previousCommand != Enums.Command.TurnOff)
                    nextCommand = Enums.Command.TurnOff;
            }
            sendCommand(nextCommand);
            Console.WriteLine("POSLAO PECI");

            if (previousCommand != nextCommand && temperatures.Count != 0)
            {
                Console.WriteLine("USAO U IF");
                foreach (var tmp in temperatures)
                {
                    Console.WriteLine("USAO U FOREACH");
                    sendMessageToRegulator(nextCommand, Common.Constants.PortRegulatorDevice + tmp.Key);
                    Console.WriteLine("GOTOV FOREACH");
                }
            }
            previousCommand = nextCommand;
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

            string message = "";
            if (komanda == Enums.Command.TurnOn)
                message = "upaljena";
            else if (komanda == Enums.Command.TurnOff)
                message = "ugasena";
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


            Console.Clear();
            Console.WriteLine("Regulator");
            Console.WriteLine("DNEVNI REZIM: " + od + ":00 - " + doo + ":00 Temp: " + temperaturaDnevnog);
            Console.WriteLine("NOCNI REZIM:  " + doo + ":00 - " + od + ":00 Temp: " + temperaturaNocnog);
            
            Console.WriteLine("Regulator je poceo sa radom...");
        }



    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("REGULATOR");

            TemperatureRegulator tr = new TemperatureRegulator();
            Thread t1 = new Thread(tr.receiveTemperature);

            t1.Start();



            Console.ReadLine();
        }
    }



}