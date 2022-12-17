using System;
using Common;

namespace TemperatureRegulator
{
    public class TemperatureRegulator
    {
        private string mode; //trenutni rezim 
        private int temperature; //trenutna temperatura

        //metoda za postavljanje rezima
        public void SetMode(string mode)
        {
            this.mode = mode;
        }

        //metoda za postavljanje temperature u odredjenom rezimu

        public void SetTemperature(int temperature)
        {
            if(mode=="cooling")
            {
                //ukoliko je rezim hladjenje,temperatura
                // ne moze biti veca od 25°C
                this.temperature = Math.Min(temperature, 25);
            }
            else if(mode=="heating")
            {
                //ukoliko je rezim grejanja,temperatura
                // ne moze biti niza od 15°C
                this.temperature = Math.Max(temperature, 25);
            }
            else
            {
                //ako je temperatura izmedju 15°C i 25°C
                this.temperature = Math.Max(Math.Min(temperature, 25), 15);

            }
        }

        //metoda za dohvatanje trenutnog rezima
        public string GetMode()
        {
            return mode;
        }

        //metoda za dohvatanje trenutne temperature
        public int GetTemperature()
        {
            return temperature;
        }


        
    }
}

