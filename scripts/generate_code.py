#!/usr/bin/env python3
#
# Copyright 2021 Google Inc. All rights reserved.
# Copyright 2025-2026 bigjt-dev. All rights reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from util import flatc, root_path

# FlatSpanBuffers Tests
flatspanbuffers_fbs = "net/FlatSpanBuffers.GeneratedCode/fbs"
flatspanbuffers_gen = "net/FlatSpanBuffers.GeneratedCode/Generated"

CS_SPANBUF_OPTS = ["--csharp-spanbufs", "--gen-object-api"]

flatc(
    CS_SPANBUF_OPTS,
    prefix=flatspanbuffers_gen + "/key_test",
    schema=flatspanbuffers_fbs + "/key_test.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS,
    prefix=flatspanbuffers_gen + "/optional_scalars",
    schema=flatspanbuffers_fbs + "/optional_scalars.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS,
    prefix=flatspanbuffers_gen + "/keyword_test",
    schema=flatspanbuffers_fbs + "/keyword_test.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS,
    prefix=flatspanbuffers_gen + "/comprehensive_test",
    schema=flatspanbuffers_fbs + "/comprehensive_test.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS + ["--gen-mutable"],
    prefix=flatspanbuffers_gen + "/monster_test",
    schema=flatspanbuffers_fbs + "/monster_test.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS + ["--gen-mutable", "--cs-gen-json-serializer"],
    prefix=flatspanbuffers_gen + "/mygame_example",
    schema=flatspanbuffers_fbs + "/mygame_example.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS + ["--cs-gen-json-serializer"],
    prefix=flatspanbuffers_gen + "/arrays_test",
    schema=flatspanbuffers_fbs + "/arrays_test.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS,
    prefix=flatspanbuffers_gen + "/multidim_arrays_test",
    schema=flatspanbuffers_fbs + "/multidim_arrays_test.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS + ["--cs-gen-json-serializer"],
    prefix=flatspanbuffers_gen + "/json_test",
    schema=flatspanbuffers_fbs + "/json_test.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS + ["--cs-gen-json-serializer"],
    prefix=flatspanbuffers_gen + "/union_vector",
    schema=flatspanbuffers_fbs + "/union_vector.fbs",
    cwd=root_path,
)

flatc(
    CS_SPANBUF_OPTS + ["--gen-onefile"],
    prefix=flatspanbuffers_gen + "/onefile_test",
    schema=flatspanbuffers_fbs + "/onefile_test.fbs",
    cwd=root_path,
)

# FlatSpanBuffers Benchmarks
benchmark_fbs = "net/FlatSpanBuffers.Benchmarks/fbs"
benchmark_gen = "net/FlatSpanBuffers.Benchmarks/Generated"

flatc(
    ["--csharp-spanbufs", "--gen-object-api", "--gen-mutable"],
    prefix=benchmark_gen + "/FlatSpanBuffers",
    schema=benchmark_fbs + "/benchmark.fbs",
    cwd=root_path,
)
