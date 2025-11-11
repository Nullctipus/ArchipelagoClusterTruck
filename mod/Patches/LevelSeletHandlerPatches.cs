using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ArchipelagoClusterTruck.Patches;

public class LevelSeletHandlerPatches : ClassPatch
{
    internal static int selectedLevel;
    internal static int currentWorld;

    private static bool PrefixSetButtons(LevelSeletHandler __instance, GameObject[] ___mLevels)
    {
        Plugin.Logger.LogDebug("PrefixSetButtons");
        var playButton = GameObject.Find("GameManager/levelSelect/Canvas/play");
        playButton?.SetActive(false);
        __instance.left.SetActive(true);
        __instance.right.SetActive(true);

        switch (currentWorld)
        {
            case <= 0:
                __instance.left.SetActive(false);
                break;
            case >= 10:
                __instance.right.SetActive(false);
                break;
        }

        for (var index = 0; index < ___mLevels.Length; index++)
        {
            var go = ___mLevels[index];
            if (currentWorld == 10 && index > 4)
                go.SetActive(false);
            else
                go.SetActive(true);

            var lb = go.GetComponent<levelButton>();
            if (lb)
                Object.Destroy(lb);
            var levelIdx = currentWorld * 10 + int.Parse(go.name) - 1;
            var box = go.transform.FindChild("box");

            if (Plugin.Data.AvailableLevels.Count > levelIdx &&
                Plugin.Data.CompletedLevels.Contains(Plugin.Data.AvailableLevels[levelIdx]))
            {
                box.gameObject.SetActive(true);
                go.GetComponentInChildren<Text>().text =
                    SaveData.FormatLevelName(Plugin.Data.AvailableLevels[levelIdx]);
                go.transform.FindChild("road").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
                box.GetComponent<blink>().enabled = false;
                box.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            }
            else if (Plugin.Data.AvailableLevels.Count > levelIdx)
            {
                go.GetComponentInChildren<Text>().text =
                    SaveData.FormatLevelName(Plugin.Data.AvailableLevels[levelIdx]);
                go.transform.FindChild("road").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
                box.gameObject.SetActive(true);
                box.GetComponent<blink>().enabled = true;
            }
            else
            {
                go.GetComponentInChildren<Text>().text = "???";
                box.gameObject.SetActive(false);
                go.transform.FindChild("road").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            }
        }

        return false;
    }


    private static bool OnSubmitPrefix(LevelSeletHandler __instance, BaseEventData b,
        ref GameObject ____currentlySelectedLevel, ref Vector3 ___selectTarget)
    {
        if (!b.selectedObject.CompareTag("levelButton"))
            return false;
        var level = int.Parse(b.selectedObject.name) + currentWorld * 10 - 1;

        if (Plugin.Data.AvailableLevels.Count > level)
        {
            if (!____currentlySelectedLevel)
                ____currentlySelectedLevel = b.selectedObject;
            else if (____currentlySelectedLevel == b.selectedObject)
            {
                switch (selectedLevel)
                {
                    case <= 89:
                        LevelSeletHandler.seasonal = 0;
                        info.currentLevel = selectedLevel;
                        break;
                    case <= 99:
                        LevelSeletHandler.seasonal = info.HalloweenSeasonalSpecifier;
                        info.currentLevel = selectedLevel - 89;
                        info.currentHalloweenLevel = selectedLevel - 89;
                        break;
                    case <= 104:
                        LevelSeletHandler.seasonal = info.WinterSeasonalSpecifier;
                        info.currentLevel = selectedLevel - 99;
                        info.CurrentChristmasLevel = selectedLevel - 99;
                        break;
                }

                Manager.Instance().Play();
            }
            else
                ____currentlySelectedLevel = b.selectedObject;

            ___selectTarget = b.selectedObject.transform.position;
            if (Plugin.Data.AvailableLevels[level] <= 89)
                info.currentLevel = Plugin.Data.AvailableLevels[level] + 1;
            selectedLevel = Plugin.Data.AvailableLevels[level];
        }
        else
        {
            __instance.selectShake.SetShake(10f);
        }

        return false;
    }

    private static bool MoveWorldPrefix(LevelSeletHandler __instance, bool forward)
    {
        Object.FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
        if (forward)
            info.currentWorld = currentWorld = currentWorld >= 10 ? 10 : currentWorld + 1;
        else
            info.currentWorld = currentWorld = currentWorld <= 0 ? 0 : currentWorld - 1;

        for (var i = 0; i < __instance.worlds.Length; i++)
            __instance.worlds[i].SetActive(i==currentWorld);
        
        __instance.setButtons();
        return false;
    }

    public override Exception Patch(Harmony harmony)
    {
        var e1 = MakePatch(harmony, typeof(LevelSeletHandler), nameof(LevelSeletHandler.setButtons),
            nameof(PrefixSetButtons));
        var e2 = MakePatch(harmony, typeof(LevelSeletHandler), nameof(LevelSeletHandler.OnSubmit),
            nameof(OnSubmitPrefix));
        var e3 = MakePatch(harmony, typeof(LevelSeletHandler), nameof(LevelSeletHandler.MoveWorld),
            nameof(MoveWorldPrefix));
        return e1 ?? e2 ?? e3;
    }
}