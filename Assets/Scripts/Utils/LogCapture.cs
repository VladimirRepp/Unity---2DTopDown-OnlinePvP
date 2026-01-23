using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LogCapture : MonoBehaviour
{
    public static LogCapture Instance { get; private set; }

    public event Action<LogEntry> OnLogReceived;
    public event Action OnLogsCleared;

    [Header("Settings")]
    [SerializeField] private int maxLogs = 500;
    [SerializeField] private bool includeStackTrace = true;

    private readonly Queue<LogEntry> _logs = new Queue<LogEntry>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        var entry = new LogEntry
        {
            Message = condition,
            StackTrace = includeStackTrace ? stackTrace : string.Empty,
            Type = type,
            Time = DateTime.Now
        };

        _logs.Enqueue(entry);
        if (_logs.Count > maxLogs)
            _logs.Dequeue();

        OnLogReceived?.Invoke(entry);
    }

    public void Clear()
    {
        _logs.Clear();
        OnLogsCleared?.Invoke();
    }

    public string GetAllLogsAsText()
    {
        var sb = new StringBuilder();

        foreach (var log in _logs)
        {
            sb.AppendLine(
                $"[{log.Time:HH:mm:ss}] {log.Type}: {log.Message}");

            if (!string.IsNullOrEmpty(log.StackTrace))
                sb.AppendLine(log.StackTrace);
        }

        return sb.ToString();
    }
}

[Serializable]
public struct LogEntry
{
    public string Message;
    public string StackTrace;
    public LogType Type;
    public DateTime Time;
}