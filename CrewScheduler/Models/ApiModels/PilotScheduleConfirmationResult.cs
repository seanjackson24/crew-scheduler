namespace CrewScheduler.Models.ApiModels
{
	public class PilotScheduleConfirmationResult
	{
		public bool IsConfirmed { get; }

		public PilotScheduleConfirmationResult(bool isConfirmed)
		{
			IsConfirmed = isConfirmed;
		}
	}
}
