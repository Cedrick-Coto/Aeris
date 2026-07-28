namespace Aeris.Engine;

public struct TimeResource
{
    public double SimulationTime;
    public float DeltaReal;
    public float DeltaSimulation;
    public float TimeScale;
    public long Tick;
    public int CurrentDay;
    public float DayFraction;
    public int CurrentSeason;
    public int CurrentYear;

    public static TimeResource Create()
    {
        return new TimeResource
        {
            SimulationTime = 0.0,
            DeltaReal = 0f,
            DeltaSimulation = 0f,
            TimeScale = 1f,
            Tick = 0,
            CurrentDay = 1,
            DayFraction = 0f,
            CurrentSeason = 0,
            CurrentYear = 1
        };
    }

    public void Advance(float realDeltaTime)
    {
        Tick++;
        DeltaReal = realDeltaTime;
        DeltaSimulation = realDeltaTime * TimeScale;
        SimulationTime += DeltaSimulation;

        UpdateCalendar();
    }

    public void SetFromSnapshot(long tick, double simulationTime)
    {
        Tick = tick;
        SimulationTime = simulationTime;
        UpdateCalendar();
    }

    private void UpdateCalendar()
    {
        const double SecondsPerDay = 86400.0;
        const int DaysPerSeason = 91;
        const int DaysPerYear = 365;

        var totalDays = SimulationTime / SecondsPerDay;
        var dayInYear = (int)(totalDays % DaysPerYear) + 1;
        var season = dayInYear / DaysPerSeason;
        var dayFraction = (float)(totalDays % 1.0);

        CurrentDay = dayInYear;
        DayFraction = dayFraction;
        CurrentSeason = Math.Min(season, 3);
        CurrentYear = (int)(totalDays / DaysPerYear) + 1;
    }
}
