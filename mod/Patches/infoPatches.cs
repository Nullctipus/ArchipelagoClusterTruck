using System;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace ArchipelagoClusterTruck.Patches;

public class infoPatches : ClassPatch
{
    // ReSharper disable once RedundantAssignment
    static bool HasAbilityBeenUnlockedPrefix(info.Abilities _abilityEnum, string Abilityname, ref bool __result)
    {
        __result = Plugin.Data.UnlockedAbilities.Contains(_abilityEnum);
        return false;
    }
    public override Exception Patch(Harmony harmony)
    {
        return new NotImplementedException("ArchipelagoClusterTruck.Patches.InfoPatches");
    }
}