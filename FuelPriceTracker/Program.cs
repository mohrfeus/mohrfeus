using FuelPriceTracker.Config;
using FuelPriceTracker.Data;
using FuelPriceTracker.Repositories;
using FuelPriceTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<TankerkoenigOptions>(builder.Configuration.GetSection(TankerkoenigOptions.SectionName));
builder.Services.Configure<PollingOptions>(builder.Configuration.GetSection(PollingOptions.SectionName));

builder.Services.AddDbContext<FuelPriceDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("FuelPriceDb")
                           ?? "Data Source=fuelprices.db";
    options.UseSqlite(connectionString);
});

builder.Services.AddHttpClient<ITankerkoenigApiClient, TankerkoenigApiClient>();

builder.Services.AddScoped<IStationRepository, StationRepository>();
builder.Services.AddScoped<IPriceSnapshotRepository, PriceSnapshotRepository>();

builder.Services.AddHostedService<FuelPricePollingWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FuelPriceDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();
