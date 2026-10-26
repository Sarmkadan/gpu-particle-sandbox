using System;
using System.Drawing;
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

        private ColorModeMapper _velocityMapper = null!;
        private ColorModeMapper _ageMapper = null!;
        private ColorModeMapper _uniformMapper = null!;

        public ColorModeMapperBenchmarks()
        {
            var random = new Random(42);
            for (int i = 0; i < BatchSize; i++)
            {
                _velocities[i] = (float)random.NextDouble();
                _ages[i] = (float)random.NextDouble();
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            _velocityMapper = ColorModeMapper.CreateBuilder().WithMode(ColorMode.Velocity).Build();
            _ageMapper = ColorModeMapper.CreateBuilder().WithMode(ColorMode.Age).Build();
            _uniformMapper = ColorModeMapper.CreateBuilder()
                .WithMode(ColorMode.Uniform)
                .WithFallback(Color.White)
                .Build();
        }

        [Benchmark]
        public void Map_Velocity()
        {
            for (int i = 0; i < BatchSize; i++)
            {
                _actualColors[i] = _velocityMapper.Map(_velocities[i], _ages[i]);
            }
        }

        [Benchmark]
        public void Map_Age()
        {
            for (int i = 0; i < BatchSize; i++)
            {
                _actualColors[i] = _ageMapper.Map(_velocities[i], _ages[i]);
            }
        }

        [Benchmark]
        public void Map_Uniform()
        {
            for (int i = 0; i < BatchSize; i++)
            {
                _actualColors[i] = _uniformMapper.Map(_velocities[i], _ages[i]);
            }
        }
    }
}
