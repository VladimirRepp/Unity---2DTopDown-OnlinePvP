using System.Text;
using TMPro;
using UnityEngine;

public class LogTMPUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text logText;

    private readonly StringBuilder _builder = new StringBuilder();

    private void OnEnable()
    {
        LogCapture.Instance.OnLogReceived += AppendLog;
        LogCapture.Instance.OnLogsCleared += ClearUI;
    }

    private void OnDisable()
    {
        if (LogCapture.Instance == null) return;

        LogCapture.Instance.OnLogReceived -= AppendLog;
        LogCapture.Instance.OnLogsCleared -= ClearUI;
    }

    private void AppendLog(LogEntry entry)
    {
        _builder.AppendLine(Format(entry));
        logText.text = _builder.ToString();
    }

    private string Format(LogEntry entry)
    {
        string color = entry.Type switch
        {
            LogType.Warning => "#FFD700",
            LogType.Error => "#FF4C4C",
            LogType.Exception => "#FF4C4C",
            _ => "#FFFFFF"
        };

        return $"<color={color}>[{entry.Time:HH:mm:ss}] {entry.Type}: {entry.Message}</color>";
    }

    private void ClearUI()
    {
        _builder.Clear();
        logText.text = string.Empty;
    }

    // ======================
    // UI BUTTON CALLBACKS
    // ======================

    public void OnClearButton()
    {
        LogCapture.Instance.Clear();
    }

    public void OnCopyButton()
    {
        string allLogs = LogCapture.Instance.GetAllLogsAsText();
        GUIUtility.systemCopyBuffer = allLogs;
    }
}
