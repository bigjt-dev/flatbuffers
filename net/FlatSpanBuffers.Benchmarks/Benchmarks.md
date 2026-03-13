# FlatSpanBuffers Benchmark Results

<!-- Generated from BenchmarkDotNet output. -->
<!-- Run: dotnet run -c Release --project net/FlatSpanBuffers.Benchmarks -->

BenchmarkDotNet v0.15.0, Linux Pop!_OS 22.04 LTS
AMD Ryzen 7 7800X3D 5.05GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.103
[Host] : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

---

## vs Google.FlatBuffers

### DecodeBenchmarks

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_Decode | 83.95 ns | 0.338 ns | 0.300 ns | 1.00 | 0.0012 | 64 B | 1.00 |
| FlatSpanBuffers_Decode | 29.02 ns | 0.089 ns | 0.079 ns | 0.35 | - | - | 0.00 |
| FlatStackBuf_Decode | 22.56 ns | 0.036 ns | 0.034 ns | 0.27 | - | - | 0.00 |

### DecodeObjectApiBenchmarks

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_ObjectApi_Decode | 340.1 ns | 1.42 ns | 1.26 ns | 1.00 | 0.0205 | 1032 B | 1.00 |
| FlatSpanBuffers_ObjectApi_Decode | 153.9 ns | 1.64 ns | 1.54 ns | 0.45 | 0.0129 | 648 B | 0.63 |
| FlatStackBuf_ObjectApi_Decode | 133.7 ns | 0.36 ns | 0.30 ns | 0.39 | 0.0129 | 648 B | 0.63 |

### EncodeBenchmarks

| Method | Mean | Error | StdDev | Ratio | Allocated | Alloc Ratio |
|---|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_Encode | 272.9 ns | 2.08 ns | 1.84 ns | 1.00 | - | NA |
| FlatSpanBuffers_Encode | 175.5 ns | 0.47 ns | 0.44 ns | 0.64 | - | NA |
| FlatStackBuf_Encode | 126.9 ns | 0.27 ns | 0.26 ns | 0.47 | - | NA |

### EncodeObjectApiBenchmarks

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_ObjectApi_Encode | 315.6 ns | 0.94 ns | 0.88 ns | 1.00 | 0.0005 | 40 B | 1.00 |
| FlatSpanBuffers_ObjectApi_Encode | 184.1 ns | 0.18 ns | 0.16 ns | 0.58 | - | - | 0.00 |
| FlatStackBuf_ObjectApi_Encode | 177.7 ns | 0.12 ns | 0.12 ns | 0.56 | - | - | 0.00 |

### VerifyBenchmarks

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_Verify | 158.07 ns | 0.317 ns | 0.265 ns | 1.00 | 0.0026 | 136 B | 1.00 |
| FlatSpanBuffers_Verify | 62.20 ns | 0.091 ns | 0.081 ns | 0.39 | - | - | 0.00 |
| FlatStackBuf_Verify | 65.21 ns | 0.169 ns | 0.158 ns | 0.41 | - | - | 0.00 |

---

## vs FlatSharp

### FlatSharpLazyDecodeComparison

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|--:|--:|--:|--:|--:|--:|--:|
| FlatSharp_LazyDecode | 82.66 ns | 0.326 ns | 0.272 ns | 1.00 | 0.0122 | 616 B | 1.00 |
| FlatSpanBuffers_LazyDecode | 22.51 ns | 0.033 ns | 0.028 ns | 0.27 | - | - | 0.00 |

### FlatSharpGreedyDecodeComparison

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|--:|--:|--:|--:|--:|--:|--:|
| FlatSharp_GreedyDecode | 132.6 ns | 0.42 ns | 0.35 ns | 1.00 | 0.0169 | 856 B | 1.00 |
| FlatSpanBuffers_GreedyDecode | 132.7 ns | 0.45 ns | 0.40 ns | 1.00 | 0.0129 | 648 B | 0.76 |

### FlatSharpEncodeComparison

| Method | Mean | Error | StdDev | Ratio | Allocated | Alloc Ratio |
|---|--:|--:|--:|--:|--:|--:|
| FlatSharp_Encode | 139.6 ns | 1.59 ns | 1.48 ns | 1.00 | - | NA |
| FlatSpanBuffers_SpanBuilderEncode | 126.5 ns | 0.19 ns | 0.16 ns | 0.91 | - | NA |
| FlatSpanBuffers_ObjectApiEncode | 174.1 ns | 0.34 ns | 0.28 ns | 1.25 | - | NA |
