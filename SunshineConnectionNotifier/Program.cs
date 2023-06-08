﻿using Microsoft.Toolkit.Uwp.Notifications;

using Serilog;
using Serilog.Events;
using System.Diagnostics;

using WmiLight;

namespace SunshineConnectionNotifier;

internal class Program
{
    private static Timer? _timer;

    private static bool _connectionExisted;

    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Debug()
            .WriteTo.File("log.txt", restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();

        _timer = new Timer(TimerCallback, null, 0, 10000);

        await Task.Delay(-1);

        await Log.CloseAndFlushAsync();
    }

    private static void TimerCallback(object? state)
    {
        int? sunshinePid = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "sunshine")?.Id;

        if (sunshinePid is null)
        {
            Log.Verbose("Sunshine is not running");
            return;
        }

        using WmiConnection con = new();

        bool connectionActive = con.CreateQuery($"SELECT Name FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine WHERE Name LIKE \"pid_{sunshinePid}_%\"").Any();

        Log.Verbose($"Connection is {(connectionActive ? "" : "not ")}active");

        if (connectionActive != _connectionExisted)
        {
            if (connectionActive)
            {
                new ToastContentBuilder()
                    .AddText("New connection to Sunshine is established")
                    .Show();
            }
            else
            {
                new ToastContentBuilder()
                    .AddText("Connection to Sunshine was closed")
                    .Show();
            }

            Log.Information("Sent notification");
        }

        _connectionExisted = connectionActive;
    }
}
