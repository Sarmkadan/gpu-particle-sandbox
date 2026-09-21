using System;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using GpuParticleSandbox;

namespace GpuParticleSandbox.Benchmarks
{
    [MemoryDiagnoser]
    public class ColorModeMapperBenchmarks
    {
        private const int BatchSize = 10000;
        private readonly float[] _velocities = new float[BatchSize];
        private readonly float[] _ages = new float[BatchSize];
        private readonly Vector3[] _actualColors = new Vector3[BatchSize];

        public ColorModeMapperBenchmarks()
        {
            var random = new Random(42);
            for (int i = 0; i < BatchSize; i++)
            {
                _velocities[i] = (float)random.NextDouble();
                _ages[i] = (float)random.NextDouble();
            }
        }

        [Benchmark]
        public void Map_ScalarLoop()
        {
            for (int i = 0; i < BatchSize; i++)
            {
                _actualColors[i] = ColorModeMapper.Map(_velocities[i], _ages[i], ColorMode.Velocity);
            }
        }

        [Benchmark]
        public void MapBatch_Velocity()
        {
            ColorModeMapper.MapBatch(_actualColors.AsSpan(), _velocities, _ages, ColorMode.Velocity);
        }

        [Benchmark]
        public void MapBatch_Age()
        {
            ColorModeMapper.MapBatch(_actualColors.AsSpan(), _velocities, _ages, ColorMode.Age);
        }

        [Benchmark]
        public void MapBatch_Uniform()
        {
            ColorModeMapper.MapBatch(_actualColors.AsSpan(), _velocities, _ages, ColorMode.Uniform);
        }
    }
}
