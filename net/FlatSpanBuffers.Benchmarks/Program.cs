// Copyright 2014 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.NativeAot;

namespace FlatSpanBuffers.Benchmarks;

public static class Program
{
    private static IConfig BuildConfig(bool includeAot)
    {
        if (!includeAot)
            return DefaultConfig.Instance;

        // NativeAOT job targeting Ryzen 7 7800X3D. TODO: Revisit IlcInstructionSet for CI.
        return DefaultConfig.Instance
            .AddJob(Job.Default
                .WithId("AOT")
                .WithToolchain(
                    NativeAotToolchain.CreateBuilder()
                        .UseNuGet("10.0.4")
                        .IlcInstructionSet("base,sse4.2,avx,avx2,avx512,aes")
                        .ToToolchain()
                ));
    }

    public static void Main(string[] args)
    {
        bool includeAot = args.Contains("--aot", StringComparer.OrdinalIgnoreCase);
        var config = BuildConfig(includeAot);

        List<Summary> results =
        [
            //BenchmarkRunner.Run<SimpleMonsterBenchmarks>(config),
            BenchmarkRunner.Run<DecodeBenchmarks>(config),
            BenchmarkRunner.Run<DecodeObjectApiBenchmarks>(config),
            BenchmarkRunner.Run<EncodeBenchmarks>(config),
            BenchmarkRunner.Run<EncodeObjectApiBenchmarks>(config),
            BenchmarkRunner.Run<VerifyBenchmarks>(config),

            BenchmarkRunner.Run<FlatSharpEncode>(config),
            BenchmarkRunner.Run<FlatSharpDecodeLazy>(config),
            BenchmarkRunner.Run<FlatSharpDecodeGreedy>(config),
            BenchmarkRunner.Run<FlatSharpSortedVectorStringKey>(config),
            BenchmarkRunner.Run<FlatSharpSortedVectorIntKey>(config),
        ];

        if (results.Count == 0)
            return;

        var logger = ConsoleLogger.Default;

        Console.WriteLine();
        logger.WriteLine(LogKind.Header, @"// * Final Summary *");
        Console.WriteLine();

        logger.WriteLine(LogKind.Default, "```");
        logger.WriteLine(LogKind.Info, HostEnvironmentInfo.GetInformation());
        foreach (var summary in results)
        {
            logger.WriteLine(LogKind.Header, $"{summary.Title}");
            PrintTableViaReflection(summary, logger, MarkdownExporter.Console);
            Console.WriteLine();
        }
        logger.WriteLine(LogKind.Default, "```");
    }

    private static void PrintTableViaReflection(Summary summary, ILogger logger, IExporter exporter)
    {
        var exporterType = exporter.GetType();
        var printTableMethod = exporterType.GetMethod(
            "PrintTable",
            BindingFlags.NonPublic | BindingFlags.Instance, null,
            new[] { typeof(SummaryTable), typeof(ILogger) }, null);

        if (printTableMethod == null)
        {
            logger.WriteLine(LogKind.Error, "Could not find PrintTable method via reflection");
        }

        printTableMethod.Invoke(exporter, new object[] { summary.Table, logger });
    }
}
