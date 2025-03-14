using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using JetBrains.Annotations;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace ArchipelagoClusterTruck;

public static class ArchipelagoManager
{
    private const string GameName = "ClusterTruck";
    private const string DefaultServer = "Archipelago.gg";
    private const int DefaultPort = 38281;
    private const ItemsHandlingFlags HandledItems = ItemsHandlingFlags.AllItems;
    private static readonly Version MinimumVersion = new(5, 1, 0);
    
    
    [CanBeNull] internal static ArchipelagoSession Session { get; private set; }
    internal static int? SlotNumber { get; private set; } = null;
    internal static int? TeamNumber { get; private set; } = null;
    internal static Dictionary<string, object> SlotData { get; private set; } = null;

    internal static event Action OnDisconnect;
    internal static event Action OnConnect;
    
    internal static DeathLinkService DeathLinkService;
    static bool _deathLinkEnabled = false;
    public static bool DeathLinkEnabled
    {
        get => _deathLinkEnabled;
        set
        {
            if (value == _deathLinkEnabled)
                return;
            _deathLinkEnabled = value;
            
            if(value)
                DeathLinkService.EnableDeathLink();
            else
                DeathLinkService.DisableDeathLink();
            
        }
    }

    internal static bool DeathLinkSendingDeath = false;
    static void OnDeathLink(DeathLink deathLink)
    {
        #if VERBOSE
        Plugin.Logger.LogInfo($"Detected death link: {deathLink.Source} {deathLink.Cause} {deathLink.Timestamp.ToLocalTime()}");
        #endif
        Plugin._uiManager.PushMessage($"{deathLink.Source} died by <color=red>{deathLink.Cause}</color>");
        if(!info.playing) return;
        GameManager gm = Object.FindObjectOfType<GameManager>();
        if(!gm) return;
        DeathLinkSendingDeath = true;
        gm.LoseLevel();
    }

    static void SetupSession()
    {
        Debug.Assert(Session != null, nameof(Session) + " != null");
        Session.Socket.SocketClosed += OnSocketClosed;
        Session.Socket.ErrorReceived += OnError;
#if VERBOSE
        Session.Socket.PacketsSent += OnSentPacket;
        Session.Socket.PacketReceived += OnReceivedPacket;
#endif
    }
    public static bool Connect(string host, string slotName, bool deathLink, string password = null)
    {

        LoginResult result = new LoginFailure("Nop");
        try
        {
            Exception e = null;
            if (!host.StartsWith("wss://"))
            {
                Session = ArchipelagoSessionFactory.CreateSession(host);
                SetupSession();

                try
                {
                    result = Session!.TryConnectAndLogin(GameName, slotName, HandledItems, MinimumVersion, [],
                        null, password, true);
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }

            if (e != null || Session == null)
            {
                int port = ProxyHelper.StartProxy(host);
                Session = ArchipelagoSessionFactory.CreateSession($"ws://127.0.0.1:{port}");
                SetupSession();
                result = Session!.TryConnectAndLogin(GameName, slotName, HandledItems, MinimumVersion, [],
                    null, password, true);
            }

            DeathLinkService = Session.CreateDeathLinkService();
            DeathLinkService.OnDeathLinkReceived += OnDeathLink;
            _deathLinkEnabled = deathLink;
            if (deathLink)
                DeathLinkService.EnableDeathLink();
            
        }
        catch (Exception e)
        {
            #if VERBOSE
            Plugin.Logger.LogError($"[VERBOSE] Failed to connect to {host}: {e.Message}\n{e.StackTrace}");
            #endif
            Session = null;
            result = new LoginFailure(e.GetBaseException().Message);
        }

        if (!result.Successful)
        {
            LoginFailure failure = (LoginFailure)result;
            string errorMessage = $"Failed to connect to {host} as {slotName}:\n" + failure.Errors.Join(delimiter: "\n\t") +failure.ErrorCodes.Join(delimiter: "\n\t");
            Plugin.Logger.LogError(errorMessage);
            return false;
        }
        
        LoginSuccessful loginSuccessful = (LoginSuccessful)result;
        SlotNumber = loginSuccessful.Slot;
        TeamNumber = loginSuccessful.Team;
        SlotData = loginSuccessful.SlotData;
        Plugin.Logger.LogInfo($"Successfully connected to {host}\nGot slot number: {SlotNumber}\nTeam number: {TeamNumber}");
        #if VERBOSE
        foreach (KeyValuePair<string, object> kvp in loginSuccessful.SlotData)
        {
            try
            {
                Plugin.Logger.LogInfo($"{kvp.Key}: {kvp.Value}");
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }
        #endif
        
        OnConnect?.Invoke();
        return true;
    }

    #if VERBOSE
    static void OnSentPacket(ArchipelagoPacketBase[] packets)
    {
        string joined = string.Join(", ", packets.Select(x=>$"{{{x}}}").ToArray());
        Plugin.Logger.LogInfo($"Sent packet: {joined}");
    }

    static void OnReceivedPacket(ArchipelagoPacketBase packet)
    {
        Plugin.Logger.LogInfo($"Received packet: {packet}");
        
    }
    #endif
    static void OnError(Exception exception, string errorMessage)
    {
        Plugin.Logger.LogError($"{errorMessage}\n{exception}");
    }
    static void OnSocketClosed(string reason)
    {
        Plugin.Logger.LogWarning(new StackTrace());
        Plugin.Logger.LogWarning($"Socket closed: {reason}");
        Disconnect();
    }
    public static bool Disconnect()
    {
        SlotNumber = null;
        TeamNumber = null;
        SlotData = null;
        Session?.Socket?.Disconnect();
        Session = null;
        OnDisconnect?.Invoke();
        if(ProxyHelper.ProxyRunning != null && !ProxyHelper.ProxyRunning.HasExited)
            ProxyHelper.ProxyRunning.Kill();
        return true;
    }
    public static void Check(int level,bool ace = false)
    {
        Debug.Assert(Session != null, nameof(Session) + " != null");
        Session.Locations.CompleteLocationChecksAsync((_) => { },Plugin.Data.BaseID + level + (ace ? 104 : 0));
    }

    public static void Check(info.Abilities ability)
    {
        Debug.Assert(Session != null, nameof(Session) + " != null");
        Session.Locations.CompleteLocationChecksAsync((_) => { },Plugin.Data.BaseID + 90 + (int)ability);
    }

}