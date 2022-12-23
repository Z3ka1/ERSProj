using System;
using Common;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;

namespace ReadingDevice
{
    public class ReadingDevice : IReadingDevice
    {
        public int Id { get; set; }
        public double Temperature { get; set; }

        //TODO Promeniti generisanje IDa ovako nece raditi
        public ReadingDevice()
        {
        }

        public void SendInitialMessage(int id, double temperature)
        {
                                                                // Ovde treba povezati sa serverom da upisemo ID i inicijalnu temperaturu i da registrujemo ReadingDevice na TemperatureRegulator
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
        static ReadingDevice readingDevice = new ReadingDevice();
        static Mutex m = new Mutex();
        static void Main(string[] args)
        {

            while (true)
            {
                Console.WriteLine("Unesite ID:");
                if (Int32.TryParse(Console.ReadLine(), out int id))
                {
                    readingDevice.Id = id;
                    break;
                }
            }

            while (true)
            {
                Console.WriteLine("Unesite inicijalnu temperaturu:");
                if (double.TryParse(Console.ReadLine(), out double temp) && temp >= 0 && temp <= 35)
                {
                    readingDevice.Temperature = temp;
                    break;
                }
            }

           readingDevice.SendInitialMessage(readingDevice.Id, readingDevice.Temperature);  // Saljem inicijalnu poruku


            Thread t1 = new Thread(new ThreadStart(Grijac));       // Pozivam nit koja povecava temperaturu ako je grijac upaljen
            t1.Start();


            while (true)
            {
                m.WaitOne();
                readingDevice.sendTemperature();                        // Na svaka 3 minuta saljem poruku
                m.ReleaseMutex();

                Thread.Sleep(1000 * 60 * 3);
            }


        }
        
        static void Grijac()
        {
            while (true)
            {
                // Ovde treba cekati poruku od servera i kad je primi provjeriti da li je upaljen grijac

                bool upaljen = true;

                if (upaljen)
                {
                    m.WaitOne();
                    readingDevice.raiseTemperature();
                    m.ReleaseMutex();
                }
            }

           
        }
    
    }
}

