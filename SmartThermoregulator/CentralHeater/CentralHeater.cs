using System;
using System.IO;
using Common;

namespace CentralHeater
{
    //TODO Naslediti ICentralHeater
    public class CentralHeater
    {
        // Privatno polje koje cuva vreme kada je grejac ukljucen
        private DateTime startTime;
        // Privatno polje koje cuva ukupno vreme koliko je grejac bio ukljucen
        private TimeSpan runTime;

        // Metoda koja se poziva kada CentralHeater primi komandu od TemperatureRegulator-a

        public void OnCommandReceived(string command)
        {
            // Ako grejac treba da se ukljuci, pokreni tajmer i postavi vreme pocetka
            if (command == "TurnOn")
            {
                startTime = DateTime.Now;
                runTime = TimeSpan.Zero;

                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    Log($"Grijac se upalio", w);
                }
            }
            // Ako grejac treba da se iskljuci, zaustavi tajmer i izracunaj vreme trajanja

            else if (command == "TurnOff")
            {
                runTime = DateTime.Now - startTime;


                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    Log($"Grijac se ugasio", w);
                }
            }
        }

        // Metoda koja vraca ukupno vreme koliko je grejac bio ukljucen
        public TimeSpan GetRunTime()
        {

            return runTime;
       
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine($"  :  {logMessage}");
            w.WriteLine("-------------------------------");
        }

    }
}

