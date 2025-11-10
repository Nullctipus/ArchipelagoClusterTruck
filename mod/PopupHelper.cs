using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ArchipelagoClusterTruck;

public static class PopupHelper
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "ShellMessageBoxW")]
    private static extern int ShellMessageBoxW(
        IntPtr hAppInst,
        IntPtr hWnd,
        string lpcText,
        string lpcTitle,
        uint fuStyle,
        IntPtr[] args
    );

    private static bool ShellMessageBoxWExists = true;

    public static void MessageBox(string message, string title)
    {
        if (!ShellMessageBoxWExists) LinuxMessageBox(message, title);

        try
        {
            ShellMessageBoxW(
                IntPtr.Zero,
                IntPtr.Zero,
                message,
                title,
                0,
                null
            );
            return;
        }
        catch (DllNotFoundException)
        {
            ShellMessageBoxWExists = false;
            LinuxMessageBox(message, title);
        }
        catch (EntryPointNotFoundException)
        {
            ShellMessageBoxWExists = false;
            LinuxMessageBox(message, title);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"MessageBox Failed for unexpected reason: {ex.Message}");
        }
    }

    private static void LinuxMessageBox(string message, string title)
    {
        var zenityStartInfo = new ProcessStartInfo("zenity")
        {
            Arguments = $"--error " +
                        $"--text=\"{message}\" " +
                        $"--title=\"{title}\" ",
            UseShellExecute = false,
            RedirectStandardError = true,
        };
        var zenity = Process.Start(zenityStartInfo);
        zenity.WaitForExit();
        if (zenity.ExitCode != 127) return;
        var notifyStartInfo = new ProcessStartInfo("notify-send")
        {
            Arguments = $"{title}: {message}"
        };
        // if it doesn't work I can't imagine the user's system
        Process.Start(notifyStartInfo);
    }
}