using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ArchipelagoClusterTruck.Patches;

// ReSharper disable once InconsistentNaming
public class abilityMenuPatches : ClassPatch
{
    private static MethodInfo _abilityInfoAbilityEnumGetter;
    // ReSharper disable once InconsistentNaming
    static bool BuyOrEquipPrefix(abilityMenu __instance)
    {
        info.Abilities ability = (info.Abilities)_abilityInfoAbilityEnumGetter.Invoke(__instance.selectedButton,null);
        if (Plugin.Data.UnlockedAbilities.Contains(ability))
        {
            if (info.abilityName == __instance.selectedButton.name ||
                info.utilityName == __instance.selectedButton.name)
            {
                if (!Plugin.Data.CheckedAbilities.Contains(ability))
                {
                    pointsHandler.AddPoints(-__instance.selectedButton.AbilityCost);
                    Plugin.Data.CheckedAbilities.Add(ability);
                    //TODO run location check for ability
                }
                else
                {
                    if (info.abilityName == __instance.selectedButton.name)
                    {
                        info.abilityName = string.Empty;
                        PlayerPrefs.SetString(info.ABILITY_MOVEMENT_KEY, string.Empty);
                        __instance.usedM = null;
                    }
                    else
                    {
                        info.utilityName = string.Empty;
                        PlayerPrefs.SetString(info.ABILITY_UTILITY_KEY, string.Empty);
                    }
                }
            }
            else if (__instance.selectedButton.movement)
            {
                info.abilityName = __instance.selectedButton.infoField[0];
                PlayerPrefs.SetString(info.ABILITY_MOVEMENT_KEY, __instance.selectedButton.name);
                __instance.usedM = __instance.selectedButton.transform;
            }
            else
            {
                info.utilityName = __instance.selectedButton.infoField[0];
                PlayerPrefs.SetString(info.ABILITY_UTILITY_KEY, __instance.selectedButton.name);
                __instance.usedU = __instance.selectedButton.transform;
            }
        }
        if (!Plugin.Data.CheckedAbilities.Contains(ability))
        {
            pointsHandler.AddPoints(-__instance.selectedButton.AbilityCost);
            Plugin.Data.CheckedAbilities.Add(ability);
            //TODO run location check for ability
        }
        return false;
    }
    public override Exception Patch(Harmony harmony)
    {
        _abilityInfoAbilityEnumGetter = AccessTools.PropertyGetter(typeof(abilityInfo), "_abilityNumber");
        return MakePatch(harmony, typeof(abilityMenu), "BuyOrEquip", nameof(BuyOrEquipPrefix));
    }
}