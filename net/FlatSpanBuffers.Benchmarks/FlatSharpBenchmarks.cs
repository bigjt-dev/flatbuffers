// FlatSharp vs FlatSpanBuffers comparison benchmarks.
// Both libraries use lazy (on-demand) deserialization for the decode benchmarks.
// Encode benchmarks compare the primary serialization API of each library.

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FlatSharp;

// FlatSpanBuffers generated types (ByteBuffer variant)
using FlatSpanFooBarContainer = Benchmarks.FlatSpanBuffers.FooBarContainer;
using FlatSpanFooBar = Benchmarks.FlatSpanBuffers.FooBar;
using FlatSpanBar = Benchmarks.FlatSpanBuffers.Bar;
using FlatSpanFruit = Benchmarks.FlatSpanBuffers.Fruit;

// FlatSpanBuffers generated types (StackBuffer / span variant)
using StackFooBarContainer = Benchmarks.FlatSpanBuffers.StackBuffer.FooBarContainer;
using StackFooBar = Benchmarks.FlatSpanBuffers.StackBuffer.FooBar;
using StackBar = Benchmarks.FlatSpanBuffers.StackBuffer.Bar;

// FlatSpanBuffers Object API types
using FlatSpanFooBarContainerT = Benchmarks.FlatSpanBuffers.FooBarContainerT;
using FlatSpanFooBarT = Benchmarks.FlatSpanBuffers.FooBarT;
using FlatSpanBarT = Benchmarks.FlatSpanBuffers.BarT;
using FlatSpanFooT = Benchmarks.FlatSpanBuffers.FooT;

// FlatSharp generated types
using FsFooBarContainer = Benchmarks.FlatSharp.FooBarContainer;
using FsFooBar = Benchmarks.FlatSharp.FooBar;
using FsBar = Benchmarks.FlatSharp.Bar;
using FsFoo = Benchmarks.FlatSharp.Foo;
using FsFruit = Benchmarks.FlatSharp.Fruit;

namespace FlatSpanBuffers.Benchmarks;

// Compares encode performance between FlatSpanBuffers and FlatSharp.
// FlatSharp uses object-based serialization, so we compare against both
// the FlatSpanBuffers builder API and the Object API.
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class FlatSharpEncodeComparison
{
    private const int ListCount = 3;

    private string[] _encodeStrings = null;

    private FlatSpanFooBarContainerT _flatSpanContainerT = null;

    private FsFooBarContainer _fsContainer = null;
    private int _fsMaxSize;

    [GlobalSetup]
    public void Setup()
    {
        _encodeStrings = new string[ListCount];
        for (int i = 0; i < ListCount; i++)
            _encodeStrings[i] = $"FooBar{i}";

        // FlatSpanBuffers Object API container
        _flatSpanContainerT = new FlatSpanFooBarContainerT
        {
            List = new List<FlatSpanFooBarT>(),
            Initialized = true,
            Fruit = FlatSpanFruit.Bananas,
            Location = "SomeLocation"
        };
        for (int j = 0; j < ListCount; j++)
        {
            _flatSpanContainerT.List.Add(new FlatSpanFooBarT
            {
                Sibling = new FlatSpanBarT
                {
                    Parent = new FlatSpanFooT
                    {
                        Id = (ulong)(3 + j),
                        Count = (short)j,
                        Prefix = (sbyte)j,
                        Length = (uint)(j * 10)
                    },
                    Time = j * 1000,
                    Ratio = 0.5f + j,
                    Size = (ushort)(100 + j)
                },
                Name = $"FooBar{j}",
                Rating = 3.14159 + j,
                Postfix = (byte)j
            });
        }

        // FlatSharp container (object graph)
        _fsContainer = new FsFooBarContainer
        {
            List = new List<FsFooBar>(),
            Initialized = true,
            Fruit = FsFruit.Bananas,
            Location = "SomeLocation"
        };
        for (int j = 0; j < ListCount; j++)
        {
            _fsContainer.List.Add(new FsFooBar
            {
                Sibling = new FsBar
                {
                    Parent = new FsFoo
                    {
                        Id = (ulong)(3 + j),
                        Count = (short)j,
                        Prefix = (sbyte)j,
                        Length = (uint)(j * 10)
                    },
                    Time = j * 1000,
                    Ratio = 0.5f + j,
                    Size = (ushort)(100 + j)
                },
                Name = $"FooBar{j}",
                Rating = 3.14159 + j,
                Postfix = (byte)j
            });
        }

        // Compute FlatSharp encode buffer size for stackalloc
        _fsMaxSize = FsFooBarContainer.Serializer.GetMaxSize(_fsContainer);
    }

    [Benchmark(Baseline = true)]
    public int FlatSharp_Encode()
    {
        Span<byte> buffer = stackalloc byte[1024];
        return FsFooBarContainer.Serializer.Write(buffer, _fsContainer);
    }

    [Benchmark]
    public void FlatSpanBuffers_SpanBuilderEncode()
    {
        Span<byte> buffer = stackalloc byte[512];
        Span<int> vtableSpace = stackalloc int[16];
        Span<int> vtableOffsetSpace = stackalloc int[16];
        var byteSpanBuffer = new ByteSpanBuffer(buffer);
        var builder = new FlatSpanBufferBuilder(byteSpanBuffer, vtableSpace, vtableOffsetSpace);

        Span<FlatSpanBuffers.Offset<StackFooBar>> fooBarOffsets =
            stackalloc FlatSpanBuffers.Offset<StackFooBar>[ListCount];

        for (int j = 0; j < ListCount; j++)
        {
            var nameOffset = builder.CreateString(_encodeStrings[j]);
            StackFooBar.StartFooBar(ref builder);
            StackFooBar.AddSibling(ref builder, StackBar.CreateBar(ref builder,
                (ulong)(3 + j), (short)j, (sbyte)j, (uint)(j * 10),
                j * 1000, 0.5f + j, (ushort)(100 + j)));
            StackFooBar.AddName(ref builder, nameOffset);
            StackFooBar.AddRating(ref builder, 3.14159 + j);
            StackFooBar.AddPostfix(ref builder, (byte)j);
            fooBarOffsets[j] = StackFooBar.EndFooBar(ref builder);
        }

        var listOffset = StackFooBarContainer.CreateListVector(ref builder, fooBarOffsets);
        var locationOffset = builder.CreateString("SomeLocation");

        StackFooBarContainer.StartFooBarContainer(ref builder);
        StackFooBarContainer.AddList(ref builder, listOffset);
        StackFooBarContainer.AddInitialized(ref builder, true);
        StackFooBarContainer.AddFruit(ref builder, FlatSpanFruit.Bananas);
        StackFooBarContainer.AddLocation(ref builder, locationOffset);
        var rootOffset = StackFooBarContainer.EndFooBarContainer(ref builder);
        builder.Finish(rootOffset.Value);
    }

    [Benchmark]
    public void FlatSpanBuffers_ObjectApiEncode()
    {
        Span<byte> buffer = stackalloc byte[512];
        Span<int> vtableSpace = stackalloc int[16];
        Span<int> vtableOffsetSpace = stackalloc int[16];
        var byteSpanBuffer = new ByteSpanBuffer(buffer);
        var builder = new FlatSpanBufferBuilder(byteSpanBuffer, vtableSpace, vtableOffsetSpace);

        var offset = StackFooBarContainer.Pack(ref builder, _flatSpanContainerT);
        StackFooBarContainer.FinishFooBarContainerBuffer(ref builder, offset);
    }
}

// Compares lazy decode performance between FlatSpanBuffers and FlatSharp.
// Fields are read from the buffer on demand; the benchmark accesses all fields
// to measure real-world traversal cost.
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class FlatSharpLazyDecodeComparison
{
    private byte[] _encodedData = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Build a FooBarContainer with FlatSharp and use that buffer.
        // Wire format is standard FlatBuffers, readable by both libraries.
        var container = new FsFooBarContainer
        {
            List = new List<FsFooBar>(),
            Initialized = true,
            Fruit = FsFruit.Bananas,
            Location = "SomeLocation"
        };
        for (int j = 0; j < 3; j++)
        {
            container.List.Add(new FsFooBar
            {
                Sibling = new FsBar
                {
                    Parent = new FsFoo
                    {
                        Id = (ulong)j,
                        Count = (short)j,
                        Prefix = (sbyte)j,
                        Length = (uint)(j * 10)
                    },
                    Time = j * 1000,
                    Ratio = 0.5f + j,
                    Size = (ushort)(100 + j)
                },
                Name = $"FooBar{j}",
                Rating = 3.14159 + j,
                Postfix = (byte)j
            });
        }

        int maxSize = FsFooBarContainer.Serializer.GetMaxSize(container);
        byte[] buf = new byte[maxSize];
        int written = FsFooBarContainer.Serializer.Write(buf, container);
        _encodedData = buf.AsSpan(0, written).ToArray();
    }

    [Benchmark(Baseline = true)]
    public long FlatSharp_LazyDecode()
    {
        long sum = 0;
        var container = FsFooBarContainer.Serializer.Parse(_encodedData);

        sum += container.Initialized ? 1 : 0;
        sum += (long)container.Fruit;

        var list = container.List;
        if (list != null)
        {
            for (int j = 0; j < list.Count; j++)
            {
                var fooBar = list[j];
                sum += (long)fooBar.Rating;
                sum += fooBar.Postfix;
                var sibling = fooBar.Sibling;
                if (sibling != null)
                {
                    sum += sibling.Time;
                    sum += sibling.Size;
                    var parent = sibling.Parent;
                    sum += (long)parent.Id;
                    sum += parent.Count;
                }
            }
        }

        return sum;
    }

    [Benchmark]
    public long FlatSpanBuffers_LazyDecode()
    {
        long sum = 0;
        var bb = new ByteSpanBuffer(_encodedData);
        var container = StackFooBarContainer.GetRootAsFooBarContainer(bb);

        sum += container.Initialized ? 1 : 0;
        sum += (long)container.Fruit;

        var list = container.List;
        if (list.HasValue)
        {
            var listValue = list.Value;
            for (int j = 0; j < listValue.Length; j++)
            {
                var fooBar = listValue[j];
                sum += (long)fooBar.Rating;
                sum += fooBar.Postfix;
                var sibling = fooBar.Sibling;
                if (sibling.HasValue)
                {
                    var siblingValue = sibling.Value;
                    sum += siblingValue.Time;
                    sum += siblingValue.Size;
                    var parent = siblingValue.Parent;
                    sum += (long)parent.Id;
                    sum += parent.Count;
                }
            }
        }

        return sum;
    }
}

// Compares greedy decode performance between FlatSpanBuffers and FlatSharp.
// FlatSharp uses its Greedy serializer; FlatSpanBuffers uses the Object API UnPack.
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class FlatSharpGreedyDecodeComparison
{
    private byte[] _encodedData = null!;

    private static ISerializer<FsFooBarContainer> _fsGreedySerializer = GetGreedySerializer<FsFooBarContainer>();
    private static ISerializer<T> GetGreedySerializer<T>() where T : class, IFlatBufferSerializable<T> => T.GreedySerializer;

    [GlobalSetup]
    public void Setup()
    {
        var container = new FsFooBarContainer
        {
            List = new List<FsFooBar>(),
            Initialized = true,
            Fruit = FsFruit.Bananas,
            Location = "SomeLocation"
        };
        for (int j = 0; j < 3; j++)
        {
            container.List.Add(new FsFooBar
            {
                Sibling = new FsBar
                {
                    Parent = new FsFoo
                    {
                        Id = (ulong)j,
                        Count = (short)j,
                        Prefix = (sbyte)j,
                        Length = (uint)(j * 10)
                    },
                    Time = j * 1000,
                    Ratio = 0.5f + j,
                    Size = (ushort)(100 + j)
                },
                Name = $"FooBar{j}",
                Rating = 3.14159 + j,
                Postfix = (byte)j
            });
        }

        int maxSize = FsFooBarContainer.Serializer.GetMaxSize(container);
        byte[] buf = new byte[maxSize];
        int written = FsFooBarContainer.Serializer.Write(buf, container);
        _encodedData = buf.AsSpan(0, written).ToArray();
    }

    [Benchmark(Baseline = true)]
    public FsFooBarContainer FlatSharp_GreedyDecode()
    {
        return _fsGreedySerializer.Parse(_encodedData);
    }

    [Benchmark]
    public FlatSpanFooBarContainerT FlatSpanBuffers_GreedyDecode()
    {
        var bb = new ByteSpanBuffer(_encodedData);
        return StackFooBarContainer.GetRootAsFooBarContainer(bb).UnPack();
    }
}
