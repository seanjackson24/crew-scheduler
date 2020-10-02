using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrewScheduler.Models;

namespace CrewScheduler.Services
{
	public interface IFileService
	{
		Task<IEnumerable<PilotScheduleInfo>> GetPilotSchedulesForDay(DateTime day);
		Task<IEnumerable<PilotWorkSchedule>> GetPilotWorkSchedules();
		Task Update(IEnumerable<PilotScheduleInfo> pilotSchedules);
	}
	public class FileService : IFileService
	{
		public async Task Update(IEnumerable<PilotScheduleInfo> pilotSchedules)
		{

		}

		public async Task<IEnumerable<PilotScheduleInfo>> GetPilotSchedulesForDay(DateTime day)
		{
			return null;
		}

		public async Task<IEnumerable<PilotWorkSchedule>> GetPilotWorkSchedules()
		{
			return null;
		}
	}
}