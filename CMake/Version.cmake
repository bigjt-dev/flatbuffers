set(VERSION_MAJOR 1)
set(VERSION_MINOR 0)
set(VERSION_PATCH 0)
set(VERSION_COMMIT 0)

if(EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/.git")
  find_program(GIT git)
  if(GIT)
    # Use --match to only consider FlatSpanBuffers SemVer tags (v1.x, v2.x, etc.)
    # and ignore legacy upstream FlatBuffers CalVer tags (v25.x).
    execute_process(
      COMMAND ${GIT} describe --tags --match "v[0-9]*.[0-9]*.[0-9]*"
      WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
      OUTPUT_VARIABLE GIT_DESCRIBE_DIRTY
      OUTPUT_STRIP_TRAILING_WHITESPACE
      RESULT_VARIABLE GIT_DESCRIBE_RESULT
    )

    if(GIT_DESCRIBE_RESULT EQUAL 0)
      if(GIT_DESCRIBE_DIRTY MATCHES "^v([0-9]+)\\.([0-9]+)\\.([0-9]+)")
        set(TAG_MAJOR "${CMAKE_MATCH_1}")
        # Reject legacy upstream CalVer tags (major >= 20).
        if(TAG_MAJOR LESS 20)
          string(REGEX REPLACE "^v([0-9]+)\\..*" "\\1" VERSION_MAJOR "${GIT_DESCRIBE_DIRTY}")
          string(REGEX REPLACE "^v[0-9]+\\.([0-9]+).*" "\\1" VERSION_MINOR "${GIT_DESCRIBE_DIRTY}")
          string(REGEX REPLACE "^v[0-9]+\\.[0-9]+\\.([0-9]+).*" "\\1" VERSION_PATCH "${GIT_DESCRIBE_DIRTY}")
          string(REGEX REPLACE "^v[0-9]+\\.[0-9]+\\.[0-9]+\\-([0-9]+).*" "\\1" VERSION_COMMIT "${GIT_DESCRIBE_DIRTY}")
          if(VERSION_COMMIT STREQUAL GIT_DESCRIBE_DIRTY)
            set(VERSION_COMMIT 0)
          endif()
        else()
          message(STATUS "Ignoring legacy upstream tag: ${GIT_DESCRIBE_DIRTY}, using default version ${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_PATCH}")
        endif()
      else()
        message(WARNING "\"${GIT_DESCRIBE_DIRTY}\" does not match pattern v<major>.<minor>.<patch>-<commit>")
      endif()
    else()
      message(WARNING "git describe failed with exit code: ${GIT_DESCRIBE_RESULT}\nUsing default version ${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_PATCH}.")
    endif()
  else()
    message(WARNING "git is not found")
  endif()
endif()

message(STATUS "Proceeding with version: ${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_PATCH}.${VERSION_COMMIT}")
