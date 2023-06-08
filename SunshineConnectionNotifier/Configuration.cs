namespace SunshineConnectionNotifier;

public class Configuration
{
    public int PollingInterval { get; set; } = 100;
    public string LogPath { get; set; } = GetDefaultLogPath();

    private static string GetDefaultLogPath()
    {
        if (OperatingSystem.IsWindows())
            return $@"{Environment.GetEnvironmentVariable("ProgramFiles")}\Sunshine\config\sunshine.log";
        if (OperatingSystem.IsLinux())
            return @"~/.config/sunshine/sunshine.log";
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            return @"~/.config/sunshine/sunshine.log";

        throw new PlatformNotSupportedException();
    }
}
