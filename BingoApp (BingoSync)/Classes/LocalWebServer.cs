using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Microsoft.AspNetCore;
using BingoApp.Properties;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace BingoApp;
public class LocalWebServer
{
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public event EventHandler? OnServerStarted;

    public LocalWebServer()
    {
        serverThread = new System.Threading.Thread(RunServer);
        IsServerStarted = false;
    }

    private Thread serverThread = default!;

    public bool IsServerStarted { get; private set; } = false;

    public void Start()
    {
        if (IsServerStarted) return;
        cancellationTokenSource = new CancellationTokenSource();
        serverThread = new System.Threading.Thread(RunServer);
        serverThread.Start();
    }

    public async Task StopAsync()
    {
        await (new TaskFactory()).StartNew(() =>
        {
            cancellationTokenSource.Cancel();            
        });
    }

    async void RunServer()
    {
        var portNumber = Settings.Default.LocalServerPort;
        var server = WebHost
       .CreateDefaultBuilder()
       .UseKestrel(x =>
       {
           x.ListenAnyIP(portNumber);
           x.ListenLocalhost(portNumber);
       })
        .UseStartup<LocalServerStartup>()
        .Build();
        try
        {
            IsServerStarted = true;
            OnServerStarted?.Invoke(this, new EventArgs());
            await server.RunAsync(cancellationTokenSource.Token);
            IsServerStarted = false;
        }
        catch (Exception ex)
        {

        }
    }

    public static string[] GetIPAddresses()
    {
        var list = new List<string>();

        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                list.Add(ip.ToString());
            }
        }

        return list.ToArray();
    }
}