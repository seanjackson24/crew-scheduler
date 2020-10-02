using System;

namespace CrewScheduler.Models
{
	public class PilotWorkSchedule
	{
		public int PilotId { get; set; }
		public string Name { get; set; }
		public Base Base { get; set; }
		public DayOfWeek[] WorkDays { get; set; }
	}
}