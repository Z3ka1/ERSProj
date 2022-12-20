using System;

namespace Common
{
	public static class Constants
	{
		//Broj sekundi na koje Reading Device proverava temperaturu
		public const int ReadingDeviceCheckTime = 180; //seconds

		//Broj sekundi za koje se temperatura menja
		public const int ReadingDeviceTempChangeTime = 120; //seconds

		//Broj za koji se menja temperatura kada je grejac ukljucen
		public const double ReadingDeviceTempChange = 0.01; //Celsius

		//Lokalna IP adresa
		public const string localIpAddress = "127.0.0.1";

		//Port za komunikaciju izmedju uredjaja i regulatora (Gde uredjaj salje regulatoru trenutnu temperaturu)
		public const int PortDeviceRegulator = 21000;

		//Port za komunikaciju izmedju regulatora i uredjaja (Gde regulator salje poruku da je grejanje zapoceto)
		public const int PortRegulatorDevice = 21001;
	}
}

