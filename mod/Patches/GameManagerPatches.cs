using System;
using System.Diagnostics;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

public class GameManagerPatches : ClassPatch
{
    static void LoseLevelPostFix(bool ___done)
    {
        if (ArchipelagoManager.DeathLinkSendingDeath)
        {
            ArchipelagoManager.DeathLinkSendingDeath = false;
            return;
        }
        if(___done) return;
#if VERBOSE
        Plugin.Logger.LogInfo(new StackTrace());
#endif
        if(!ArchipelagoManager.DeathLinkEnabled || ArchipelagoManager.Session == null) return;

        Debug.Assert(ArchipelagoManager.SlotNumber != null, "ArchipelagoManager.SlotNumber != null");
        ArchipelagoManager.DeathLinkService.SendDeathLink(new DeathLink(ArchipelagoManager.Session.Players.GetPlayerAlias(ArchipelagoManager.SlotNumber.Value)));
    }
    static void WinLevelPrefix()
    {
        Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        if (!Plugin.Data.CompletedLevels.Contains(info.currentLevel-1))
        {
            Plugin.Data.CompletedLevels.Add(info.currentLevel-1);
            if (Plugin.Data.Goal == info.currentLevel - 1)
                ArchipelagoManager.Session.SetGoalAchieved();
            else
                ArchipelagoManager.Check(info.currentLevel - 1);
            
            if (Plugin.Data.CompletedLevels.Count >= Plugin.Data.GoalRequirement)
                Plugin.Data.AvailableLevels.Add(Plugin.Data.Goal);
        }

        ArchipelagoManager.Session.DataStorage[Scope.Slot, "points"] = pointsHandler.Points;
    }

    static void FinalBossBeatPostfix()
    {
        WinLevelPrefix();
    }
    public override Exception Patch(Harmony harmony)
    {
        var e1 = MakePatch(harmony,typeof(GameManager), nameof(GameManager.WinLevel),nameof(WinLevelPrefix));
        var e2 = MakePatch(harmony, typeof(GameManager), nameof(GameManager.LoseLevel), nameof(LoseLevelPostFix));
        var e3 = MakePatch(harmony, typeof(LastBoss),"TheEnd",nameof(FinalBossBeatPostfix));
        return e1 ?? e2 ?? e3;
    }
}