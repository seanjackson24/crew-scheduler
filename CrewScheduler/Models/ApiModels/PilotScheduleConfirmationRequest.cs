using System;

namespace CrewScheduler.Models.ApiModels
{
	public class PilotScheduleConfirmationRequest
	{
		public string Location { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime ReturnDateTime { get; set; }
		public int PilotId { get; set; }
		public string ReservationKey { get; set; }
	}
}
