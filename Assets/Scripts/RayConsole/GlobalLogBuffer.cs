using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalLogBuffer
{
    public struct LogEntry
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }

    public static readonly List<LogEntry> Entries = new();
    public static event Action<LogEntry> OnLogAdded;

    static GlobalLogBuffer()
    {
        Application.logMessageReceived += HandleLog;
    }

    private static void HandleLog(string logString, string stackTrace, LogType type)
    {
        var entry = new LogEntry
        {
            message = logString,
            stackTrace = stackTrace,
            type = type
        };

        Entries.Add(entry);
        OnLogAdded?.Invoke(entry);
    }
}
