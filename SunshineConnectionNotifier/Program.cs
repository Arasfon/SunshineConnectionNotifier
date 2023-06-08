using Microsoft.Toolkit.Uwp.Notifications;

using Serilog;
using Serilog.Events;

namespace SunshineConnectionNotifier;

internal class Program
{
    private static readonly ConfigurationManager ConfigurationManager = new();

    private static bool _exiting = false;

    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Debug()
            .WriteTo.File("log.txt", restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();

        AppDomain.CurrentDomain.ProcessExit += (_, _) => _exiting = true;

        await ConfigurationManager.LoadConfiguration();

        ConfigurationManager.StartWatching();

        await using FileStream sunshineLogFileStream = File.Open(ConfigurationManager.Configuration!.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader sunshineLogReader = new(sunshineLogFileStream);
        
        // Move to an end
        sunshineLogReader.BaseStream.Seek(0, SeekOrigin.End);

        while (!_exiting)
        {
            while (sunshineLogReader.EndOfStream != true)
            {
                var line = (await sunshineLogReader.ReadLineAsync())!.ToLowerInvariant();
                if (line.Contains("client connected"))
                {
                    new ToastContentBuilder()
                        .AddText("New connection to Sunshine is established")
                        .Show();

                    Log.Information("New connection found, notification sent");
                }
                else if (line.Contains("client disconnected"))
                {
                    new ToastContentBuilder()
                        .AddText("Connection to Sunshine was closed")
                        .Show();

                    Log.Information("Connection close found, notification sent");
                }
            }
            await Task.Delay(ConfigurationManager.Configuration!.PollingInterval);
        }

        ConfigurationManager.StopWatching();
        await Log.CloseAndFlushAsync();
    }
}
