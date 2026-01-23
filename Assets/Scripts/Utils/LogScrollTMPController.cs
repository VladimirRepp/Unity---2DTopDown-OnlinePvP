using System.Collections;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogScrollTMPController : MonoBehaviour
{
    [Header("Scroll View")]
    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Font Size Input")]
    [SerializeField] private TMP_InputField fontSizeInput;
    [SerializeField] private float minFontSize = 10f;
    [SerializeField] private float maxFontSize = 80f;

    private readonly StringBuilder _builder = new StringBuilder();
    private bool _subscribed = false;

    private void Awake()
    {
        fontSizeInput.contentType = TMP_InputField.ContentType.DecimalNumber;
        fontSizeInput.onEndEdit.AddListener(OnFontSizeChanged);

        // инициализация текущего значения
        fontSizeInput.text = logText.fontSize.ToString(CultureInfo.InvariantCulture);
    }


    private void OnEnable()
    {
        StartCoroutine(WaitForLoggerRoutine());
    }

    private IEnumerator WaitForLoggerRoutine()
    {
        // ждём инициализации LogCapture
        while (LogCapture.Instance == null)
            yield return null;

        Subscribe();
    }

    private void Subscribe()
    {
        if (_subscribed)
            return;

        LogCapture.Instance.OnLogReceived += AppendLog;
        LogCapture.Instance.OnLogsCleared += ClearUI;

        _subscribed = true;
    }

    private void OnDisable()
    {
        if (!_subscribed || LogCapture.Instance == null)
            return;

        LogCapture.Instance.OnLogReceived -= AppendLog;
        LogCapture.Instance.OnLogsCleared -= ClearUI;

        _subscribed = false;
    }

    // ======================
    // LOG OUTPUT
    // ======================

    private void AppendLog(LogEntry entry)
    {
        _builder.AppendLine(Format(entry));
        logText.text = _builder.ToString();

        ForceScrollToBottom();
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
        ForceScrollToBottom();
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
        GUIUtility.systemCopyBuffer = LogCapture.Instance.GetAllLogsAsText();
    }

    // ======================
    // FONT SIZE INPUT
    // ======================

    private void OnFontSizeChanged(string input)
    {
        if (!float.TryParse(input, NumberStyles.Float,
                CultureInfo.InvariantCulture, out float size))
        {
            ResetInput();
            return;
        }

        size = Mathf.Clamp(size, minFontSize, maxFontSize);
        logText.fontSize = size;

        ResetInput(size);
        ForceScrollToBottom();
    }

    private void ResetInput()
    {
        ResetInput(logText.fontSize);
    }

    private void ResetInput(float value)
    {
        fontSizeInput.text = value.ToString(CultureInfo.InvariantCulture);
    }

    // ======================
    // SCROLL UTILS
    // ======================

    private void ForceScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
