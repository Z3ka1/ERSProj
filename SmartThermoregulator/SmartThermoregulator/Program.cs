using System;

namespace SmartThermoregulator
{
    class Program
    {
        static void Main(string[] args)
        {
            string com;

            int od;     //Temperatura od koje pocinje dnevni rezim
            int doo;    //Temperatura do koje traje dnevni rezim
            int temperaturaDnevnog;     // Temperatura dnevnog rezima
            int temperaturaNocnog;      //Temperatura nocnog rezima



            //TODO Izmeniti ucitavanje vremena, napraviti zastitu unosa 
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
                Console.WriteLine("Izvrsavanje...");





            //Test ReadingDevice
            //ReadingDevice.ReadingDevice rd1 = new ReadingDevice.ReadingDevice();
            //ReadingDevice.ReadingDevice rd2 = new ReadingDevice.ReadingDevice();

            //Console.WriteLine(rd1);
            //Console.WriteLine(rd2);

            //Console.WriteLine();
            //rd1.raiseTemperature();
            //Console.WriteLine(rd1);
            //

         

            //TemperatureRegulator.TemperatureRegulator tr = new TemperatureRegulator.TemperatureRegulator();
            //tr.SetMode("cooling");
            //tr.SetTemperature(20);

            //string currentMode = tr.GetMode();

            // int currentTemperature = tr.GetTemperature();


            //Console.WriteLine("Trenutni rezim rada:" + currentMode);
            // Console.WriteLine("Trenutna temperatura:" + currentTemperature);








            Console.ReadLine();
        }
    }
}

