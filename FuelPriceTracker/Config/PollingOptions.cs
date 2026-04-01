namespace FuelPriceTracker.Config;

public sealed class PollingOptions
{
    public const string SectionName = "Polling";

    public int IntervalMinutes { get; set; } = 10;
    public bool SkipUnchangedSnapshots { get; set; } = true;
}
