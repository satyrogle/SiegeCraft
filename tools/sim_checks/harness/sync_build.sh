#!/bin/bash
# usage: sync_build.sh [filter]  copies the repo's Sim and EditMode tests to the work folder, builds, runs the tests (all, or a name filter)
# The work folder is $KS_SIM_WORK (default /tmp/ks_sim_checks), outside the repo; the full results land in its work-run.txt.
set -e
H="$(cd "$(dirname "$0")" && pwd)"
R="$(cd "$H/../../.." && pwd)"
W="$(cygpath -m "${KS_SIM_WORK:-/tmp/ks_sim_checks}")"
U="C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data"
rm -rf "$W/work" && mkdir -p "$W/work/sim" "$W/work/tests"
cd "$R/Assets/Scripts/Sim" && find . -name '*.cs' | while read f; do mkdir -p "$W/work/sim/$(dirname "$f")"; cp "$f" "$W/work/sim/$f"; done
cp "$R"/Assets/Tests/EditMode/*.cs "$W/work/tests/"
bash "$H/build.sh" "$W/work" "$W/out/work" 2>&1 | grep -v "^$" | tail -40
cd "$W/out/work" && "$U/MonoBleedingEdge/bin/mono.exe" --debug runner.exe KindredSiege.Sim.Tests.dll "${1:-all}" > "$W/work-run.txt" 2>&1 || true
grep "^FAIL" -A6 "$W/work-run.txt" | head -80
tail -1 "$W/work-run.txt"
