using Microsoft.Toolkit.Uwp.Notifications;

using System.Diagnostics;
using System.Text.RegularExpressions;

using WmiLight;

bool connectionExisted = false;

Timer timer = new(TimerCallback, null, 0, 10000);

await Task.Delay(-1);

void TimerCallback(object? state)
{
    int? sunshinePid = Process.GetProcesses().Where(x => x.ProcessName == "sunshine").FirstOrDefault()?.Id;

    if (sunshinePid is null)
    {
        Console.WriteLine($"{DateTime.Now} [check] Sunshine is not running");
        return;
    }

    using WmiConnection con = new();

    bool connectionActive = con.CreateQuery("SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine").Cast<WmiObject>()
        .Select(x => PidRegex().Match((string)x["Name"]).Groups[1].Value)
        .Distinct()
        .Contains(sunshinePid.ToString());

    Console.WriteLine($"{DateTime.Now} [check] Connection is {(connectionActive ? "" : "not ")}active");

    if (connectionActive != connectionExisted)
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

        Console.WriteLine($"{DateTime.Now} [notif] Sent notification");
    }

    connectionExisted = connectionActive;
}

partial class Program
{
    [GeneratedRegex("pid_(\\d+)_.+")]
    private static partial Regex PidRegex();
}
