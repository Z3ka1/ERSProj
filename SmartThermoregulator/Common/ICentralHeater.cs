using System;
namespace Common
{
	public interface ICentralHeater
	{
        public void OnCommandReceived(string command);

        public TimeSpan GetRunTime();
    }
}

