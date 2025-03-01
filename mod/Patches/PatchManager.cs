
#nullable enable
using System;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

public static class PatchManager {
    public static void PatchAll(){
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        Exception? e = null;
        try{
		e = new ArchipelagoClusterTruck.Patches.abilityManagerPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
		e = new ArchipelagoClusterTruck.Patches.abilityMenuPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
		e = new ArchipelagoClusterTruck.Patches.carPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
		e = new ArchipelagoClusterTruck.Patches.GameManagerPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
		e = new ArchipelagoClusterTruck.Patches.LevelSeletHandlerPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
		e = new ArchipelagoClusterTruck.Patches.pointsHandlerPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
		e = new ArchipelagoClusterTruck.Patches.SteamStatsAndAchievementsPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
		e = new ArchipelagoClusterTruck.Patches.steam_WorkshopHandlerPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
		e = new ArchipelagoClusterTruck.Patches.WinScreenPatches().Patch(harmony);
		if(e != null) Plugin.Logger.LogError("Harmony Patch Error: " + e);
        }
        catch(Exception ex){
            Plugin.Logger.LogError($"An Error Escaped Patch Manager. This Should never Happen.\n{ex}");
        }
    }
}