# FlatSpanBuffers Benchmark Results

<!-- Generated from BenchmarkDotNet output. -->
<!-- Run: dotnet run -c Release --project net/FlatSpanBuffers.Benchmarks -->

BenchmarkDotNet v0.15.0, Linux Pop!_OS 22.04 LTS
AMD Ryzen 7 7800X3D 5.05GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.104
  [Host]     : .NET 10.0.4 (10.0.426.12010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 10.0.4 (10.0.426.12010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  AOT        : .NET 10.0.4, X64 NativeAOT AVX-512F+CD+BW+DQ+VL+VBMI

---

## vs Google.FlatBuffers

### DecodeBenchmarks

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_Decode | DefaultJob | Default           |  83.13 ns | 0.307 ns | 0.256 ns | 1.00 | 0.0012 |  64 B | 1.00 |
| FlatSpanBuffers_Decode      | DefaultJob | Default           |  30.73 ns | 0.097 ns | 0.086 ns | 0.37 |      - |   - | 0.00 |
| FlatStackBuf_Decode         | DefaultJob | Default           |  21.84 ns | 0.045 ns | 0.040 ns | 0.26 |      - |   - | 0.00 |
|                             |            |                   |           |          |          |      |        |     |      |
| Original_FlatBuffers_Decode | AOT        | ILCompiler 10.0.4 | 126.03 ns | 0.323 ns | 0.270 ns | 1.00 | 0.0012 |  64 B | 1.00 |
| FlatSpanBuffers_Decode      | AOT        | ILCompiler 10.0.4 |  73.15 ns | 0.125 ns | 0.117 ns | 0.58 |      - |   - | 0.00 |
| FlatStackBuf_Decode         | AOT        | ILCompiler 10.0.4 |  70.95 ns | 0.124 ns | 0.116 ns | 0.56 |      - |   - | 0.00 |

### DecodeObjectApiBenchmarks

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | RatioSD | Gen0 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_ObjectApi_Decode | DefaultJob | Default           | 344.7 ns | 0.98 ns | 0.76 ns | 1.00 | 0.00 | 0.0205 | 1032 B | 1.00 |
| FlatSpanBuffers_ObjectApi_Decode      | DefaultJob | Default           | 164.0 ns | 0.58 ns | 0.51 ns | 0.48 | 0.00 | 0.0129 |  648 B | 0.63 |
| FlatStackBuf_ObjectApi_Decode         | DefaultJob | Default           | 135.0 ns | 0.38 ns | 0.30 ns | 0.39 | 0.00 | 0.0129 |  648 B | 0.63 |
|                                       |            |                   |          |         |         |      |      |        |        |      |
| Original_FlatBuffers_ObjectApi_Decode | AOT        | ILCompiler 10.0.4 | 363.7 ns | 6.30 ns | 5.90 ns | 1.00 | 0.02 | 0.0205 | 1032 B | 1.00 |
| FlatSpanBuffers_ObjectApi_Decode      | AOT        | ILCompiler 10.0.4 | 243.6 ns | 0.97 ns | 0.86 ns | 0.67 | 0.01 | 0.0129 |  648 B | 0.63 |
| FlatStackBuf_ObjectApi_Decode         | AOT        | ILCompiler 10.0.4 | 229.4 ns | 0.41 ns | 0.34 ns | 0.63 | 0.01 | 0.0129 |  648 B | 0.63 |

### EncodeBenchmarks

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_Encode | DefaultJob | Default           | 275.8 ns | 0.85 ns | 0.75 ns | 1.00 |      - | - | NA |
| FlatSpanBuffers_Encode      | DefaultJob | Default           | 175.7 ns | 0.23 ns | 0.22 ns | 0.64 |      - | - | NA |
| FlatStackBuf_Encode         | DefaultJob | Default           | 128.0 ns | 0.40 ns | 0.36 ns | 0.46 |      - | - | NA |
|                             |            |                   |          |         |         |      |        |   |    |
| Original_FlatBuffers_Encode | AOT        | ILCompiler 10.0.4 | 364.5 ns | 0.83 ns | 0.69 ns | 1.00 | 0.0005 | 40 B | 1.00 |
| FlatSpanBuffers_Encode      | AOT        | ILCompiler 10.0.4 | 188.8 ns | 0.22 ns | 0.19 ns | 0.52 |      - |  - | 0.00 |
| FlatStackBuf_Encode         | AOT        | ILCompiler 10.0.4 | 138.3 ns | 0.15 ns | 0.13 ns | 0.38 |      - |  - | 0.00 |

### EncodeObjectApiBenchmarks

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_ObjectApi_Encode | DefaultJob | Default           | 312.0 ns | 1.45 ns | 1.21 ns | 1.00 | 0.0005 |  40 B | 1.00 |
| FlatSpanBuffers_ObjectApi_Encode      | DefaultJob | Default           | 183.9 ns | 0.22 ns | 0.20 ns | 0.59 |      - |   - | 0.00 |
| FlatStackBuf_ObjectApi_Encode         | DefaultJob | Default           | 177.9 ns | 0.48 ns | 0.37 ns | 0.57 |      - |   - | 0.00 |
|                                       |            |                   |          |         |         |      |        |     |      |
| Original_FlatBuffers_ObjectApi_Encode | AOT        | ILCompiler 10.0.4 | 372.3 ns | 1.32 ns | 1.10 ns | 1.00 | 0.0005 |  40 B | 1.00 |
| FlatSpanBuffers_ObjectApi_Encode      | AOT        | ILCompiler 10.0.4 | 194.7 ns | 0.31 ns | 0.27 ns | 0.52 |      - |   - | 0.00 |
| FlatStackBuf_ObjectApi_Encode         | AOT        | ILCompiler 10.0.4 | 193.3 ns | 0.61 ns | 0.57 ns | 0.52 |      - |   - | 0.00 |

### VerifyBenchmarks

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | RatioSD | Gen0 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| Original_FlatBuffers_Verify | DefaultJob | Default           | 160.04 ns | 0.365 ns | 0.285 ns | 1.00 | 0.00 | 0.0026 | 136 B | 1.00 |
| FlatSpanBuffers_Verify      | DefaultJob | Default           |  59.96 ns | 0.095 ns | 0.084 ns | 0.37 | 0.00 |      - |   - | 0.00 |
| FlatStackBuf_Verify         | DefaultJob | Default           |  60.17 ns | 0.273 ns | 0.255 ns | 0.38 | 0.00 |      - |     | 0.00 |
|                             |            |                   |           |          |          |      |      |        |     |      |
| Original_FlatBuffers_Verify | AOT        | ILCompiler 10.0.4 | 228.26 ns | 4.426 ns | 5.597 ns | 1.00 | 0.03 | 0.0026 | 136 B | 1.00 |
| FlatSpanBuffers_Verify      | AOT        | ILCompiler 10.0.4 |  65.67 ns | 0.662 ns | 0.619 ns | 0.29 | 0.01 |      - |   - | 0.00 |
| FlatStackBuf_Verify         | AOT        | ILCompiler 10.0.4 |  72.10 ns | 0.322 ns | 0.269 ns | 0.32 | 0.01 |      - |   - | 0.00 |

---

## vs FlatSharp

30-item FooBarContainer payload. FlatSharp = 1.00 baseline.

### FlatSharpEncode

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|
| FlatSharp_Encode                 | DefaultJob | Default           |   925.1 ns | 1.33 ns | 1.18 ns | 1.00 | - | NA |
| FlatSpanBuffers_Encode           | DefaultJob | Default           | 1,025.8 ns | 2.87 ns | 2.69 ns | 1.11 | - | NA |
| FlatSpanBuffers_ObjectApi_Encode | DefaultJob | Default           | 1,455.0 ns | 0.78 ns | 0.65 ns | 1.57 | - | NA |
|                                  |            |                   |            |         |         |      |   |    |
| FlatSharp_Encode                 | AOT        | ILCompiler 10.0.4 |   876.2 ns | 0.83 ns | 0.78 ns | 1.00 | - | NA |
| FlatSpanBuffers_Encode           | AOT        | ILCompiler 10.0.4 | 1,107.5 ns | 1.95 ns | 1.73 ns | 1.26 | - | NA |
| FlatSpanBuffers_ObjectApi_Encode | AOT        | ILCompiler 10.0.4 | 1,514.6 ns | 0.63 ns | 0.59 ns | 1.73 | - | NA |

### FlatSharpDecodeLazy

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | RatioSD | Gen0 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| FlatSharp_Decode                 | DefaultJob | Default           |   582.8 ns |  3.42 ns |  3.03 ns | 1.00 | 0.01 | 0.1020 | 5152 B | 1.00 |
| FlatSpanBuffers_Decode           | DefaultJob | Default           |   181.6 ns |  0.54 ns |  0.50 ns | 0.31 | 0.00 |      - |     - | 0.00 |
| FlatSpanBuffers_ObjectApi_Decode | DefaultJob | Default           | 1,086.6 ns |  3.62 ns |  3.21 ns | 1.86 | 0.01 | 0.1030 | 5184 B | 1.01 |
|                                  |            |                   |            |          |          |      |      |        |        |      |
| FlatSharp_Decode                 | AOT        | ILCompiler 10.0.4 |   947.7 ns | 12.73 ns | 10.63 ns | 1.00 | 0.02 | 0.1020 | 5152 B | 1.00 |
| FlatSpanBuffers_Decode           | AOT        | ILCompiler 10.0.4 |   553.9 ns |  2.22 ns |  2.08 ns | 0.58 | 0.01 |      - |     - | 0.00 |
| FlatSpanBuffers_ObjectApi_Decode | AOT        | ILCompiler 10.0.4 | 1,992.4 ns | 15.41 ns | 13.66 ns | 2.10 | 0.03 | 0.1030 | 5184 B | 1.01 |

### FlatSharpDecodeGreedy

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | Gen0 | Gen1 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| FlatSharp_Decode                 | DefaultJob | Default           |   993.5 ns |  5.59 ns |  5.23 ns | 1.00 | 0.1373 | 0.0019 | 6904 B | 1.00 |
| FlatSpanBuffers_Decode           | DefaultJob | Default           |   181.6 ns |  0.33 ns |  0.27 ns | 0.18 |      - |      - |     - | 0.00 |
| FlatSpanBuffers_ObjectApi_Decode | DefaultJob | Default           | 1,005.5 ns |  1.37 ns |  1.14 ns | 1.01 | 0.1030 |      - | 5184 B | 0.75 |
|                                  |            |                   |            |          |          |      |        |        |        |      |
| FlatSharp_Decode                 | AOT        | ILCompiler 10.0.4 | 1,433.4 ns | 11.97 ns | 11.20 ns | 1.00 | 0.1373 | 0.0019 | 6904 B | 1.00 |
| FlatSpanBuffers_Decode           | AOT        | ILCompiler 10.0.4 |   554.3 ns |  0.93 ns |  0.82 ns | 0.39 |      - |      - |     - | 0.00 |
| FlatSpanBuffers_ObjectApi_Decode | AOT        | ILCompiler 10.0.4 | 1,972.4 ns |  4.28 ns |  3.58 ns | 1.38 | 0.1030 |      - | 5184 B | 0.75 |

### FlatSharpSortedVectorStringKey

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|
| FlatSharp_EncodeSorted       | DefaultJob | Default           | 1.159 μs | 0.0022 μs | 0.0019 μs | 1.00 |      - |  88 B | 1.00 |
| FlatSpanBuffers_EncodeSorted | DefaultJob | Default           | 1.871 μs | 0.0084 μs | 0.0079 μs | 1.61 |      - |   - | 0.00 |
| FlatSharp_Lookup             | DefaultJob | Default           | 2.040 μs | 0.0037 μs | 0.0031 μs | 1.76 | 0.0420 | 2160 B | 24.55 |
| FlatSpanBuffers_Lookup       | DefaultJob | Default           | 1.219 μs | 0.0012 μs | 0.0011 μs | 1.05 |      - |   - |  0.00 |
|                              |            |                   |          |           |           |      |        |       |       |
| FlatSharp_EncodeSorted       | AOT        | ILCompiler 10.0.4 | 1.456 μs | 0.0046 μs | 0.0040 μs | 1.00 | 0.0019 | 128 B |  1.00 |
| FlatSpanBuffers_EncodeSorted | AOT        | ILCompiler 10.0.4 | 2.812 μs | 0.0031 μs | 0.0027 μs | 1.93 |      - |   - |  0.00 |
| FlatSharp_Lookup             | AOT        | ILCompiler 10.0.4 | 2.944 μs | 0.0215 μs | 0.0201 μs | 2.02 | 0.0839 | 4368 B | 34.12 |
| FlatSpanBuffers_Lookup       | AOT        | ILCompiler 10.0.4 | 1.222 μs | 0.0012 μs | 0.0012 μs | 0.84 |      - |   - |  0.00 |

### FlatSharpSortedVectorIntKey

| Method | Job | Toolchain | Mean | Error | StdDev | Ratio | RatioSD | Gen0 | Allocated | Alloc Ratio |
|---|---|---|--:|--:|--:|--:|--:|--:|--:|--:|
| FlatSharp_EncodeSorted       | DefaultJob | Default           |   919.6 ns |  1.26 ns |  1.05 ns | 1.00 | 0.00 |      - |    88 B |   1.00 |
| FlatSpanBuffers_EncodeSorted | DefaultJob | Default           |   985.6 ns |  1.19 ns |  1.11 ns | 1.07 | 0.00 |      - |       - |   0.00 |
| FlatSharp_Lookup             | DefaultJob | Default           | 1,444.6 ns | 19.99 ns | 18.70 ns | 1.57 | 0.02 | 0.1469 |  7392 B |  84.00 |
| FlatSpanBuffers_Lookup       | DefaultJob | Default           |   395.0 ns |  0.36 ns |  0.32 ns | 0.43 | 0.00 |      - |       - |   0.00 |
|                              |            |                   |            |          |          |      |      |        |         |        |
| FlatSharp_EncodeSorted       | AOT        | ILCompiler 10.0.4 | 1,008.5 ns |  1.32 ns |  1.10 ns | 1.00 | 0.00 | 0.0019 |   128 B |   1.00 |
| FlatSpanBuffers_EncodeSorted | AOT        | ILCompiler 10.0.4 | 1,481.5 ns |  6.13 ns |  5.44 ns | 1.47 | 0.01 |      - |       - |   0.00 |
| FlatSharp_Lookup             | AOT        | ILCompiler 10.0.4 | 3,211.1 ns | 61.19 ns | 68.01 ns | 3.18 | 0.07 | 0.1907 |  9600 B |  75.00 |
| FlatSpanBuffers_Lookup       | AOT        | ILCompiler 10.0.4 |   440.7 ns |  0.92 ns |  0.86 ns | 0.44 | 0.00 |      - |       - |   0.00 |
