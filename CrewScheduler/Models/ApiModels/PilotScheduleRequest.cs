using System;

namespace CrewScheduler.Models.ApiModels
{
	public class PilotScheduleRequest
	{
		public string Location { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime ReturnDateTime { get; set; }
	}
}
