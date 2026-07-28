using Aeris.Engine;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

var config = DefaultConfig.Instance
    .WithOptions(ConfigOptions.DisableOptimizationsValidator)
    .AddJob(Job.ShortRun
        .WithWarmupCount(1)
        .WithIterationCount(3)
        .WithToolchain(InProcessNoEmitToolchain.Instance));

BenchmarkSwitcher.FromTypes([
    typeof(Aeris.Benchmarks.EngineTickBenchmarks),
]).RunAll(config);
