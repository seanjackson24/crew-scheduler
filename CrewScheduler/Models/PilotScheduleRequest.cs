using System;

namespace CrewScheduler.Models
{
	public class PilotScheduleRequest
	{
		public Base Location { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime ReturnDateTime { get; set; }
	}
}
