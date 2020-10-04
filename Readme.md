# Crew Scheduler

A Crew Scheduling API to provide on-demand crew scheduling info to other services

# Prerequisites:

-   dotnet core SDK 3.1 or later (https://dotnet.microsoft.com/download/dotnet-core/3.1)

-   Ensure the local development certificate is trusted by running:
    > dotnet dev-certs https --trust

# Run API Backend:

-   Browse to /CrewScheduler in your favourite command prompt, and run:

    > dotnet build

    > dotnet run

-   The application will start listening on `https://localhost:5001/`

# Make a request to the application:

As an example using the command-line utility [httpie](https://httpie.org/), you can simulate the below samples by running the commands:

-   Get Next Available Pilot:

    > http --verify=no --timeout=300 post "https://localhost:5001/Pilot/GetNextAvailable" Location=Munich DepartureDateTime="2021-01-03T06:00" ReturnDateTime="2021-01-03T08:00"

    If successful, you should get back a response such as:

    > {

        "pilotId": 3,
        "reservationKey": "e4f1327e-e6a3-4ec7-b860-ac440eb41d38"

    }
    See below for usage of this reservation key.

-   If there are no pilots available, this should then return a response such as:

    > { "pilotId": null, "reservationKey": null }

-   Confirm a pilot reservation:

    > http --verify=no --timeout=300 post "https://localhost:5001/Pilot/ConfirmSchedule" Location=Munich DepartureDateTime="2021-01-03T06:01" ReturnDateTime="2021-01-03T08:00" ReservationKey="e4f1327e-e6a3-4ec7-b860-ac440eb41d38" PilotId:=3

    This should then return a response such as:

    > { "isConfirmed": true }

# How it works:

When you perform a request to get the next available pilot, we pencil in a schedule item for this time, to avoid race conditions where another user may perform the same action, then confirm the pilot you originally were assigned before you can confirm them. We generate a reservation key, which is just a GUID, which must then be used (along with the pilot id of the original response) in order to confirm your reservation.

If no reservation is confirmed within ten minutes of the original request, then the pencilled in schedule is considered expired and that time will be freed up again for that pilot.

In the case of no pilots being available, the pilot ID returned will be null, along with the pilot reservation key.

If the reservation key provided and the pilot information do not match a known pencilled in schedule, or the reservation has expired (as mentioned above), then the isConfirmed will be set to false.

# Considerations:

If I were to expand on this work, I think I would make a few minor changes to the API surface:

-   For the confirmation of the schedule, only the reservation key and pilot id would be needed.
-   I would also change the response object to give a bit more information about why the confirmation was not confirmed
-   Similarly for the get next available pilot call, I would provide more information about why we were unable to find a pilot, and perhaps suggest a better time (for example, if there would be a pilot available five minutes later than originally requested, the user would probably want to know this).
-   A Cancel Reservation endpoint would also make sense, so that a pencilled in schedule item would not have to necessarily wait the full ten minutes to be freed up again.

I would also add on some form of authentication in order to prevent DOS attacks blocking out a pilot's schedule with flights that were never intended to be used.

Each of these points were considered to be beyond the scope of this exercise, but are certainly some of the things worth thinking about.
