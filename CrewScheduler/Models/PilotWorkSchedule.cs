using System;
using System.Collections.Generic;

namespace CrewScheduler.Models
{
	public class PilotWorkSchedule
	{
		public int PilotId { get; set; }
		public string Name { get; set; }
		public string Base { get; set; }
		public IList<DayOfWeek> WorkDays { get; set; }
	}

	public class CrewScheduleInfo
	{
		public IEnumerable<PilotWorkSchedule> Crew
		{
			get; set;
		}
	}
}