/*
 * Copyright 2017 Google Inc. All rights reserved.
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

#include <cstdio>
#include <memory>

#include "flatbuffers/base.h"
#include "flatbuffers/code_generator.h"
#include "flatbuffers/flatc.h"
#include "flatbuffers/util.h"
#include "idl_gen_binary.h"
#include "idl_gen_csharp_spanbufs.h"
#include "idl_gen_text.h"

static const char* g_program_name = nullptr;

static void Warn(const flatbuffers::FlatCompiler* flatc,
                 const std::string& warn, bool show_exe_name) {
  (void)flatc;
  if (show_exe_name) {
    printf("%s: ", g_program_name);
  }
  fprintf(stderr, "\nwarning:\n  %s\n\n", warn.c_str());
}

static void Error(const flatbuffers::FlatCompiler* flatc,
                  const std::string& err, bool usage, bool show_exe_name) {
  if (show_exe_name) {
    printf("%s: ", g_program_name);
  }
  if (usage && flatc) {
    fprintf(stderr, "%s\n", flatc->GetShortUsageString(g_program_name).c_str());
  }
  fprintf(stderr, "\nerror:\n  %s\n\n", err.c_str());
  exit(1);
}

namespace flatbuffers {
void LogCompilerWarn(const std::string& warn) {
  Warn(static_cast<const flatbuffers::FlatCompiler*>(nullptr), warn, true);
}
void LogCompilerError(const std::string& err) {
  Error(static_cast<const flatbuffers::FlatCompiler*>(nullptr), err, false,
        true);
}
}  // namespace flatbuffers

int main(int argc, const char* argv[]) {
  const std::string flatbuffers_version(flatbuffers::FLATSPANBUFFERS_VERSION());

  g_program_name = argv[0];

  flatbuffers::FlatCompiler::InitParams params;
  params.warn_fn = Warn;
  params.error_fn = Error;

  flatbuffers::FlatCompiler flatc(params);

  flatc.RegisterCodeGenerator(
      flatbuffers::FlatCOption{
          "b", "binary", "",
          "Generate wire format binaries for any data definitions"},
      flatbuffers::NewBinaryCodeGenerator());

  flatc.RegisterCodeGenerator(
      flatbuffers::FlatCOption{"ns", "csharp-spanbufs", "",
                               "Generate C# span friendly classes for tables/structs"},
      flatbuffers::NewCSharpSpanBufsCodeGenerator());

  flatc.RegisterCodeGenerator(
      flatbuffers::FlatCOption{"t", "json", "",
                               "Generate text output for any data definitions"},
      flatbuffers::NewTextCodeGenerator());

  // Create the FlatC options by parsing the command line arguments.
  flatbuffers::FlatCOptions options =
      flatc.ParseFromCommandLineArguments(argc, argv);

  // this exists here to ensure file_saver outlives the compilation process
  std::unique_ptr<flatbuffers::FileSaver> file_saver;
  if (options.file_names_only) {
    file_saver.reset(new flatbuffers::FileNameSaver{});
  } else {
    file_saver.reset(new flatbuffers::RealFileSaver{});
  }

  options.opts.file_saver = file_saver.get();
  FLATBUFFERS_ASSERT(options.opts.file_saver);

  // Compile with the extracted FlatC options.
  int success = flatc.Compile(options);

  // print file names if file-names-only option is set
  options.opts.file_saver->Finish();

  return success;
}
