using System;
using HarmonyLib;

namespace ArchipelagoClusterTruck.Patches;

public class pointsHandlerPatches : ClassPatch
{
    private static bool StartPrefix(ref int ____currentPoints)
    {
        ____currentPoints = 0;
        return false;
    }

    private static bool AddPointsPrefix(ref int ____currentPoints, float p)
    {
        if (p > 0)
            p *= Plugin.Data.PointMultiplier;

        var points = (int)p;
        ____currentPoints = int.MaxValue - points < ____currentPoints ? int.MaxValue : ____currentPoints + points;
        return false;
    }

    private static bool ResetPointsPrefix(ref int ____currentPoints)
    {
        ____currentPoints = 0;
        return false;
    }

    public override Exception Patch(Harmony harmony)
    {
        var pointsHandlerType = typeof(pointsHandler);
        var e1 = MakePatch(harmony, pointsHandlerType, "Start", nameof(StartPrefix));
        var e2 = MakePatch(harmony, pointsHandlerType, "AddPoints", nameof(AddPointsPrefix));
        var e3 = MakePatch(harmony, pointsHandlerType, "ResetPoints", nameof(ResetPointsPrefix));
        return e1 ?? e2 ?? e3;
    }
}