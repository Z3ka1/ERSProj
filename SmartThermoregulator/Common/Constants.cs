using System;

namespace Common
{
	public static class Constants
	{
		//Broj sekundi na koje Reading Device proverava i salje temperaturu
		public const int ReadingDeviceCheckTime = 18; //seconds (180)

		//Broj sekundi za koje se temperatura menja
		public const int ReadingDeviceTempChangeTime = 12; //seconds (120)

		//Broj za koji se menja temperatura kada je grejac ukljucen
		public const double ReadingDeviceTempChange = 0.1; //Celsius (0.01)

		//Broj koji oznacava dozvoljeno odstupanje od zeljene temperature pri gasenju peci
		//Kako se pec ne bi stalno palila i gasila
		public const double TempRegulatorTempGap = 1; //Celsius

		//Konstanta koja oznacava koliko pec trosi wati na sat vremena
		public const double CentralHeaterResourcesPerHour = 1500; //Watts

		//Konstanta koja oznacava koliko pec trosi wati pri paljenju
		public const double CentralHeaterResourcesOnTurnOn = 15000; //Watts


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

		//Port za slanje stanja grejaca (Gde grejac salje stanje uredjaju)
		//(Port servera na CentralHeater-u)
		public const int PortHeaterDevice = 23000;

		//Koliko poslednjih ocitavanja temperature prikazujemo na grafiku
		public const int MaxGraphHistory = 10;

	}
}

