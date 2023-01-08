using System;
namespace Common
{
	public interface ITemperatureRegulator
	{


        public void SetDayTemperature(int temperature);

        public void SetNightTemperature(int temperature);


        public int GetTemperature(int hour);

        public void receiveTemperature();
        public void regulate();
        public void sendCommand(Common.Enums.Command komanda);
        public void sendMessageToDevice(Common.Enums.Command komanda, int port);
        public void unosPodataka();
    }
}

