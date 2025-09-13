using System;
using Common;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace ReadingDevice
{
    public class ReadingDevice : IReadingDevice
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public bool HeaterIsOn { get; set; }

        //Koristi se samo za UI
        private string lastState;
        
        private List<(DateTime time, double Temp)> tempHistory = new List<(DateTime, double)>();
        

        public ReadingDevice()
        {
            lastState = "";
            
        }

        //Koristi se pri pozivanju konstruktora i dobija se od Heatera
        public void getHeaterState()
        {
            TcpClient client = new TcpClient("localhost", Common.Constants.PortHeaterDevice);
            NetworkStream stream = client.GetStream();

            byte[] data = new byte[256];
            int bytesRead = stream.Read(data, 0, data.Length);
            string state = Encoding.UTF8.GetString(data, 0, bytesRead);

            if (state.Equals("True"))
            {
                HeaterIsOn = true;
            }
            else
            {
                HeaterIsOn = false;
            }
            client.Close();
        }

        public void initialize()
        {
            while (true)
            {
                Console.WriteLine("Unesite ID:");
                if (Int32.TryParse(Console.ReadLine(), out int id) && id >= 1 && id <= 999)
                {
                    Id = id;

                    //Provera da li je uneti ID jedinstven
                    //Vrsi se tako sto pokusavamo da startujemo server na portu koji je potencijalno zauzet
                    //Mozda nije optimalno ali radi :)
                    try
                    {
                        IPAddress localAddr = IPAddress.Parse(Constants.localIpAddress);
                        TcpListener listener = new TcpListener(localAddr, Common.Constants.PortRegulatorDevice + Id);
                        listener.Start();
                        listener.Stop();
                    }
                    catch
                    {
                        Console.WriteLine("Uneti ID je vec u upotrebi");
                        continue;
                    }
                    break;
                }
            }

            while (true)
            {
                Console.WriteLine("Unesite inicijalnu temperaturu:");
                if (double.TryParse(Console.ReadLine(), out double temp) && temp >= 0 && temp <= 35)
                {
                    Temperature = temp;
                    break;
                }
            }
        }

        //Salje temperaturu regulatoru
        public void sendTemperature()
        {
            //Konektovanje na server (Regulator)
            TcpClient client = new TcpClient("localhost", Common.Constants.PortDeviceRegulator);
            NetworkStream stream = client.GetStream();

            //Slanje poruke sa temperaturom regulatoru
            string message = Id.ToString() + "," + Temperature.ToString();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            //Console.WriteLine("Temperatura poslata " + Temperature);

            client.Close();
        }

        //Koristi se pri promeni stanja grejaca i dobija se od Regulatora
        public void receiveStateHeater()
        {
           
            IPAddress localAddr = IPAddress.Parse(Constants.localIpAddress);
            TcpListener listener = new TcpListener(localAddr, Common.Constants.PortRegulatorDevice + Id);
            listener.Start();
            

            while (true)
            {

                TcpClient client = listener.AcceptTcpClient();

                NetworkStream stream = client.GetStream();
                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string request = Encoding.UTF8.GetString(data, 0, bytes);


                if (request == "upaljena")
                {
                    lastState = String.Format("{0,-25} {1,-10}", "Grijac je poceo sa radom. ", DateTime.Now.ToString("H:mm:ss"));
                    //Console.WriteLine("{0,-25} {1,-10}", "Grijac je poceo sa radom. ", DateTime.Now.ToString("H:mm:ss"));
                    updateUI();
                    HeaterIsOn = true;
                }
                else if(request == "ugasena")
                {
                    lastState = String.Format("{0,-25}  {1,-10}", "Grijac je zaustavio rad. ", DateTime.Now.ToString("H:mm:ss"));
                    //Console.WriteLine("{0,-25}  {1,-10}", "Grijac je zaustavio rad. ", DateTime.Now.ToString("H:mm:ss"));
                    updateUI();
                    HeaterIsOn = false;
                }
                client.Close();
            }

        }

        //Povecava/smanjuje temperaturu ukoliko je grejac ukljucen/iskljucen
        public void regulateTemperature()
        {
            while(true)
            {
                Thread.Sleep(Common.Constants.ReadingDeviceTempChangeTime * 1000);
                if(HeaterIsOn)
                    Temperature += Common.Constants.ReadingDeviceTempChange;
                else
                    Temperature -= Common.Constants.ReadingDeviceTempChange;

                lock (tempHistory)
                {
                    tempHistory.Add((DateTime.Now, Temperature));
                    if (tempHistory.Count > Common.Constants.MaxGraphHistory)
                        tempHistory.RemoveAt(0);
                }

                updateUI();
            }

        }

        public void updateUI()
        {
            Console.Clear();
            Console.WriteLine("Uredjaj za merenje ID: " + Id);
            Console.WriteLine("Temperatura: {0:0.00}", Temperature);
            Console.WriteLine("{0,-25}  {1,-10}", "OBAVESTENJE", "VREME");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine(lastState);

            Console.WriteLine();
            Console.WriteLine();

            lock (tempHistory)
            {
                if (tempHistory.Count < 2)
                {
                    Console.WriteLine("Nema podataka za grafik.");
                    return;
                }

                int graphHeight = 22;
                int graphWidth = tempHistory.Count;

                double minTemp = tempHistory.Min(t => t.Temp);
                double maxTemp = tempHistory.Max(t => t.Temp);
                double range = maxTemp - minTemp;
                if (range < 1) range = 1; // minimum range

                int[] barHeights = new int[graphWidth];
                for (int i = 0; i < graphWidth; i++)
                {
                    double t = tempHistory[i].Temp;

                    if (t < minTemp) t = minTemp;
                    if (t > maxTemp) t = maxTemp;

                    barHeights[i] = (int)Math.Round((t - minTemp) / range * graphHeight);
                    //if (barHeights[i] == 0) barHeights[i] = 1;
                }

                int labelWidth = 6;

                for (int row = graphHeight; row >= 0; row--)
                {
                    if (row == 0)
                        Console.Write(" ".PadLeft(labelWidth + 1));
                    else
                    {
                        double tempLabel = minTemp + ((double)(row - 1) / graphHeight) * range;
                        string tempLabelStr = tempLabel.ToString("0.0").PadLeft(labelWidth - 1) + " ";
                        Console.Write(tempLabelStr);
                    }

                    for (int col = 0; col < graphWidth; col++)
                    {
                        int fullBlocks = barHeights[col];  // integer bar height

                        if (row <= fullBlocks && row != 0)
                        {
                            // Full block rows
                            Console.Write("  ███  ");
                        }
                        else if (row == fullBlocks + 1 && row != 0)
                        {
                            // Half block row, always one above full blocks
                            Console.Write("  ▄▄▄  ");
                        }
                        else if (row == 0)
                        {
                            Console.Write("_______");
                        }
                        else
                        {
                            Console.Write("       ");
                        }

                        // if (barHeights[col] >= row && row != 0)
                        //     Console.Write("  ███  ");
                        // else if (row == 0)
                        //     Console.Write("_______");
                        // else
                        //     Console.Write("       ");
                    }
                    Console.WriteLine();
                }

                Console.Write(" ".PadLeft(labelWidth));
                Console.Write(" ");
                for (int col = 0; col < graphWidth; col++)
                {
                    string timeLabel = tempHistory[col].time.ToString("HH:mm");
                    Console.Write(timeLabel + "  ");
                }
                Console.WriteLine();
            }
            
        }


        public override string ToString()
        {
                                                                    //Da bi se temp ispisala na 2 decimale
            string s = "Id: " + Id + " Temperature: " + Temperature.ToString("0.00"); 
            return s;
        }

        
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("UREDJAJ");
            ReadingDevice rd = new ReadingDevice();
            rd.initialize();
            try
            {
                rd.getHeaterState();
            }
            catch
            {
                rd.HeaterIsOn = false;
            }

            rd.updateUI();

            Thread t1 = new Thread(rd.receiveStateHeater);
            t1.Start();
            Thread t2 = new Thread(rd.regulateTemperature);
            t2.Start();

            while (true)
            {
                try
                {
                    rd.sendTemperature();
                    Thread.Sleep(Common.Constants.ReadingDeviceCheckTime * 1000);
                }
                catch
                {
                    Console.Clear();
                    rd.updateUI();
                    Console.WriteLine("Greska: REGULATOR NIJE POKRENUT");
                    System.Environment.Exit(501);
                }
            }

        }

    }
}

