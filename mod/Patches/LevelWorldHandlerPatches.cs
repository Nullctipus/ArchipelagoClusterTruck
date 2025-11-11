using System;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

public class LevelWorldHandlerPatches : ClassPatch
{
    private static bool UpdatePrefix(LevelWorldHandler __instance)
    {
        __instance.worldTextUnder.text = LevelSeletHandlerPatches.currentWorld switch
        {
            0 => "DESERT",
            1 => "FOREST",
            2 => "WINTER",
            3 => "LASER",
            4 => "MEDIEVAL",
            5 => "ANCIENT",
            6 => "SCI-FI",
            7 => "STEAMPUNK",
            8 => "HELL",
            9 => "SEASONAL",
            10 => "WINTER SEASONAL",
            _ => throw new Exception("Invalid World number: " + info.currentWorld)
        };
        __instance.worldText.text = LevelSeletHandlerPatches.currentWorld switch
        {
            9 => "HALLOWEEN",
            10 => "HOLIDAYS",
            _ => "WORLD " + (LevelSeletHandlerPatches.currentWorld + 1),
        };
        return false;
    }

    public override Exception Patch(Harmony harmony)
    {
        return MakePatch(harmony, typeof(LevelWorldHandler), "Update", nameof(UpdatePrefix));
    }
}