using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Mono.Cecil;
using UnityEngine;

namespace ArchipelagoClusterTruck;

public static class ProxyHelper
{
    private const string BaseName = "ArchipelagoProxy";
    private static string ExeName => BaseName + (Environment.OSVersion.Platform == PlatformID.Unix ? "" : ".exe");
    private static string ExePath => Path.Combine(Application.streamingAssetsPath, ExeName);
    public static Process ProxyRunning;

    public static int StartProxy(string host)
    {
        EnsureProxyExecutable();
        
        int port = FreeTcpPort();
        Plugin.Logger.LogInfo($"Starting proxy on 127.0.0.1:{port} for {host}");


        ProcessStartInfo startInfo = new(ExePath)
        {
            Arguments = $"{port} {host}",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        var process = ProxyRunning = new Process() { StartInfo = startInfo, EnableRaisingEvents = true };
        
        ManualResetEvent mre = new(false);
        
        void KillProcess(object sender, EventArgs e)
        {
            if(!process.HasExited)
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Failed to kill process '{process.StartInfo.FileName}': {ex}");
                }
            AppDomain.CurrentDomain.ProcessExit -= KillProcess;
        }
        
        process.OutputDataReceived += (_, args) =>
        {
            if(string.IsNullOrEmpty(args.Data))
                return;
            if(args.Data.StartsWith("Listening"))
                mre.Set();
            Plugin.Logger.LogInfo($"[PROXY] {args.Data}");
        };process.ErrorDataReceived += (_, args) =>
        {
            if(string.IsNullOrEmpty(args.Data))
                return;
            Plugin.Logger.LogError($"[PROXY] {args.Data}");
        };
        process.Exited += (_, args) =>
        {
            Plugin.Logger.LogInfo($"[PROXY] Closed");
            process.Close();
            AppDomain.CurrentDomain.ProcessExit -= KillProcess;
        };
        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!mre.WaitOne(5000))
            {
                Plugin.Logger.LogError($"[PROXY] No output received within timeout!");
            }
            //WaitForPortToOpen(port,TimeSpan.FromMinutes(1));
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"[PROXY] {ex.Message}");
        }

        AppDomain.CurrentDomain.ProcessExit += KillProcess;
        
        return port;
    }

    
    // https://stackoverflow.com/a/150974
    static int FreeTcpPort()
    {
      TcpListener l = new TcpListener(IPAddress.Loopback, 0);
      l.Start();
      int port = ((IPEndPoint)l.LocalEndpoint).Port;
      l.Stop();
      return port;
    }
    
    static void EnsureProxyExecutable()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath);
        if (File.Exists(ExePath))
            return;
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(ExeName));
        using var inStream = assembly.GetManifestResourceStream(resourcePath);
        using var outStream = File.OpenWrite(ExePath);
        const int bufferSize = 64 * 1024; // 64 kb
        var buffer = new byte[bufferSize];
        long remaining = inStream.Length;
        while (remaining > 0)
        {
            int bytesToRead = (int)Math.Min(buffer.Length, remaining);
            int bytesRead = inStream.Read(buffer, 0, bytesToRead);
            outStream.Write(buffer, 0, bytesRead);
            remaining -= bytesRead;
        }

        if (Environment.OSVersion.Platform != PlatformID.Unix) return;
        
        
        ProcessStartInfo chmodProcessInfo = new ProcessStartInfo("chmod", $"u+x {ExePath}")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
        };
        var chmodProcess = Process.Start(chmodProcessInfo);
        string error = chmodProcess.StandardError.ReadToEnd();
        chmodProcess!.WaitForExit();
        if (!string.IsNullOrEmpty(error))
        {
            Plugin.Logger.LogError($"[PROXY] chmod error: {error}");
        }
    }
    

    static bool WaitForPortToOpen(int port, TimeSpan timeout)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        using var client = new TcpClient();
        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                client.Connect("127.0.0.1", port);
                return true;
            }catch{}
            Thread.Sleep(100);
        }
        return false;
    }
}