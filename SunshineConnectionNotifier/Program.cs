using Microsoft.Toolkit.Uwp.Notifications;

using System.Diagnostics;
using System.Text.RegularExpressions;

using WmiLight;

namespace SunshineConnectionNotifier;

internal partial class Program
{
    [GeneratedRegex("pid_(\\d+)_.+")]
    private static partial Regex PidRegex();

    private static Timer? _timer;

    private static bool _connectionExisted;

    public static async Task Main(string[] args)
    {
        _timer = new Timer(TimerCallback, null, 0, 10000);

        await Task.Delay(-1);
    }

    private static void TimerCallback(object? state)
    {
        int? sunshinePid = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "sunshine")?.Id;

        if (sunshinePid is null)
        {
            Console.WriteLine($"{DateTime.Now} [check] Sunshine is not running");
            return;
        }

        using WmiConnection con = new();

        bool connectionActive = con.CreateQuery("SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine")
            .Select(x => PidRegex().Match((string)x["Name"]).Groups[1].Value)
            .Distinct()
            .Contains(sunshinePid.ToString());

        Console.WriteLine($"[{DateTime.Now}] Connection is {(connectionActive ? "" : "not ")}active");

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

            Console.WriteLine($"[{DateTime.Now}] Sent notification");
        }

        _connectionExisted = connectionActive;
    }
}
