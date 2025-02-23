using System;
using System.Collections.Generic;
using System.Diagnostics;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace ArchipelagoClusterTruck;

public class SaveData
{
    public SaveData()
    {
        ArchipelagoManager.OnConnect += OnConnect;
        ArchipelagoManager.OnDisconnect += OnDisconnect;
    }

    void ReceiveItem(ItemInfo item)
    {
        //TODO:
    }
    void OnConnect()
    {
        Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        ArchipelagoManager.Session.Items.ItemReceived += (item) => ReceiveItem(item.DequeueItem());
        
        // In case we somehow missed anything
        foreach(var item in ArchipelagoManager.Session.Items.AllItemsReceived)
            ReceiveItem(item);

        string startString = ArchipelagoManager.SlotData["start"].ToString();
        int start = 0;
        try
        {
            string[] split = startString.Split('-');
            start = (int.Parse(split[0])-1)*10 + int.Parse(split[1])-1;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Start level is not recognized: {ex}");
        }

        if (start > 100)
        {
            start = 0;
            Plugin.Logger.LogError("Start level is out of range resetting to 0");
        }
        AvailableLevels.Add(start);
        
        string goalString = ArchipelagoManager.SlotData["goal"].ToString();
        Goal = 0;
        try
        {
            string[] split = goalString.Split('-');
            Goal = (int.Parse(split[0])-1)*10 + int.Parse(split[1])-1;    
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Start level is not recognized: {ex}");
        }

        if (Goal > 89)
        {
            Goal = 89;
            Plugin.Logger.LogError("Start level is out of range resetting to 89");
        }
    }

    void OnDisconnect()
    {
        CompletedLevels.Clear();
        AvailableLevels.Clear();
    }

    public int Goal = 0;
    
    public HashSet<int> CompletedLevels = [];
    public HashSet<int> AvailableLevels = [];
    
    public HashSet<info.Abilities> UnlockedAbilities = [];
    public HashSet<info.Abilities> CheckedAbilities = [];
}