using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CrewScheduler.Models;
using Microsoft.AspNetCore.Hosting;

namespace CrewScheduler.Services
{
	public interface IFileService
	{
		Task<IEnumerable<PilotScheduleInfo>> GetPilotSchedulesForDay(DateTime day);
		Task<IEnumerable<PilotWorkSchedule>> GetPilotWorkSchedules();
		Task AddPilotToSchedule(PilotScheduleInfo pilotSchedules);
	}
	public class FileService : IFileService
	{
		private const string CrewFile = "Crew.json";
		private const string ScheduleFile = "Schedule.json";
		private readonly JsonSerializerOptions _options = new JsonSerializerOptions();
		private readonly IWebHostEnvironment _environment;

		public FileService(IWebHostEnvironment environment)
		{
			_environment = environment;
			_options.Converters.Add(new JsonStringEnumConverter());
		}

		public async Task AddPilotToSchedule(PilotScheduleInfo pilotSchedules)
		{
			var existingSchedule = (await GetPilotSchedulesForDay(DateTime.Now)).ToList();
			existingSchedule.Add(pilotSchedules);

			var content = JsonSerializer.Serialize(existingSchedule, _options);
			await File.WriteAllTextAsync(ScheduleFile, content);
		}

		// TODO: filter to just this day
		public async Task<IEnumerable<PilotScheduleInfo>> GetPilotSchedulesForDay(DateTime day)
		{
			string path = Path.Combine(_environment.ContentRootPath, ScheduleFile);

			if (!File.Exists(path))
			{
				File.WriteAllText(path, "[]");
			}
			var contents = await File.ReadAllTextAsync(path);
			return JsonSerializer.Deserialize<IEnumerable<PilotScheduleInfo>>(contents, _options);
		}

		public async Task<IEnumerable<PilotWorkSchedule>> GetPilotWorkSchedules()
		{
			string path = Path.Combine(_environment.ContentRootPath, CrewFile);
			var contents = await File.ReadAllTextAsync(path);

			return JsonSerializer.Deserialize<CrewScheduleInfo>(contents, _options).Crew;
		}
	}
}