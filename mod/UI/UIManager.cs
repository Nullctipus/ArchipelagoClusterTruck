using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace ArchipelagoClusterTruck.UI;

public class UIManager : MonoBehaviour
{
    private GUIStyle _messageStyle;
    private readonly LinkedList<string> _messages = [];

    public void PushMessage(string text)
    {
        while (_messages.Count > Configuration.Instance.MessageLogCount.Value)
            _messages.RemoveLast();
        _messages.AddFirst(text);
    }
    private void Awake()
    {
        Configuration.Instance.MessageLogOrigin.SettingChanged += (sender, args) =>
        {
            if (_messageStyle == null) return;

            _messageStyle.alignment = Configuration.Instance.MessageLogOrigin.Value;
        };
        ArchipelagoManager.OnConnect += OnArchipelagoConnect;
    }

    private void OnArchipelagoConnect()
    {
        Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        ArchipelagoManager.Session.MessageLog.OnMessageReceived += (message) =>
        {
            var text = string.Join("", message.Parts.Select(part => ColorUtility.CreateRichColor(part.Text, part.IsBackgroundColor,part.Color)).ToArray());

            PushMessage(text);
        };
        
    }
    private bool _showMessageLog = true;
    
    private Rect _connectionWindow = new Rect(10, 10, 320, 500);
    private readonly int _connectionWindowId = Random.RandomRangeInt(0, int.MaxValue);
    private bool _showConnectionWindow = true;

    private void Update()
    {
        if(_tryDisconnect > 0)
            _tryDisconnect-=Time.deltaTime;
        if(Input.GetKeyDown(Configuration.Instance.ToggleMessageLogKey.Value))
            _showMessageLog = !_showMessageLog;
        if(Input.GetKeyDown(Configuration.Instance.ToggleConnectionWindowKey.Value))
            _showConnectionWindow = !_showConnectionWindow;
        #if DEBUG
        if (Input.GetKeyDown(Configuration.Instance.ToggleDebugKey.Value))
            _showDebugWindow = !_showDebugWindow;
        #endif
    }

    private void OnGUI()
    {
        _messageStyle ??= new GUIStyle(GUI.skin.label)
        {
            richText = true,
            alignment = Configuration.Instance.MessageLogOrigin.Value
        };
        
        if(_showMessageLog)
            DrawMessageLog();
        
        if (_showConnectionWindow)
            _connectionWindow = GUI.Window(_connectionWindowId, _connectionWindow, ArchipelagoManager.Session != null ? DrawConnected : DrawDisconnected,"Connection");
        
        #if DEBUG
        if(_showDebugWindow)
            _debugWindow = GUI.Window(_debugWindowId,_debugWindow,DrawDebugWindow, "Debug");
        #endif
    }

    private string _sendMessage = string.Empty;
    private bool _deathLink;
    private float _tryDisconnect;
    private void DrawConnected(int windowId)
    {
        Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        if (GUILayout.Button(_tryDisconnect > 0 ? "Are You Sure?" : "Disconnect"))
        {
            if(_tryDisconnect > 0)
                ArchipelagoManager.Disconnect();
            else
                _tryDisconnect = 5f;
        }
        GUILayout.Label($"Connected to {ArchipelagoManager.Session.Socket.Uri.Host}:{ArchipelagoManager.Session.Socket.Uri.Port}");
        GUILayout.Label($"{ArchipelagoManager.Session.Locations.AllLocationsChecked.Count}/{ArchipelagoManager.Session.Locations.AllLocations.Count} locations checked");
        GUILayout.Label($"{Plugin.Data.CompletedLevels.Count}/{Plugin.Data.GoalRequirement} levels to unlock the goal.");
        ArchipelagoManager.DeathLinkEnabled = GUILayout.Toggle(ArchipelagoManager.DeathLinkEnabled, "Death Link");
        _sendMessage = GUILayout.TextArea(_sendMessage);
        if (GUILayout.Button("Send Message"))
        {
            ArchipelagoManager.Session.Say(_sendMessage);
            _sendMessage = string.Empty;
        }
        GUI.DragWindow();
    }

    private void DrawDisconnected(int windowId)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Host:",GUILayout.Width(100));
        Configuration.Instance.Host.Value = GUILayout.TextField(Configuration.Instance.Host.Value);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Slot Name:",GUILayout.Width(100));
        Configuration.Instance.SlotName.Value = GUILayout.TextField(Configuration.Instance.SlotName.Value);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Password:",GUILayout.Width(100));
        Configuration.Instance.Password.Value = GUILayout.TextField(Configuration.Instance.Password.Value);
        GUILayout.EndHorizontal();
        _deathLink = GUILayout.Toggle(_deathLink, "Death Link");

        if (GUILayout.Button("Connect"))
        {
            ArchipelagoManager.Connect(Configuration.Instance.Host.Value, Configuration.Instance.SlotName.Value,
                _deathLink,
                string.IsNullOrEmpty(Configuration.Instance.Password.Value)
                    ? null
                    : Configuration.Instance.Password.Value);
            Configuration.Instance.Host.ConfigFile.Save();
        }

        GUI.DragWindow();
    }

    private void DrawMessageLog()
    {
        var fullScreen = new Rect(0, 0, Screen.width, Screen.height);
        var messages = String.Empty;
        var current = _messages.First;
        while (current != null)
        {
            messages += current.Value + "\n";
            current = current.Next;
        }
        GUI.Label(fullScreen, messages, _messageStyle);
    }
    
    #if DEBUG
    private bool _showDebugWindow = false;
    private Rect _debugWindow = new Rect(170, 10, 1000, 1000);
    private readonly int _debugWindowId = Random.RandomRangeInt(0, int.MaxValue);

    private int _debugTab = 2;
    private readonly string[] _tabs = [ "Locations", "SlotData", "Abilities", "Levels"];
    private GUIStyle _debugStyle;

    private void DrawDebugWindow(int windowId)
    {
        _debugStyle ??= new GUIStyle(GUI.skin.label)
        {
            richText = true,
        };
        
        if (ArchipelagoManager.Session == null)
        {
            GUILayout.Label("Not connected to any session (location)");
            return;
        }
        GUILayout.BeginHorizontal();
        for (var i = 0; i < _tabs.Length; i++){
            if (GUILayout.Button(_tabs[i]))
            {
                _debugTab = i;
                Plugin.Logger.LogInfo("Switching tab "+ i);
            }
        }
        if(GUILayout.Button("Test Assert"))
            try
            {
                Plugin.Assert(false, "Test Assert");
            }
            catch
            {
                // ignored
            } 

        GUILayout.EndHorizontal();
        switch (_debugTab)
        {
            case 0:
                DebugDrawLocationTab();
                break;
            case 1:
                DebugDrawSlotData();
                break;
            case 2:
                DebugDrawAbilities();
                break;
            case 3: 
                DebugDrawLevels();
                break;
        }
        GUI.DragWindow();
    }

    private Vector2 _debugScroll = Vector2.zero;

    private void DebugDrawLocationTab()
    {
        _debugScroll = GUILayout.BeginScrollView(_debugScroll);
        Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        foreach (var locationID in ArchipelagoManager.Session.Locations.AllLocations)
        {
            var check = ArchipelagoManager.Session.Locations.AllLocationsChecked.Contains(locationID);
            var locationName = ArchipelagoManager.Session.Locations.GetLocationNameFromId(locationID);
            GUILayout.BeginHorizontal();
            _debugStyle.normal.textColor = check ? Color.green : Color.white;
            var newCheck = GUILayout.Toggle(check, locationName, _debugStyle);
            if (newCheck != check && newCheck)
            {
                if(locationName == ArchipelagoManager.SlotData["goal"].ToString())
                    ArchipelagoManager.Session.SetGoalAchieved();
                else
                    ArchipelagoManager.Session.Locations.CompleteLocationChecksAsync(null, locationID);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    private void DebugDrawSlotData()
    {
        _debugScroll = GUILayout.BeginScrollView(_debugScroll);
        foreach ( var slot in ArchipelagoManager.SlotData)
        {
            GUILayout.Label($"{slot.Key}: {slot.Value}");
        }
        GUILayout.EndScrollView();
    }

    private static readonly info.Abilities[] Abilities = Enum.GetValues(typeof(info.Abilities)).Cast<info.Abilities>().ToArray();
    private static readonly TextInfo EnUs = new CultureInfo("en-US", false).TextInfo;

    private void DebugDrawAbilities()
    {
        if (GUILayout.Button("+10000 points"))
        {
            pointsHandler.AddPoints(10000);
            Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
            ArchipelagoManager.Session.DataStorage[Scope.Slot,"points"] = pointsHandler.Points;
        }

        _debugScroll = GUILayout.BeginScrollView(_debugScroll);
        foreach (var ability in Abilities)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(EnUs.ToTitleCase(ability.ToString().Replace('_', ' ')),GUILayout.Width(150));
            GUI.changed = false;
            var check = GUILayout.Toggle(Plugin.Data.CheckedAbilities.Contains(ability),"Checked",GUILayout.Width(100));
            if (GUI.changed)
            {
                if(check)
                    Plugin.Data.CheckedAbilities.Add(ability);
                else
                    Plugin.Data.CheckedAbilities.Remove(ability);
            }
            GUI.changed = false;
            var owned = GUILayout.Toggle(Plugin.Data.UnlockedAbilities.Contains(ability),"Unlocked",GUILayout.Width(100));
            if (GUI.changed)
            {
                if(owned)
                    Plugin.Data.UnlockedAbilities.Add(ability);
                else
                    Plugin.Data.UnlockedAbilities.Remove(ability);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        
    }

    private void DebugDrawLevels()
    {
        _debugScroll = GUILayout.BeginScrollView(_debugScroll);
        for (var i = 0; i <= 89; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{i/10+1}-{i%10+1}",GUILayout.Width(150));
            var check = GUILayout.Toggle(Plugin.Data.CompletedLevels.Contains(i),"Checked",GUILayout.Width(100));
            if (GUI.changed)
            {
                if(check)
                    Plugin.Data.CompletedLevels.Add(i);
                else
                    Plugin.Data.CompletedLevels.Remove(i);
            }
            GUI.changed = false;
            var owned = GUILayout.Toggle(Plugin.Data.AvailableLevels.Contains(i),"Unlocked",GUILayout.Width(100));
            if (GUI.changed)
            {
                if(owned)
                    Plugin.Data.AvailableLevels.Add(i);
                else
                    Plugin.Data.AvailableLevels.Remove(i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }
    #endif
}