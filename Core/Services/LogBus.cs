namespace FakeHostLocalLab.Core.Services;

public static class LogBus
{
    public static event Action<string>? OnLog;

    public static void Log(string message)
    {
        OnLog?.Invoke(message);
    }
}
