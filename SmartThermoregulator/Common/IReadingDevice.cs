using System;
namespace Common
{
	public interface IReadingDevice
	{
		public int Id { get; set; }
		public double Temperature { get; set; }
		public bool HeaterIsOn { get; set; }

		public void getHeaterState();
		public void initialize();
        public void sendTemperature();
        public void regulateTemperature();
		public void receiveStateHeater();
		public void updateUI();

    }
}

