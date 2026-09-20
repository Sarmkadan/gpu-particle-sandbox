using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace GpuParticleSandbox.Benchmarks;

[MemoryDiagnoser]
public class ShaderProgramBenchmarks
{
    private readonly UniformLocationCache _uniformCache = new();
    private readonly string[] _uniformNames = new[]
    {
        "uDeltaTime", "uParticleCount", "uColorMode", "uGravity", "uWellStrength",
        "uWellRadius", "uSpawnRate", "uPointSize", "uResolution", "uTime"
    };

    [GlobalSetup]
    public void Setup()
    {
        // Pre-populate cache to simulate a warm state for the cached benchmark
        foreach (var name in _uniformNames)
        {
            _uniformCache.GetOrResolve(name, n => n.GetHashCode());
        }
    }

    [Benchmark]
    public int UniformLookupCached()
    {
        int result = 0;
        foreach (var name in _uniformNames)
        {
            result += _uniformCache.GetOrResolve(name, n => n.GetHashCode());
        }
        return result;
    }

    [Benchmark]
    public int UniformLookupUncached()
    {
        var cache = new Dictionary<string, int>();
        int result = 0;
        foreach (var name in _uniformNames)
        {
            if (cache.TryGetValue(name, out int cached))
                result += cached;
            else
            {
                int loc = name.GetHashCode();
                cache[name] = loc;
                result += loc;
            }
        }
        return result;
    }

    [Benchmark]
    public string ProcessIncludes()
    {
        string source = "#version 430\n#include \"common.glsl\"\nvoid main() { }";
        return ShaderSourcePreprocessor.ProcessIncludes(source, p => "#define COMMON 1\n");
    }
}
