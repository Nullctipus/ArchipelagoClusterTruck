using System;
using HarmonyLib;
using Steamworks;

namespace ArchipelagoClusterTruck.Patches;

// ReSharper disable InconsistentNaming
public class steam_WorkshopHandlerPatches : ClassPatch
{
    // disable leaderboard uploading
    static bool onLeaderboardScoresDownloadedUploadScoresLaterPrefix(LeaderboardScoresDownloaded_t _Callresult,
        bool biofail)
    {
        return false;
    }
    public override Exception Patch(Harmony harmony)
    {
        return MakePatch(harmony, typeof(steam_WorkshopHandler),
            nameof(steam_WorkshopHandler.onLeaderboardScoresDownloadedUploadScoresLater),
            nameof(onLeaderboardScoresDownloadedUploadScoresLaterPrefix));
    }
}