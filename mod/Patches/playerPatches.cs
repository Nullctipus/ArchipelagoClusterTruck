using System;
using System.Diagnostics;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

// ReSharper disable once InconsistentNaming
public class playerPatches : ClassPatch
{
    static void FreezeDiePrefix()
    {
        if(!ArchipelagoManager.DeathLinkEnabled || ArchipelagoManager.Session == null) return;

        Debug.Assert(ArchipelagoManager.SlotNumber != null, "ArchipelagoManager.SlotNumber != null");
        ArchipelagoManager.DeathLinkService.SendDeathLink(new DeathLink(ArchipelagoManager.Session.Players.GetPlayerAlias(ArchipelagoManager.SlotNumber.Value)));
    }
    
    public override Exception Patch(Harmony harmony)
    {
        Exception e1 = MakePatch(harmony,typeof(player),nameof(player.Freeze),nameof(FreezeDiePrefix));
        Exception e2 = MakePatch(harmony,typeof(player),nameof(player.Die),nameof(FreezeDiePrefix));
        return e1 ?? e2;
    }
}