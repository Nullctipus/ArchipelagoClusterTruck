#nullable enable
using System;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace ArchipelagoClusterTruck.Patches;

public abstract class ClassPatch
{
    // ReSharper disable once UnusedMemberInSuper.Global
    public abstract Exception? Patch(Harmony harmony);

    protected Exception? MakePatch(Harmony harmony,Type originalType, string original, string? prefix = null, string? postfix = null)
    {
        HarmonyMethod? prefixMethod = null;
        HarmonyMethod? postfixMethod = null;
        try
        {
            if (!string.IsNullOrEmpty(prefix))
                prefixMethod =
                    new HarmonyMethod(GetType().GetMethod(prefix, BindingFlags.Static | BindingFlags.NonPublic));
            if (!string.IsNullOrEmpty(postfix))
                prefixMethod =
                    new HarmonyMethod(GetType().GetMethod(postfix, BindingFlags.Static | BindingFlags.NonPublic));
            harmony.Patch(originalType.GetMethod(original,BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic), prefixMethod, postfixMethod);
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}