using System;
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
            }
            // Ako grejac treba da se iskljuci, zaustavi tajmer i izracunaj vreme trajanja

            else if (command == "TurnOff")
            {
                runTime = DateTime.Now - startTime;

            }
        }

        // Metoda koja vraca ukupno vreme koliko je grejac bio ukljucen
        public TimeSpan GetRunTime()
        {

            return runTime;
       
        }



    }
}

