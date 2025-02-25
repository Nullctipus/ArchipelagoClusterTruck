using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ArchipelagoClusterTruck.Patches;

public class LevelSeletHandlerPatches : ClassPatch
{
    // ReSharper disable twice InconsistentNaming
    static bool PrefixSetButtons(LevelSeletHandler __instance, GameObject[] ___mLevels )
    {
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
            int level = info.currentWorld*10 + int.Parse(go.name)-1;
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

    static bool OnSubmitPrefix(LevelSeletHandler __instance, BaseEventData b,ref GameObject ____currentlySelectedLevel, ref Vector3 ___selectTarget )
    {
        if (!b.selectedObject.CompareTag("levelButton"))
            return false;
        int level = int.Parse(b.selectedObject.name) + info.currentWorld * 10 - 1;
        
        if (Plugin.Data.AvailableLevels.Contains(level))
        {
            if (!____currentlySelectedLevel)
                ____currentlySelectedLevel = b.selectedObject;
            else if (____currentlySelectedLevel == b.selectedObject)
                Manager.Instance().Play();
            else
                ____currentlySelectedLevel = b.selectedObject;
            
            ___selectTarget = b.selectedObject.transform.position;
            info.currentLevel = int.Parse(b.selectedObject.name) + info.currentWorld * 10;
            
        }
        else
        {
            __instance.selectShake.SetShake(10f);
        }
        return false;
    }

    static void MoveWorldPostfix()
    {
        Object.FindObjectOfType<EventSystem>().SetSelectedGameObject(null);
    }

    public override Exception Patch(Harmony harmony)
    {
        var e1 = MakePatch(harmony, typeof(LevelSeletHandler), nameof(LevelSeletHandler.setButtons), nameof(PrefixSetButtons));
        var e2 = MakePatch(harmony, typeof(LevelSeletHandler), nameof(LevelSeletHandler.OnSubmit), nameof(OnSubmitPrefix));
        var e3 = MakePatch(harmony,typeof(LevelSeletHandler),nameof(LevelSeletHandler.MoveWorld),null,nameof(MoveWorldPostfix));
        return e1 ?? e2 ?? e3;
    }
}