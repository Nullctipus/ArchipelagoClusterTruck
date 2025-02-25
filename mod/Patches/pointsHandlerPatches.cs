using System;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

public class pointsHandlerPatches : ClassPatch
{
    static bool StartPrefix(ref int ____currentPoints)
    {
        ____currentPoints = 0;
        return false;
    }

    static bool AddPointsPrefix(ref int ____currentPoints, float p)
    {
        ____currentPoints+=(int) p;
        return false;
    }

    static bool ResetPointsPrefix(ref int ____currentPoints)
    {
        ____currentPoints = 0;
        return false;
    }

    public override Exception Patch(Harmony harmony)
    {
        Type pointsHandlerType = typeof(pointsHandler);
        var e1 = MakePatch(harmony, pointsHandlerType, "Start", nameof(StartPrefix));
        var e2 = MakePatch(harmony, pointsHandlerType, "AddPoints", nameof(AddPointsPrefix));
        var e3 = MakePatch(harmony, pointsHandlerType, "ResetPoints", nameof(ResetPointsPrefix));
        return e1 ?? e2 ?? e3;
    }
}