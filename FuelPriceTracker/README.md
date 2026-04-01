# FuelPriceTracker (.NET 8 Worker)

Tracks fuel prices for all gas stations in a 30km radius (or configured radius) using the Tankerkoenig API and stores historical snapshots in SQLite.

## Features

- Initial station discovery via `stations/search`
- Station metadata persistence
- 10-minute polling (configurable) via `stations/ids`
- Timestamped price snapshots (`Diesel`, `E5`, `E10`, `IsOpen`)
- Optional duplicate suppression when prices have not changed
- Optional price-change table for analytics
- Structured logging and simple retry logic

## Project structure

```text
FuelPriceTracker/
  Config/
    PollingOptions.cs
    TankerkoenigOptions.cs
  Data/
    FuelPriceDbContext.cs
  Models/
    Station.cs
    PriceSnapshot.cs
    PriceChange.cs
  Repositories/
    IStationRepository.cs
    StationRepository.cs
    IPriceSnapshotRepository.cs
    PriceSnapshotRepository.cs
  Services/
    ITankerkoenigApiClient.cs
    TankerkoenigApiClient.cs
    TankerkoenigContracts.cs
    FuelPricePollingWorker.cs
  Program.cs
  appsettings.json
  FuelPriceTracker.csproj
```

## Setup

1. Install .NET 8 SDK.
2. From the project folder, restore/build:

   ```bash
   dotnet restore
   dotnet build
   ```

3. Configure `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "FuelPriceDb": "Data Source=fuelprices.db"
     },
     "Tankerkoenig": {
       "ApiKey": "YOUR_API_KEY",
       "BaseUrl": "https://creativecommons.tankerkoenig.de/json",
       "Latitude": 52.520008,
       "Longitude": 13.404954,
       "RadiusKm": 30,
       "FuelType": "all",
       "Sort": "dist"
     },
     "Polling": {
       "IntervalMinutes": 10,
       "SkipUnchangedSnapshots": true
     }
   }
   ```

4. Run:

   ```bash
   dotnet run
   ```

On startup, the worker applies EF Core migrations automatically (`Database.MigrateAsync()`).

## Database schema

### Stations
- `Id` (PK)
- `Name`
- `Brand`
- `Street`
- `Place`
- `Lat`
- `Lng`

### PriceSnapshots
- `Id` (auto increment)
- `StationId` (FK)
- `TimestampUtc`
- `Diesel`
- `E5`
- `E10`
- `IsOpen`

### PriceChanges (optional helper table)
- `Id` (auto increment)
- `StationId` (FK)
- `TimestampUtc`
- previous/new values for Diesel, E5, E10

## Notes

- If `SkipUnchangedSnapshots = true`, the worker stores only changes over time (plus first value per station).
- If you want every 10-minute sample regardless of change, set `SkipUnchangedSnapshots = false`.
