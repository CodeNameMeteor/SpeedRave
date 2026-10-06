using BepInEx.Logging;

namespace SpeedRave
{
    // All mod logging goes through BepInEx's log source, so messages are tagged with the plugin name and
    // follow BepInEx's log level settings (LogOutput.log and the console).
    internal static class Log
    {
        internal static ManualLogSource Source;

        internal static void Info(string message) => Write(LogLevel.Info, message);
        internal static void Warning(string message) => Write(LogLevel.Warning, message);
        internal static void Error(string message) => Write(LogLevel.Error, message);

        private static void Write(LogLevel level, string message)
        {
            if (Source != null)
            {
                Source.Log(level, message);
            }
            else
            {
                // Before Plugin.Awake has run.
                UnityEngine.Debug.Log("[SpeedRave] " + message);
            }
        }
    }
}
