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
        private static readonly DateTime Now = new DateTime(2020, 10, 01, 11, 0, 0);
        private static readonly DateTime ExpiredDate = Now.AddMinutes(-10).AddSeconds(-1);
        private static readonly DateTime TwoPm = new DateTime(2020, 10, 01, 14, 0, 0);
        private static readonly DateTime FourPm = new DateTime(2020, 10, 01, 16, 0, 0);
        private static readonly DateTime SixPm = new DateTime(2020, 10, 01, 18, 0, 0);
        private static readonly DateTime EightPm = new DateTime(2020, 10, 01, 20, 0, 0);
        private static readonly DayOfWeek[] AllDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

        private readonly PilotService _service;
        private readonly Mock<IFileService> _fileServiceMock = new Mock<IFileService>();
        private readonly Mock<ITimeProvider> _timeProviderMock = new Mock<ITimeProvider>();
        public ScheduleServiceTests()
        {
            _service = new PilotService(_fileServiceMock.Object, _timeProviderMock.Object);
        }

        private void SetDefaultNow()
        {
            _timeProviderMock.Setup(x => x.UtcNow()).Returns(Now);
        }

        [Fact]
        public async Task NoPilotsAvailable_ReturnsNull()
        {
            SetDefaultNow();
            _fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).Returns(Task.FromResult(Enumerable.Empty<PilotWorkSchedule>()));
            var request = new PilotScheduleRequest()
            {
                Location = Base.Berlin,
                DepartureDateTime = TwoPm,
                ReturnDateTime = FourPm
            };
            var result = await _service.GetNextAvailablePilot(request);
            Assert.Null(result.PilotId);
            Assert.Null(result.ReservationKey);
        }

        [Fact]
        public async Task NoPilotsAvailable_ForThisLocation_ReturnsNull()
        {
            SetDefaultNow();

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
            Assert.Null(result.PilotId);
            Assert.Null(result.ReservationKey);
        }

        [Fact]
        public async Task NoPilotsAvailable_ForThisDay_ReturnsNull()
        {
            SetDefaultNow();

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
            Assert.Null(result.PilotId);
            Assert.Null(result.ReservationKey);
        }

        [Fact]
        public async Task OnePilot_NothingScheduled_GetsReturned()
        {
            SetDefaultNow();

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
            Assert.Equal(Pilot1, result.PilotId);
            Assert.NotNull(result.ReservationKey);
        }

        // one pilot, departs 30 min after request, unavailable
        [Fact]
        public async Task OnePilot_DepartsAfterRequest_Unavailable()
        {
            SetDefaultNow();

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
                    ReturnDateTime = FourPm,
                    IsConfirmed = true
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
            Assert.Null(result.PilotId);
            Assert.Null(result.ReservationKey);
        }


        // two pilots, one departs 30min after request, return other
        [Fact]
        public async Task TwoPilots_OneDepartsAfterRequest_ReturnOther()
        {
            SetDefaultNow();

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
                    ReturnDateTime = FourPm,
                    IsConfirmed = true
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
            Assert.Equal(Pilot2, result.PilotId);
            Assert.NotNull(result.ReservationKey);
        }


        // two pilots, one returns 30 min after request, return other
        [Fact]
        public async Task TwoPilots_OneReturnsAfterRequest_ReturnOther()
        {
            SetDefaultNow();

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
                    ReturnDateTime = FourPm.AddSeconds(1),
                    IsConfirmed = true
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
            Assert.Equal(Pilot2, result.PilotId);
            Assert.NotNull(result.ReservationKey);
        }

        // two pilots, both available, one has nothing, other has 1 trip, return first
        [Fact]
        public async Task TwoPilots_BothAvailable_ReturnsPilotWithNoSchedule()
        {
            SetDefaultNow();

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
                    ReturnDateTime = EightPm,
                    IsConfirmed = true
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
            Assert.Equal(Pilot2, result.PilotId);
            Assert.NotNull(result.ReservationKey);
        }

        // two pilots, both available, one has 1 trip, other has 2, return first
        [Fact]
        public async Task TwoPilots_BothAvailable_ReturnsPilotWithFewerTrips()
        {
            SetDefaultNow();

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
                    ReturnDateTime = EightPm,
                    IsConfirmed = true
                },
                new PilotScheduleInfo() {
                    PilotId = Pilot2,
                    DepartureDateTime = SixPm,
                    ReturnDateTime = EightPm,
                    IsConfirmed = true
                },
                new PilotScheduleInfo() {
                    PilotId = Pilot2,
                    DepartureDateTime = EightPm,
                    ReturnDateTime = EightPm.AddHours(2),
                    IsConfirmed = true
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
            Assert.Equal(Pilot1, result.PilotId);
            Assert.NotNull(result.ReservationKey);
        }

        [Fact]
        public async Task PilotOnShortTrip_InsideRequestWindow_Unavailable()
        {
            SetDefaultNow();

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
                    ReturnDateTime = SixPm,
                    IsConfirmed = true
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
            Assert.Null(result.PilotId);
            Assert.Null(result.ReservationKey);
        }

        [Fact]
        public async Task PilotOnLongTrip_EitherSideOfRequestWindow_Unavailable()
        {
            SetDefaultNow();

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
                    ReturnDateTime = EightPm,
                    IsConfirmed = true
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
            Assert.Null(result.PilotId);
            Assert.Null(result.ReservationKey);
        }

        [Fact]
        public async Task ConfirmPilot_NotReserved_ReturnsTrue()
        {
            SetDefaultNow();
            var reservationKey = "test";

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
                    ReturnDateTime = FourPm,
                    IsConfirmed = false,
                    ReservationTime = Now,
                    ReservationKey = reservationKey
                }
            };

            _fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
            _fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
            var request = new PilotScheduleConfirmation()
            {
                Location = Base.Berlin,
                DepartureDateTime = TwoPm,
                ReturnDateTime = FourPm,
                PilotId = Pilot1,
                ReservationKey = reservationKey
            };
            var result = await _service.ConfirmPilotSchedule(request);
            Assert.True(result);
        }

        [Fact]
        public async Task ConfirmPilot_AlreadyReserved_ReturnsFalse()
        {
            SetDefaultNow();
            var reservationKey = "test";

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
                    ReturnDateTime = FourPm,
                    IsConfirmed = true,
                    ReservationTime = Now,
                    ReservationKey = reservationKey
                }
            };

            _fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
            _fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
            var request = new PilotScheduleConfirmation()
            {
                Location = Base.Berlin,
                DepartureDateTime = TwoPm,
                ReturnDateTime = FourPm,
                PilotId = Pilot1,
                ReservationKey = reservationKey
            };
            var result = await _service.ConfirmPilotSchedule(request);
            Assert.False(result);
        }

        [Fact]
        public async Task ConfirmPilot_HasExpiredReservation_ReturnsFalse()
        {
            SetDefaultNow();
            var reservationKey = "test";

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
                    ReturnDateTime = FourPm,
                    IsConfirmed = false,
                    ReservationTime = ExpiredDate,
                    ReservationKey = reservationKey
                }
            };

            _fileServiceMock.Setup(x => x.GetPilotWorkSchedules()).ReturnsAsync(pilots);
            _fileServiceMock.Setup(x => x.GetPilotSchedulesForDay(TwoPm.Date)).ReturnsAsync(schedules);
            var request = new PilotScheduleConfirmation()
            {
                Location = Base.Berlin,
                DepartureDateTime = TwoPm,
                ReturnDateTime = FourPm,
                PilotId = Pilot1,
                ReservationKey = reservationKey
            };
            var result = await _service.ConfirmPilotSchedule(request);
            Assert.False(result);
        }

        [Fact]
        public async Task GetAvailablePilot_ExcludesUnconfirmedReservations()
        {
            SetDefaultNow();
            const string reservationKey = "hello";

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
                    ReturnDateTime = FourPm,
                    IsConfirmed = false,
                    ReservationKey = reservationKey,
                    ReservationTime = Now.AddSeconds(-1)
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
            Assert.Null(result.PilotId);
        }

        [Fact]
        public async Task GetAvailablePilot_IgnoresUnconfirmedExpiredReservations()
        {
            SetDefaultNow();

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
                    ReturnDateTime = FourPm,
                    IsConfirmed = false,
                    ReservationTime = ExpiredDate,
                    ReservationKey = "test"
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
            Assert.Equal(Pilot1, result.PilotId);
            Assert.NotNull(result.ReservationKey);

        }
    }
}
