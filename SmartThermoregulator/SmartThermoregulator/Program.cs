using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SmartThermoregulator
{
    class Program
    {
       
        static void Main(string[] args)
        {

            ProcessStartInfo startInfo1 = new ProcessStartInfo();
            startInfo1.FileName = "..\\..\\..\\..\\TemperatureRegulator\\bin\\Debug\\netcoreapp3.1\\TemperatureRegulator.exe";
            startInfo1.UseShellExecute = true;

            ProcessStartInfo startInfo2 = new ProcessStartInfo();
            startInfo2.FileName = "..\\..\\..\\..\\CentralHeater\\bin\\Debug\\netcoreapp3.1\\CentralHeater.exe";
            startInfo2.UseShellExecute = true;

            Console.Clear();

            int x = 0;
            do
            {
                Console.WriteLine("Uneti broj uredjaja za citanje temperature: ");
                try { x = int.Parse(Console.ReadLine()); } catch { }
            } while (x < 4 || x > 999);

            Process.Start(startInfo1);
            Process.Start(startInfo2);

            for (int i = 0; i < x; i++)
            {
                ProcessStartInfo startInfo3 = new ProcessStartInfo();
                startInfo3.FileName = "..\\..\\..\\..\\ReadingDevice\\bin\\Debug\\netcoreapp3.1\\ReadingDevice.exe";
                startInfo3.UseShellExecute = true; 
                Process.Start(startInfo3);
            }


            //Console.ReadLine();
        }


    }
}

