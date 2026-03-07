# FlatSpanBuffers

**A Modern C# Runtime for FlatBuffers**

FlatSpanBuffers is a span-centric C# runtime and code generator for
[FlatBuffers](https://github.com/google/flatbuffers). It minimizes heap
allocations with `Span<T>`, `struct`, and `ref struct`. The API gives more control of memory allocations to the caller, enabling buffers that can be read or built entirely on the stack.

The API intentionally preserves the FlatBuffers "flavor" -- table builders, field
accessors, and the Object API are nearly identical so the adoption of FlatSpanBuffers requires only minor changes.

There are two variants of the generated code: IFlatbufferObject and IFlatbufferSpanObject.
IFlatbufferObject supports heap allocations and builders using the convinent BufferBuilding patterns.
IFlatbufferSpanObject uses `ref struct` and accepts `Span<T>` arguments for more control over how memory is allocated.

> For full FlatBuffers documentation (schema language, wire format, etc.) see the
> [FlatBuffers Documentation](https://flatbuffers.dev) and the
> [FlatBuffers GitHub repository](https://github.com/google/flatbuffers).

---

## Key Changes from FlatBuffers

| Area | What Changed |
|------|-------------|
| **Buffer types** | `ByteBuffer` is now a `struct`; `ByteSpanBuffer` is a new `ref struct` wrapping `Span<byte>` for stack-allocated buffers. |
| **Builders** | `FlatBufferBuilder` (array-backed) and `FlatSpanBufferBuilder` (`ref struct`, span-backed) share common logic through `BufferBuilder`. |
| **Generics** | `allows ref struct` constraints (requires .NET 9+) let a single generic function serve both regular and ref struct buffer types. |
| **No unsafe code** | `AllowUnsafeBlocks` is not required. No `ENABLE_SPAN_T` / `UNSAFE_BYTEBUFFER` preprocessor defines. |
| **Vectors** | Scalar vectors return `ReadOnlySpan<T>` / `Span<T>` directly. Table/struct vectors use lightweight wrapper structs. |
| **Verification** | `Verifier` is a `ref struct` operating on span-backed data. |
| **GetRoot<T>** | Generic `TryGetRoot<T>` and `GetRootUnchecked<T>` to read root tables with or without calling the Verifier. |
| **Nullables** | `RefStructNullable<T>` provides `.HasValue` / `.Value` for optional ref struct fields since `Nullable<T>` cannot wrap a `ref struct`. |
| **JSON** | Migrated from `Newtonsoft.Json` to `System.Text.Json`. |
| **Object API** | `Pack` / `UnPack` pre-size collections and reuse objects to reduce allocations. |
| **Target** | .NET 10. |

## Benchmarks

All benchmarks compare the original `Google.FlatBuffers`, with `FlatSpanBuffers`. The summarized results below compare the original against stackalloc'd, ref struct `StackBuffer` objects.

| Scenario | Improvement |
|----------|-------------|
| Decode | ~4.8x |
| Encode | ~1.6x |
| Decode (Object API) | ~2.5x |
| Encode (Object API) | ~1.35x |
| Verify | ~2.9x |

---

## Quick Start

### 1. Build the compiler

```bash
cmake -G "Unix Makefiles" -DCMAKE_BUILD_TYPE=Release \
      -DFLATBUFFERS_BUILD_TESTS=OFF -DFLATBUFFERS_BUILD_FLATLIB=OFF \
      -DFLATBUFFERS_BUILD_FLATHASH=OFF .
make -j
```

### 2. Write a schema

```fbs
namespace MyGame;

table Weapon {
  name: string;
  damage: short;
}

table Monster {
  name: string;
  hp: short;
  weapons: [Weapon];
}

root_type Monster;
```

### 3. Generate C# code

```bash
./flatspan --csharp-spanbufs --gen-object-api monster.fbs
```

### 4. Build with `FlatBufferBuilder`

```csharp
using FlatSpanBuffers;
using MyGame;

var builder = new FlatBufferBuilder(1024);

var weaponOneName = builder.CreateString("Sword");
var weaponTwoName = builder.CreateString("Axe");

var sword = Weapon.CreateWeapon(builder, weaponOneName, 3);
var axe = Weapon.CreateWeapon(builder, weaponTwoName, 5);

Span<Offset<Weapon>> weaponOffsets = stackalloc Offset<Weapon>[2];
weaponOffsets[0] = sword;
weaponOffsets[1] = axe;
var weapons = Monster.CreateWeaponsVectorBlock(builder, weaponOffsets);

var name = builder.CreateString("Orc");

Monster.StartMonster(builder);
Monster.AddName(builder, name);
Monster.AddHp(builder, 300);
Monster.AddWeapons(builder, weapons);
var orc = Monster.EndMonster(builder);
Monster.FinishMonsterBuffer(builder, orc);
```

### 5. Build with `FlatSpanBufferBuilder`

```csharp
using FlatSpanBuffers;
using MyGame.StackBuffer;

// ByteSpanBuffer and FlatSpanBufferBuilder do not dynamically resize,
// provide all required buffer space up front. Use the stack or the heap.
Span<byte> buffer = stackalloc byte[1024];
Span<int>  vtables = stackalloc int[64];
Span<int>  vtableOffsets = stackalloc int[64];
var buf = new ByteSpanBuffer(buffer);
var builder = new FlatSpanBufferBuilder(buf, vtables, vtableOffsets);

var weaponOneName = builder.CreateString("Sword");
var weaponTwoName = builder.CreateString("Axe");

var sword = Weapon.CreateWeapon(ref builder, weaponOneName, 3);
var axe = Weapon.CreateWeapon(ref builder, weaponTwoName, 5);

Span<Offset<Weapon>> weaponOffsets = stackalloc Offset<Weapon>[2];
weaponOffsets[0] = sword;
weaponOffsets[1] = axe;
var weapons = Monster.CreateWeaponsVector(ref builder, weaponOffsets);

var name = builder.CreateString("Orc");

Monster.StartMonster(ref builder);
Monster.AddName(ref builder, name);
Monster.AddHp(ref builder, 300);
Monster.AddWeapons(ref builder, weapons);
var orc = Monster.EndMonster(ref builder);
builder.Finish(orc.Value);
```

### 6. Read data

```csharp
using MyGame;

var bb = new ByteBuffer(receivedBytes);
var monster = Monster.GetRootAsMonster(bb);

// Scalar vector
var inventory = monster.Inventory;
if (inventory.HasValue)
{
    ReadOnlySpan<byte> items = inventory.Value;
    for (int i = 0; i < items.Length; i++)
        Console.WriteLine($"  item[{i}] = {items[i]}");
}

// Table vector
var weapons = monster.Weapons;
if (weapons.HasValue)
{
    var weaponsVec = weapons.Value;
    for (int i = 0; i < weaponsVec.Length; i++)
    {
        var w = weaponsVec[i];
        Console.WriteLine($"  {w.Name}: {w.Damage}");
    }
}
```

---

## License

This project is derived from [Google FlatBuffers](https://github.com/google/flatbuffers)
and is licensed under the Apache License 2.0. See [LICENSE](LICENSE) for details.

## Acknowledgments

FlatSpanBuffers is built on top of the FlatBuffers serialization library created
by Google. The schema compiler, wire format, and much of the core C++ tooling
originate from that project. The C# runtime and code generator were reworked by
[@bigjt-dev](https://github.com/bigjt-dev).
