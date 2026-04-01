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

namespace FlatSpanBuffers.Tests
{
    [FlatBuffersTestClass]
    public class MultidimArraysTests
    {
        private static FlatBufferBuilder CreateBuilder(int initialSize = 256)
        {
            var buffer = new ByteBuffer(initialSize);
            return new FlatBufferBuilder(buffer, new int[32], new int[32]);
        }

        [FlatBuffersTestMethod]
        public void Pack_SameTypeMatrix_TwoSameTypeArraysRoundTrip()
        {
            // SameTypeMatrix has two independent arrays of BigRow (rows1 and rows2).
            // Verify each row array round trips correctly with distinct values.
            var m = new MyGame.Example.SameTypeMatrixT();
            for (var r = 0; r < 3; ++r)
            {
                m.Rows1[r] = new MyGame.Example.BigRowT();
                for (var c = 0; c < 256; ++c)
                    m.Rows1[r].Values[c] = r * 100.0 + c;

                m.Rows2[r] = new MyGame.Example.BigRowT();
                for (var c = 0; c < 256; ++c)
                    m.Rows2[r].Values[c] = r * 200.0 + c + 0.5;
            }

            var tableT = new MyGame.Example.SameTypeMatrixTableT { Data = m };

            var fbb = CreateBuilder(16384);
            var offset = MyGame.Example.SameTypeMatrixTable.Pack(fbb, tableT);
            fbb.Finish(offset.Value);

            var table = MyGame.Example.SameTypeMatrixTable.GetRootAsSameTypeMatrixTable(fbb.DataBuffer);
            Assert.IsTrue(table.Data.HasValue);
            var matrix = table.Data.Value;

            for (var r = 0; r < 3; ++r)
            {
                var row1 = matrix.Rows1(r);
                Assert.AreEqual(r * 100.0 + 0, row1.Values(0), 6);
                Assert.AreEqual(r * 100.0 + 127, row1.Values(127), 6);
                Assert.AreEqual(r * 100.0 + 255, row1.Values(255), 6);

                var row2 = matrix.Rows2(r);
                Assert.AreEqual(r * 200.0 + 0 + 0.5, row2.Values(0), 6);
                Assert.AreEqual(r * 200.0 + 127 + 0.5, row2.Values(127), 6);
                Assert.AreEqual(r * 200.0 + 255 + 0.5, row2.Values(255), 6);
            }
        }

        [FlatBuffersTestMethod]
        public void Pack_Depth3Table_ThreeLevelNestedStructRoundTrip()
        {
            // Depth3Table -> Depth3Outer[mats:2] -> Depth3Mid[rows:3] -> Depth3Leaf[vals:4].
            // Encodes each leaf value as a unique function of its coordinates.
            var outerT = new MyGame.Example.Depth3OuterT();
            for (var m = 0; m < 2; ++m)
            {
                outerT.Mats[m] = new MyGame.Example.Depth3MidT();
                for (var r = 0; r < 3; ++r)
                {
                    outerT.Mats[m].Rows[r] = new MyGame.Example.Depth3LeafT();
                    for (var v = 0; v < 4; ++v)
                        outerT.Mats[m].Rows[r].Vals[v] = m * 100 + r * 10 + v;
                }
            }

            var tableT = new MyGame.Example.Depth3TableT { A = outerT };

            var fbb = CreateBuilder(512);
            var offset = MyGame.Example.Depth3Table.Pack(fbb, tableT);
            fbb.Finish(offset.Value);

            var table = MyGame.Example.Depth3Table.GetRootAsDepth3Table(fbb.DataBuffer);
            Assert.IsTrue(table.A.HasValue);
            var outer = table.A.Value;

            for (var m = 0; m < 2; ++m)
            {
                var mid = outer.Mats(m);
                for (var r = 0; r < 3; ++r)
                {
                    var leaf = mid.Rows(r);
                    for (var v = 0; v < 4; ++v)
                        Assert.AreEqual(m * 100 + r * 10 + v, leaf.Vals(v));
                }
            }
        }

        [FlatBuffersTestMethod]
        public void Pack_Depth4Table_FourLevelNestedStructRoundTrip()
        {
            // Depth4Table -> Depth4Wrap[cubes:5] -> Depth3Outer[mats:2] -> Depth3Mid[rows:3] -> Depth3Leaf[vals:4].
            // Encodes each leaf value uniquely across all four index levels.
            var wrapT = new MyGame.Example.Depth4WrapT();
            for (var cube = 0; cube < 5; ++cube)
            {
                wrapT.Cubes[cube] = new MyGame.Example.Depth3OuterT();
                for (var m = 0; m < 2; ++m)
                {
                    wrapT.Cubes[cube].Mats[m] = new MyGame.Example.Depth3MidT();
                    for (var r = 0; r < 3; ++r)
                    {
                        wrapT.Cubes[cube].Mats[m].Rows[r] = new MyGame.Example.Depth3LeafT();
                        for (var v = 0; v < 4; ++v)
                            wrapT.Cubes[cube].Mats[m].Rows[r].Vals[v] = cube * 1000 + m * 100 + r * 10 + v;
                    }
                }
            }

            var tableT = new MyGame.Example.Depth4TableT { A = wrapT };

            var fbb = CreateBuilder(4096);
            var offset = MyGame.Example.Depth4Table.Pack(fbb, tableT);
            fbb.Finish(offset.Value);

            var table = MyGame.Example.Depth4Table.GetRootAsDepth4Table(fbb.DataBuffer);
            Assert.IsTrue(table.A.HasValue);
            var wrap = table.A.Value;

            for (var cube = 0; cube < 5; ++cube)
            {
                var outer = wrap.Cubes(cube);
                for (var m = 0; m < 2; ++m)
                {
                    var mid = outer.Mats(m);
                    for (var r = 0; r < 3; ++r)
                    {
                        var leaf = mid.Rows(r);
                        for (var v = 0; v < 4; ++v)
                            Assert.AreEqual(cube * 1000 + m * 100 + r * 10 + v, leaf.Vals(v));
                    }
                }
            }
        }

        [FlatBuffersTestMethod]
        public void StackBuffer_3DArray_CreateAndReadBack()
        {
            // Depth3Outer.mats[2] -> Depth3Mid.rows[3] -> Depth3Leaf.vals[4] -> int
            // CreateDepth3Outer accepts a flat ReadOnlySpan<int> in row-major order
            // (dim0=2, dim1=3, dim2=4) -> flat index = m*12 + r*4 + v
            Span<int> data = stackalloc int[24];
            for (int m = 0; m < 2; m++)
                for (int r = 0; r < 3; r++)
                    for (int v = 0; v < 4; v++)
                        data[m * 12 + r * 4 + v] = m * 100 + r * 10 + v;

            var bb = new ByteSpanBuffer(stackalloc byte[1024]);
            var fbb = new FlatSpanBufferBuilder(bb, vtableSpace: stackalloc int[4], vtableOffsetSpace: stackalloc int[4]);

            var outerOffset = MyGame.Example.StackBuffer.Depth3Outer.CreateDepth3Outer(ref fbb, data);
            MyGame.Example.StackBuffer.Depth3Table.StartDepth3Table(ref fbb);
            MyGame.Example.StackBuffer.Depth3Table.AddA(ref fbb, outerOffset);
            var tableOffset = MyGame.Example.StackBuffer.Depth3Table.EndDepth3Table(ref fbb);
            fbb.Finish(tableOffset.Value);

            var table = MyGame.Example.StackBuffer.Depth3Table.GetRootAsDepth3Table(fbb.DataBuffer);
            Assert.IsTrue(table.A.HasValue);
            var outer = table.A.Value;

            for (int m = 0; m < 2; m++)
                for (int r = 0; r < 3; r++)
                    for (int v = 0; v < 4; v++)
                        Assert.AreEqual(m * 100 + r * 10 + v, outer.Mats(m).Rows(r).Vals(v));
        }

        [FlatBuffersTestMethod]
        public void StackBuffer_3DArray_ObjectApiPackRoundTrip()
        {
            // Pack via Object API: Depth3OuterT[] -> Pack -> GetRootAs -> UnPack
            var outerT = new MyGame.Example.Depth3OuterT();
            outerT.Mats = new MyGame.Example.Depth3MidT[2];
            for (int m = 0; m < 2; m++)
            {
                outerT.Mats[m] = new MyGame.Example.Depth3MidT();
                outerT.Mats[m].Rows = new MyGame.Example.Depth3LeafT[3];
                for (int r = 0; r < 3; r++)
                {
                    outerT.Mats[m].Rows[r] = new MyGame.Example.Depth3LeafT();
                    outerT.Mats[m].Rows[r].Vals = new int[4];
                    for (int v = 0; v < 4; v++)
                        outerT.Mats[m].Rows[r].Vals[v] = m * 100 + r * 10 + v;
                }
            }

            var bb = new ByteSpanBuffer(stackalloc byte[1024]);
            var fbb = new FlatSpanBufferBuilder(bb, vtableSpace: stackalloc int[4], vtableOffsetSpace: stackalloc int[4]);

            var tableT = new MyGame.Example.Depth3TableT { A = outerT };
            fbb.Finish(MyGame.Example.StackBuffer.Depth3Table.Pack(ref fbb, tableT).Value);

            var table = MyGame.Example.StackBuffer.Depth3Table.GetRootAsDepth3Table(fbb.DataBuffer);
            var roundTripped = table.A.Value;

            for (int m = 0; m < 2; m++)
                for (int r = 0; r < 3; r++)
                    for (int v = 0; v < 4; v++)
                        Assert.AreEqual(m * 100 + r * 10 + v, roundTripped.Mats(m).Rows(r).Vals(v));

            // Also verify UnPack produces the same values
            var unpackedT = table.UnPack();
            for (int m = 0; m < 2; m++)
                for (int r = 0; r < 3; r++)
                    for (int v = 0; v < 4; v++)
                        Assert.AreEqual(m * 100 + r * 10 + v, unpackedT.A.Mats[m].Rows[r].Vals[v]);
        }

        [FlatBuffersTestMethod]
        public void StackBuffer_4DArray_CreateAndReadBack()
        {
            // Depth4Wrap.cubes[5] -> Depth3Outer.mats[2] -> Depth3Mid.rows[3] -> Depth3Leaf.vals[4] -> int
            // CreateDepth4Wrap accepts a flat ReadOnlySpan<int> in row-major order
            // (dim0=5, dim1=2, dim2=3, dim3=4) -> flat index = c*24 + m*12 + r*4 + v
            Span<int> data = stackalloc int[120];
            for (int c = 0; c < 5; c++)
                for (int m = 0; m < 2; m++)
                    for (int r = 0; r < 3; r++)
                        for (int v = 0; v < 4; v++)
                            data[c * 24 + m * 12 + r * 4 + v] = c * 1000 + m * 100 + r * 10 + v;

            var bb = new ByteSpanBuffer(new byte[8192]);
            var fbb = new FlatSpanBufferBuilder(bb, vtableSpace: new int[4], vtableOffsetSpace: new int[4]);

            var wrapOffset = MyGame.Example.StackBuffer.Depth4Wrap.CreateDepth4Wrap(ref fbb, data);
            MyGame.Example.StackBuffer.Depth4Table.StartDepth4Table(ref fbb);
            MyGame.Example.StackBuffer.Depth4Table.AddA(ref fbb, wrapOffset);
            var tableOffset = MyGame.Example.StackBuffer.Depth4Table.EndDepth4Table(ref fbb);
            fbb.Finish(tableOffset.Value);

            var table = MyGame.Example.StackBuffer.Depth4Table.GetRootAsDepth4Table(fbb.DataBuffer);
            Assert.IsTrue(table.A.HasValue);
            var wrap = table.A.Value;

            for (int c = 0; c < 5; c++)
                for (int m = 0; m < 2; m++)
                    for (int r = 0; r < 3; r++)
                        for (int v = 0; v < 4; v++)
                            Assert.AreEqual(c * 1000 + m * 100 + r * 10 + v, wrap.Cubes(c).Mats(m).Rows(r).Vals(v));
        }

        [FlatBuffersTestMethod]
        public void Pack_BigMatrix_LargeFixedStructUsesPoolAndRoundTrips()
        {
            var bigMatrixT = new MyGame.Example.BigMatrixT();
            for (var r = 0; r < 3; ++r)
            {
                bigMatrixT.Rows[r] = new MyGame.Example.BigRowT();
                for (var c = 0; c < 256; ++c)
                    bigMatrixT.Rows[r].Values[c] = r * 1000.0 + c;
            }

            var tableT = new MyGame.Example.BigMatrixTableT { Data = bigMatrixT };

            var fbb = CreateBuilder(8192);
            var offset = MyGame.Example.BigMatrixTable.Pack(fbb, tableT);
            fbb.Finish(offset.Value);

            var table = MyGame.Example.BigMatrixTable.GetRootAsBigMatrixTable(fbb.DataBuffer);
            Assert.IsTrue(table.Data.HasValue);
            var matrix = table.Data.Value;

            for (var r = 0; r < 3; ++r)
            {
                var row = matrix.Rows(r);
                Assert.AreEqual(r * 1000.0 + 0, row.Values(0), 6);
                Assert.AreEqual(r * 1000.0 + 127, row.Values(127), 6);
                Assert.AreEqual(r * 1000.0 + 255, row.Values(255), 6);
            }
        }

        [FlatBuffersTestMethod]
        public void Pack_MixedMatrix_CombinedPoolWithCast_RoundTrips()
        {
            var m = new MyGame.Example.MixedMatrixT();
            for (var r = 0; r < 3; ++r)
            {
                m.SmallRows[r] = new MyGame.Example.SmallRowT();
                for (var c = 0; c < 4; ++c)
                    m.SmallRows[r].Values[c] = r * 10.0 + c;

                m.BigRows[r] = new MyGame.Example.BigRowT();
                for (var c = 0; c < 256; ++c)
                    m.BigRows[r].Values[c] = r * 1000.0 + c;

                m.BigRows2[r] = new MyGame.Example.BigRowT();
                for (var c = 0; c < 256; ++c)
                    m.BigRows2[r].Values[c] = r * 2000.0 + c;

                m.FloatRows[r] = new MyGame.Example.FloatRowT();
                for (var c = 0; c < 400; ++c)
                    m.FloatRows[r].Values[c] = r * 100.0f + c;

                m.ByteRows[r] = new MyGame.Example.ByteRowT();
                for (var c = 0; c < 2048; ++c)
                    m.ByteRows[r].Values[c] = (byte)((r * 7 + c) & 0xFF);
            }

            var tableT = new MyGame.Example.MixedMatrixTableT { Data = m };
            var fbb = CreateBuilder(32768);
            var offset = MyGame.Example.MixedMatrixTable.Pack(fbb, tableT);
            fbb.Finish(offset.Value);

            var table = MyGame.Example.MixedMatrixTable.GetRootAsMixedMatrixTable(fbb.DataBuffer);
            Assert.IsTrue(table.Data.HasValue);
            var matrix = table.Data.Value;

            for (var r = 0; r < 3; ++r)
            {
                var row = matrix.SmallRows(r);
                Assert.AreEqual(r * 10.0 + 0, row.Values(0), 6);
                Assert.AreEqual(r * 10.0 + 3, row.Values(3), 6);
            }

            for (var r = 0; r < 3; ++r)
            {
                var row = matrix.BigRows(r);
                Assert.AreEqual(r * 1000.0 + 0, row.Values(0), 6);
                Assert.AreEqual(r * 1000.0 + 127, row.Values(127), 6);
                Assert.AreEqual(r * 1000.0 + 255, row.Values(255), 6);
            }

            for (var r = 0; r < 3; ++r)
            {
                var row = matrix.BigRows2(r);
                Assert.AreEqual(r * 2000.0 + 0, row.Values(0), 6);
                Assert.AreEqual(r * 2000.0 + 127, row.Values(127), 6);
                Assert.AreEqual(r * 2000.0 + 255, row.Values(255), 6);
            }

            for (var r = 0; r < 3; ++r)
            {
                var row = matrix.FloatRows(r);
                Assert.AreEqual(r * 100.0f + 0, row.Values(0), 3);
                Assert.AreEqual(r * 100.0f + 199, row.Values(199), 3);
                Assert.AreEqual(r * 100.0f + 399, row.Values(399), 3);
            }

            for (var r = 0; r < 3; ++r)
            {
                var row = matrix.ByteRows(r);
                Assert.AreEqual((byte)((r * 7 + 0) & 0xFF), row.Values(0));
                Assert.AreEqual((byte)((r * 7 + 1023) & 0xFF), row.Values(1023));
                Assert.AreEqual((byte)((r * 7 + 2047) & 0xFF), row.Values(2047));
            }
        }
    }
}
