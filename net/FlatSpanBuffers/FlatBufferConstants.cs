/*
 * Copyright 2014 Google Inc. All rights reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlatSpanBuffers
{
    public static class FlatBufferConstants
    {
        public const int FileIdentifierLength = 4;
        public const int SizePrefixLength = 4;

        /// <summary>
        /// FlatSpanBuffers version constant. This method exists solely as a
        /// compile-time compatibility check. Generated code calls this method
        /// in ValidateVersion() -- if the runtime library version does not
        /// match the version the code was generated with, the method name
        /// won't exist and compilation will fail with a clear error.
        /// </summary>
        public static void FLATSPANBUFFERS_1_1_0() {}
    }
}
