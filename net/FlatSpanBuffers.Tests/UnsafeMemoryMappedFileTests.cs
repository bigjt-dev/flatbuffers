/*
 * Copyright 2025-2026 bigjt-dev. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * This file is part of FlatSpanBuffers
 * (https://github.com/bigjt-dev/flatbuffers).
 */

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using MyGame.Example;

using SpanMonster = MyGame.Example.StackBuffer.Monster;

namespace FlatSpanBuffers.Tests
{
    // Demonstrates reading FlatBuffers from memory-mapped files using the
    // StackBuffer (ref struct) API, which operates over <see cref="Span{T}"/>.
    [FlatBuffersTestClass]
    public class UnsafeMemoryMappedFileTests
    {
        private static string MonsterDataPath =>
            Path.Combine(AppContext.BaseDirectory, "monsterdata_test.mon");

        // Read the full monster and verify all populated fields
        [FlatBuffersTestMethod]
        public unsafe void MemoryMapped_CanReadAndVerifyMonsterWithoutCopy()
        {
            using var mmf = MemoryMappedFile.CreateFromFile(MonsterDataPath, FileMode.Open);
            using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

            byte* ptr = null;
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            try
            {
                long length = new FileInfo(MonsterDataPath).Length;
                var span = new Span<byte>(ptr, (int)length);
                var bb = new ByteSpanBuffer(span);

                Assert.IsTrue(SpanMonster.VerifyMonster(bb));

                var monster = SpanMonster.GetRootAsMonster(bb);

                Assert.AreEqual(80, monster.Hp);
                Assert.AreEqual(150, monster.Mana);
                Assert.AreEqual("MyMonster", monster.Name);

                Assert.IsTrue(monster.Pos.HasValue);
                var pos = monster.Pos.Value;
                Assert.AreEqual(1.0f, pos.X, 6);
                Assert.AreEqual(2.0f, pos.Y, 6);
                Assert.AreEqual(3.0f, pos.Z, 6);
                Assert.AreEqual(3.0, pos.Test1, 6);
                Assert.AreEqual(Color.Green, pos.Test2);
                Assert.AreEqual((short)5, pos.Test3.A);
                Assert.AreEqual((sbyte)6, pos.Test3.B);

                Assert.AreEqual(Any.Monster, monster.TestType);
                var fred = monster.Test<SpanMonster>().Value;
                Assert.AreEqual("Fred", fred.Name);

                Assert.IsTrue(monster.Inventory.HasValue);
                var inv = monster.Inventory.Value;
                Assert.AreEqual(5, inv.Length);
                var invSum = 0;
                for (int i = 0; i < inv.Length; i++)
                    invSum += inv[i];
                Assert.AreEqual(10, invSum);

                Assert.IsTrue(monster.Test4.HasValue);
                var test4 = monster.Test4.Value;
                Assert.AreEqual(2, test4.Length);
                Assert.AreEqual(100, test4[0].A + test4[0].B + test4[1].A + test4[1].B);

                Assert.IsTrue(monster.Testarrayofstring.HasValue);
                var strings = monster.Testarrayofstring.Value;
                Assert.AreEqual(2, strings.Length);
                Assert.AreEqual("test1", strings[0]);
                Assert.AreEqual("test2", strings[1]);

                Assert.AreEqual(true, monster.Testbool);

                var nameBytes = monster.GetNameBytes();
                Assert.AreEqual("MyMonster",
                    Encoding.UTF8.GetString(nameBytes.ToArray(), 0, nameBytes.Length));

                Assert.IsTrue(monster.VectorOfLongs.HasValue);
                Assert.IsTrue(monster.VectorOfLongs.Value.Length > 0);

                Assert.IsTrue(monster.VectorOfDoubles.HasValue);
                Assert.IsTrue(monster.VectorOfDoubles.Value.Length > 0);
            }
            finally
            {
                accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }
    }
}
