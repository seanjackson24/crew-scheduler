using System;
using System.ComponentModel.DataAnnotations;

namespace CrewScheduler.Models
{
	public class PilotScheduleRequest
	{
		public string Location { get; set; }
		public DateTime DepartureDateTime { get; set; }
		public DateTime ReturnDateTime { get; set; }
	}
}
