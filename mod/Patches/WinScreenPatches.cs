using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ArchipelagoClusterTruck.Patches;

public class WinScreenPatches : ClassPatch
{
    private static bool SmallEnablePrefix(leaderboardsManager __instance, string map, string PureRun = "")
    {
        __instance.gameObject.SetActive(false);
        return false;
    }

    static IEnumerator SwitchSelected(GameObject ___buttons, GameObject ___endOfCycleButtons)
    {
        yield return null;
        ___endOfCycleButtons.SetActive(false);
        ___buttons.SetActive(true);   
    }
    private static void ScoreScreenPostfix(scoreScreenHandler __instance, GameObject ___buttons, GameObject ___endOfCycleButtons)
    {
        __instance.StartCoroutine(SwitchSelected(___buttons, ___endOfCycleButtons));
        var nextButton = __instance.transform.Find("scoreStuff/buttons/next");
        nextButton.GetComponentInChildren<Text>().text = "LEVEL SELECT";
        var trigger = nextButton.GetComponent<EventTrigger>();
        var pause = __instance.transform.parent.parent.Find("pause").GetComponent<pauseScreen>();
        var call = new InvokableCall(() =>
        {
            LevelSeletHandler.seasonal = 0;
            pause.LevelSelect();
            var levelSeletHandler = Resources.FindObjectsOfTypeAll<LevelSeletHandler>().FirstOrDefault();
            Plugin.Assert(levelSeletHandler != null, nameof(levelSeletHandler) + " != null");
            levelSeletHandler.setButtons();
        });
        foreach (var t in trigger.triggers)
        {
            t.callback.m_Calls.Clear();
            t.callback.m_PersistentCalls.Clear();
            t.callback.m_Calls.AddListener(call);
        }
    }

    public override Exception Patch(Harmony harmony)
    {
        var e1 = MakePatch(harmony, typeof(leaderboardsManager), nameof(leaderboardsManager.SmallEnable),
            nameof(SmallEnablePrefix));
        var e2 = MakePatch(harmony, typeof(scoreScreenHandler), nameof(scoreScreenHandler.StartScoreScreen),
            null,nameof(ScoreScreenPostfix));
        return e1 ?? e2;
    }
}