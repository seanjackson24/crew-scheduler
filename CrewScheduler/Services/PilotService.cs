using System.Linq;
using System.Threading.Tasks;
using CrewScheduler.Models;

namespace CrewScheduler.Services
{
	public interface IPilotService
	{
		Task<int?> GetNextAvailablePilot(PilotScheduleRequest request);
		Task<bool> ConfirmPilotSchedule(PilotScheduleConfirmation request);
	}

	public class PilotService : IPilotService
	{
		private readonly IFileService _fileService;

		public PilotService(IFileService fileService)
		{
			this._fileService = fileService;
		}

		public async Task<bool> ConfirmPilotSchedule(PilotScheduleConfirmation request)
		{
			var scheduleRequest = new PilotScheduleRequest() { DepartureDateTime = request.DepartureDateTime, Location = request.Location, ReturnDateTime = request.ReturnDateTime };
			var availablePilots = await GetAvailablePilots(scheduleRequest);
			var pilot = availablePilots?.FirstOrDefault(id => request.PilotId == id);
			if (pilot == null) return false;
			var schedule = new PilotScheduleInfo()
			{
				PilotId = request.PilotId,
				DepartureDateTime = request.DepartureDateTime,
				ReturnDateTime = request.ReturnDateTime
			};
			await _fileService.AddPilotToSchedule(schedule);
			return true;
		}

		public async Task<int?> GetNextAvailablePilot(PilotScheduleRequest request)
		{
			return (await GetAvailablePilots(request))?.FirstOrDefault();
		}

		private async Task<IOrderedEnumerable<int>> GetAvailablePilots(PilotScheduleRequest request)
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

			return availablePilots.OrderBy(p => pilotsBySchedule.TryGetValue(p, out var count) ? count : 0);
		}
	}
}
