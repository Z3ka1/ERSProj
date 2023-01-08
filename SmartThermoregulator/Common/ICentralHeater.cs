﻿using System;
namespace Common
{
	public interface ICentralHeater
	{
        //public void OnCommandReceived(string command);
        public void TurnOn();
        public void TurnOff();

        public bool IsOn();

        public TimeSpan GetRunTime();
        public void receiveCommand();
        public void waitNewDevice();
        public void ispisUI();

    }
}

