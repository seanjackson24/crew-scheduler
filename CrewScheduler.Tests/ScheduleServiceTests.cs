using System;
using Xunit;
using Moq;
using System.Threading.Tasks;
using CrewScheduler.Services;
using System.Linq;
using CrewScheduler.Models;
using System.Collections.Generic;

namespace CrewScheduler.Tests
{
	public class ScheduleServiceTests
	{
		private const int Pilot1 = 1;
		private const int Pilot2 = 2;
		private static readonly DateTime TwoPm = new DateTime(2020, 10, 01, 14, 0, 0);
		private static readonly DateTime FourPm = new DateTime(2020, 10, 01, 16, 0, 0);
		private static readonly DateTime SixPm = new DateTime(2020, 10, 01, 18, 0, 0);
		private static readonly DateTime EightPm = new DateTime(2020, 10, 01, 20, 0, 0);
		private static readonly DayOfWeek[] AllDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

		private readonly PilotService _service;
		private readonly Mock<IFileService> _fileServiceMock = new Mock<IFileService>();
		public ScheduleServiceTests()
		{
			_service = new PilotService(_fileServiceMock.Object);
		}

		[Fact]
		public async Task NoPilotsAvailable_ReturnsNull()
		{
			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).Returns(Task.FromResult(Enumerable.Empty<PilotWorkSchedule>()));
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = FourPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Null(result);
		}

		[Fact]
		public async Task NoPilotsAvailable_ForThisLocation_ReturnsNull()
		{
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Munich,
				PilotId = Pilot1,
				WorkDays = AllDays
			}};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).Returns(Task.FromResult<IEnumerable<PilotWorkSchedule>>(pilots));
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = FourPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Null(result);
		}

		[Fact]
		public async Task NoPilotsAvailable_ForThisDay_ReturnsNull()
		{
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Munich,
				PilotId = Pilot1,
				WorkDays = new[] {DayOfWeek.Monday}
			}};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).Returns(Task.FromResult<IEnumerable<PilotWorkSchedule>>(pilots));
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = FourPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Null(result);
		}

		[Fact]
		public async Task OnePilot_NothingScheduled_GetsReturned()
		{
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot1,
				WorkDays = AllDays
			}};
			var schedules = Enumerable.Empty<PilotScheduleInfo>();

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
			_fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = FourPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Equal(Pilot1, result);
		}

		// one pilot, departs 30 min after request, unavailable
		[Fact]
		public async Task OnePilot_DepartsAfterRequest_Unavailable()
		{
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot1,
				WorkDays = AllDays
			}};
			var schedules = new[] {
				new PilotScheduleInfo() {
					PilotId = Pilot1,
					DepartureDateTime = TwoPm.AddSeconds(1),
					ReturnDateTime = FourPm
				}
			};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
			_fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = FourPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Null(result);
		}


		// two pilots, one departs 30min after request, return other
		[Fact]
		public async Task TwoPilots_OneDepartsAfterRequest_ReturnOther()
		{
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot1,
				WorkDays = AllDays
			}, new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot2,
				WorkDays = AllDays
			}};
			var schedules = new[] {
				new PilotScheduleInfo() {
					PilotId = Pilot1,
					DepartureDateTime = TwoPm.AddSeconds(1),
					ReturnDateTime = FourPm
				}
			};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
			_fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = FourPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Equal(Pilot2, result);
		}


        // two pilots, one returns 30 min after request, return other
        [Fact]
        public async Task TwoPilots_OneReturnsAfterRequest_ReturnOther()
		{
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot1,
				WorkDays = AllDays
			}, new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot2,
				WorkDays = AllDays
			}};
			var schedules = new[] {
				new PilotScheduleInfo() {
					PilotId = Pilot1,
					DepartureDateTime = TwoPm,
					ReturnDateTime = FourPm.AddSeconds(1)
				}
			};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
			_fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = FourPm,
				ReturnDateTime = SixPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Equal(Pilot2, result);
		}

		// two pilots, both available, one has nothing, other has 1 trip, return first
		[Fact]
		public async Task TwoPilots_BothAvailable_ReturnsPilotWithNoSchedule()
		{
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot1,
				WorkDays = AllDays
			}, new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot2,
				WorkDays = AllDays
			}};
			var schedules = new[] {
				new PilotScheduleInfo() {
					PilotId = Pilot1,
					DepartureDateTime = SixPm,
					ReturnDateTime = EightPm
				}
			};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
			_fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = FourPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Equal(Pilot2, result);
		}

		// two pilots, both available, one has 1 trip, other has 2, return first
		[Fact]
		public async Task TwoPilots_BothAvailable_ReturnsPilotWithFewerTrips()
		{
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot1,
				WorkDays = AllDays
			}, new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot2,
				WorkDays = AllDays
			}};
			var schedules = new[] {
				new PilotScheduleInfo() {
					PilotId = Pilot1,
					DepartureDateTime = SixPm,
					ReturnDateTime = EightPm
				},
				new PilotScheduleInfo() {
					PilotId = Pilot2,
					DepartureDateTime = SixPm,
					ReturnDateTime = EightPm
				},
				new PilotScheduleInfo() {
					PilotId = Pilot2,
					DepartureDateTime = EightPm,
					ReturnDateTime = EightPm.AddHours(2)
				}
			};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
			_fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = FourPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Equal(Pilot1, result);
		}

		[Fact]
		public async Task PilotOnShortTrip_InsideRequestWindow_Unavailable()
        {
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot1,
				WorkDays = AllDays
			}};
			var schedules = new[] {
				new PilotScheduleInfo() {
					PilotId = Pilot1,
					DepartureDateTime = FourPm,
					ReturnDateTime = SixPm
				}
			};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
			_fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = TwoPm,
				ReturnDateTime = EightPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Null(result);
		}

		[Fact]
		public async Task PIlotOnLongTrip_EitherSideOfRequestWindow_Unavailable()
        {
			var pilots = new[] {
				new PilotWorkSchedule() {
				Base = Base.Berlin,
				PilotId = Pilot1,
				WorkDays = AllDays
			}};
			var schedules = new[] {
				new PilotScheduleInfo() {
					PilotId = Pilot1,
					DepartureDateTime = TwoPm,
					ReturnDateTime = EightPm
				}
			};

			_fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
			_fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
			var request = new PilotScheduleRequest()
			{
				Location = Base.Berlin,
				DepartureDateTime = FourPm,
				ReturnDateTime = SixPm
			};
			var result = await _service.GetNextAvailablePilot(request);
			Assert.Null(result);
		}
	}
}
