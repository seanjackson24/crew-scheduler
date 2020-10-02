using System;

namespace CrewScheduler.Models
{
	public class Pilot
	{
		public int PilotId { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime ReturnDateTime { get; set; }
	}
}