using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Utilities.CustomDebugging
{
    public static class CustomDebugger
    {
        private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

        private static readonly Dictionary<DebugLog, bool> _debugTurnOns =
            new Dictionary<DebugLog, bool>();

        static CustomDebugger()
        {
            foreach (DebugLog log in Enum.GetValues(typeof(DebugLog)))
            {
                _debugTurnOns[log] = true;
            }
        }

        public static bool DebugEnabled { get; set; } = true;

        public static IReadOnlyDictionary<DebugLog, bool> DebugTurnOns => _debugTurnOns;

        #region Configuration

        public static void SetLogEnabled(DebugLog logType, bool enabled)
        {
            _debugTurnOns[logType] = enabled;
        }

        public static bool IsLogEnabled(DebugLog logType)
        {
            return _debugTurnOns.TryGetValue(logType, out bool enabled) && enabled;
        }

        #endregion

        #region Log

        public static void Log(string messageLog, bool time = false)
        {
            if (!DebugEnabled)
                return;

            Console.WriteLine(BuildMessage(messageLog, time));
        }

        public static void Log(
            string messageLog,
            DebugLog logType,
            bool time = false)
        {
            if (!DebugEnabled)
                return;

            if (!IsLogEnabled(logType))
                return;

            Console.WriteLine(
                BuildMessage($"{GetTypeMessage(logType)}{messageLog}", time));
        }

        public static void Log(
            string messageLog,
            object caller,
            bool time = false)
        {
            if (!DebugEnabled)
                return;

            string callerName = caller?.GetType().Name ?? "Unknown";

            Console.WriteLine(
                BuildMessage($"{messageLog} <- [{callerName}]", time));
        }

        public static void Log(
            string messageLog,
            DebugLog logType,
            object caller,
            bool time = false)
        {
            if (!DebugEnabled)
                return;

            if (!IsLogEnabled(logType))
                return;

            string callerName = caller?.GetType().Name ?? "Unknown";

            Console.WriteLine(
                BuildMessage(
                    $"[{logType}] {messageLog} <- [{callerName}]",
                    time));
        }

        #endregion

        #region Error

        public static void LogError(string messageLog, bool time = false)
        {
            Console.Error.WriteLine(BuildMessage(messageLog, time));
        }

        public static void LogError(
            string messageLog,
            DebugLog logType,
            bool time = false)
        {
            if (!IsLogEnabled(logType))
                return;

            Console.Error.WriteLine(
                BuildMessage($"{GetTypeMessage(logType)}{messageLog}", time));
        }

        public static void LogError(
            string messageLog,
            object caller,
            bool time = false)
        {
            string callerName = caller?.GetType().Name ?? "Unknown";

            Console.Error.WriteLine(
                BuildMessage($"{messageLog} <- [{callerName}]", time));
        }

        public static void LogError(
            string messageLog,
            DebugLog logType,
            object caller,
            bool time = false)
        {
            string callerName = caller?.GetType().Name ?? "Unknown";

            Console.Error.WriteLine(
                BuildMessage(
                    $"[{logType}] {messageLog} <- [{callerName}]",
                    time));
        }

        #endregion

        #region Assert

        public static void AssertNull<T>(
            T obj,
            string messageLog,
            DebugLog logType,
            object? caller = null,
            bool time = false)
            where T : class
        {
            if (obj != null)
                return;

            string callerName = caller?.GetType().Name ?? "Unknown";

            Console.Error.WriteLine(
                BuildMessage(
                    $"[{logType}] {messageLog} <- [{callerName}]",
                    time));
        }

        #endregion

        private static string BuildMessage(string message, bool includeTime)
        {
            if (!includeTime)
                return message;

            return $"({Stopwatch.Elapsed.TotalSeconds:F3}) {message}";
        }

        private static string GetTypeMessage(DebugLog logType)
        {
            return $"[{logType}] ";
        }
    }

    public enum DebugLog
    {
        Default,
        UI,
        Controller,
        Animation,
        Player,
        Model,
        Server,
        SpriteLoader
    }
}