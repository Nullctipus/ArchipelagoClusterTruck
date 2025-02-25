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

    static bool UploadLeaderboardScorePrefix(SteamLeaderboard_t hSteamLeaderboard, ELeaderboardUploadScoreMethod eLeaderboardUploadScoreMethod, int nScore, int[] pScoreDetails, int cScoreDetailsCount)
    {
        return false;
    }
    public override Exception Patch(Harmony harmony)
    {
        var e1 = MakePatch(harmony, typeof(steam_WorkshopHandler),
            nameof(steam_WorkshopHandler.onLeaderboardScoresDownloadedUploadScoresLater),
            nameof(onLeaderboardScoresDownloadedUploadScoresLaterPrefix));
        var e2 = MakePatch(harmony, typeof(SteamUserStats), nameof(SteamUserStats.UploadLeaderboardScore),
            nameof(UploadLeaderboardScorePrefix));
        return e1 ?? e2;
    }
}