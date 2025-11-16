using System;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

public class GameManagerPatches : ClassPatch
{
    private static int deaths = 0;
    private static void LoseLevelPostFix(bool ___done)
    {
        if (ArchipelagoManager.DeathLinkSendingDeath)
        {
            ArchipelagoManager.DeathLinkSendingDeath = false;
            return;
        }
        if (___done) return;
#if VERBOSE
        Plugin.Logger.LogInfo(new System.Diagnostics.StackTrace());
#endif
        if (!ArchipelagoManager.DeathLinkEnabled || ArchipelagoManager.Session == null) return;

        Plugin.Assert(ArchipelagoManager.SlotNumber != null, "ArchipelagoManager.SlotNumber != null");
        if (++deaths < Plugin.Data.DeathsForDeathlink) return;

        deaths = 0;
        ArchipelagoManager.DeathLinkService.SendDeathLink(
            new DeathLink(ArchipelagoManager.Session.Players.GetPlayerAlias(ArchipelagoManager.SlotNumber.Value)));
    }

    private static void WinLevelPrefix()
    {
        Plugin.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        if (!Plugin.Data.CompletedLevels.Contains(LevelSeletHandlerPatches.selectedLevel))
        {
            Plugin.Data.CompletedLevels.Add(LevelSeletHandlerPatches.selectedLevel);
            if (Plugin.Data.Goal == LevelSeletHandlerPatches.selectedLevel)
                ArchipelagoManager.Session.SetGoalAchieved();
            else
                ArchipelagoManager.Check(LevelSeletHandlerPatches.selectedLevel);

            if (Plugin.Data.CompletedLevels.Count >= Plugin.Data.GoalRequirement && !Plugin.Data.AvailableLevels.Contains(Plugin.Data.Goal))
                Plugin.Data.AvailableLevels.Add(Plugin.Data.Goal);
        }

        ArchipelagoManager.Session.DataStorage[Scope.Slot, "points"] = pointsHandler.Points;
    }

    private static void FinalBossBeatPostfix()
    {
        WinLevelPrefix();
    }
    public override Exception Patch(Harmony harmony)
    {
        var e1 = MakePatch(harmony, typeof(GameManager), nameof(GameManager.WinLevel), nameof(WinLevelPrefix));
        var e2 = MakePatch(harmony, typeof(GameManager), nameof(GameManager.LoseLevel), nameof(LoseLevelPostFix));
        var e3 = MakePatch(harmony, typeof(LastBoss), "TheEnd", nameof(FinalBossBeatPostfix));
        return e1 ?? e2 ?? e3;
    }
}
