using System.Linq;
using System.Threading.Tasks;
using CrewScheduler.Models;

namespace CrewScheduler.Services
{
	public class PilotService
	{
		private readonly IFileService _fileService;

		public PilotService(IFileService fileService)
		{
			this._fileService = fileService;
		}

		public async Task<int?> GetNextAvailablePilot(PilotScheduleRequest request)
		{
			var date = request.DepartureDateTime.DayOfWeek;

			var workSchedules = (await _fileService.GetPilotWorkSchedules()).Where(p => p.Base == request.Location && p.WorkDays.Any(d => d == date));
			if (!workSchedules.Any())
			{
				return null;
			}

			var pilots = await _fileService.GetPilotSchedulesForDay(request.DepartureDateTime.Date);
			var unAvailable = pilots.Where(p =>
				!((p.DepartureDateTime < request.DepartureDateTime && p.ReturnDateTime < request.DepartureDateTime)
				||
				(p.DepartureDateTime > request.ReturnDateTime))
				)
				.Select(p => p.PilotId);

			var availablePilots = workSchedules.Where(ws => !unAvailable.Contains(ws.PilotId)).Select(p => p.PilotId);
			if (!availablePilots.Any())
				return null;
			var pilotsBySchedule = pilots.GroupBy(p => p.PilotId, (id, grp) => grp.Count()).ToDictionary(i => i, g => g);

			return availablePilots.OrderBy(p => pilotsBySchedule.TryGetValue(p, out var count) ? count : 0).FirstOrDefault();
		}
	}
}
