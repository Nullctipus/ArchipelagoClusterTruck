using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
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
        
        var port = FreeTcpPort();
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
    }

    
    // https://stackoverflow.com/a/150974
    private static int FreeTcpPort()
    {
      var l = new TcpListener(IPAddress.Loopback, 0);
      l.Start();
      var port = ((IPEndPoint)l.LocalEndpoint).Port;
      l.Stop();
      return port;
    }

    private static void EnsureProxyExecutable()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath);
        var assembly = Assembly.GetExecutingAssembly();
        var resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(ExeName));
        if (File.Exists(ExePath))
        {
            string target;
            using (var md5 = MD5.Create())
            {
                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    target = Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }

            string file;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(ExePath))
                {
                    file = Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }

            if (file == target)
            {
                return;
            }
        }

        using var inStream = assembly.GetManifestResourceStream(resourcePath);
        using var outStream = File.OpenWrite(ExePath);
        const int bufferSize = 64 * 1024; // 64 kb
        var buffer = new byte[bufferSize];
        var remaining = inStream.Length;
        while (remaining > 0)
        {
            var bytesToRead = (int)Math.Min(buffer.Length, remaining);
            var bytesRead = inStream.Read(buffer, 0, bytesToRead);
            outStream.Write(buffer, 0, bytesRead);
            remaining -= bytesRead;
        }

        if (Environment.OSVersion.Platform != PlatformID.Unix) return;


        var chmodProcessInfo = new ProcessStartInfo("chmod", $"u+x {ExePath}")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
        };
        var chmodProcess = Process.Start(chmodProcessInfo);
        var error = chmodProcess.StandardError.ReadToEnd();
        chmodProcess!.WaitForExit();
        if (!string.IsNullOrEmpty(error))
        {
            Plugin.Logger.LogError($"[PROXY] chmod error: {error}");
        }
    }
}