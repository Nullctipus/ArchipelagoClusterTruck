using System;
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

    private static void ScoreScreenPrefix(scoreScreenHandler __instance)
    {
        var nextButton = __instance.transform.Find("scoreStuff/buttons/next");
        nextButton.GetComponentInChildren<Text>().text = "LEVEL SELECT";
        var trigger = nextButton.GetComponent<EventTrigger>();
        var pause = __instance.transform.parent.parent.Find("pause").GetComponent<pauseScreen>();
        var call = new InvokableCall(() =>
        {
            pause.LevelSelect();
            var lsh = Resources.FindObjectsOfTypeAll<LevelSeletHandler>().FirstOrDefault();
            if(lsh)
                
                lsh.setButtons();
            else
                Plugin.Logger.LogError("Could not find LevelSeletHandler");
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
        var e1 = MakePatch(harmony,typeof(leaderboardsManager),nameof(leaderboardsManager.SmallEnable), nameof(SmallEnablePrefix));
        var e2 = MakePatch(harmony,typeof(scoreScreenHandler),nameof(scoreScreenHandler.StartScoreScreen), nameof(ScoreScreenPrefix));
        return e1 ?? e2;
    }
}