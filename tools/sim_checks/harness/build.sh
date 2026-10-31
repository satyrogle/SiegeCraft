#!/bin/bash
# usage: build.sh <srcdir with sim/ and tests/> <outdir>
set -e
U="C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data"
NS="$U/NetStandard/ref/2.1.0/netstandard.dll"
SH="$U/NetStandard/compat/2.1.0/shims/netfx"
MS="$SH/mscorlib.dll"
NUNIT="C:/Users/jacob/KindredSiege-Clean/Library/PackageCache/com.unity.ext.nunit@1.0.6/net35/unity-custom/nunit.framework.dll"
SRC="$1"; OUT="$2"; mkdir -p "$OUT"
CSC() { "$U/NetCoreRuntime/dotnet.exe" "$U/DotNetSdkRoslyn/csc.dll" -nologo -noconfig -nostdlib -langversion:9.0 -warn:0 "$@"; }
mapfile -t SIM < <(find "$SRC/sim" -name '*.cs')
CSC -target:library -out:"$OUT/KindredSiege.Sim.dll" -reference:"$NS" "${SIM[@]}"
CSC -target:library -out:"$OUT/KindredSiege.Sim.Tests.dll" -reference:"$NS" -reference:"$MS" -reference:"$SH/System.Core.dll" -reference:"$SH/System.dll" -reference:"$NUNIT" -reference:"$OUT/KindredSiege.Sim.dll" "$SRC"/tests/*.cs "$(dirname "$0")"/shim/*.cs
cp "$NUNIT" "$OUT/"
CSC -target:exe -out:"$OUT/runner.exe" -reference:"$NS" "$(dirname "$0")/runner.cs"
echo BUILD OK
