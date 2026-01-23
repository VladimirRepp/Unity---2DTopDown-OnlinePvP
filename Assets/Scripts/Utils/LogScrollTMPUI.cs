using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogScrollTMPUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Font Size")]
    [SerializeField] private float defaultFontSize = 24f;

    private readonly StringBuilder _builder = new StringBuilder();

    private void Awake()
    {
        logText.fontSize = defaultFontSize;
    }

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

        // автоскролл вниз
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
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
    // FONT SIZE API
    // ======================

    public void SetFontSize(float size)
    {
        logText.fontSize = size;

        // обновляем layout
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
