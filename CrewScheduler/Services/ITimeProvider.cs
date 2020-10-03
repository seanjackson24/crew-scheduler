using System;

namespace CrewScheduler.Services
{
    public interface ITimeProvider
    {
        DateTime UtcNow();
    }

    public class TimeProvider : ITimeProvider
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}