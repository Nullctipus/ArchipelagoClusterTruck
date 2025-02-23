#!/bin/bash
if ! command -v t4 &> /dev/null; then
	read -p "t4 was not found. would you like to install it? (Y/n)" installit
	if [ ! "${installit}" -o "${installit^^}" -eq "Y" -o "${installit}" -eq "1" ]; then
		dotnet tool install -g dotnet-t4
		export PATH="$PATH:$HOME/.dotnet/tools"
	else
		exit 1
	fi
fi
export SOLUTION_DIR="$(dirname $(realpath "$0"))"
if [ -f ".env" ]; then
  source .env
else
	read -p "Enter Game Directory: " GAME_DIR
	echo """
	#!/bin/bash
	export GAME_DIR=\"${GAME_DIR}\"
	""" > .env
	chmod u+x .env
fi

echo """
namespace ArchipelagoClusterTruck.Patches;

public static class PatchManager {
    public static void PatchAll(){}
}
""" > ./Patches/PatchManager.cs
echo "First build pre code gen"
dotnet build

echo "Code Gen"
t4 -r "${SOLUTION_DIR}/bin/Debug/net35/ArchipelagoClusterTruck.dll" \
	-r "${GAME_DIR}/Clustertruck_Data/Managed/UnityEngine.dll" \
	-r "${GAME_DIR}/BepInEx/core/BepInEx.dll" \
	./Patches/PatchManager.tt

echo "Final Build"
dotnet build $@
