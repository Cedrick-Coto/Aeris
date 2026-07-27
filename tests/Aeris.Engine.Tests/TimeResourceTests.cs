using Aeris.Engine;
using FluentAssertions;

namespace Aeris.Engine.Tests;

public class TimeResourceTests
{
    [Fact]
    public void Create_ShouldHaveDefaultValues()
    {
        var time = TimeResource.Create();

        time.SimulationTime.Should().Be(0.0);
        time.TimeScale.Should().Be(1f);
        time.Tick.Should().Be(0);
        time.CurrentDay.Should().Be(1);
        time.CurrentYear.Should().Be(1);
    }

    [Fact]
    public void Advance_WithTimeScale1_ShouldAdvanceDeltaTime()
    {
        var time = TimeResource.Create();
        time.TimeScale = 1f;

        time.Advance(0.016f);

        time.DeltaSimulation.Should().BeApproximately(0.016f, 0.0001f);
        time.SimulationTime.Should().BeApproximately(0.016, 0.001);
        time.Tick.Should().Be(1);
    }

    [Fact]
    public void Advance_WithTimeScale2_ShouldDoubleDeltaTime()
    {
        var time = TimeResource.Create();
        time.TimeScale = 2f;

        time.Advance(0.016f);

        time.DeltaSimulation.Should().BeApproximately(0.032f, 0.0001f);
        time.SimulationTime.Should().BeApproximately(0.032, 0.001);
    }

    [Fact]
    public void Advance_WithTimeScaleHalf_ShouldHalveDeltaTime()
    {
        var time = TimeResource.Create();
        time.TimeScale = 0.5f;

        time.Advance(0.016f);

        time.DeltaSimulation.Should().BeApproximately(0.008f, 0.0001f);
        time.SimulationTime.Should().BeApproximately(0.008, 0.001);
    }

    [Fact]
    public void Advance_WithTimeScaleZero_ShouldNotAdvanceSimulationTime()
    {
        var time = TimeResource.Create();
        time.TimeScale = 0f;

        time.Advance(0.016f);

        time.DeltaSimulation.Should().Be(0f);
        time.SimulationTime.Should().Be(0.0);
        time.Tick.Should().Be(1);
    }

    [Fact]
    public void Advance_ShouldUpdateDeltaReal()
    {
        var time = TimeResource.Create();

        time.Advance(0.033f);

        time.DeltaReal.Should().Be(0.033f);
    }

    [Fact]
    public void Advance_MultipleTicks_ShouldAccumulateTime()
    {
        var time = TimeResource.Create();
        time.TimeScale = 1f;

        for (int i = 0; i < 100; i++)
        {
            time.Advance(0.016f);
        }

        time.SimulationTime.Should().BeApproximately(1.6, 0.01);
        time.Tick.Should().Be(100);
    }

    [Theory]
    [InlineData(1f)]
    [InlineData(2f)]
    [InlineData(0.5f)]
    [InlineData(0f)]
    [InlineData(60f)]
    public void Advance_VariousTimeScales_ShouldCalculateCorrectly(float timeScale)
    {
        var time = TimeResource.Create();
        time.TimeScale = timeScale;
        var realDelta = 0.016f;

        time.Advance(realDelta);

        var expectedSimulation = realDelta * timeScale;
        time.DeltaSimulation.Should().BeApproximately(expectedSimulation, 0.0001f);
        time.SimulationTime.Should().BeApproximately(expectedSimulation, 0.001);
    }

    [Fact]
    public void Advance_LongSequence_ShouldMaintainPrecision()
    {
        var time = TimeResource.Create();
        time.TimeScale = 1f;
        var realDelta = 0.016f;

        for (int i = 0; i < 100_000; i++)
        {
            time.Advance(realDelta);
        }

        var expectedTime = 100_000 * realDelta;
        time.SimulationTime.Should().BeApproximately(expectedTime, 0.1);
        time.Tick.Should().Be(100_000);
    }

    [Fact]
    public void Advance_ShouldUpdateCalendar()
    {
        var time = TimeResource.Create();
        time.TimeScale = 1f;

        var oneDayInSeconds = 86400f;
        time.Advance(oneDayInSeconds);

        time.CurrentDay.Should().Be(2);
        time.DayFraction.Should().BeApproximately(0f, 0.01f);
    }

    [Fact]
    public void Advance_PauseAndResume_ShouldNotLoseTime()
    {
        var time = TimeResource.Create();
        time.TimeScale = 1f;

        time.Advance(1f);
        var timeAfterFirst = time.SimulationTime;

        time.TimeScale = 0f;
        time.Advance(1f);
        time.SimulationTime.Should().Be(timeAfterFirst);

        time.TimeScale = 1f;
        time.Advance(1f);
        time.SimulationTime.Should().Be(timeAfterFirst + 1.0);
    }
}
