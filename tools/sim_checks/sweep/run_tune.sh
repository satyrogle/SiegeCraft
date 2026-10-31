#!/bin/bash
# usage: run_tune.sh crack dmg parry style scale [seeds]
# Whole runs from seed 7000 with each class sent to every duel (RunSweep tune mode). The two duel numbers are set by
# reflection, so $KS_SIM_SRC must be a copy of the sim where HeavyRules.DuelParryNerve and HeavyRules.DuelDamagePercent
# are static fields, not consts, and the duel's crack reads DuelParryNerve. Writes
# $KS_SIM_WORK/tune/out/t_<crack>_<dmg>_<parry>_<style>_<scale>.csv (default /tmp/ks_sim_checks), one line per fight;
# score them with score.py.
H="$(cd "$(dirname "$0")" && pwd)"
S="$(cygpath -m "${KS_SIM_WORK:-/tmp/ks_sim_checks}")/tune"
M="C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/MonoBleedingEdge/bin/mono.exe"
[ -n "$KS_SIM_SRC" ] || { echo "set KS_SIM_SRC to a sim copy with the two duel numbers as static fields"; exit 1; }
B="$S/bin"; O="$S/out"; mkdir -p "$O"
bash "$H/../harness/build_exe.sh" "$KS_SIM_SRC" "$H/RunSweep.cs" "$B" || exit 1
crack=$1; dmg=$2; parry=$3; style=$4; scale=$5; n=${6:-300}
f="$O/t_${crack}_${dmg}_${parry}_${style}_${scale}"
per=$(( (n + 7) / 8 ))
for part in 0 1 2 3 4 5 6 7; do
  cnt=$(( n - part * per )); [ $cnt -gt $per ] && cnt=$per; [ $cnt -le 0 ] && continue
  "$M" "$B/RunSweep.exe" tune $((7000 + part * per)) $cnt $scale $style $parry $crack $dmg > "${f}_$part.csv" &
done
wait
cat "${f}"_[0-7].csv > "$f.csv" 2>/dev/null; rm -f "${f}"_[0-7].csv
echo "$(basename "$f"): $(wc -l < "$f.csv") fight lines"
