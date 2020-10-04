using System.Threading.Tasks;
using CrewScheduler.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CrewScheduler.Models.ApiModels;

namespace CrewScheduler.Controllers
{
    [ApiController]
	[Route("[controller]")]
	public class PilotController : ControllerBase
	{
		private readonly ILogger<PilotController> _logger;
		private readonly IPilotService _scheduleService;
		private readonly ITimeProvider _timeProvider;

		public PilotController(ILogger<PilotController> logger, IPilotService scheduleService, ITimeProvider timeProvider)
		{
			_logger = logger;
			_scheduleService = scheduleService;
			_timeProvider = timeProvider;
		}

		[Route("GetNextAvailable")]
		[HttpPost]
		public async Task<ActionResult<GetNextAvailablePilotResponse>> Post(PilotScheduleRequest request)
		{
			if (request.DepartureDateTime < _timeProvider.UtcNow() || request.ReturnDateTime < request.DepartureDateTime)
			{
				return BadRequest();
			}
			return await _scheduleService.GetNextAvailablePilot(request);
		}

		[Route("ConfirmSchedule")]
		[HttpPost]
		public async Task<ActionResult<PilotScheduleConfirmationResult>> Confirm(PilotScheduleConfirmationRequest request)
		{
			bool result = await _scheduleService.ConfirmPilotSchedule(request);
			return new PilotScheduleConfirmationResult(result);
		}
	}
}
