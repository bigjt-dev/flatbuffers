#!/usr/bin/env python3
#
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

"""
Bump the FlatSpanBuffers version across all required files.

Updates the version in all 4 locations that must stay in sync:
  1. include/flatbuffers/base.h        (C++ preprocessor defines)
  2. net/FlatSpanBuffers/FlatBufferConstants.cs  (C# runtime method name)
  3. net/Directory.Build.props          (.NET assembly/package version)
  4. net/FlatSpanBuffers/FlatSpanBuffers.csproj  (NuGet PackageVersion)

Usage:
  python3 scripts/bump_version.py 1.2.3
  python3 scripts/bump_version.py 1.2.3 --dry-run
  python3 scripts/bump_version.py          # prints current version
"""

import argparse
import re
import sys
from pathlib import Path

# Root of the repository (parent of scripts/).
ROOT = Path(__file__).parent.parent.resolve()

# --- File paths relative to repo root ---
BASE_H = ROOT / "include" / "flatbuffers" / "base.h"
CONSTANTS_CS = ROOT / "net" / "FlatSpanBuffers" / "FlatBufferConstants.cs"
DIR_BUILD_PROPS = ROOT / "net" / "Directory.Build.props"
CSPROJ = ROOT / "net" / "FlatSpanBuffers" / "FlatSpanBuffers.csproj"

# --- Patterns ---
# base.h: #define FLATSPANBUFFERS_VERSION_MAJOR 1
RE_BASE_MAJOR = re.compile(
    r"(#define\s+FLATSPANBUFFERS_VERSION_MAJOR\s+)\d+"
)
RE_BASE_MINOR = re.compile(
    r"(#define\s+FLATSPANBUFFERS_VERSION_MINOR\s+)\d+"
)
RE_BASE_REVISION = re.compile(
    r"(#define\s+FLATSPANBUFFERS_VERSION_REVISION\s+)\d+"
)

# FlatBufferConstants.cs: public static void FLATSPANBUFFERS_1_0_0() {}
RE_CS_METHOD = re.compile(
    r"(public\s+static\s+void\s+FLATSPANBUFFERS_)\d+_\d+_\d+(\s*\(\s*\)\s*\{)"
)

# Directory.Build.props: <FlatSpanBuffersVersion>1.0.0</FlatSpanBuffersVersion>
RE_PROPS_VERSION = re.compile(
    r"(<FlatSpanBuffersVersion>)[^<]+(</FlatSpanBuffersVersion>)"
)

# FlatSpanBuffers.csproj: <PackageVersion>1.0.0</PackageVersion>
RE_PKG_VERSION = re.compile(
    r"(<PackageVersion>)[^<]+(</PackageVersion>)"
)


def read_current_version():
    """Read the current version from base.h defines."""
    content = BASE_H.read_text()
    major = RE_BASE_MAJOR.search(content)
    minor = RE_BASE_MINOR.search(content)
    revision = RE_BASE_REVISION.search(content)
    if not all([major, minor, revision]):
        print("ERROR: Could not read current version from base.h")
        sys.exit(1)
    # Extract the number after the matched prefix group
    m = int(re.search(r"\d+$", major.group()).group())
    n = int(re.search(r"\d+$", minor.group()).group())
    r = int(re.search(r"\d+$", revision.group()).group())
    return m, n, r


def update_file(path, replacements, dry_run=False):
    """Apply a list of (pattern, replacement) to a file.

    Returns True if at least one substitution was made.
    """
    content = path.read_text()
    original = content
    for pattern, repl in replacements:
        content, count = pattern.subn(repl, content)
        if count == 0:
            print(f"  WARNING: No match for pattern in {path.name}: {pattern.pattern}")
    changed = content != original
    if changed and not dry_run:
        path.write_text(content)
    return changed


def bump(major, minor, revision, dry_run=False):
    """Update all 4 files to the specified version."""
    version_str = f"{major}.{minor}.{revision}"
    version_underscore = f"{major}_{minor}_{revision}"

    files_changed = 0

    # 1. base.h
    print(f"  [1/4] {BASE_H.relative_to(ROOT)}")
    changed = update_file(BASE_H, [
        (RE_BASE_MAJOR, rf"\g<1>{major}"),
        (RE_BASE_MINOR, rf"\g<1>{minor}"),
        (RE_BASE_REVISION, rf"\g<1>{revision}"),
    ], dry_run)
    if changed:
        files_changed += 1

    # 2. FlatBufferConstants.cs
    print(f"  [2/4] {CONSTANTS_CS.relative_to(ROOT)}")
    changed = update_file(CONSTANTS_CS, [
        (RE_CS_METHOD, rf"\g<1>{version_underscore}\2"),
    ], dry_run)
    if changed:
        files_changed += 1

    # 3. Directory.Build.props
    print(f"  [3/4] {DIR_BUILD_PROPS.relative_to(ROOT)}")
    changed = update_file(DIR_BUILD_PROPS, [
        (RE_PROPS_VERSION, rf"\g<1>{version_str}\2"),
    ], dry_run)
    if changed:
        files_changed += 1

    # 4. FlatSpanBuffers.csproj
    print(f"  [4/4] {CSPROJ.relative_to(ROOT)}")
    changed = update_file(CSPROJ, [
        (RE_PKG_VERSION, rf"\g<1>{version_str}\2"),
    ], dry_run)
    if changed:
        files_changed += 1

    return files_changed


def parse_version(version_str):
    """Parse a version string like '1.2.3' into (major, minor, revision)."""
    match = re.match(r"^(\d+)\.(\d+)\.(\d+)$", version_str)
    if not match:
        print(f"ERROR: Invalid version format '{version_str}'. Expected MAJOR.MINOR.REVISION (e.g. 1.2.3)")
        sys.exit(1)
    return int(match.group(1)), int(match.group(2)), int(match.group(3))


def main():
    parser = argparse.ArgumentParser(
        description="Bump FlatSpanBuffers version across all required files."
    )
    parser.add_argument(
        "version",
        nargs="?",
        help="New version in MAJOR.MINOR.REVISION format (e.g. 1.2.3). "
             "Omit to print the current version.",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Show what would change without modifying files.",
    )
    args = parser.parse_args()

    cur_major, cur_minor, cur_revision = read_current_version()
    current = f"{cur_major}.{cur_minor}.{cur_revision}"

    if args.version is None:
        print(f"Current FlatSpanBuffers version: {current}")
        print("\nTo bump: python3 scripts/bump_version.py <MAJOR.MINOR.REVISION>")
        return

    new_major, new_minor, new_revision = parse_version(args.version)
    new_version = f"{new_major}.{new_minor}.{new_revision}"

    if new_version == current:
        print(f"Version is already {current}. Nothing to do.")
        return

    mode = "[DRY RUN] " if args.dry_run else ""
    print(f"{mode}Bumping FlatSpanBuffers version: {current} -> {new_version}")
    print()

    files_changed = bump(new_major, new_minor, new_revision, args.dry_run)

    print()
    if args.dry_run:
        print(f"[DRY RUN] {files_changed} file(s) would be modified.")
    else:
        print(f"Done. {files_changed} file(s) updated to {new_version}.")
        print()
        print("Next steps:")
        print("  1. Rebuild the flatspan compiler:  cmake --build build --target flatspan -j")
        print("  2. Regenerate code:                python3 scripts/generate_code.py --flatspan build/flatspan")
        print("  3. Build and test:                 dotnet run --project net/FlatSpanBuffers.Tests/FlatSpanBuffers.Tests.csproj")
        print("  4. Commit, tag, and push:          git tag v{0} && git push origin v{0}".format(new_version))


if __name__ == "__main__":
    main()
