using Serilog;

using System.Text.Json;

namespace SunshineConnectionNotifier;

public class ConfigurationManager
{
    public Configuration? Configuration { get; private set; }

    private FileSystemWatcher? _watcher;

    public event EventHandler? ConfigurationChanged;

    public async Task LoadConfiguration()
    {
        if (!File.Exists("configuration.json"))
        {
            Configuration configuration = new();
            Configuration = configuration;

            try
            {
                await using StreamWriter sw = new("configuration.json");
                await JsonSerializer.SerializeAsync(sw.BaseStream, configuration, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                Log.Verbose("Configuration created and loaded");
            }
            catch
            {
                Log.Warning("Failed to create default configuration");
                Log.Verbose("Configuration loaded");
            }

            return;
        }

        try
        {
            await using FileStream fsr = File.Open("configuration.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader sr = new(fsr);

            Configuration cfg = (await JsonSerializer.DeserializeAsync<Configuration>(sr.BaseStream,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }))!;

            Configuration = cfg;
        }
        catch
        {
            Log.Warning("Configuration failed to load");
            Configuration ??= new Configuration();
            return;
        }

        Log.Verbose("Configuration loaded");
    }

    public void StartWatching()
    {
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher("./")
        {
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "configuration.json"
        };

        _watcher.Changed += OnWatcherEvent;
        _watcher.Deleted += OnWatcherEvent;

        _watcher.EnableRaisingEvents = true;

        Log.Information("ConfigurationManager watch started");
    }

    public void StopWatching()
    {
        _watcher?.Dispose();
        _watcher = null;

        Log.Information("ConfigurationManager watch stopped");
    }

    private async void OnWatcherEvent(object sender, FileSystemEventArgs e)
    {
        Log.Debug("Configuration changed");

        await LoadConfiguration();
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }
}
