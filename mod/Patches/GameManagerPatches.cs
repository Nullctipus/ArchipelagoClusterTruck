using System;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

public class GameManagerPatches : ClassPatch
{
    static void WinLevelPrefix(GameManager __instance)
    {
        if (!Plugin.Data.CompletedLevels.Contains(info.currentLevel))
        {
            Plugin.Data.CompletedLevels.Add(info.currentLevel);
            //TODO: send location check
        }
    }
    public override Exception Patch(Harmony harmony)
    {
        return MakePatch(harmony,typeof(GameManager), nameof(GameManager.WinLevel),nameof(WinLevelPrefix));
    }
}