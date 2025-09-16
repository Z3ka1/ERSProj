using System;
using Common;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Net;
using CentralHeater;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Collections.Concurrent;

namespace TemperatureRegulator
{
    public class TemperatureRegulator : ITemperatureRegulator
    {
        //private int dayTemperature; //polje za cuvanje temperature za dnevni rezim
        //private int nightTemperature;//polje za cuvanje temperature za nocni rezim

        public int dayTemperature { get; set; }
        public int nightTemperature { get; set; }

        public Common.Enums.Command previousCommand;
        public Common.Enums.Command nextCommand;

        public int dnevniPocetak { get; set; }
        public int dnevniKraj { get; set; }

        //Lista temperatura , public zbog testiranja
        public ConcurrentDictionary<Int32, double> temperatures;

        // Key - reading device id, value - last temperature reading time
        public ConcurrentDictionary<Int32, DateTime> temperaturesLastReadingTime;

        //Za proveru da li je regulator poceo sa radom
        private bool working;

        // If regulator in manual mode, user can manually turn on and of central heater
        private bool isRegulatorInManualMode;

        public TemperatureRegulator()
        {
            temperatures = new ConcurrentDictionary<int, double>();
            temperaturesLastReadingTime = new ConcurrentDictionary<int, DateTime>();
            previousCommand = Enums.Command.TurnOff;
            nextCommand = Enums.Command.TurnOff;
            working = false;
            isRegulatorInManualMode = false;
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

        //metoda koja vraca ocekivanu temperaturu u prosledjenom satu
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

        //Prima temperaturu od uredjaja (ReadingDevice)
        public void receiveTemperature()
        {
            IPAddress localAddr = IPAddress.Parse(Constants.localIpAddress);
            TcpListener listener = new TcpListener(localAddr, Common.Constants.PortDeviceRegulator);
            try
            {
                listener.Start();
            }
            catch
            {
                Console.WriteLine("Greska: REGULATOR VEC POSTOJI");
                System.Environment.Exit(502);
            }

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                NetworkStream stream = client.GetStream();

                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string request = Encoding.UTF8.GetString(data, 0, bytes);

                //parts[0] sadrzi id uredjaja, parts[1] sadrzi temperaturu uredjaja
                string[] parts = request.Split(",");

                if (temperatures.ContainsKey(Int32.Parse(parts[0])))
                    Log(String.Format("Pristigla temperatura '{0:0.00}' od uredjaja '{1}'", double.Parse(parts[1]), Int32.Parse(parts[0])));
                else
                    Log(String.Format("Uredjaj '{1}' je dodat u sistem sa temperaturom '{0:0.00}'", double.Parse(parts[1]), Int32.Parse(parts[0])));

                temperatures[Int32.Parse(parts[0])] = double.Parse(parts[1]);
                temperaturesLastReadingTime[Int32.Parse(parts[0])] = DateTime.Now;

                if (!isRegulatorInManualMode)
                    regulate();

                client.Close();
            }

        }

        public void regulate()
        {
            double avgTemp = 0;
            int numOfReadings = 0;
            foreach (var tmp in temperatures)
            {
                avgTemp += tmp.Value;
                numOfReadings++;
            }

            //Obezbedjuje da regulator ne izvrsava svoj zadatak dok nema 4 uredjaja
            if (numOfReadings < 4)
                return;
            if (!working)
            {
                Console.WriteLine("Regulator je poceo sa radom...");
                Console.Write("> ");
                working = true;
                Log("Regulator je poceo sa radom!");
            }
            avgTemp = avgTemp / numOfReadings;

            // Provera da li je potrebno upaliti ili ugasiti peć
            int currentHour = DateTime.Now.Hour;
            if (currentHour >= dnevniPocetak && currentHour < dnevniKraj)
            {
                //trenutno je dan
                if (avgTemp < dayTemperature)
                    nextCommand = Enums.Command.TurnOn;
                //Ne gasi se cim dostigne zeljenu temp zato sto bi se u tom slucaju stalno palila/gasila
                else if (avgTemp >= (dayTemperature + Common.Constants.TempRegulatorTempGap) && previousCommand != Enums.Command.TurnOff)
                    nextCommand = Enums.Command.TurnOff;
            }
            else
            {
                //trenutno je noc
                if (avgTemp < nightTemperature)
                    nextCommand = Enums.Command.TurnOn;
                else if (avgTemp >= (nightTemperature + Common.Constants.TempRegulatorTempGap) && previousCommand != Enums.Command.TurnOff)
                    nextCommand = Enums.Command.TurnOff;
            }

            try
            {
                sendCommand(nextCommand);
            }
            catch
            {
                Console.WriteLine("GRESKA: Grejac nije pokrenut!");
                Console.WriteLine("Nakon paljenja grejaca ponovo pokrenuti regulator!");
                System.Environment.Exit(503);
            }


            if (previousCommand != nextCommand && temperatures.Count != 0)
            {
                foreach (var tmp in temperatures)
                {
                    try
                    {
                        sendMessageToDevice(nextCommand, Common.Constants.PortRegulatorDevice + tmp.Key);

                    }
                    catch
                    {
                        //U zadatku nije naglaseno sta raditi ukoliko u sistemu bude manje od 4 uredjaja
                        //u jednom trenutku, u catchu se moze izmeniti desavanje po potrebi.
                        Log("Uredjaj '" + tmp.Key + "' je pokvaren.");
                    }
                }
                if (nextCommand == Enums.Command.TurnOn)
                    Log("Poslata poruka svim uredjajima da je grejac UPALJEN!");
                else if (nextCommand == Enums.Command.TurnOff)
                    Log("Poslata poruka svim uredjajima da je grejac UGASEN!");

            }
            previousCommand = nextCommand;
        }


        //Salje centralnoj peci znak da se ukljuci/iskljuci
        public void sendCommand(Common.Enums.Command komanda)
        {
            //Konektovanje na server (CentralHeater)
            TcpClient client = new TcpClient("localhost", Common.Constants.PortRegulatorHeater);
            NetworkStream stream = client.GetStream();

            //Slanje poruke sa komandom heateru
            string message = komanda.ToString();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            client.Close();
        }

        public void sendMessageToDevice(Common.Enums.Command komanda, int port)
        {
            TcpClient client = new TcpClient("localhost", port);
            NetworkStream stream = client.GetStream();

            string message = "";
            if (komanda == Enums.Command.TurnOn)
                message = "upaljena";
            else if (komanda == Enums.Command.TurnOff)
                message = "ugasena";
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            client.Close();
        }

        public void unosPodataka()
        {
            string input;

            int dayTimeStart;           //Vreme od kojeg pocinje dnevni rezim
            int dayTimeEnd;             //Vreme do kojeg traje dnevni rezim
            int temperaturaDnevnog;     // Temperatura dnevnog rezima
            int temperaturaNocnog;      //Temperatura nocnog rezima



            while (true)
            {
                Console.Write("Vreme pocetka dnevnog rezima: ");
                input = Console.ReadLine();
                if (Int32.TryParse(input, out dayTimeStart) != true)
                {
                    continue;
                }
                if (dayTimeStart <= 23 && dayTimeStart >= 0)
                {
                    break;
                }
            }

            dnevniPocetak = dayTimeStart;

            while (true)
            {
                Console.Write("Vreme kraja dnevnog rezima: ");
                input = Console.ReadLine();
                if (Int32.TryParse(input, out dayTimeEnd) != true)
                {
                    continue;
                }
                if (dayTimeEnd <= 23 && dayTimeEnd >= 0)
                {
                    break;
                }
            }

            dnevniKraj = dayTimeEnd;

            while (true)
            {
                Console.Write("Temperatura dnevnog rezima: ");
                input = Console.ReadLine();
                if (Int32.TryParse(input, out temperaturaDnevnog) != true)
                {
                    continue;
                }
                if (temperaturaDnevnog >= 0 && temperaturaDnevnog <= 35)
                {
                    break;
                }
            }

            SetDayTemperature(temperaturaDnevnog);

            while (true)
            {
                Console.Write("Temperatura nocnog rezima: ");
                input = Console.ReadLine();
                if (Int32.TryParse(input, out temperaturaNocnog) != true)
                {
                    continue;
                }
                if (temperaturaNocnog >= 0 && temperaturaNocnog <= 35)
                {
                    break;
                }
            }

            SetNightTemperature(temperaturaNocnog);


            Console.Clear();
            Console.WriteLine("Regulator");
            Console.WriteLine("DNEVNI REZIM: " + dayTimeStart + ":00 - " + dayTimeEnd + ":00 Temp: " + temperaturaDnevnog);
            Console.WriteLine("NOCNI REZIM:  " + dayTimeEnd + ":00 - " + dayTimeStart + ":00 Temp: " + temperaturaNocnog);
            Console.Write("Regulator je u modu: ");
            if (isRegulatorInManualMode)
                Console.WriteLine("MANUELNI");
            else
                Console.WriteLine("AUTOMATSKI");
        }

        public void Log(string logMessage)
        {
            using (StreamWriter writer = new StreamWriter("regulatorLog.txt", true))
            {
                writer.WriteLine(DateTime.Now.ToLongDateString() + " | " + DateTime.Now.ToString("H:mm:ss") + " | " + logMessage);
                writer.WriteLine("-------------------------------------");
            }
        }
        public void StartHttpServer()
        {
            Task.Run(() =>
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:5001/status/");
                listener.Start();

                Console.WriteLine("HTTP API server pokrenut na http://localhost:5001/status/");

                while (true)
                {
                    var context = listener.GetContext();
                    var response = context.Response;
                    var request = context.Request;

                    if (request.HttpMethod == "OPTIONS")
                    {
                        response.StatusCode = 204;
                        response.AddHeader("Access-Control-Allow-Origin", "*");
                        response.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");
                        response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                        response.OutputStream.Close();
                        continue;
                    }

                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    response.ContentType = "application/json";

                    var safeTemperatures = this.temperatures.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);

                    // Prepare data
                    var data = new
                    {
                        dayTemperature = this.dayTemperature,
                        nightTemperature = this.nightTemperature,
                        dnevniPocetak = this.dnevniPocetak,
                        dnevniKraj = this.dnevniKraj,
                        temperatures = safeTemperatures,
                        previousCommand = this.previousCommand.ToString(),
                        nextCommand = this.nextCommand.ToString()
                    };

                    string json = JsonSerializer.Serialize(data);

                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
            });
        }
        
        public void ChangeDefaultTimes()
        {
            string input;
            int dayTimeStart;
            int dayTimeEnd;

            while (true)
            {
                Console.Write("Vreme pocetka dnevnog rezima: ");
                input = Console.ReadLine();
                if (Int32.TryParse(input, out dayTimeStart) != true)
                {
                    continue;
                }
                if (dayTimeStart <= 23 && dayTimeStart >= 0)
                {
                    break;
                }
            }


            while (true)
            {
                Console.Write("Vreme kraja dnevnog rezima: ");
                input = Console.ReadLine();
                if (Int32.TryParse(input, out dayTimeEnd) != true)
                {
                    continue;
                }
                if (dayTimeEnd <= 23 && dayTimeEnd >= 0)
                {
                    break;
                }
            }

            dnevniPocetak = dayTimeStart;
            dnevniKraj = dayTimeEnd;

            Console.Clear();
            Console.WriteLine("Regulator");
            Console.WriteLine("DNEVNI REZIM: " + dnevniPocetak + ":00 - " + dnevniKraj + ":00 Temp: " + dayTemperature);
            Console.WriteLine("NOCNI REZIM:  " + dnevniKraj + ":00 - " + dnevniPocetak + ":00 Temp: " + nightTemperature);
            Console.Write("Regulator je u modu: ");
            if (isRegulatorInManualMode)
                Console.WriteLine("MANUELNI");
            else
                Console.WriteLine("AUTOMATSKI");
        }

        public void ChangeDefaultTemperatures()
        {
            int dayTemperature;
            int nightTemperature;
            string input;

            while (true)
            {
                Console.Write("Temperatura dnevnog rezima: ");
                input = Console.ReadLine();
                if (Int32.TryParse(input, out dayTemperature) != true)
                {
                    continue;
                }
                if (dayTemperature >= 0 && dayTemperature <= 35)
                {
                    break;
                }
            }


            while (true)
            {
                Console.Write("Temperatura nocnog rezima: ");
                input = Console.ReadLine();
                if (Int32.TryParse(input, out nightTemperature) != true)
                {
                    continue;
                }
                if (nightTemperature >= 0 && nightTemperature <= 35)
                {
                    break;
                }
            }

            SetDayTemperature(dayTemperature);
            SetNightTemperature(nightTemperature);

            Console.Clear();
            Console.WriteLine("Regulator");
            Console.WriteLine("DNEVNI REZIM: " + dnevniPocetak + ":00 - " + dnevniKraj + ":00 Temp: " + dayTemperature);
            Console.WriteLine("NOCNI REZIM:  " + dnevniKraj + ":00 - " + dnevniPocetak + ":00 Temp: " + nightTemperature);
            Console.Write("Regulator je u modu: ");
            if (isRegulatorInManualMode)
                Console.WriteLine("MANUELNI");
            else
                Console.WriteLine("AUTOMATSKI");
        }

        public void ChangeRegulatorMode()
        {
            isRegulatorInManualMode = !isRegulatorInManualMode;

            if (isRegulatorInManualMode)
                Log("Regulator prebacen u manuelni mod.");
            else
                Log("Regulator prebacen u automatski mod.");

            Console.Clear();
            Console.WriteLine("Regulator");
            Console.WriteLine("DNEVNI REZIM: " + dnevniPocetak + ":00 - " + dnevniKraj + ":00 Temp: " + dayTemperature);
            Console.WriteLine("NOCNI REZIM:  " + dnevniKraj + ":00 - " + dnevniPocetak + ":00 Temp: " + nightTemperature);
            Console.Write("Regulator je u modu: ");
            if (isRegulatorInManualMode)
                Console.WriteLine("MANUELNI");
            else
                Console.WriteLine("AUTOMATSKI");
        }

        public void TurnHeaterOn()
        {
            if (!isRegulatorInManualMode)
            {
                Console.WriteLine("GRESKA: Regulator nije u manuelnom modu.");
                return;
            }

            Log("Pokrenuto paljenje uredjaja u manuelnom modu...");

            nextCommand = Enums.Command.TurnOn;

            try
            {
                sendCommand(nextCommand);
            }
            catch
            {
                Console.WriteLine("GRESKA: Grejac nije pokrenut!");
                Console.WriteLine("Nakon paljenja grejaca ponovo pokrenuti regulator!");
                System.Environment.Exit(503);
            }


            if (previousCommand != nextCommand && temperatures.Count != 0)
            {
                foreach (var tmp in temperatures)
                {
                    try
                    {
                        sendMessageToDevice(nextCommand, Common.Constants.PortRegulatorDevice + tmp.Key);

                    }
                    catch
                    {
                        //U zadatku nije naglaseno sta raditi ukoliko u sistemu bude manje od 4 uredjaja
                        //u jednom trenutku, u catchu se moze izmeniti desavanje po potrebi.
                        Log("Uredjaj '" + tmp.Key + "' je pokvaren.");
                    }
                }
                if (nextCommand == Enums.Command.TurnOn)
                    Log("Poslata poruka svim uredjajima da je grejac UPALJEN!");
                else if (nextCommand == Enums.Command.TurnOff)
                    Log("Poslata poruka svim uredjajima da je grejac UGASEN!");

            }
            previousCommand = nextCommand;
        }

        public void TurnHeaterOff()
        {
            if (!isRegulatorInManualMode)
            {
                Console.WriteLine("GRESKA: Regulator nije u manuelnom modu.");
                return;
            }

            Log("Pokrenuto gasenje uredjaja u manuelnom modu...");

            nextCommand = Enums.Command.TurnOff;

            try
            {
                sendCommand(nextCommand);
            }
            catch
            {
                Console.WriteLine("GRESKA: Grejac nije pokrenut!");
                Console.WriteLine("Nakon paljenja grejaca ponovo pokrenuti regulator!");
                System.Environment.Exit(503);
            }


            if (previousCommand != nextCommand && temperatures.Count != 0)
            {
                foreach (var tmp in temperatures)
                {
                    try
                    {
                        sendMessageToDevice(nextCommand, Common.Constants.PortRegulatorDevice + tmp.Key);

                    }
                    catch
                    {
                        //U zadatku nije naglaseno sta raditi ukoliko u sistemu bude manje od 4 uredjaja
                        //u jednom trenutku, u catchu se moze izmeniti desavanje po potrebi.
                        Log("Uredjaj '" + tmp.Key + "' je pokvaren.");
                    }
                }
                if (nextCommand == Enums.Command.TurnOn)
                    Log("Poslata poruka svim uredjajima da je grejac UPALJEN!");
                else if (nextCommand == Enums.Command.TurnOff)
                    Log("Poslata poruka svim uredjajima da je grejac UGASEN!");

            }
            previousCommand = nextCommand;
        }

        public void ShowTemperatureReadings()
        {
            if (temperatures.Count > 0)
            {
                Console.WriteLine("Poslednje izmerene temperature ");
                foreach (var temp in temperatures)
                    Console.WriteLine($"Uredjaj {temp.Key}: {temp.Value:F2} °C. Vreme merenja {temperaturesLastReadingTime[temp.Key]:H:mm}");
            }
            else
            {
                Console.WriteLine("Trenutno nema registrovanih uredjaja u sistemu");
            }
        }

        public void CheckForReadingDeviceAvailability()
        {
            while (true)
            {
                List<Int32> keysToRemove = new List<Int32>();

                foreach (var temp in temperaturesLastReadingTime)
                {
                    if ((DateTime.Now - temp.Value).TotalSeconds > Common.Constants.ReadingDeviceCheckTime * 2.1)
                        keysToRemove.Add(temp.Key);
                }

                foreach (var id in keysToRemove)
                {
                    temperatures.TryRemove(id, out _);
                    temperaturesLastReadingTime.TryRemove(id, out _);
                }

                Thread.Sleep(10000);
            }
        }

        public void ShowHelpDialog()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("pomoc");
            Console.ResetColor();
            Console.WriteLine(" - Za prikaz i objasnjenje mogucih komandi temperaturnog regulatora.");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("merenja");
            Console.ResetColor();
            Console.WriteLine(" - Prikazuje poslednje izmerene temperature za svaki uredjaj.");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("promeni_mod");
            Console.ResetColor();
            Console.WriteLine(" - Menja mod regulatora iz automatskog u manuelni i obrnuto.");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("upali_pec");
            Console.ResetColor();
            Console.WriteLine(" - Pali pec ukoliko je regulator u manuelnom modu.");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("ugasi_pec");
            Console.ResetColor();
            Console.WriteLine(" - Gasi pec ukoliko je regulator u manuelnom modu.");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("promeni_vremena_rezima");
            Console.ResetColor();
            Console.WriteLine(" - Dialog za promenu vremena za koje se primenjuje dnevna/nocna temperatura.");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("promeni_podrazumevane_temperature");
            Console.ResetColor();
            Console.WriteLine(" - Dialog za promenu podrazumevane dnevne i nocne temperature.");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("izlaz");
            Console.ResetColor();
            Console.WriteLine(" - Za prekidanje rada temperaturnog regulatora koristiti Ctrl+C.");

            //Console.WriteLine();
        }

        public void CommandLine()
        {
            string command;
            while (true)
            {
                Console.WriteLine();
                Console.Write("> ");
                command = Console.ReadLine();

                switch (command)
                {
                    case "pomoc":
                        ShowHelpDialog();
                        break;
                    case "merenja":
                        ShowTemperatureReadings();
                        break;
                    case "promeni_mod":
                        ChangeRegulatorMode();
                        break;
                    case "upali_pec":
                        TurnHeaterOn();
                        break;
                    case "ugasi_pec":
                        TurnHeaterOff();
                        break;
                    case "promeni_vremena_rezima":
                        ChangeDefaultTimes();
                        break;
                    case "promeni_podrazumevane_temperature":
                        ChangeDefaultTemperatures();
                        break;
                    case "izlaz":
                        return;
                    default:
                        Console.WriteLine("Nepoznata komanda: " + command + ". Koristite 'pomoc' kako bi ste videli listu svih komandi.");
                        break;
                }
            }
        }


    }
    

    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("REGULATOR");

            TemperatureRegulator tr = new TemperatureRegulator();
            tr.unosPodataka();

            tr.StartHttpServer();

            //false kako bi se obrisali prethodni podaci iz txt fajla
            using (StreamWriter writer = new StreamWriter("regulatorLog.txt", false))
            {
                writer.WriteLine("\t\t\t--------------------------------------");
                writer.WriteLine("\t\t\t|DNEVNI REZIM: " + tr.dnevniPocetak + ":00 - " + tr.dnevniKraj + ":00 Temp: " + tr.dayTemperature + "|");
                writer.WriteLine("\t\t\t|NOCNI REZIM:  " + tr.dnevniKraj + ":00 - " + tr.dnevniPocetak + ":00 Temp: " + tr.nightTemperature + "|");
                writer.WriteLine("\t\t\t--------------------------------------");
                writer.WriteLine("\tDATUM I VREME DOGADJAJA\t\t\t\tDOGADJAJ");
                writer.WriteLine("--------------------------------------------------------------------------------------------");
            }

            Thread t1 = new Thread(tr.receiveTemperature);
            Thread t2 = new Thread(tr.CheckForReadingDeviceAvailability);

            t1.Start();
            t2.Start();

            Thread.Sleep(2000);
            tr.CommandLine();

            Console.ReadLine();
        }
    }



}