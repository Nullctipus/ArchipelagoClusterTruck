using System;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

public class SteamStatsAndAchievementsPatches : ClassPatch
{
    private static readonly string[] DisabledMethods =
    [
        "Update", "OnUserStatsReceived", "OnUserStatsStored", "OnAchievementStored", "ResetALL", "OnStatsUpdated",
        "OnEnable"
    ];

    private static bool Disable()
    {
        return false;
    }
    public override Exception Patch(Harmony harmony)
    {

        var steamStatsAndAchievementsClass = typeof(SteamStatsAndAchievements);
        Exception e = null;
        foreach (var method in DisabledMethods)
        {
            var tmp = MakePatch(harmony, steamStatsAndAchievementsClass, method, nameof(Disable));
            if(tmp is not null) e = tmp;
        }
        return e;
    }
}