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
 * This file is part of FlatSpanBuffers, derived from the Google FlatBuffers
 * project (https://github.com/google/flatbuffers).
 */

namespace FlatSpanBuffers
{
    public static class FlatBufferRoot
    {
        public static bool TryGetRoot<T>(ByteBuffer bb, out T result)
            where T : struct, IFlatbufferObject, IRootTable
            => InternalTryGetRoot(bb, new Options(), false, out result);

        public static bool TryGetRoot<T>(ByteSpanBuffer bb, out T result)
            where T : struct, IFlatbufferSpanObject, IRootTable, allows ref struct
            => InternalTryGetRoot(bb, new Options(), false, out result);

        public static bool TryGetRoot<T>(ByteBuffer bb, Options options, out T result)
            where T : struct, IFlatbufferObject, IRootTable
            => InternalTryGetRoot(bb, options, false, out result);

        public static bool TryGetRoot<T>(ByteSpanBuffer bb, Options options, out T result)
            where T : struct, IFlatbufferSpanObject, IRootTable, allows ref struct
            => InternalTryGetRoot(bb, options, false, out result);

        public static bool TryGetSizePrefixedRoot<T>(ByteBuffer bb, out T result)
            where T : struct, IFlatbufferObject, IRootTable
            => InternalTryGetRoot(bb, new Options(), true, out result);

        public static bool TryGetSizePrefixedRoot<T>(ByteSpanBuffer bb, out T result)
            where T : struct, IFlatbufferSpanObject, IRootTable, allows ref struct
            => InternalTryGetRoot(bb, new Options(), true, out result);

        public static bool TryGetSizePrefixedRoot<T>(ByteBuffer bb, Options options, out T result)
            where T : struct, IFlatbufferObject, IRootTable
            => InternalTryGetRoot(bb, options, true, out result);

        public static bool TryGetSizePrefixedRoot<T>(ByteSpanBuffer bb, Options options, out T result)
            where T : struct, IFlatbufferSpanObject, IRootTable, allows ref struct
            => InternalTryGetRoot(bb, options, true, out result);

        public static T GetRootUnchecked<T>(ByteBuffer bb)
            where T : struct, IFlatbufferObject, IRootTable
        {
            T obj = default;
            obj.__init(bb.Get<int>(bb.Position) + bb.Position, bb);
            return obj;
        }

        public static T GetRootUnchecked<T>(ByteSpanBuffer bb)
            where T : struct, IFlatbufferSpanObject, IRootTable, allows ref struct
        {
            T obj = default;
            obj.__init(bb.Get<int>(bb.Position) + bb.Position, bb);
            return obj;
        }

        public static T GetSizePrefixedRootUnchecked<T>(ByteBuffer bb)
            where T : struct, IFlatbufferObject, IRootTable
        {
            bb.Position += FlatBufferConstants.SizePrefixLength;
            return GetRootUnchecked<T>(bb);
        }

        public static T GetSizePrefixedRootUnchecked<T>(ByteSpanBuffer bb)
            where T : struct, IFlatbufferSpanObject, IRootTable, allows ref struct
        {
            bb.Position += FlatBufferConstants.SizePrefixLength;
            return GetRootUnchecked<T>(bb);
        }

        private static bool InternalTryGetRoot<T>(ByteBuffer bb, Options options, bool sizePrefixed, out T result)
            where T : struct, IFlatbufferObject, IRootTable
        {
            var verifier = new Verifier(bb, options);
            if (!T.Verify(ref verifier, sizePrefixed))
            {
                result = default;
                return false;
            }
            result = sizePrefixed ? GetSizePrefixedRootUnchecked<T>(bb) : GetRootUnchecked<T>(bb);
            return true;
        }

        private static bool InternalTryGetRoot<T>(ByteSpanBuffer bb, Options options, bool sizePrefixed, out T result)
            where T : struct, IFlatbufferSpanObject, IRootTable, allows ref struct
        {
            var verifier = new Verifier(bb, options);
            if (!T.Verify(ref verifier, sizePrefixed))
            {
                result = default;
                return false;
            }
            result = sizePrefixed ? GetSizePrefixedRootUnchecked<T>(bb) : GetRootUnchecked<T>(bb);
            return true;
        }
    }
}
