using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using ArchipelagoClusterTruck.Patches;
using UnityEngine;
using Color = UnityEngine.Color;
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
        var id = (int)(item.ItemId - BaseID);
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

                var p = Object.FindObjectOfType<player>();
                if (!p) break;
                
                p.rig.AddForce(Random.insideUnitSphere * 100);
                break;
            case "Brake":
                if(!info.playing) break;    
                
                var cars = Object.FindObjectsOfType<car>();
                foreach(var car in cars)
                    car.waitTime = Random.Range(10f, 20f);
                
                break;
        }
    }

    private void OnConnect()
    {
        Plugin.Assert(ArchipelagoManager.Session != null, "ArchipelagoManager.Session != null");
        
        Plugin.Assert(ArchipelagoManager.SlotData.ContainsKey("base_id"),"ArchipelagoManager.SlotData.ContainsKey('base_id')");
        BaseID = Convert.ToInt64(ArchipelagoManager.SlotData["base_id"]);
        Plugin.Assert(ArchipelagoManager.SlotData.ContainsKey("goal_requirement"),"ArchipelagoManager.SlotData.ContainsKey('goal_requirement')");
        GoalRequirement = Convert.ToInt32(ArchipelagoManager.SlotData["goal_requirement"]);
        Plugin.Assert(ArchipelagoManager.SlotData.ContainsKey("point_multiplier"),"ArchipelagoManager.SlotData.ContainsKey('point_multiplier')");
        PointMultiplier = Convert.ToInt32(ArchipelagoManager.SlotData["point_multiplier"]);
        Plugin.Assert(ArchipelagoManager.SlotData.ContainsKey("deathlink_amnesty"),"ArchipelagoManager.SlotData.ContainsKey('deathlink_amnesty')");
        DeathsForDeathlink = Convert.ToInt32(ArchipelagoManager.SlotData["deathlink_amnesty"]);
        
        ArchipelagoManager.Session.Items.ItemReceived += (item) => ReceiveItem(item.DequeueItem());
        while(ArchipelagoManager.Session.Items.DequeueItem() is { } item)
        {
            ReceiveItem(item);
        }
        // In case we somehow missed anything
        foreach (var item in ArchipelagoManager.Session.Items.AllItemsReceived)
            ReceiveItem(item);
        
        var startString = ArchipelagoManager.SlotData["start"].ToString();
        var start = 0;
        try
        {
            var split = startString.Split('-');
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

        var goalString = ArchipelagoManager.SlotData["goal"].ToString();
        Goal = 0;
        try
        {
            var split = goalString.Split('-');
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
                    // ensure someone on restart doesn't get soft-locked
                    if (CompletedLevels.Count >= GoalRequirement)
                        AvailableLevels.Add(Goal);
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
        
        
        ArchipelagoManager.Session.Locations.ScoutLocationsAsync((itemInfo) =>
        {
            Dictionary<info.Abilities, int> abilityAssertion = new();
            foreach (var abilityObj in Enum.GetValues(typeof(info.Abilities)))
            {
                Plugin.Assert(abilityObj is info.Abilities, "abilityObj is info.Abilities");
                var ability = (info.Abilities)(int)abilityObj;
                abilityAssertion.Add(ability,0);
            }
            foreach (var kvp in itemInfo)
            {
                var abilityId = (int)(kvp.Key - BaseID-90);
                var ability = (info.Abilities)abilityId;
                Plugin.Assert(Enum.IsDefined(typeof(info.Abilities),ability), "");
                abilityAssertion[ability]++;
                var color = kvp.Value.Flags switch
                {
                    ItemFlags.Advancement => Configuration.Instance.ProgressionColor.Value,
                    ItemFlags.Trap  => Configuration.Instance.TrapColor.Value,
                    ItemFlags.NeverExclude => Configuration.Instance.UsefulColor.Value,
                    _ => Configuration.Instance.NormalColor.Value,
                };
                var colorString =
                    $"#{Mathf.RoundToInt(color.r * 255):X2}{Mathf.RoundToInt(color.g * 255):X2}{Mathf.RoundToInt(color.b * 255):X2}";
                abilityHintsTitle.Add(ability,$"{kvp.Value.Player}");
                abilityHintsDescription.Add(ability,$"<color={colorString}> {kvp.Value.ItemDisplayName}</color>\nWas {ability}");
            }

            foreach (var kvp in abilityAssertion)
                Plugin.Assert(kvp.Value == 1, $"{kvp.Key} was not seen");
        },HintCreationPolicy.CreateAndAnnounceOnce,Enumerable.Range(90,103-89).Select(x=>BaseID+x).ToArray());
    }

    private void OnDisconnect()
    {
        CompletedLevels.Clear();
        AvailableLevels.Clear();
        UnlockedAbilities.Clear();
        CheckedAbilities.Clear();
        abilityHintsTitle.Clear();
        abilityHintsDescription.Clear();
    }

    public long BaseID;

    public int PointMultiplier;
    public int DeathsForDeathlink;
    
    public int Goal = 0;
    public int GoalRequirement;
    
    public readonly HashSet<int> CompletedLevels = [];
    public readonly HashSet<int> AvailableLevels = [];
    
    public readonly HashSet<info.Abilities> UnlockedAbilities = [];
    public readonly HashSet<info.Abilities> CheckedAbilities = [];

    public readonly Dictionary<info.Abilities, string> abilityHintsTitle = new ();
    public readonly Dictionary<info.Abilities, string> abilityHintsDescription = new ();


}