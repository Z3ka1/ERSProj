using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SmartThermoregulator
{
    class Program
    {
        static TemperatureRegulator.TemperatureRegulator temperatureRegulator = new TemperatureRegulator.TemperatureRegulator();
        //TODO Sve iz main metode prebaciti u pripadajuce klase
        static void Main(string[] args)
        {
            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.FileName = "./ReadingDevice";
            //startInfo.UseShellExecute = true;

            //Process process = Process.Start(startInfo);

            //if (process == null)
            //    Console.WriteLine("Failed");

            //if (process.HasExited)
            //{
            //    Console.WriteLine("Process exited with code {0}", process.ExitCode);
            //}

            //Process.Start("ReadingDevice");


            string com;

            int od;     //Temperatura od koje pocinje dnevni rezim
            int doo;    //Temperatura do koje traje dnevni rezim
            int temperaturaDnevnog;     // Temperatura dnevnog rezima
            int temperaturaNocnog;      //Temperatura nocnog rezima



            while (true)
            {
                Console.WriteLine("Unesite od koliko sati pocinje dnevni rezim!");
                com = Console.ReadLine();
                if(Int32.TryParse(com, out od) != true)
                {
                    continue;
                }
                if (od <= 23 && od >= 0)
                {
                    break;
                }
            }

            temperatureRegulator.dnevniPocetak = od;

            while (true)
            {
                Console.WriteLine("Unesite do koliko sati traje dnevni rezim!");
                com = Console.ReadLine();
                if(Int32.TryParse(com, out doo) != true)
                {
                    continue;
                }
                if (doo <=23 && doo >= 0)
                {
                    break;
                }
            }

            temperatureRegulator.dnevniKraj = doo;

            while (true)
            {
                Console.WriteLine("Unesite temperaturu dnevnog rezima!");
                com = Console.ReadLine();
                if(Int32.TryParse(com, out temperaturaDnevnog) != true)
                {
                    continue;
                }
                if (temperaturaDnevnog >= 0 && temperaturaDnevnog <= 35)
                {
                    break;
                }
            }

            temperatureRegulator.SetDayTemperature(temperaturaDnevnog);

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

            temperatureRegulator.SetNightTemperature(temperaturaNocnog);

                Console.WriteLine("Izvrsavanje...");


            Thread t1 = new Thread(new ThreadStart(Konekcije));       // Pozivam nit koja ce provjeravati i prihvatati konekcije
            t1.Start();


            while (true)
            {
                if (temperatureRegulator.readingDevices.Count >= 4)
                {
                                // Dalji rad sa Temperature Regulatorom kada se pozovu 4 ili vise ReadingDevice
                }
            }

            

            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Log($"Ovde se unosi poruka koja se ispisuje u logeru", w);
            }

            using (StreamReader r = File.OpenText("log.txt"))
            {
                DumpLog(r);
            }





            //TemperatureRegulator.TemperatureRegulator tr = new TemperatureRegulator.TemperatureRegulator();
            //tr.SetMode("cooling");
            //tr.SetTemperature(20);

            //string currentMode = tr.GetMode();

            // int currentTemperature = tr.GetTemperature();


            //Console.WriteLine("Trenutni rezim rada:" + currentMode);
            // Console.WriteLine("Trenutna temperatura:" + currentTemperature);








            Console.ReadLine();
        }


        public static void Konekcije()
        {
            while (true)
            {

                //Ovde se prima inicijalna poruka od ReadingDevice

                int id = 0;
                string port = "PORT";

                temperatureRegulator.readingDevices.Add(id, port);           // Dictionary u koji smjestam sve Reading device


            }
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
}

