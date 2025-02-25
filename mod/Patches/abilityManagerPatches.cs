using System;
using HarmonyLib;
using UnityEngine;

namespace ArchipelagoClusterTruck.Patches;

public class abilityManagerPatches : ClassPatch
{
    public static float InfiniteStaminaTime = -1;
    private static void UpdatePrefix(ref float ___energy)
    {
        if(InfiniteStaminaTime < 0) return;
        InfiniteStaminaTime -= Time.deltaTime;
        ___energy = 1;
    }
    public override Exception Patch(Harmony harmony)
    {
        return MakePatch(harmony, typeof(abilityManager), "Update", nameof(UpdatePrefix));
    }
}