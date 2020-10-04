using System;

namespace CrewScheduler.Models.DomainModels
{
	public class PilotScheduleInfo
	{
		public int PilotId { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime ReturnDateTime { get; set; }
		public string ReservationKey { get; set; }
		public DateTime ReservationTime { get; set; }
		public bool IsConfirmed { get; set; }
	}
}