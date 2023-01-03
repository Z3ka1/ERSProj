using System;
using System.ComponentModel.DataAnnotations;

namespace EFDataBase
{
	public class InfoCentralHeater
	{
		[Key]
		public int InfoId { get; set; }
		public TimeSpan RunTime { get; set; }
		public DateTime StartTime { get; set; }
		public double ResourcesSpent { get; set; }

	}
}

