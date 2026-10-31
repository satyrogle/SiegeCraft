#!/bin/bash
# Pairs of combos (every two on different classes) at the two settings where the plain squad wins about half, and random
# whole builds at the smart pilot's one. Each setting split into processes by seed range.
# Builds Sweep.cs against the repo's Assets/Scripts/Sim (or $KS_SIM_SRC) first. The CSVs land in $KS_SIM_WORK/sweep
# (default /tmp/ks_sim_checks, outside the repo); read them with analyze.py pairs and analyze.py builds.
H="$(cd "$(dirname "$0")" && pwd)"
R="$(cd "$H/../../.." && pwd)"
S="$(cygpath -m "${KS_SIM_WORK:-/tmp/ks_sim_checks}")/sweep"
M="C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/MonoBleedingEdge/bin/mono.exe"
bash "$H/../harness/build_exe.sh" "${KS_SIM_SRC:-$R/Assets/Scripts/Sim}" "$H/Sweep.cs" "$S/bin" || exit 1
start=$(date +%s)
for setting in "Reference 160" "Passive 135"; do
  set -- $setting
  for part in 0 1 2 3 4 5; do
    "$M" "$S/bin/Sweep.exe" pairs $((7000 + part * 50)) 50 $2 $1 4 > "$S/pairs_$1_$2_$part.csv" &
  done
done
for part in 0 1 2 3; do
  "$M" "$S/bin/Sweep.exe" builds $((7000 + part * 75)) 75 160 Reference 4 > "$S/builds_Reference_160_$part.csv" &
done
wait
for setting in "Reference 160" "Passive 135"; do set -- $setting; cat "$S"/pairs_$1_$2_*.csv > "$S/pairs_$1_$2.csv"; echo "pairs $1 $2: $(wc -l < "$S/pairs_$1_$2.csv") fights"; done
cat "$S"/builds_Reference_160_*.csv > "$S/builds_Reference_160.csv"; echo "builds: $(wc -l < "$S/builds_Reference_160.csv") fights"
echo "took $(( $(date +%s) - start )) s"
