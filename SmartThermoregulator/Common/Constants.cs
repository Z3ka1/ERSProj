using System;

namespace Common
{
	public static class Constants
	{
		//Broj sekundi na koje Reading Device proverava temperaturu
		public const int ReadingDeviceCheckTime = 10; //seconds

		//Broj sekundi za koje se temperatura menja
		public const int ReadingDeviceTempChangeTime = 5; //seconds

		//Broj za koji se menja temperatura kada je grejac ukljucen
		public const double ReadingDeviceTempChange = 0.1; //Celsius

		//Broj koji oznacava dozvoljeno odstupanje od zeljene temperature pri gasenju peci
		//Kako se pec ne bi stalno palila i gasila
		public const double TempRegulatorTempGap = 1; //Celsius

		//Lokalna IP adresa
		public const string localIpAddress = "127.0.0.1";

		//Port za komunikaciju izmedju uredjaja i regulatora (Gde uredjaj salje regulatoru trenutnu temperaturu)
		//(Port servera na TemperatureRegulator-u)
		public const int PortDeviceRegulator = 21000;

		//Port + Id Ukoliko saljemo specificnom uredjaju neku poruku
		//Port za komunikaciju izmedju regulatora i uredjaja (Gde regulator salje poruku da je grejanje zapoceto)
		//(Port servera na ReadingDevice-u)
		public const int PortRegulatorDevice = 21001;

		//Port za komunikaciju izmedju regulatora i grejaca (Gde regulator komanduje sa grejacem)
		//(Port servera na CentralHeater-u)
		public const int PortRegulatorHeater = 22000;
	}
}

