using System;
namespace Common
{
	public interface ITemperatureRegulator
	{
        public void SetDayTemperature(int temperature);

        public void SetNightTemperature(int temperature);


        public int GetTemperature(int hour);

        public void receiveTemperature();
    }
}

