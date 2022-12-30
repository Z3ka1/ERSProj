using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SmartThermoregulator
{
    class Program
    {
       
        //TODO Sve iz main metode prebaciti u pripadajuce klase
        static void Main(string[] args)
        {

            ProcessStartInfo startInfo1 = new ProcessStartInfo();
            startInfo1.FileName = "..\\..\\..\\..\\TemperatureRegulator\\bin\\Debug\\netcoreapp3.1\\TemperatureRegulator.exe";
            startInfo1.UseShellExecute = true;

            ProcessStartInfo startInfo2 = new ProcessStartInfo();
            startInfo2.FileName = "..\\..\\..\\..\\CentralHeater\\bin\\Debug\\netcoreapp3.1\\CentralHeater.exe";
            startInfo2.UseShellExecute = true;


            Console.WriteLine("Uneti broj uredjaja za citanje temperature: ");
            int x = int.Parse(Console.ReadLine());

            Process.Start(startInfo1);
            Process.Start(startInfo2);

            for (int i = 0; i < x; i++)
            {
                ProcessStartInfo startInfo3 = new ProcessStartInfo();
                startInfo3.FileName = "..\\..\\..\\..\\ReadingDevice\\bin\\Debug\\netcoreapp3.1\\ReadingDevice.exe";
                startInfo3.UseShellExecute = true; 
                Process.Start(startInfo3);
            }

            






            //Thread t1 = new Thread(new ThreadStart(Konekcije));       // Pozivam nit koja ce provjeravati i prihvatati konekcije
            //t1.Start();


            //while (true)
            //{
            //    if (temperatureRegulator.readingDevices.Count >= 4)
            //    {
            //                    // Dalji rad sa Temperature Regulatorom kada se pozovu 4 ili vise ReadingDevice
            //    }
            //}



            //using (StreamWriter w = File.AppendText("log.txt"))
            //{
            //    Log($"Ovde se unosi poruka koja se ispisuje u logeru", w);
            //}

            //using (StreamReader r = File.OpenText("log.txt"))
            //{
            //    DumpLog(r);
            //}





            //TemperatureRegulator.TemperatureRegulator tr = new TemperatureRegulator.TemperatureRegulator();
            //tr.SetMode("cooling");
            //tr.SetTemperature(20);

            //string currentMode = tr.GetMode();

            // int currentTemperature = tr.GetTemperature();


            //Console.WriteLine("Trenutni rezim rada:" + currentMode);
            // Console.WriteLine("Trenutna temperatura:" + currentTemperature);








            Console.ReadLine();
        }


        //public static void Konekcije()
        //{
        //    while (true)
        //    {

        //        //Ovde se prima inicijalna poruka od ReadingDevice

        //        int id = 0;
        //        string port = "PORT";

        //        temperatureRegulator.readingDevices.Add(id, port);           // Dictionary u koji smjestam sve Reading device


        //    }
        //}

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

