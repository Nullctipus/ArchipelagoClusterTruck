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
        Host = config.Bind(ArchipelagoHeader, "Host", "wss://Archipelago.gg:");
        SlotName = config.Bind(ArchipelagoHeader, "SlotName", string.Empty);
        Password = config.Bind(ArchipelagoHeader, "Password", string.Empty);
        
        //https://rosepinetheme.com/
        OwnedCheckedColor = config.Bind(AbilityHeader, "OwnedChecked", new Color(224/255f, 222/255f, 244/255f),"Color for owned and checked abilities");
        OwnedUncheckedColor = config.Bind(AbilityHeader, "OwnedUnchecked", new Color(235/255f, 188/255f, 186/255f),"Color for owned and unchecked abilities");
        UnownedCheckedColor = config.Bind(AbilityHeader, "UnownedChecked", new Color(156/255f, 207/255f, 216/255f),"Color for unowned and checked abilities");
        UnownedUncheckedColor = config.Bind(AbilityHeader, "UnownedUnchecked", new Color(110/255f,106/255f, 134/255f),"Color for unowned and unchecked abilities");
        ProgressionColor = config.Bind(AbilityHeader, "ProgressionColor", new Color(156/255f, 207/255f, 216/255f),"Color for progression Items in ability menu");
        UsefulColor = config.Bind(AbilityHeader, "UsefulColor", new Color(196/255f, 167/255f, 231/255f),"Color for useful Items in ability menu");
        TrapColor = config.Bind(AbilityHeader, "TrapColor", new Color(235/255f, 111/255f, 146/255f),"Color for trap Items in ability menu");
        NormalColor = config.Bind(AbilityHeader, "NormalColor", new Color(224/255f, 222/255f, 244/255f),"Color for normal Items in ability menu");
        
        RandomTruckColor = config.Bind(FunHeader,"RandomTruckColor",false);
        
        ToggleMessageLogKey = config.Bind(KeysHeader, "ToggleMessageLogKey", KeyCode.F3);
        ToggleConnectionWindowKey = config.Bind(KeysHeader, "ToggleConnectionWindowKey", KeyCode.F4);
        #if DEBUG
        ToggleDebugKey = config.Bind(DebugHeader, "ToggleDebugWindowKey", KeyCode.F6);
        ToggleNoclipKey = config.Bind(DebugHeader, "ToggleNoclipKey", KeyCode.V);
        NoclipSpeed = config.Bind(DebugHeader,"NoclipSpeed",10f); 
        #endif
    }

    private const string ArchipelagoHeader = "Archipelago"; 
    public readonly ConfigEntry<TextAnchor> MessageLogOrigin;
    public readonly ConfigEntry<int> MessageLogCount;
    public readonly ConfigEntry<string> Host,
        SlotName,
        Password;

    private const string AbilityHeader = "Abilities";
    public readonly ConfigEntry<Color> OwnedCheckedColor,
        OwnedUncheckedColor,
        UnownedCheckedColor,
        UnownedUncheckedColor,
        ProgressionColor,
        UsefulColor,
        TrapColor,
        NormalColor;
    
    public const string FunHeader = "Fun";
    public readonly ConfigEntry<bool> RandomTruckColor;
    
    private const string KeysHeader = "Keys";
    public readonly ConfigEntry<KeyCode> ToggleMessageLogKey,
        ToggleConnectionWindowKey;
    #if DEBUG
    private const string DebugHeader = "Debug";
    public readonly ConfigEntry<KeyCode> ToggleDebugKey;
    public readonly ConfigEntry<KeyCode> ToggleNoclipKey;
    public readonly ConfigEntry<float> NoclipSpeed;
#endif
}