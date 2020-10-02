using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrewScheduler.Models;
using CrewScheduler.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CrewScheduler.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class PilotController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<PilotController> _logger;
		private readonly PilotService _scheduleService;

		public PilotController(ILogger<PilotController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public async Task<int?> Get(PilotScheduleRequest request)
		{
			return await _scheduleService.GetNextAvailablePilot(request);
		}
	}
}
