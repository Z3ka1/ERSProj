using System;
namespace Common
{
	public interface IReadingDevice
	{
		public int Id { get; set; }
		public double Temperature { get; set; }


        public void sendTemperature();
        public void raiseTemperature();


    }
}

