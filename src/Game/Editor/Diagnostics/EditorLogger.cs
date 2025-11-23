using System;

namespace CubeSurvivor.Game.Editor.Diagnostics
{
    /// <summary>
    /// Centralized logging for editor debugging.
    /// Toggle Enabled to turn on/off all editor logs.
    /// </summary>
    public static class EditorLogger
    {
        public static bool Enabled { get; set; } = true;

        public static void Log(string tag, string message)
        {
            if (!Enabled) return;
            Console.WriteLine($"[Editor/{tag}] {message}");
        }

        public static void LogError(string tag, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Editor/{tag}] ERROR: {message}");
            Console.ResetColor();
        }

        public static void LogWarning(string tag, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Editor/{tag}] WARN: {message}");
            Console.ResetColor();
        }
    }
}

