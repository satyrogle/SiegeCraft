#!/bin/bash
# usage: build_exe.sh <sim source folder> <program .cs> <out folder>
# Builds KindredSiege.Sim.dll from every .cs in the sim folder, then the program against it as <out folder>/<program>.exe.
# The probe and the sweeps build this way. Run the exe with Unity's Mono, Editor/Data/MonoBleedingEdge/bin/mono.exe.
set -e
U="C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data"
NS="$U/NetStandard/ref/2.1.0/netstandard.dll"
SRC="$(cygpath -m "$1")"; PROG="$(cygpath -m "$2")"; OUT="$(cygpath -m "$3")"; mkdir -p "$OUT"
CSC() { "$U/NetCoreRuntime/dotnet.exe" "$U/DotNetSdkRoslyn/csc.dll" -nologo -noconfig -nostdlib -langversion:9.0 -warn:0 "$@"; }
mapfile -t SIM < <(find "$SRC" -name '*.cs')
CSC -target:library -out:"$OUT/KindredSiege.Sim.dll" -reference:"$NS" "${SIM[@]}"
EXE="$OUT/$(basename "$PROG" .cs).exe"
CSC -target:exe -out:"$EXE" -reference:"$NS" -reference:"$OUT/KindredSiege.Sim.dll" "$PROG"
echo "BUILD OK $EXE"
