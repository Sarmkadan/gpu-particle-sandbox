using BenchmarkDotNet.Running;
using GpuParticleSandbox.Benchmarks;

var summary = BenchmarkRunner.Run<ShaderProgramBenchmarks>();
