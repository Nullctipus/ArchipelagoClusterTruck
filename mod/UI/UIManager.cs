using System;
using System.Collections.Generic;
using System.Linq;
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
    
    void OnArchipelagoConnect()
    {
        Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        ArchipelagoManager.Session.MessageLog.OnMessageReceived += (message) =>
        {
            string text = string.Join("", message.Parts.Select(part => ColorUtility.CreateRichColor(part.Text, part.IsBackgroundColor,part.Color)).ToArray());

            PushMessage(text);
        };
        
    }
    private bool _showMessageLog = true;
    
    private Rect _connectionWindow = new Rect(10, 10, 320, 500);
    private readonly int _connectionWindowId = Random.RandomRangeInt(0, int.MaxValue);
    private bool _showConnectionWindow = true;
    private void OnGUI()
    {
        _messageStyle ??= new GUIStyle(GUI.skin.label)
        {
            richText = true,
            alignment = Configuration.Instance.MessageLogOrigin.Value
        };
        
        if(Input.GetKeyDown(Configuration.Instance.ToggleMessageLogKey.Value))
            _showMessageLog = !_showMessageLog;
        if(_showMessageLog)
            DrawMessageLog();
        
        if(Input.GetKeyDown(Configuration.Instance.ToggleConnectionWindowKey.Value))
            _showConnectionWindow = !_showConnectionWindow;
        if (_showConnectionWindow)
            _connectionWindow = GUI.Window(_connectionWindowId, _connectionWindow, ArchipelagoManager.Session != null ? DrawConnected : DrawDisconnected,"Connection");
        
        #if DEBUG
        if (Input.GetKeyDown(Configuration.Instance.ToggleDebugKey.Value))
            _showDebugWindow = !_showDebugWindow;
        if(_showDebugWindow)
            _debugWindow = GUI.Window(_debugWindowId,_debugWindow,DrawDebugWindow, "Debug");
        #endif
    }

    private string _sendMessage = string.Empty;
    private bool _deathLink;
    private void DrawConnected(int windowId)
    {
        Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        GUILayout.Label($"Connected to {ArchipelagoManager.Session.Socket.Uri.Host}:{ArchipelagoManager.Session.Socket.Uri.Port}");
        GUILayout.Label($"{ArchipelagoManager.Session.Locations.AllLocationsChecked.Count}/{ArchipelagoManager.Session.Locations.AllLocations} locations checked");
        ArchipelagoManager.DeathLinkEnabled = GUILayout.Toggle(ArchipelagoManager.DeathLinkEnabled, "Death Link");
        _sendMessage = GUILayout.TextArea(_sendMessage);
        if (GUILayout.Button("Send Message"))
        {
            ArchipelagoManager.Session.Say(_sendMessage);
            _sendMessage = string.Empty;
        }
        GUI.DragWindow(_connectionWindow);
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

        GUI.DragWindow(_connectionWindow);
    }

    private void DrawMessageLog()
    {
        Rect fullScreen = new Rect(0, 0, Screen.width, Screen.height);
        string messages = String.Empty;
        LinkedListNode<string> current = _messages.First;
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

    private int _debugTab = 0;
    private readonly string[] _tabs = [ "Locations", "SlotData" ];
    private GUIStyle _debugStyle;

    private void DrawDebugWindow(int windowId)
    {
        GUI.DragWindow(_debugWindow);
        _debugStyle ??= new GUIStyle(GUI.skin.label)
        {
            richText = true,
        };
        
        GUILayout.BeginHorizontal();
        for (int i = 0; i < _tabs.Length; ++i)
            if(GUILayout.Button(_tabs[i]))
                _debugTab = i;
        GUILayout.EndHorizontal();
        switch (_debugTab)
        {
            case 0:
                DebugDrawLocationTab();
                break;
            case 1:
                DebugDrawSlotData();
                break;
        }
    }
    Vector2 _debugScroll = Vector2.zero;
    void DebugDrawLocationTab()
    {
        if (ArchipelagoManager.Session == null)
        {
            GUILayout.Label("Not connected to any session");
            return;
        }
        _debugScroll = GUILayout.BeginScrollView(_debugScroll);
        foreach (var locationID in ArchipelagoManager.Session.Locations.AllLocations)
        {
            bool check = ArchipelagoManager.Session.Locations.AllLocationsChecked.Contains(locationID);
            string locationName = ArchipelagoManager.Session.Locations.GetLocationNameFromId(locationID);
            GUILayout.BeginHorizontal();
            _debugStyle.normal.textColor = check ? Color.green : Color.white;
            bool newCheck = GUILayout.Toggle(check, locationName, _debugStyle);
            if (newCheck != check && newCheck)
            {
                ArchipelagoManager.Session.Locations.CompleteLocationChecksAsync(null, locationID);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    void DebugDrawSlotData()
    {
        if (ArchipelagoManager.Session == null)
        {
            GUILayout.Label("Not connected to any session");
            return;
        }
        _debugScroll = GUILayout.BeginScrollView(_debugScroll);
        foreach ( var slot in ArchipelagoManager.SlotData)
        {
            GUILayout.Label($"{slot.Key}: {slot.Value}");
        }
        GUILayout.EndScrollView();
    }
    #endif
}