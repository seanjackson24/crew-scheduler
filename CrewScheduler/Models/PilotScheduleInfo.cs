using System;

namespace CrewScheduler.Models
{
	// move to domain
	public class PilotScheduleInfo
	{
		public int PilotId { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime ReturnDateTime { get; set; }
	}
}