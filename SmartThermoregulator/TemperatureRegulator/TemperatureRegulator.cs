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
        //private int dayTemperature; //polje za cuvanje temperature za dnevni rezim
        //private int nightTemperature;//polje za cuvanje temperature za nocni rezim

        public int dayTemperature { get; set; }
        public int nightTemperature { get; set; }

        public Common.Enums.Command previousCommand;
        public Common.Enums.Command nextCommand;

        public int dnevniPocetak { get; set; }
        public int dnevniKraj { get; set; }

        //Lista temperatura , public zbog testiranja
        public Dictionary<Int32, double> temperatures;

        //Za proveru da li je regulator poceo sa radom
        private bool working;

        public TemperatureRegulator()
        {
            temperatures = new Dictionary<int, double>();
            previousCommand = Enums.Command.TurnOff;
            nextCommand = Enums.Command.TurnOff;
            working = false;

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
            try
            {
                listener.Start();
            }
            catch
            {
                Console.WriteLine("Greska: REGULATOR VEC POSTOJI");
                System.Environment.Exit(502);
            }

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                NetworkStream stream = client.GetStream();

                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string request = Encoding.UTF8.GetString(data, 0, bytes);

                //parts[0] sadrzi id uredjaja, parts[1] sadrzi temperaturu uredjaja
                string[] parts = request.Split(",");

                if (temperatures.ContainsKey(Int32.Parse(parts[0])))
                    Log(String.Format("Pristigla temperatura '{0:0.00}' od uredjaja '{1}'",double.Parse(parts[1]), Int32.Parse(parts[0])));
                else
                    Log(String.Format("Uredjaj '{1}' je dodat u sistem sa temperaturom '{0:0.00}'", double.Parse(parts[1]), Int32.Parse(parts[0])));

                temperatures[Int32.Parse(parts[0])] = double.Parse(parts[1]);

                regulate();

                client.Close();
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

            //Obezbedjuje da regulator ne izvrsava svoj zadatak dok nema 4 uredjaja
            if (numOfReadings < 4)
                return;
            if (!working)
            {
                Console.WriteLine("Regulator je poceo sa radom...");
                working = true;
                Log("Regulator je poceo sa radom!");
            }  
            avgTemp = avgTemp / numOfReadings;

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

            try
            {
                sendCommand(nextCommand);
            }
            catch
            {
                Console.WriteLine("GRESKA: Grejac nije pokrenut!");
                Console.WriteLine("Nakon paljenja grejaca ponovo pokrenuti regulator!");
                System.Environment.Exit(503);
            }


            if (previousCommand != nextCommand && temperatures.Count != 0)
            {
                foreach (var tmp in temperatures)
                {
                    try
                    {
                        sendMessageToDevice(nextCommand, Common.Constants.PortRegulatorDevice + tmp.Key);

                    }
                    catch
                    {
                        //U zadatku nije naglaseno sta raditi ukoliko u sistemu bude manje od 4 uredjaja
                        //u jednom trenutku, u catchu se moze izmeniti desavanje po potrebi.
                        Log("Uredjaj '" + tmp.Key + "' je pokvaren.");
                    }
                }
                if (nextCommand == Enums.Command.TurnOn)
                    Log("Poslata poruka svim uredjajima da je grejac UPALJEN!");
                else if (nextCommand == Enums.Command.TurnOff)
                    Log("Poslata poruka svim uredjajima da je grejac UGASEN!");

            }
            previousCommand = nextCommand;
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

        public void sendMessageToDevice(Common.Enums.Command komanda, int port)
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
            
            
        }

        public void Log(string logMessage)
        {
            using (StreamWriter writer = new StreamWriter("regulatorLog.txt", true))
            {
                writer.WriteLine(DateTime.Now.ToLongDateString() + " | " + DateTime.Now.ToString("H:mm:ss") + " | " + logMessage);
                writer.WriteLine("-------------------------------------");
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("REGULATOR");

            TemperatureRegulator tr = new TemperatureRegulator();
            tr.unosPodataka();
            //false kako bi se obrisali prethodni podaci iz txt fajla
            using (StreamWriter writer = new StreamWriter("regulatorLog.txt", false))
            {
                writer.WriteLine("\t\t\t--------------------------------------");
                writer.WriteLine("\t\t\t|DNEVNI REZIM: " + tr.dnevniPocetak + ":00 - " + tr.dnevniKraj + ":00 Temp: " + tr.dayTemperature + "|");
                writer.WriteLine("\t\t\t|NOCNI REZIM:  " + tr.dnevniKraj + ":00 - " + tr.dnevniPocetak + ":00 Temp: " + tr.nightTemperature + "|");
                writer.WriteLine("\t\t\t--------------------------------------");
                writer.WriteLine("\tDATUM I VREME DOGADJAJA\t\t\t\tDOGADJAJ");
                writer.WriteLine("--------------------------------------------------------------------------------------------");
            }

            Thread t1 = new Thread(tr.receiveTemperature);

            t1.Start();



            Console.ReadLine();
        }
    }



}