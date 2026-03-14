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

// Benchmarks modelled after the FlatSharp project's primary benchmark suite.
// https://github.com/jamescourtney/FlatSharp

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FlatSharp;

using StackFooBarContainer = Benchmarks.FlatSpanBuffers.StackBuffer.FooBarContainer;
using StackFooBar = Benchmarks.FlatSpanBuffers.StackBuffer.FooBar;
using StackBar = Benchmarks.FlatSpanBuffers.StackBuffer.Bar;
using FlatSpanFruit = Benchmarks.FlatSpanBuffers.Fruit;

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

// FlatSharp sorted vector types
using FsSortedStringItem = Benchmarks.FlatSharp.SortedStringItem;
using FsSortedStringContainer = Benchmarks.FlatSharp.SortedStringContainer;
using FsSortedIntItem = Benchmarks.FlatSharp.SortedIntItem;
using FsSortedIntContainer = Benchmarks.FlatSharp.SortedIntContainer;

// FlatSpanBuffers sorted vector types
using StackSortedStringItem = Benchmarks.FlatSpanBuffers.StackBuffer.SortedStringItem;
using StackSortedStringContainer = Benchmarks.FlatSpanBuffers.StackBuffer.SortedStringContainer;
using StackSortedIntItem = Benchmarks.FlatSpanBuffers.StackBuffer.SortedIntItem;
using StackSortedIntContainer = Benchmarks.FlatSpanBuffers.StackBuffer.SortedIntContainer;

// FlatSpanBuffers runtime helpers
using ByteSpanBuffer = FlatSpanBuffers.ByteSpanBuffer;
using FlatSpanBufferBuilder = FlatSpanBuffers.FlatSpanBufferBuilder;

namespace FlatSpanBuffers.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class FlatSharpEncode
{
    private const int ListCount = 30;
    private const int SerializeBufferSize = 4096;

    private byte[] _writeBuffer = null!;
    private int[] _vtableSpace = null!;
    private int[] _vtableOffsetSpace = null!;
    private string[] _encodeStrings = null!;
    private FsFooBarContainer _fsContainer = null!;
    private FlatSpanFooBarContainerT _flatSpanContainerT = null!;

    [GlobalSetup]
    public void Setup()
    {
        _encodeStrings = new string[ListCount];
        for (int i = 0; i < ListCount; i++)
            _encodeStrings[i] = $"FooBar{i}";

        _writeBuffer = new byte[SerializeBufferSize];
        _vtableSpace = new int[32];
        _vtableOffsetSpace = new int[32];

        _fsContainer = new FsFooBarContainer
        {
            List = new List<FsFooBar>(ListCount),
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
                Name = _encodeStrings[j],
                Rating = 3.14159 + j,
                Postfix = (byte)j
            });
        }

        _flatSpanContainerT = new FlatSpanFooBarContainerT
        {
            List = new List<FlatSpanFooBarT>(ListCount),
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
                Name = _encodeStrings[j],
                Rating = 3.14159 + j,
                Postfix = (byte)j
            });
        }
    }

    [Benchmark(Baseline = true)]
    public int FlatSharp_Encode()
    {
        return FsFooBarContainer.Serializer.Write(_writeBuffer, _fsContainer);
    }

    [Benchmark]
    public void FlatSpanBuffers_Encode()
    {
        var spanBb = new ByteSpanBuffer(_writeBuffer);
        var builder = new FlatSpanBufferBuilder(spanBb, _vtableSpace, _vtableOffsetSpace);

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
    public void FlatSpanBuffers_ObjectApi_Encode()
    {
        var spanBb = new ByteSpanBuffer(_writeBuffer);
        var builder = new FlatSpanBufferBuilder(spanBb, _vtableSpace, _vtableOffsetSpace);
        var rootOffset = StackFooBarContainer.Pack(ref builder, _flatSpanContainerT, Span<int>.Empty);
        builder.Finish(rootOffset.Value);
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class FlatSharpDecodeLazy
{
    private const int ListCount = 30;
    private byte[] _encodedData = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Encode using FlatSharp so the wire bytes match what FlatSharp produces.
        var writeBuffer = new byte[4096];
        var fsContainer = new FsFooBarContainer
        {
            List = new List<FsFooBar>(ListCount),
            Initialized = true,
            Fruit = FsFruit.Bananas,
            Location = "SomeLocation"
        };
        for (int j = 0; j < ListCount; j++)
        {
            fsContainer.List.Add(new FsFooBar
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
        int written = FsFooBarContainer.Serializer.Write(writeBuffer, fsContainer);
        _encodedData = writeBuffer[..written];
    }

    [Benchmark(Baseline = true)]
    public long FlatSharp_Decode()
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
    public long FlatSpanBuffers_Decode()
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

    [Benchmark]
    public long FlatSpanBuffers_ObjectApi_Decode()
    {
        long sum = 0;
        var bb = new ByteSpanBuffer(_encodedData);
        var container = StackFooBarContainer.GetRootAsFooBarContainer(bb).UnPack();

        sum += container.Initialized ? 1 : 0;
        sum += (long)container.Fruit;

        if (container.List != null)
        {
            for (int j = 0; j < container.List.Count; j++)
            {
                var fooBar = container.List[j];
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
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class FlatSharpDecodeGreedy
{
    private const int ListCount = 30;
    private byte[] _encodedData = null!;

    private static readonly ISerializer<FsFooBarContainer> FsGreedySerializer =
        GetGreedySerializer<FsFooBarContainer>();

    private static ISerializer<T> GetGreedySerializer<T>()
        where T : class, IFlatBufferSerializable<T> => T.GreedySerializer;

    [GlobalSetup]
    public void Setup()
    {
        var writeBuffer = new byte[4096];
        var fsContainer = new FsFooBarContainer
        {
            List = new List<FsFooBar>(ListCount),
            Initialized = true,
            Fruit = FsFruit.Bananas,
            Location = "SomeLocation"
        };
        for (int j = 0; j < ListCount; j++)
        {
            fsContainer.List.Add(new FsFooBar
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
        int written = FsFooBarContainer.Serializer.Write(writeBuffer, fsContainer);
        _encodedData = writeBuffer[..written];
    }

    [Benchmark(Baseline = true)]
    public FsFooBarContainer FlatSharp_Decode()
    {
        return FsGreedySerializer.Parse(_encodedData);
    }

    [Benchmark]
    public long FlatSpanBuffers_Decode()
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

    [Benchmark]
    public FlatSpanFooBarContainerT FlatSpanBuffers_ObjectApi_Decode()
    {
        var bb = new ByteSpanBuffer(_encodedData);
        return StackFooBarContainer.GetRootAsFooBarContainer(bb).UnPack();
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class FlatSharpSortedVectorStringKey
{
    private const int ListCount = 30;
    private const int SerializeBufferSize = 4096;

    private byte[] _writeBuffer = null!;
    private int[] _vtableSpace = null!;
    private int[] _vtableOffsetSpace = null!;
    private string[] _stringKeys = null!;
    private byte[] _sortedStringData = null!;
    private FsSortedStringContainer _fsSortedStringContainer = null!;
    private FsSortedStringContainer _parsedFsStringContainer = null!;
    private FlatSharp.IIndexedVector<string, FsSortedStringItem> _stringItemsLookup = null!;

    [GlobalSetup]
    public void Setup()
    {
        _stringKeys = new string[ListCount];
        for (int i = 0; i < ListCount; i++)
            _stringKeys[i] = Guid.NewGuid().ToString();

        _writeBuffer = new byte[SerializeBufferSize];
        _vtableSpace = new int[32];
        _vtableOffsetSpace = new int[32];

        var fsStringItems = new FlatSharp.IndexedVector<string, FsSortedStringItem>();
        for (int i = 0; i < ListCount; i++)
            fsStringItems.AddOrReplace(new FsSortedStringItem { Key = _stringKeys[i] });
        _fsSortedStringContainer = new FsSortedStringContainer { Items = fsStringItems };

        int written = FsSortedStringContainer.Serializer.Write(_writeBuffer, _fsSortedStringContainer);
        _sortedStringData = _writeBuffer[..written];

        _parsedFsStringContainer = FsSortedStringContainer.Serializer.Parse(_sortedStringData);
        _parsedFsStringContainer.Items!.TryGetValue(_stringKeys[0], out _);
        _stringItemsLookup = _parsedFsStringContainer.Items!;
    }

    [Benchmark(Baseline = true)]
    public int FlatSharp_EncodeSorted()
    {
        return FsSortedStringContainer.Serializer.Write(_writeBuffer, _fsSortedStringContainer);
    }

    [Benchmark]
    public void FlatSpanBuffers_EncodeSorted()
    {
        var spanBb = new ByteSpanBuffer(_writeBuffer);
        var builder = new FlatSpanBufferBuilder(spanBb, _vtableSpace, _vtableOffsetSpace);

        Span<FlatSpanBuffers.Offset<StackSortedStringItem>> offsets =
            stackalloc FlatSpanBuffers.Offset<StackSortedStringItem>[ListCount];

        for (int j = 0; j < ListCount; j++)
        {
            var keyOffset = builder.CreateString(_stringKeys[j]);
            StackSortedStringItem.StartSortedStringItem(ref builder);
            StackSortedStringItem.AddKey(ref builder, keyOffset);
            offsets[j] = StackSortedStringItem.EndSortedStringItem(ref builder);
        }
        var itemsOffset = StackSortedStringItem.CreateSortedVectorOfSortedStringItem(ref builder, offsets);
        StackSortedStringContainer.StartSortedStringContainer(ref builder);
        StackSortedStringContainer.AddItems(ref builder, itemsOffset);
        var rootOffset = StackSortedStringContainer.EndSortedStringContainer(ref builder);
        builder.Finish(rootOffset.Value);
    }

    [Benchmark]
    public int FlatSharp_Lookup()
    {
        int count = 0;
        var itemsLookup = _stringItemsLookup;
        for (int i = 0; i < ListCount; i++)
        {
            if (itemsLookup.TryGetValue(_stringKeys[i], out _))
                count++;
        }
        return count;
    }

    [Benchmark]
    public int FlatSpanBuffers_Lookup()
    {
        int count = 0;
        var bb = new ByteSpanBuffer(_sortedStringData);
        var container = StackSortedStringContainer.GetRootAsSortedStringContainer(bb);
        for (int i = 0; i < ListCount; i++)
        {
            if (container.TryGetItemsByKey(_stringKeys[i], out _))
                count++;
        }
        return count;
    }
}

// Measures serialize-sorted (sort + encode) and lookup (binary search all 30 keys on a pre-parsed container)
// for a 30-item sorted vector keyed by int.
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class FlatSharpSortedVectorIntKey
{
    private const int ListCount = 30;
    private const int SerializeBufferSize = 1024;

    private byte[] _writeBuffer = null!;
    private int[] _vtableSpace = null!;
    private int[] _vtableOffsetSpace = null!;
    private int[] _intKeys = null!;
    private byte[] _sortedIntData = null!;
    private FsSortedIntContainer _fsSortedIntContainer = null!;
    private FsSortedIntContainer _parsedFsIntContainer = null!;
    private FlatSharp.IIndexedVector<int, FsSortedIntItem> _intItemsLookup = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        var keySet = new System.Collections.Generic.HashSet<int>(ListCount);
        _intKeys = new int[ListCount];
        for (int i = 0; i < ListCount; i++)
        {
            int k;
            do { k = rng.Next(); } while (!keySet.Add(k));
            _intKeys[i] = k;
        }

        _writeBuffer = new byte[SerializeBufferSize];
        _vtableSpace = new int[32];
        _vtableOffsetSpace = new int[32];

        var fsIntItems = new FlatSharp.IndexedVector<int, FsSortedIntItem>();
        for (int i = 0; i < ListCount; i++)
            fsIntItems.AddOrReplace(new FsSortedIntItem { Key = _intKeys[i] });
        _fsSortedIntContainer = new FsSortedIntContainer { Items = fsIntItems };

        int written = FsSortedIntContainer.Serializer.Write(_writeBuffer, _fsSortedIntContainer);
        _sortedIntData = _writeBuffer[..written];

        _parsedFsIntContainer = FsSortedIntContainer.Serializer.Parse(_sortedIntData);
        _parsedFsIntContainer.Items!.TryGetValue(_intKeys[0], out _); // warm up the indexed vector
        _intItemsLookup = _parsedFsIntContainer.Items!;
    }

    [Benchmark(Baseline = true)]
    public int FlatSharp_EncodeSorted()
    {
        return FsSortedIntContainer.Serializer.Write(_writeBuffer, _fsSortedIntContainer);
    }

    [Benchmark]
    public void FlatSpanBuffers_EncodeSorted()
    {
        var spanBb = new ByteSpanBuffer(_writeBuffer);
        var builder = new FlatSpanBufferBuilder(spanBb, _vtableSpace, _vtableOffsetSpace);

        Span<FlatSpanBuffers.Offset<StackSortedIntItem>> offsets =
            stackalloc FlatSpanBuffers.Offset<StackSortedIntItem>[ListCount];

        for (int j = 0; j < ListCount; j++)
        {
            StackSortedIntItem.StartSortedIntItem(ref builder);
            StackSortedIntItem.AddKey(ref builder, _intKeys[j]);
            offsets[j] = StackSortedIntItem.EndSortedIntItem(ref builder);
        }
        var itemsOffset = StackSortedIntItem.CreateSortedVectorOfSortedIntItem(ref builder, offsets);
        StackSortedIntContainer.StartSortedIntContainer(ref builder);
        StackSortedIntContainer.AddItems(ref builder, itemsOffset);
        var rootOffset = StackSortedIntContainer.EndSortedIntContainer(ref builder);
        builder.Finish(rootOffset.Value);
    }

    [Benchmark]
    public int FlatSharp_Lookup()
    {
        int count = 0;
        var itemsLookup = _intItemsLookup;
        for (int i = 0; i < ListCount; i++)
        {
            if (itemsLookup!.TryGetValue(_intKeys[i], out _))
                count++;
        }
        return count;
    }

    [Benchmark]
    public int FlatSpanBuffers_Lookup()
    {
        int count = 0;
        var bb = new ByteSpanBuffer(_sortedIntData);
        var container = StackSortedIntContainer.GetRootAsSortedIntContainer(bb);
        for (int i = 0; i < ListCount; i++)
        {
            if (container.TryGetItemsByKey(_intKeys[i], out _))
                count++;
        }
        return count;
    }
}
