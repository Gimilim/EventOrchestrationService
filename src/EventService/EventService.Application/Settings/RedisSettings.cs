namespace EventService.Application.Settings;

public class RedisSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 3000;
    public bool AbortOnConnectFail { get; set; } = false;
}