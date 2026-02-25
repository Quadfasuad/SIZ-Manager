using System.IO;

namespace SizManager.Helpers;

public static class Logger
{
    private static readonly object _lock = new();

    public static void LogError(Exception ex, string? context = null)
    {
        try
        {
            lock (_lock)
            {
                var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ";
                if (!string.IsNullOrEmpty(context))
                    message += $"[{context}] ";
                message += $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n";

                File.AppendAllText(AppPaths.ErrorLogPath, message);
            }
        }
        catch
        {
            // Ignore logging errors
        }
    }

    public static void LogInfo(string message)
    {
        try
        {
            lock (_lock)
            {
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] {message}\n";
                File.AppendAllText(AppPaths.ErrorLogPath, line);
            }
        }
        catch
        {
            // Ignore logging errors
        }
    }
}
