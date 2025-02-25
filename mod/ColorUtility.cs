using UnityEngine;

namespace ArchipelagoClusterTruck;

public static class ColorUtility
{
    public static string CreateRichColor(string text, bool isBackground, Archipelago.MultiClient.Net.Models.Color color)
        => $"<{(isBackground ? "mark" : "color")}=#{color.R:X2}{color.G:X2}{color.B:X2}>{text}</color>";

    public static Color RandomColor()
    {
        return Color.HSVToRGB(Random.value, 1, 0.5f);
    }
}