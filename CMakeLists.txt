# This is the legacy minimum version flatbuffers supported for a while.
cmake_minimum_required(VERSION 3.8...3.25.2)

# Attempt to read the current version of flatbuffers by looking at the latest tag.
include(CMake/Version.cmake)

project(FlatBuffers
        VERSION ${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_PATCH}
        LANGUAGES CXX)

# generate compile_commands.json
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# NOTE: Code coverage only works on Linux & OSX.
option(FLATBUFFERS_CODE_COVERAGE "Enable the code coverage build option." OFF)
option(FLATBUFFERS_INSTALL "Enable the installation of targets." ON)
option(FLATBUFFERS_BUILD_FLATLIB "Enable the build of the flatbuffers library"
       ON)
option(FLATBUFFERS_BUILD_FLATC "Enable the build of the flatspan compiler"
       ON)
option(FLATBUFFERS_STATIC_FLATC "Build flatspan compiler with -static flag"
       OFF)
option(FLATBUFFERS_LIBCXX_WITH_CLANG "Force libc++ when using Clang" ON)
# NOTE: Sanitizer check only works on Linux & OSX (gcc & llvm).
option(FLATBUFFERS_CODE_SANITIZE
      "Add '-fsanitize' flags to 'flatspan' target."
      OFF)
option(FLATBUFFERS_ENABLE_PCH
       "Enable precompile headers support for 'flatbuffers' and 'flatspan'. \"
        Only work if CMake supports 'target_precompile_headers'. \"
        This can speed up compilation time."
       OFF)
option(FLATBUFFERS_STRICT_MODE
      "Build flatspan with all warnings as errors (-Werror or /WX)."
      OFF)

if(NOT DEFINED FLATBUFFERS_CPP_STD)
  set(FLATBUFFERS_CPP_STD 11)
endif()

set(MSVC_LIKE OFF)
if(MSVC OR CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC")
    set(MSVC_LIKE ON)
endif()

if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
  set(IS_CLANG ON)
else()
  set(IS_CLANG OFF)
endif()

if(DEFINED FLATBUFFERS_COMPILATION_TIMINGS)
  message("Recording Compilation Timings to ${FLATBUFFERS_COMPILATION_TIMINGS}")
  file(REMOVE ${FLATBUFFERS_COMPILATION_TIMINGS})
  set_property(GLOBAL PROPERTY RULE_LAUNCH_COMPILE "time -f 'Wall: %E User: %U Sys: %S | %C' -q -a -o ${FLATBUFFERS_COMPILATION_TIMINGS}")
  set_property(GLOBAL PROPERTY RULE_LAUNCH_CUSTOM "time -f 'Wall: %E User: %U Sys: %S | %C' -q -a -o ${FLATBUFFERS_COMPILATION_TIMINGS}")
  set_property(GLOBAL PROPERTY RULE_LAUNCH_LINK "time -f 'Wall: %E User: %U Sys: %S | %C' -q -a -o ${FLATBUFFERS_COMPILATION_TIMINGS}")
endif()

if(DEFINED FLATBUFFERS_MAX_PARSING_DEPTH)
  # Override the default recursion depth limit.
  add_definitions(-DFLATBUFFERS_MAX_PARSING_DEPTH=${FLATBUFFERS_MAX_PARSING_DEPTH})
  message(STATUS "FLATBUFFERS_MAX_PARSING_DEPTH: ${FLATBUFFERS_MAX_PARSING_DEPTH}")
endif()

# Auto-detect locale-narrow 'strtod_l' and  'strtoull_l' functions.
if(NOT DEFINED FLATBUFFERS_LOCALE_INDEPENDENT)
  include(CheckCXXSymbolExists)

  set(FLATBUFFERS_LOCALE_INDEPENDENT 0)
  if(MSVC_LIKE)
    check_cxx_symbol_exists(_strtof_l stdlib.h FLATBUFFERS_HAS_STRTOF_L)
    check_cxx_symbol_exists(_strtoui64_l stdlib.h FLATBUFFERS_HAS_STRTOULL_L)
  else()
    check_cxx_symbol_exists(strtof_l stdlib.h FLATBUFFERS_HAS_STRTOF_L)
    check_cxx_symbol_exists(strtoull_l stdlib.h FLATBUFFERS_HAS_STRTOULL_L)
  endif()
  if(FLATBUFFERS_HAS_STRTOF_L AND FLATBUFFERS_HAS_STRTOULL_L)
    set(FLATBUFFERS_LOCALE_INDEPENDENT 1)
  endif()
endif()
add_definitions(-DFLATBUFFERS_LOCALE_INDEPENDENT=$<BOOL:${FLATBUFFERS_LOCALE_INDEPENDENT}>)

if(NOT WIN32)
  include(CheckSymbolExists)
  check_symbol_exists(realpath "stdlib.h" HAVE_REALPATH)
  if(NOT HAVE_REALPATH)
    add_definitions(-DFLATBUFFERS_NO_ABSOLUTE_PATH_RESOLUTION)
  endif()
endif()

set(FlatBuffers_Library_SRCS
  include/flatbuffers/allocator.h
  include/flatbuffers/array.h
  include/flatbuffers/base.h
  include/flatbuffers/buffer.h
  include/flatbuffers/buffer_ref.h
  include/flatbuffers/default_allocator.h
  include/flatbuffers/detached_buffer.h
  include/flatbuffers/code_generator.h
  include/flatbuffers/file_manager.h
  include/flatbuffers/flatbuffer_builder.h
  include/flatbuffers/flatbuffers.h
  include/flatbuffers/flexbuffers.h
  include/flatbuffers/flex_flat_util.h
  include/flatbuffers/hash.h
  include/flatbuffers/idl.h
  include/flatbuffers/minireflect.h
  include/flatbuffers/reflection.h
  include/flatbuffers/reflection_generated.h
  include/flatbuffers/registry.h
  include/flatbuffers/stl_emulation.h
  include/flatbuffers/string.h
  include/flatbuffers/struct.h
  include/flatbuffers/table.h
  include/flatbuffers/util.h
  include/flatbuffers/vector.h
  include/flatbuffers/vector_downward.h
  include/flatbuffers/verifier.h
  src/file_manager.cpp
  src/file_name_manager.cpp
  src/idl_parser.cpp
  src/idl_gen_text.cpp
  src/reflection.cpp
  src/util.cpp
)

set(FlatBuffers_Compiler_SRCS
  ${FlatBuffers_Library_SRCS}
  src/idl_gen_binary.cpp
  src/idl_gen_text.cpp
  src/idl_gen_csharp_spanbufs.h
  src/idl_gen_csharp_spanbufs.cpp
  src/idl_namer.h
  src/namer.h
  src/flatc.cpp
  src/flatc_main.cpp
  src/bfbs_gen.h
  include/codegen/idl_namer.h
  include/codegen/namer.h
  include/flatbuffers/code_generators.h
  src/binary_annotator.h
  src/binary_annotator.cpp
  src/annotated_binary_text_gen.h
  src/annotated_binary_text_gen.cpp
  src/code_generators.cpp
)



# TODO(dbaileychess): Figure out how this would now work. I posted a question on
# https://stackoverflow.com/questions/71772330/override-target-compile-options-via-cmake-command-line.
# Append FLATBUFFERS_CXX_FLAGS to CMAKE_CXX_FLAGS.
if(DEFINED FLATBUFFERS_CXX_FLAGS)
  message(STATUS "extend CXX_FLAGS with ${FLATBUFFERS_CXX_FLAGS}")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${FLATBUFFERS_CXX_FLAGS}")
endif()
message(STATUS "CMAKE_CXX_FLAGS: ${CMAKE_CXX_FLAGS}")

function(add_fsanitize_to_target _target _sanitizer)
  if(WIN32)
    target_compile_definitions(${_target} PRIVATE FLATBUFFERS_MEMORY_LEAK_TRACKING)
    message(STATUS "Sanitizer MSVC::_CrtDumpMemoryLeaks added to ${_target}")
  else()
    # FLATBUFFERS_CODE_SANITIZE: boolean {ON,OFF,YES,NO} or string with list of sanitizer.
    # List of sanitizer is string starts with '=': "=address,undefined,thread,memory".
    if(IS_CLANG OR (CMAKE_COMPILER_IS_GNUCXX AND CMAKE_CXX_COMPILER_VERSION VERSION_GREATER_EQUAL 4.9))
      set(_sanitizer_flags "=address,undefined")
      if(_sanitizer MATCHES "=.*")
        # override default by user-defined sanitizer list
        set(_sanitizer_flags ${_sanitizer})
      endif()
      target_compile_options(${_target} PRIVATE
        -g -fsigned-char -fno-omit-frame-pointer
        "-fsanitize${_sanitizer_flags}")
      target_link_libraries(${_target} PRIVATE
        "-fsanitize${_sanitizer_flags}")
      set_target_properties(${_target} PROPERTIES POSITION_INDEPENDENT_CODE ON)
      message(STATUS "Sanitizer ${_sanitizer_flags} added to ${_target}")
    endif()
  endif()
endfunction()

function(add_pch_to_target _target _pch_header)
  # the command is available since cmake 3.16
  if(COMMAND target_precompile_headers)
    target_precompile_headers(${_target} PRIVATE ${_pch_header})
    if(NOT MSVC)
      set_source_files_properties(src/util.cpp PROPERTIES SKIP_PRECOMPILE_HEADERS ON)
    endif()
  endif()
endfunction()

include_directories(include)

# Creates an interface library that stores the configuration settings that each
# target links too. This is a compromise between setting configuration globally
# with add_compile_options() and the more targetted target_compile_options().
# This way each target in this file can share settings and override them if
# needed.
add_library(ProjectConfig INTERFACE)
target_compile_features(ProjectConfig
  INTERFACE
    cxx_std_${FLATBUFFERS_CPP_STD}
)

# Force the standard to be met.
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# We shouldn't rely on any compiler-extensions to make things work.
set(CMAKE_CXX_EXTENSIONS OFF)

if(MSVC_LIKE)
  target_compile_options(ProjectConfig
    INTERFACE
      /W4
      $<$<BOOL:${FLATBUFFERS_STRICT_MODE}>:
        /WX       # Treat all compiler warnings as errors
      >
      /wd4512   # C4512: assignment operator could not be generated
      /wd4316   # C4316: object allocated on the heap may not be aligned
      /wd4456   # C4456: hides previous local declaration
      $<$<CXX_COMPILER_ID:Clang>:
        /D_CRT_SECURE_NO_WARNINGS
      >
  )
else()
  target_compile_options(ProjectConfig
    INTERFACE
      -Wall
      $<$<BOOL:${FLATBUFFERS_STRICT_MODE}>:
        -Werror   # Treat all compiler warnings as errors

        -fno-rtti # Disable runtime type information

        $<$<CXX_COMPILER_ID:GNU>:
          # False positive string overflow
          # https://github.com/google/flatbuffers/issues/7366
          -Wno-error=stringop-overflow
        >
      >
      -pedantic
      -Wextra
      -Wno-unused-parameter
      -Wold-style-cast
      -fsigned-char
      -Wnon-virtual-dtor

      # This isn't working for some reason: $<$<CXX_COMPILER_ID:CLANG>:
      $<$<BOOL:${IS_CLANG}>:
        -Wnewline-eof
        -Wno-unknown-warning-option
        -Wmissing-declarations
        -Wzero-as-null-pointer-constant
        $<$<VERSION_GREATER:$<CXX_COMPILER_VERSION>,3.8>:
          -Wimplicit-fallthrough
          -Wextra-semi
          $<$<BOOL:${FLATBUFFERS_STRICT_MODE}>:
            -Werror=unused-private-field
          >
        >
      >

      $<$<CXX_COMPILER_ID:GNU>:
        $<$<VERSION_GREATER:$<CXX_COMPILER_VERSION>,4.4>:
          -Wunused-result
          -Wunused-parameter
          -Werror=unused-parameter
          -Wmissing-declarations
        >
        $<$<VERSION_GREATER:$<CXX_COMPILER_VERSION>,4.7>:
          -Wzero-as-null-pointer-constant
        >
        $<$<VERSION_GREATER:$<CXX_COMPILER_VERSION>,7.0>:
          -faligned-new
          $<$<BOOL:${FLATBUFFERS_STRICT_MODE}>:
            -Werror=implicit-fallthrough=2
          >
        >
        $<$<VERSION_GREATER:$<CXX_COMPILER_VERSION>,8.0>:
          -Wextra-semi
        >
      >

      $<$<BOOL:${FLATBUFFERS_CODE_COVERAGE}>:
        -g
        -fprofile-arcs
        -ftest-coverage
      >
    )

  if(FLATBUFFERS_CODE_COVERAGE)
    target_link_options(ProjectConfig
      INTERFACE
        -fprofile-arcs
        -ftest-coverage
    )
  endif()
endif()

if(FLATBUFFERS_BUILD_FLATLIB)
  add_library(flatbuffers STATIC ${FlatBuffers_Library_SRCS})

  # Attach header directory for when build via add_subdirectory().
  target_include_directories(flatbuffers
    INTERFACE
      $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
  )
  target_link_libraries(flatbuffers PRIVATE $<BUILD_INTERFACE:ProjectConfig>)

  if(FLATBUFFERS_ENABLE_PCH)
    add_pch_to_target(flatbuffers include/flatbuffers/pch/pch.h)
  endif()
endif()

if(FLATBUFFERS_BUILD_FLATC)
  add_executable(flatspan ${FlatBuffers_Compiler_SRCS})
  if(FLATBUFFERS_ENABLE_PCH)
    add_pch_to_target(flatspan include/flatbuffers/pch/flatc_pch.h)
  endif()

  target_link_libraries(flatspan PRIVATE $<BUILD_INTERFACE:ProjectConfig>)
  target_compile_options(flatspan
    PRIVATE
      $<$<AND:$<BOOL:${MSVC_LIKE}>,$<CONFIG:Release>>:
        /MT
      >
  )

  if(FLATBUFFERS_CODE_SANITIZE AND NOT WIN32)
    add_fsanitize_to_target(flatspan ${FLATBUFFERS_CODE_SANITIZE})
  endif()
  if(NOT FLATBUFFERS_FLATC_EXECUTABLE)
    set(FLATBUFFERS_FLATC_EXECUTABLE $<TARGET_FILE:flatspan>)
  endif()
  if(FLATBUFFERS_STATIC_FLATC AND NOT MSVC)
    target_link_libraries(flatspan PRIVATE -static)
  endif()
endif()

if(FLATBUFFERS_INSTALL)
  include(GNUInstallDirs)

  install(DIRECTORY include/flatbuffers DESTINATION ${CMAKE_INSTALL_INCLUDEDIR})

  set(FB_CMAKE_DIR "${CMAKE_INSTALL_LIBDIR}/cmake/flatbuffers")

  configure_file(CMake/flatbuffers-config-version.cmake.in flatbuffers-config-version.cmake @ONLY)
  install(
      FILES
        "CMake/flatbuffers-config.cmake"
        "CMake/BuildFlatBuffers.cmake"
        "${CMAKE_CURRENT_BINARY_DIR}/flatbuffers-config-version.cmake"
      DESTINATION ${FB_CMAKE_DIR}
  )

  if(FLATBUFFERS_BUILD_FLATLIB)
    install(
      TARGETS flatbuffers EXPORT FlatBuffersTargets
      ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
      INCLUDES DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
    )

    install(EXPORT FlatBuffersTargets
      FILE FlatBuffersTargets.cmake
      NAMESPACE flatbuffers::
      DESTINATION ${FB_CMAKE_DIR}
    )
  endif()

  if(FLATBUFFERS_BUILD_FLATC)
    install(
      TARGETS flatspan EXPORT FlatSpanTargets
      RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
    )

    install(
      EXPORT FlatSpanTargets
      FILE FlatSpanTargets.cmake
      NAMESPACE flatbuffers::
      DESTINATION ${FB_CMAKE_DIR}
    )
  endif()
endif()

include(CMake/BuildFlatBuffers.cmake)
