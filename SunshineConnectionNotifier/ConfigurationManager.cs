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
            if (Configuration is null)
            {
                Configuration ??= new Configuration();
                Log.Information("Deafult configuration loaded");
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

        Log.Information("Configuration loaded from disk");
    }

    public void StartWatching()
    {
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher("./")
        {
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "configuration.json"
        };

        _watcher.Created += OnWatcherEvent;
        _watcher.Changed += OnWatcherEvent;

        _watcher.EnableRaisingEvents = true;

        Log.Information("ConfigurationManager watch started");
    }

    public void StopWatching()
    {
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
        _watcher = null;

        Log.Information("ConfigurationManager watch stopped");
    }

    private async void OnWatcherEvent(object sender, FileSystemEventArgs e)
    {
        Log.Debug("Configuration file changed");

        await LoadConfiguration();
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }
}
