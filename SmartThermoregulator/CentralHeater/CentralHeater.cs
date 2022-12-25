using System;
using System.IO;
using Common;

namespace CentralHeater
{
    //TODO Naslediti ICentralHeater
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
                Log($"Grijac se upalio", w);
            }
        }
            
            // Ako grejac treba da se iskljuci, zaustavi tajmer i izracunaj vreme trajanja

        public void TurnOff()
        {
            this.isOn = false;
            runTime = DateTime.Now - startTime;


            using (StreamWriter w = File.AppendText("log.txt"))
            {
                    Log($"Grijac se ugasio", w);
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





        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine($"  :  {logMessage}");
            w.WriteLine("-------------------------------");
        }

    }
}

