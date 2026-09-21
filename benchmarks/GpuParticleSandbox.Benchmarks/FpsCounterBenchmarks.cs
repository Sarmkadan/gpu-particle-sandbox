using System;
using BenchmarkDotNet.Attributes;

namespace GpuParticleSandbox.Benchmarks;

[MemoryDiagnoser]
public class FpsCounterBenchmarks
{
    private FpsCounter? _fpsCounter;

    [GlobalSetup]
    public void Setup()
    {
        _fpsCounter = new FpsCounter();
    }

    [Benchmark]
    public void Tick()
    {
        _fpsCounter!.Tick();
    }

    [Benchmark]
    public void Update()
    {
        _fpsCounter!.Update();
    }

    [Benchmark]
    public double AverageFps()
    {
        return _fpsCounter!.AverageFps;
    }

    [Benchmark]
    public double AverageFrameTimeMs()
    {
        return _fpsCounter!.AverageFrameTimeMs;
    }

    [Benchmark]
    public string GetDisplayString()
    {
        return _fpsCounter!.GetDisplayString();
    }
}