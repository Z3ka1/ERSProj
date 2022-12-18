using System;
using Common;

namespace TemperatureRegulator
{
    //TODO Naslediti ITemperatureRegulator
    public class TemperatureRegulator
    {
        //TODO prebaciti u common Consts
        private const int DEFAULT_DAY_TEMPERATURE = 22;
        private const int DEFAULT_NIGHT_TEMPERATURE = 18;

        private int dayTemperature; //polje za cuvanje temperature za dnevni rezim
        private int nightTemperature;//polje za cuvanje temperature za nocni rezim

        public TemperatureRegulator(int dayHours)
        {
            dayTemperature = DEFAULT_DAY_TEMPERATURE;
            nightTemperature = DEFAULT_NIGHT_TEMPERATURE;

            //ako je unet broj sati koji nije izmedju 0 i 24 imamo izuzetak
            if (dayHours < 0 || dayHours > 24)
            {
                throw new ArgumentOutOfRangeException("dayHours", "Day hours mora biti izmedju 0 i 24.");
            }
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

        //metoda koja vraca trenutnu temperaturu za prosledjenji sat
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
    }



}

