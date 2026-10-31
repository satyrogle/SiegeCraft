#!/bin/bash
# usage: run_probe.sh <sim source folder> <tag> [other tag]
# Builds Probe.cs against the sim in that folder and plays every probe mode; outputs land in $KS_SIM_WORK/res/<tag>_*.txt
# (default /tmp/ks_sim_checks, outside the repo). With another tag, this build also reads that tag's saves back.
# Old against new: copy Assets/Scripts/Sim out of the repo before you change it and run this on the copy as "old"; after
# the change run it on the repo's Assets/Scripts/Sim as "new old"; then diff res/old_* against res/new_*.
set -e
H="$(cd "$(dirname "$0")" && pwd)"
W="$(cygpath -m "${KS_SIM_WORK:-/tmp/ks_sim_checks}")"
U="C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data"
M="$U/MonoBleedingEdge/bin/mono.exe"
src=$1; tag=$2; other=$3
bash "$H/../harness/build_exe.sh" "$src" "$H/Probe.cs" "$W/bin/$tag"
mkdir -p "$W/res" "$W/saves_$tag"
rm -f "$W/saves_$tag"/*.save
cd "$W/bin/$tag"
"$M" Probe.exe fights > "$W/res/${tag}_fights.txt" 2>&1
for cap in 14 15 16 17; do "$M" Probe.exe runs $cap > "$W/res/${tag}_runs$cap.txt" 2>&1; done
"$M" Probe.exe savewrite "$W/saves_$tag" > "$W/res/${tag}_savewrite.txt" 2>&1
"$M" Probe.exe saveread "$W/saves_$tag" > "$W/res/${tag}_saveread.txt" 2>&1
if [ -n "$other" ]; then "$M" Probe.exe saveread "$W/saves_$other" > "$W/res/${tag}_reads_$other.txt" 2>&1; fi
"$M" Probe.exe combofights > "$W/res/${tag}_combofights.txt" 2>&1
"$M" Probe.exe mixfights > "$W/res/${tag}_mixfights.txt" 2>&1
"$M" Probe.exe comboruns 7000 3 16 > "$W/res/${tag}_comboruns16.txt" 2>&1
"$M" Probe.exe comboruns 7000 3 > "$W/res/${tag}_comboruns.txt" 2>&1
echo "$tag finished"
