using BenchmarkDotNet.Attributes;
using GpuParticleSandbox;

namespace GpuParticleSandbox.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="FpsCounter"/> to measure per-frame overhead and property access costs.
/// </summary>
[MemoryDiagnoser]
public class FpsCounterBenchmarks
{
    private FpsCounter _counter;

    [GlobalSetup]
    public void Setup()
    {
        _counter = new FpsCounter(0.1);
        
        // Warm up to establish initial EMA averages and ensure JIT compilation
        for (int i = 0; i < 100; i++)
        {
            _counter.Tick();
        }
    }

    [Benchmark(Baseline = true)]
    public void Tick_PerFrame()
    {
        _counter.Tick();
    }

    [Benchmark]
    public void Update_PerFrame()
    {
        _counter.Update();
    }

    [Benchmark]
    public double ReadAverageFps()
    {
        return _counter.AverageFps;
    }
}
