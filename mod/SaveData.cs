using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using ArchipelagoClusterTruck.Patches;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ArchipelagoClusterTruck;

public class SaveData
{
    public SaveData()
    {
        ArchipelagoManager.OnConnect += OnConnect;
        ArchipelagoManager.OnDisconnect += OnDisconnect;
    }


    private void ReceiveItem(ItemInfo item)
    {
        int id = (int)(item.ItemId - BaseID);
        Plugin.Logger.LogInfo($"Received Item: {item.ItemName} ({id})");
        switch (id)
        {
            case < 90:
                AvailableLevels.Add(id);
                return;
            case <= 103:
                UnlockedAbilities.Add((info.Abilities)(id - 90));
                return;
            case <= 107:
                Plugin.Logger.LogInfo("Filler");
                break;
        }

        switch (item.ItemName)
        {
            //Filler
            case "+1000p":
                pointsHandler.AddPoints(1000);
                break;
            case "Infinite Stamina":
                abilityManagerPatches.InfiniteStaminaTime = Random.Range(10f, 20f);
                break;
            //Traps
            case "Impulse":
                if (!info.playing) break;

                player p = Object.FindObjectOfType<player>();
                if (!p) break;
                
                p.rig.AddForce(Random.insideUnitSphere * 100);
                break;
            case "Brake":
                if(!info.playing) break;    
                
                car[] cars = Object.FindObjectsOfType<car>();
                foreach(var car in cars)
                    car.waitTime = Random.Range(10f, 20f);
                
                break;
        }
    }

    void OnConnect()
    {
        Debug.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        
        BaseID = Convert.ToInt64(ArchipelagoManager.SlotData["base_id"]);
        GoalRequirement = Convert.ToInt32(ArchipelagoManager.SlotData["goal_requirement"]);
        
        ArchipelagoManager.Session.Items.ItemReceived += (item) => ReceiveItem(item.DequeueItem());
        while(ArchipelagoManager.Session.Items.DequeueItem() is { } item)
        {
            ReceiveItem(item);
        }
        // In case we somehow missed anything
        foreach (var item in ArchipelagoManager.Session.Items.AllItemsReceived)
            ReceiveItem(item);
        
        string startString = ArchipelagoManager.SlotData["start"].ToString();
        int start = 0;
        try
        {
            string[] split = startString.Split('-');
            start = (int.Parse(split[0]) - 1) * 10 + int.Parse(split[1]) - 1;
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
            Goal = (int.Parse(split[0]) - 1) * 10 + int.Parse(split[1]) - 1;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Goal level is not recognized: {ex}");
        }

        if (Goal > 89)
        {
            Goal = 89;
            Plugin.Logger.LogError("Goal level is out of range resetting to 89");
        }
        
        info.abilityName = string.Empty;
        PlayerPrefs.SetString(info.ABILITY_MOVEMENT_KEY, string.Empty);
        info.utilityName = string.Empty;
        PlayerPrefs.SetString(info.ABILITY_UTILITY_KEY, string.Empty);

        
        foreach (var location in ArchipelagoManager.Session.Locations.AllLocationsChecked)
        {
            var id = location - BaseID;
            switch (id)
            {
                case < 90:
                    CompletedLevels.Add((int)id);
                    break;
                case <= 103:
                {
                    var ability = (info.Abilities)(id - 90);
                    CheckedAbilities.Add(ability);
                    break;
                }
                case <= 107:
                    break;  //Filler
                default:
                    Plugin.Logger.LogError($"Unknown location: {id} - {ArchipelagoManager.Session.Locations.GetLocationNameFromId(id)}");
                    break;
            }
        }
        ArchipelagoManager.Session.DataStorage[Scope.Slot, "points"].Initialize(0);
        var points = ArchipelagoManager.Session.DataStorage[Scope.Slot, "points"].To<int>();
        
        pointsHandler.AddPoints(points - pointsHandler.Points);
    }

    void OnDisconnect()
    {
        CompletedLevels.Clear();
        AvailableLevels.Clear();
        UnlockedAbilities.Clear();
        CheckedAbilities.Clear();
    }

    public long BaseID;
    
    public int Goal = 0;
    public int GoalRequirement;
    
    public readonly HashSet<int> CompletedLevels = [];
    public readonly HashSet<int> AvailableLevels = [];
    
    public readonly HashSet<info.Abilities> UnlockedAbilities = [];
    public readonly HashSet<info.Abilities> CheckedAbilities = [];
    
    
}