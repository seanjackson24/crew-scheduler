using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrewScheduler.Models.DomainModels
{
	public class PilotWorkSchedule
	{
		[JsonPropertyName("ID")]
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