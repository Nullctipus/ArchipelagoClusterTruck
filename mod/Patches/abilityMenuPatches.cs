using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

namespace ArchipelagoClusterTruck.Patches;

// ReSharper disable InconsistentNaming
public class abilityMenuPatches : ClassPatch
{
    private static MethodInfo _abilityInfoAbilityEnumGetter;
    private static MethodInfo _abilityMenuCheckButtons;
    private static MethodInfo _abilityMenuSelectAbilityController;
    private static readonly int Active = Animator.StringToHash("active");

    static bool BuyOrEquipPrefix(abilityMenu __instance)
    {
        info.Abilities ability = (info.Abilities)_abilityInfoAbilityEnumGetter.Invoke(__instance.selectedButton,null);
        if (Plugin.Data.UnlockedAbilities.Contains(ability))
        {
            if (info.abilityName == __instance.selectedButton.name ||
                info.utilityName == __instance.selectedButton.name)
            {
                if (!Plugin.Data.CheckedAbilities.Contains(ability) && pointsHandler.Points >= __instance.selectedButton.AbilityCost)
                {
                    CheckAbility(__instance,ability);
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
                        __instance.usedU = null;
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
        else if (!Plugin.Data.CheckedAbilities.Contains(ability) && pointsHandler.Points >= __instance.selectedButton.AbilityCost)
        {
            CheckAbility(__instance,ability);
        }
        return false;
    }

    static void CheckAbility(abilityMenu menu, info.Abilities ability)
    {
        pointsHandler.AddPoints(-menu.selectedButton.AbilityCost);
        Plugin.Data.CheckedAbilities.Add(ability);
        _abilityMenuCheckButtons.Invoke(menu, null);
        ArchipelagoManager.Check(ability);
        ArchipelagoManager.Session.DataStorage[Scope.Slot, "points"] = pointsHandler.Points;
    }

    static bool CheckButtonPrefix(Button b)
    {
        var info = b.GetComponent<abilityInfo>();
        ColorBlock colors = default(ColorBlock);
        colors.pressedColor = new Color(1f, 1f, 1f, 1f);
        colors.colorMultiplier = 1f;
        bool owned = Plugin.Data.UnlockedAbilities.Contains((info.Abilities)_abilityInfoAbilityEnumGetter.Invoke(info, null));
        bool check = Plugin.Data.CheckedAbilities.Contains((info.Abilities)_abilityInfoAbilityEnumGetter.Invoke(info, null));
        Color usedColor;
        if (owned)
            usedColor = check ? Configuration.Instance.OwnedCheckedColor.Value : Configuration.Instance.OwnedUncheckedColor.Value;
        else
            usedColor = check ? Configuration.Instance.UnownedCheckedColor.Value : Configuration.Instance.UnownedUncheckedColor.Value;
        Color withAlpha = usedColor;
        withAlpha.a = 0.7f;
        colors.normalColor = usedColor;
        colors.highlightedColor = withAlpha;
        colors.disabledColor = usedColor;
        if (owned || check)
        {
            foreach (Text text in b.GetComponentsInChildren<Text>())
                text.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            foreach (Image image in b.GetComponentsInChildren<Image>().Where(x=> x.gameObject != b.gameObject))
                image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        b.colors = colors;
        
        return false;
    }
    static bool OnSubmitPrefix(abilityMenu __instance, BaseEventData p, ref GameObject ___lastObject)
    {
        if (!p.selectedObject.CompareTag("ability"))
            return false;
        bool check = false;
        if (p.selectedObject.gameObject == ___lastObject)
        { 
            if(Plugin.Data.UnlockedAbilities.Contains(
                   (info.Abilities)_abilityInfoAbilityEnumGetter.Invoke(p.selectedObject.GetComponent<abilityInfo>(), null)))
                __instance.BuyOrEquip();
        }
        else
        {
            __instance.selectedButton = p.selectedObject.GetComponent<abilityInfo>();

            info.Abilities ability =
                (info.Abilities)_abilityInfoAbilityEnumGetter.Invoke(__instance.selectedButton, null);
             check =
                Plugin.Data.CheckedAbilities.Contains(ability);
             
             Debug.Assert(Plugin.Data.abilityHints.ContainsKey(ability));
             string[] infoField = [Plugin.Data.abilityHints[ability],""];
             
            __instance.myUI.changeAbility(infoField, __instance.selectedButton.AbilityCost.ToString(),check);
        }
        check =
            Plugin.Data.CheckedAbilities.Contains(
                (info.Abilities)_abilityInfoAbilityEnumGetter.Invoke(__instance.selectedButton, null));
        InputDevice activeDevice = InputManager.ActiveDevice;
        if (activeDevice != null)
        {
            if (activeDevice.Action1.WasPressed)
            {
                if (__instance.pressedButtonController)
                {
                    if (__instance.pressedButtonController == ___lastObject.GetComponent<abilityInfo>())
                        __instance.BuyOrEquip();
                    else
                        _abilityMenuSelectAbilityController.Invoke(__instance, [__instance.selectedButton]);
                    
                }
                else
                    _abilityMenuSelectAbilityController.Invoke(__instance, [__instance.selectedButton]);
                
            }
            else
            {
                __instance.UnlockButton.GetComponentInChildren<Text>().text = "CHECK";
                __instance.pressedButtonController = null;
            }
        }
        ___lastObject = p.selectedObject.gameObject;
        __instance.UnlockButton.GetComponent<Animator>().SetBool(Active, value: !check);
        return false;
    }

    static void OnEnablePostfix(abilityMenu __instance)
    {
        //We don't know if the player obtained something from a level
        _abilityMenuCheckButtons.Invoke(__instance, null);
    }
    public override Exception Patch(Harmony harmony)
    {
        _abilityInfoAbilityEnumGetter = AccessTools.PropertyGetter(typeof(abilityInfo), "_abilityNumber");
        Debug.Assert(_abilityInfoAbilityEnumGetter != null);
        _abilityMenuCheckButtons = AccessTools.Method(typeof(abilityMenu), "CheckButtons");
        Debug.Assert(_abilityMenuCheckButtons != null);
        _abilityMenuSelectAbilityController = AccessTools.Method(typeof(abilityMenu), "SelectAbilityController");
        Debug.Assert(_abilityMenuSelectAbilityController != null);
        var e1 = MakePatch(harmony, typeof(abilityMenu), "BuyOrEquip", nameof(BuyOrEquipPrefix));
        var e2 = MakePatch(harmony, typeof(abilityMenu), "CheckButton", nameof(CheckButtonPrefix));
        var e3 = MakePatch(harmony, typeof(abilityMenu), "OnSubmit", nameof(OnSubmitPrefix));
        var e4 = MakePatch(harmony, typeof(abilityMenu), "OnEnable", null,nameof(OnEnablePostfix));
        return e1 ?? e2 ?? e3 ?? e4;
    }
}