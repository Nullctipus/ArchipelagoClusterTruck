using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Net.WebSockets;
using System.Text;

public class Program
{
    private static readonly HttpListener Listener = new HttpListener();
    private static readonly ClientWebSocket Ws = new ClientWebSocket();
    private static WebSocket? _client;
    public static void Main(string[] args)
    {
        
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: localport uri");
        }

        int port = int.Parse(args[0]);
        string host = args[1];

        Ws.Options.DangerousDeflateOptions = new WebSocketDeflateOptions()
        {
            ClientMaxWindowBits = 15,
            ServerMaxWindowBits = 15,
            ClientContextTakeover = true,
            ServerContextTakeover = true,
        };
        Task.WaitAll([ServerLoop(host), OnConnectionClient(port)]);
    }

    private static async Task ServerLoop(string host)
    {
        if(!host.StartsWith("wss://") && !host.StartsWith("ws://"))
            host = "wss://" + host;
        var uri = new Uri(host);
        while (_client is not { State: WebSocketState.Open })
            await Task.Delay(100);
        Console.WriteLine($"Connecting to {uri}");
        await Ws.ConnectAsync(uri,CancellationToken.None);
        var buffer = WebSocket.CreateServerBuffer(1024*1024*4); //4MB
        Console.WriteLine("Connected to server");
        while (Ws.State == WebSocketState.Open)
        {
            var result = await Ws.ReceiveAsync(buffer,CancellationToken.None);
            #if DEBUG
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    Console.WriteLine($"From Server: {Encoding.UTF8.GetString(buffer.Slice(0,result.Count))}");
                    break;
                case WebSocketMessageType.Binary:
                    Console.WriteLine($"From Server: {result.Count} Bytes");
                    break;
            }
            #endif
            await _client.SendAsync(buffer.Slice(0,result.Count),result.MessageType,result.EndOfMessage,CancellationToken.None);
        }
        Environment.Exit(0);
    }

    private static async Task OnConnectionClient(int port)
    {
        Listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        Listener.Start();
        do
        {
            await Task.Delay(100);
        } while (!Listener.IsListening);
            
        Console.WriteLine($"Listening on port {port}");
        HttpListenerContext context;
        while (true)
        {
            context = await Listener.GetContextAsync();
            if (!context.Request.IsWebSocketRequest)
            {
                Console.WriteLine("Not a websocket request");
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
            else
                break;
        }

        HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
        _client = webSocketContext.WebSocket;
        Console.WriteLine("Connected to client");
        await Task.Delay(10);
        var buffer = WebSocket.CreateServerBuffer(1024*1024*4); //4MB
        while (_client.State == WebSocketState.Open)
        {
            var result = await _client.ReceiveAsync(buffer, CancellationToken.None);
            #if DEBUG
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    Console.WriteLine($"From Client: {Encoding.UTF8.GetString(buffer.Slice(0,result.Count))}");
                    break;
                case WebSocketMessageType.Binary:
                    Console.WriteLine($"From Client: {result.Count} Bytes");
                    break;
            }
            #endif
            await Ws.SendAsync(buffer.Slice(0,result.Count),result.MessageType,result.EndOfMessage,CancellationToken.None);
        }
        Environment.Exit(0);
    }
}