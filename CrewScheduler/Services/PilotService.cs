using System;
using System.Linq;
using System.Threading.Tasks;
using CrewScheduler.Models.ApiModels;
using CrewScheduler.Models.DomainModels;

namespace CrewScheduler.Services
{
	public interface IPilotService
	{
		Task<GetNextAvailablePilotResponse> GetNextAvailablePilot(PilotScheduleRequest request);
		Task<bool> ConfirmPilotSchedule(PilotScheduleConfirmationRequest request);
	}

	public class PilotService : IPilotService
	{
		private readonly IFileService _fileService;
		private readonly ITimeProvider _timeProvider;

		public PilotService(IFileService fileService, ITimeProvider timeProvider)
		{
			this._fileService = fileService;
			_timeProvider = timeProvider;
		}

		public async Task<bool> ConfirmPilotSchedule(PilotScheduleConfirmationRequest request)
		{
			var reservationExpiry = _timeProvider.UtcNow().AddMinutes(-10);

			var pilotSchedules = await _fileService.GetPilotSchedulesForDay(request.DepartureDateTime.Date);
			var schedule = pilotSchedules.FirstOrDefault(p => !p.IsConfirmed && p.ReservationKey == request.ReservationKey && p.PilotId == request.PilotId);
			if (schedule == null || schedule.ReservationTime < reservationExpiry) return false;
			schedule.IsConfirmed = true;
			await _fileService.UpdatePilotSchedule(schedule);
			return true;
		}

		private async Task PencilInPilotSchedule(int pilotId, string reservationKey, DateTime departureDateTime, DateTime returnDateTime)
		{
			var schedule = new PilotScheduleInfo()
			{
				PilotId = pilotId,
				ReservationKey = reservationKey,
				ReservationTime = _timeProvider.UtcNow(),
				DepartureDateTime = departureDateTime,
				ReturnDateTime = returnDateTime,
				IsConfirmed = false
			};
			await _fileService.AddPilotToSchedule(schedule);
		}

		public async Task<GetNextAvailablePilotResponse> GetNextAvailablePilot(PilotScheduleRequest request)
		{
			var nextAvailablePilot = (await GetAvailablePilots(request))?.FirstOrDefault();
			if (nextAvailablePilot == null)
			{
				return new GetNextAvailablePilotResponse(null, null);
			}
			string reservationKey = Guid.NewGuid().ToString();
			await PencilInPilotSchedule(nextAvailablePilot.Value, reservationKey, request.DepartureDateTime, request.ReturnDateTime);
			return new GetNextAvailablePilotResponse(nextAvailablePilot, reservationKey);
		}

		private async Task<IOrderedEnumerable<int>> GetAvailablePilots(PilotScheduleRequest request)
		{
			var reservationExpiry = _timeProvider.UtcNow().AddMinutes(-10);
			var date = request.DepartureDateTime.DayOfWeek;

			var workSchedules = (await _fileService.GetPilotWorkSchedules()).Where(p => p.Base == request.Location && p.WorkDays.Any(d => d == date));
			if (!workSchedules.Any())
			{
				return null;
			}

			var pilotSchedules = await _fileService.GetPilotSchedulesForDay(request.DepartureDateTime.Date);
			var unAvailable = pilotSchedules.Where(p =>
				(p.IsConfirmed || p.ReservationTime > reservationExpiry) &&
				!((p.DepartureDateTime < request.DepartureDateTime && p.ReturnDateTime < request.DepartureDateTime)
				||
				(p.DepartureDateTime > request.ReturnDateTime))
				)
				.Select(p => p.PilotId);

			var availablePilots = workSchedules.Where(ws => !unAvailable.Contains(ws.PilotId)).Select(p => p.PilotId);
			if (!availablePilots.Any())
				return null;
			var pilotsBySchedule = pilotSchedules.GroupBy(p => p.PilotId).ToDictionary(i => i.Key, g => g.Count());

			return availablePilots.OrderBy(p => pilotsBySchedule.TryGetValue(p, out var count) ? count : 0);
		}
	}
}
