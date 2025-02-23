namespace ArchipelagoClusterTruck;

public static class ColorUtility
{
    public static string CreateRichColor(string text, bool isBackground, Archipelago.MultiClient.Net.Models.Color color)
        => $"<{(isBackground ? "mark" : "color")}=#{color.R:X2}{color.G:X2}{color.B:X2}>{text}</color>";
}