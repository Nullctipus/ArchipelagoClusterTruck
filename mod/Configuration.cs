using System;
using System.Data;
using BepInEx.Configuration;
using UnityEngine;

namespace ArchipelagoClusterTruck;

public class Configuration
{
    static Configuration _instance;
    public static Configuration Instance => _instance ?? throw new NoNullAllowedException();

    internal Configuration(ConfigFile config)
    {
        _instance = this;
        MessageLogOrigin = config.Bind(ArchipelagoHeader, "MessageLogOrigin", TextAnchor.LowerRight);
        MessageLogCount = config.Bind(ArchipelagoHeader, "MessageLogCount", 5);
        Host = config.Bind(ArchipelagoHeader, "Host", "Archipelago.gg:");
        SlotName = config.Bind(ArchipelagoHeader, "SlotName", string.Empty);
        Password = config.Bind(ArchipelagoHeader, "Password", string.Empty);
        
        ToggleMessageLogKey = config.Bind(KeysHeader, "ToggleMessageLogKey", KeyCode.F3);
        ToggleConnectionWindowKey = config.Bind(KeysHeader, "ToggleConnectionWindowKey", KeyCode.F4);
        #if DEBUG
        ToggleDebugKey = config.Bind(KeysHeader, "ToggleDebugWindowKey", KeyCode.F6);
        #endif
    }
    const string ArchipelagoHeader = "Archipelago"; 
    public readonly ConfigEntry<TextAnchor> MessageLogOrigin;
    public readonly ConfigEntry<int> MessageLogCount;
    public readonly ConfigEntry<string> Host, SlotName, Password;
    
    const string KeysHeader = "Keys";
    public readonly ConfigEntry<KeyCode> ToggleMessageLogKey;
    public readonly ConfigEntry<KeyCode> ToggleConnectionWindowKey;
    #if DEBUG
    public readonly ConfigEntry<KeyCode> ToggleDebugKey;
    #endif
}