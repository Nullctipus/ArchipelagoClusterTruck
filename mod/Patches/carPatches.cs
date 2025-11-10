using System;
using HarmonyLib;
using UnityEngine;

namespace ArchipelagoClusterTruck.Patches;

public class carPatches : ClassPatch
{
    private static void StartPostFix(car __instance)
    {
        if(!Configuration.Instance.RandomTruckColor.Value) return;
        var c = ColorUtility.RandomColor();
        __instance.rend1.material.color = c;
        __instance.rend2.material.color = c;
    }
    public override Exception Patch(Harmony harmony)
    {
        return MakePatch(harmony, typeof(car),"Start",null,nameof(StartPostFix));
    }
}