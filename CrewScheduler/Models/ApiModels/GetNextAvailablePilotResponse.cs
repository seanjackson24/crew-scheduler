namespace CrewScheduler.Models.ApiModels
{
	public class GetNextAvailablePilotResponse
	{
		public int? PilotId { get; }
		public string ReservationKey { get; }

		public GetNextAvailablePilotResponse(int? pilotId, string reservationKey)
		{
			PilotId = pilotId;
			if (pilotId != null)
			{
				ReservationKey = reservationKey;
			}
		}
	}
}
