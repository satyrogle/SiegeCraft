#!/bin/bash
# usage: game_build.sh <Sim.dll> <dir holding the Assembly-CSharp script folders> <outdir>
# Compile check only: the game scripts as a player build (no UNITY_EDITOR), against Unity's own modules.
# The compiler reads its arguments from a response file in <outdir>, since long paths pass Windows' command line limit.
set -e
U="C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data"
NS="$U/NetStandard/ref/2.1.0/netstandard.dll"
LIB="C:/Users/jacob/KindredSiege-Clean/Library/ScriptAssemblies"
SIM="$(cygpath -m "$1")"; SRC="$(cygpath -m "$2")"; OUT="$(cygpath -m "$3")"; mkdir -p "$OUT"
REFS=(-reference:"$NS" -reference:"$SIM" -reference:"$LIB/UnityEngine.UI.dll" -reference:"$LIB/Unity.TextMeshPro.dll")
for d in "$U"/Managed/UnityEngine/UnityEngine*.dll; do REFS+=(-reference:"$d"); done
mapfile -t CS < <(find "$SRC" -name '*.cs' -not -path '*/Editor/*')
printf '"%s"\n' -target:library -out:"$OUT/Assembly-CSharp.dll" "${REFS[@]}" "${CS[@]}" > "$OUT/game_build.rsp"
"$U/NetCoreRuntime/dotnet.exe" "$U/DotNetSdkRoslyn/csc.dll" -nologo -noconfig -nostdlib -langversion:9.0 -warn:0 @"$OUT/game_build.rsp"
echo GAME BUILD OK "${#CS[@]}" files
