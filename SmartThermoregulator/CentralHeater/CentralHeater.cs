using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common;

namespace CentralHeater
{
    public class CentralHeater : ICentralHeater
    {
        // Privatno polje koje cuva vreme kada je grejac ukljucen
        private DateTime startTime;
        // Privatno polje koje cuva ukupno vreme koliko je grejac bio ukljucen
        private TimeSpan runTime;

        // Promenljiva koja čuva stanje uključenosti peći
        public bool isOn;

        // Konstruktor za klasu CentralHeater
        public CentralHeater()
        {
            // Inicijalno, peć je ugašena
            this.isOn = false;
            runTime = TimeSpan.Zero;
        }

        // Metoda koja se poziva kada CentralHeater primi komandu od TemperatureRegulator-a

        public void TurnOn()
        {
            // Ako grejac treba da se ukljuci, pokreni tajmer i postavi vreme pocetka

            this.isOn = true;
            startTime = DateTime.Now;
            runTime = TimeSpan.Zero;

            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Log($"Grejac se ukljucio", w);
            }
        }

        // Ako grejac treba da se iskljuci, zaustavi tajmer i izracunaj vreme trajanja

        public void TurnOff()
        {
            this.isOn = false;
            runTime = DateTime.Now - startTime;


            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Log($"Grejac se iskljucio", w);
            }
        }

        // Metoda za proveru da li je peć upaljena
        public bool IsOn()
        {
            return this.isOn;
        }


        // Metoda koja vraca ukupno vreme koliko je grejac bio ukljucen
        public TimeSpan GetRunTime()
        {
            return runTime;
        }


        public void receiveCommand()
        {
            IPAddress localAddr = IPAddress.Parse(Constants.localIpAddress);
            TcpListener listener = new TcpListener(localAddr, Common.Constants.PortRegulatorHeater);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("KONEKTOVAN REGULATOR NA PEC");

                NetworkStream stream = client.GetStream();

                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string request = Encoding.UTF8.GetString(data, 0, bytes);

                switch (request)
                {
                    case "TurnOn":
                        TurnOn();
                        break;
                    case "TurnOff":
                        TurnOff();
                        break;
                    default:
                        break;
                }
                Console.WriteLine("PEC: " + isOn);

                client.Close();
            }
        }


        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine($"  :  {logMessage}");
            w.WriteLine("-------------------------------");
        }

       

    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CENTRALNA PEC");
            CentralHeater ch = new CentralHeater();

            Thread t1 = new Thread(ch.receiveCommand);
            t1.Start();



            Console.ReadLine();
        }
    }
}
