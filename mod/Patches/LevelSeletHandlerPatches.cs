using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ArchipelagoClusterTruck.Patches;

public class LevelSeletHandlerPatches : ClassPatch
{
    // ReSharper disable twice InconsistentNaming
    static bool PrefixSetButtons(LevelSeletHandler __instance, GameObject[] ___mLevels )
    {
        if (ArchipelagoManager.Session == null) return true;
        
        __instance.left.SetActive(true);
        __instance.right.SetActive(true);
        
        switch (info.currentWorld)
        {
            case <= 0:
                __instance.left.SetActive(false);
                break;
            case >= 9:
                __instance.right.SetActive(false);
                break;
        }

        foreach (GameObject go in ___mLevels)
        {
            int level = info.currentWorld*10 + int.Parse(go.name);
            Transform box = go.transform.FindChild("box");
            if (Plugin.Data.CompletedLevels.Contains(level))
            {
                box.gameObject.SetActive(true);
                go.transform.FindChild("road").GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
                box.GetComponent<blink>().enabled = false;
                box.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            } 
            else if (Plugin.Data.AvailableLevels.Contains(level))
            {
                go.transform.FindChild("road").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
                box.gameObject.SetActive(true);
                box.GetComponent<blink>().enabled = true; 
            } 
            else
            {
                box.gameObject.SetActive(false);
                go.transform.FindChild("road").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            }
        }

        return false;
    }

    public override Exception Patch(Harmony harmony)
    {
        return MakePatch(harmony, typeof(LevelSeletHandler), nameof(LevelSeletHandler.setButtons), nameof(PrefixSetButtons));
    }
}