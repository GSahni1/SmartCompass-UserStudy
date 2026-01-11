using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private RectTransform notificationPrefab;
    [SerializeField] private Canvas targetCanvas;

    private RectTransform currentNotification;
    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===============================
    // PUBLIC ENTRY POINT
    // ===============================
    public static void Show(string title, string body)
    {
        if (Instance == null)
            return;

        Instance.ShowInternal(title, body);
    }

    // ===============================
    // INTERNAL SHOW LOGIC
    // ===============================
    private void ShowInternal(string title, string body)
    {
        Debug.Log("Starting");
        // 🔥 Stop previous animation safely
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        // 🔥 Destroy previous notification safely
        if (currentNotification != null)
        {
            Destroy(currentNotification.gameObject);
            currentNotification = null;
        }

        // Instantiate new notification
        RectTransform rt =
            Instantiate(notificationPrefab, targetCanvas.transform);

        currentNotification = rt;

        // ===============================
        // 🔧 FIXED TEXT BINDING (IMPORTANT)
        // ===============================
        Transform content = rt.Find("Content");
        if (content == null)
        {
            Debug.LogError("❌ Content not found in Notification prefab");
            return;
        }

        TextMeshProUGUI titleText =
            content.Find("Title")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI bodyText =
            content.Find("Body")?.GetComponent<TextMeshProUGUI>();

        if (titleText == null || bodyText == null)
        {
            Debug.LogError("❌ Title or Body not found in Notification prefab");
            return;
        }

        titleText.text = title;
        bodyText.text = body;

        // Start animation
        currentRoutine = StartCoroutine(SlideSequence(rt));
    }

    // ===============================
    // ANIMATION SEQUENCE
    // ===============================
    private IEnumerator SlideSequence(RectTransform rt)
    {
        if (rt == null) yield break;

        Vector2 hiddenPos = new Vector2(0, 220);
        Vector2 visiblePos = new Vector2(0, -120);

        float duration = 0.5f;
        float stayTime = 1.5f;

        yield return MoveOverTime(rt, hiddenPos, visiblePos, duration);

        yield return new WaitForSeconds(stayTime);

        yield return MoveOverTime(rt, visiblePos, hiddenPos, duration);

        if (rt != null)
            Destroy(rt.gameObject);

        if (currentNotification == rt)
            currentNotification = null;

        currentRoutine = null;
    }

    // ===============================
    // MOVE HELPER
    // ===============================
    private IEnumerator MoveOverTime(
        RectTransform rt,
        Vector2 start,
        Vector2 end,
        float time)
    {
        if (rt == null) yield break;

        float elapsed = 0f;

        while (elapsed < time)
        {
            if (rt == null)
                yield break;

            rt.anchoredPosition =
                Vector2.Lerp(start, end, elapsed / time);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rt != null)
            rt.anchoredPosition = end;
    }
}
