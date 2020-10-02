using System;

namespace CrewScheduler.Models
{
	public class PilotScheduleConfirmation
	{
		public string Location { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime ReturnDateTime { get; set; }
		public int PilotId { get; set; }
	}
}
