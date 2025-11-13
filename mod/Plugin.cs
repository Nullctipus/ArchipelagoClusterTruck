using System.Diagnostics;
using System.Linq;
using ArchipelagoClusterTruck.Patches;
using ArchipelagoClusterTruck.UI;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;


namespace ArchipelagoClusterTruck;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;

    internal static SaveData Data;
    internal static UIManager _uiManager;

    private void Awake()
    {
        Logger = base.Logger;
        _ = new Configuration(Config);
        Data = new SaveData();
        _uiManager = gameObject.AddComponent<UIManager>();
        // Plugin startup logic
        PatchManager.PatchAll();

        OnLevelWasLoaded(0);
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnLevelWasLoaded(int _)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "MainScene":
                var bigPlayButton = Resources.FindObjectsOfTypeAll<selectMe>()
                    .First(x => x.name == "play" && x.transform.parent.name == "Main").GetComponents<EventTrigger>()
                    .First(y => y.triggers.Count == 2);
                bigPlayButton.triggers[0].callback.m_Calls.Clear();
                bigPlayButton.triggers[1].callback.m_Calls.Clear();
                bigPlayButton.triggers[0].callback.m_PersistentCalls.Clear();
                bigPlayButton.triggers[1].callback.m_PersistentCalls.Clear();
                var call = new InvokableCall(() =>
                {
                    if (ArchipelagoManager.Session != null)
                    {
                        Manager.Instance().GoToLevelSelect();
                    }
                    else
                    {
                        Logger.LogWarning(
                            "There is no Archipelago Session! stopping you to make sure your save data doesn't get deleted");
                    }
                });
                bigPlayButton.triggers[0].callback.m_Calls.AddListener(call);
                bigPlayButton.triggers[1].callback.m_Calls.AddListener(call);
                break;
        }
    }

    public static void Assert(bool condition, string message = null)
    {
        if (condition) return;
        var st = new StackTrace();
        Logger.LogError($"[ASSERTION FAILED]{(message == null ? (" " + message) : "")}\n{st}");
        PopupHelper.MessageBox($"{(message == null ? (" " + message) : "")}\n{st}", "Assert Failed!");
        //throw new AssertionException(message, message);
    }
}
