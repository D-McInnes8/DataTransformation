// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

// var config = DefaultConfig.Instance
//     .AddJob(Job
//         .MediumRun
//         .WithLaunchCount(1)
//         .WithToolchain(InProcessNoEmitToolchain.Instance));
_ = BenchmarkRunner.Run<Benchmarks.Benchmarks>();