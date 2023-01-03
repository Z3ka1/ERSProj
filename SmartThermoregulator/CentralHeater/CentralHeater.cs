using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common;
using EFDataBase;

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

        public double TotalResourcesSpent { get; set; }


        // Konstruktor za klasu CentralHeater
        public CentralHeater()
        {
            // Inicijalno, peć je ugašena
            this.isOn = false;
            runTime = TimeSpan.Zero;
            TotalResourcesSpent = 0;

            //Pravljenje data baze ako ne postoji
            using (EFContext context = new EFContext())
            {
                context.Database.EnsureCreated();
            }
        }

        // Metoda koja se poziva kada CentralHeater primi komandu od TemperatureRegulator-a

        public void TurnOn()
        {
            // Ako grejac treba da se ukljuci, pokreni tajmer i postavi vreme pocetka

            this.isOn = true;
            startTime = DateTime.Now;
            runTime = TimeSpan.Zero;
        }

        // Ako grejac treba da se iskljuci, zaustavi tajmer i izracunaj vreme trajanja

        public void TurnOff()
        {
            this.isOn = false;
            runTime = DateTime.Now - startTime;
            TotalResourcesSpent += runTime.TotalHours * Common.Constants.CentralHeaterResourcesPerHour;
            double resourcesSpentCycle = runTime.TotalHours * Common.Constants.CentralHeaterResourcesPerHour;

            //Dodavanje potrebnih podataka u bazu
            EFContext.addInfo(runTime, startTime, resourcesSpentCycle);
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

                Console.WriteLine("KOMANDA: " + request);

                switch (request)
                {
                    case "TurnOn":
                        //Obezbedjuje da se pali pec samo kad je ona ugasena
                        if(!isOn)
                            TurnOn();
                        break;

                    case "TurnOff":
                        if(isOn)
                            TurnOff();
                        break;

                    default:
                        break;
                }
                Console.WriteLine("PEC: " + isOn);
                //ispisUI();

                client.Close();
            }
        }

        //Ceka novi uredjaj i vraca mu informaciju o stanju grejaca
        public void waitNewDevice()
        {
            IPAddress localAddr = IPAddress.Parse(Constants.localIpAddress);
            TcpListener listener = new TcpListener(localAddr, Common.Constants.PortHeaterDevice);
            listener.Start();

            while(true)
            {
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                string response = isOn.ToString();

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);

                client.Close();
            }

        }

        //TODO Isprobati na win
        public void ispisUI()
        {
            Console.Clear();
            Console.WriteLine("CENTRALNA PEC");
            if (isOn)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                //Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("UKLJUJCENA");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                //Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("ISKLJUCENA");
                Console.ResetColor();
            }
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CENTRALNA PEC");
            CentralHeater ch = new CentralHeater();

            Thread t2 = new Thread(ch.waitNewDevice);
            t2.Start();
            Thread t1 = new Thread(ch.receiveCommand);
            t1.Start();


            Console.ReadLine();
        }
    }
}
