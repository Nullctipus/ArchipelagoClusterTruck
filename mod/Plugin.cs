using System;
using System.Linq;
using System.Reflection;
using ArchipelagoClusterTruck.Patches;
using ArchipelagoClusterTruck.UI;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Debug = System.Diagnostics.Debug;


namespace ArchipelagoClusterTruck;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static SaveData Data;
    internal static UIManager _uiManager;

    private void Awake()
    {
        _ = new Configuration(Config);
        Data = new SaveData();
        _uiManager = gameObject.AddComponent<UIManager>();
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        PatchManager.PatchAll();
        
        OnLevelWasLoaded(0);
    }

    void OnLevelWasLoaded(int _)
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            EventTrigger bigPlayButton = Resources.FindObjectsOfTypeAll<selectMe>().First(x => x.name == "play" && x.transform.parent.name == "Main").GetComponents<EventTrigger>().First(y=>y.triggers.Count == 2);
            bigPlayButton.triggers[0].callback.m_Calls.Clear();
            bigPlayButton.triggers[1].callback.m_Calls.Clear();
            bigPlayButton.triggers[0].callback.m_PersistentCalls.Clear();
            bigPlayButton.triggers[1].callback.m_PersistentCalls.Clear();
            InvokableCall call = new InvokableCall(() =>
            {
                Manager.Instance().GoToLevelSelect();/*
                if (ArchipelagoManager.Session != null)
                {
                    Manager.Instance().GoToLevelSelect();
                }
                else
                {
                    Logger.LogWarning("There is no Archipelago Session! stopping you to make sure your save data doesn't get deleted");
                }*/
            });
            bigPlayButton.triggers[0].callback.m_Calls.AddListener(call);
            bigPlayButton.triggers[1].callback.m_Calls.AddListener(call);

        }
    }
}
