using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using System;

public class NavigationSessionManager : MonoBehaviour
{
    public static NavigationSessionManager Instance;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    private string sessionId;
    private float startTime;

    // Step tracking
    private int stepsAtStart = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    // ===============================
    // START SESSION
    // ===============================
    public void StartSession(string destinationId)
    {
        StartCoroutine(StartSessionWhenAuthReady(destinationId));
    }

    IEnumerator StartSessionWhenAuthReady(string destinationId)
    {
        while (auth == null || auth.CurrentUser == null)
            yield return null;

        sessionId = Guid.NewGuid().ToString();
        startTime = Time.time;

        // Capture steps at start
        if (StepCounter.instance != null)
            stepsAtStart = StepCounter.instance.GetTodaysSteps();
        else
            stepsAtStart = 0;

        // 🔥 Daily key (from script #1)
        string dayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var data = new Dictionary<string, object>
        {
            { "sessionId", sessionId },
            { "userId", auth.CurrentUser.UserId },
            { "email", auth.CurrentUser.Email },
            { "destinationId", destinationId },
            { "startedAt", FieldValue.ServerTimestamp },
            { "dayKey", dayKey },
            { "completed", false },
            { "cancelled", false },
            { "platform", Application.platform.ToString() },
            { "appVersion", Application.version }
        };

        db.Collection("navigation_sessions")
          .Document(sessionId)
          .SetAsync(data);

        Debug.Log("✅ Session started: " + sessionId);
    }

    // ===============================
    // END SESSION (DESTINATION REACHED)
    // ===============================
    public void EndSession(float unused)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.LogWarning("No active navigation session to end.");
            return;
        }

        float timeTaken = Time.time - startTime;

        int stepsToday = 0;
        int stepsSession = 0;

        if (StepCounter.instance != null)
        {
            stepsToday = StepCounter.instance.GetTodaysSteps();
            stepsSession = Mathf.Max(0, stepsToday - stepsAtStart);
        }

        // 🔥 Distance from PathEstimationUtils
        float distance = 0f;
        if (PathEstimationUtils.instance != null)
            distance = PathEstimationUtils.instance.GetTotalPathDistance();

        var update = new Dictionary<string, object>
        {
            { "distance", distance },
            { "stepsSession", stepsSession },
            { "stepsToday", stepsToday },
            { "timeTaken", timeTaken },
            { "completed", true },
            { "endedAt", FieldValue.ServerTimestamp }
        };

        db.Collection("navigation_sessions")
          .Document(sessionId)
          .UpdateAsync(update)
          .ContinueWithOnMainThread(task =>
          {
              if (!task.IsFaulted)
              {
                  // 🏁 Completion time notification
                  string formattedTime = FormatTime(timeTaken);
                  StartCoroutine(ShowCompletionTimeNotification(formattedTime));

                  // 🎉 Daily factorial-5 milestone check
                  StartCoroutine(CheckDailySessionMilestone());
              }
          });

        Debug.Log("🏁 Session completed: " + sessionId);

        sessionId = null;
        stepsAtStart = 0;
    }

    // ===============================
    // 🏁 COMPLETION TIME NOTIFICATION
    // ===============================
    IEnumerator ShowCompletionTimeNotification(string formattedTime)
    {
        yield return new WaitForSeconds(1.5f);

        NotificationManager.Show(
            "Destination reached!",
            $"You completed this route in {formattedTime}"
        );
    }

    // ===============================
    // 🎉 FACTORIAL-5 DAILY NOTIFICATION
    // ===============================
    IEnumerator CheckDailySessionMilestone()
    {
        yield return new WaitForSeconds(2.5f);

        if (auth.CurrentUser == null)
            yield break;

        string todayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");

        Query query = db.Collection("navigation_sessions")
            .WhereEqualTo("userId", auth.CurrentUser.UserId)
            .WhereEqualTo("completed", true)
            .WhereEqualTo("dayKey", todayKey);

        var task = query.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError("❌ Failed to fetch daily session count: " + task.Exception);
            yield break;
        }

        int completedToday = task.Result.Count;

        if (completedToday > 0 && completedToday % 5 == 0)
        {
            NotificationManager.Show(
                "Amazing progress!",
                $"You’ve completed {completedToday} navigation sessions today"
            );
        }
    }

    // ===============================
    // ⏱ TIME FORMATTER
    // ===============================
    string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);

        if (totalSeconds < 60)
            return $"{totalSeconds} sec";

        int mins = totalSeconds / 60;
        int secs = totalSeconds % 60;

        return $"{mins} min {secs} sec";
    }

    // ===============================
    // CANCEL SESSION
    // ===============================
    public void CancelSession()
    {
        if (string.IsNullOrEmpty(sessionId))
            return;

        var update = new Dictionary<string, object>
        {
            { "cancelled", true },
            { "endedAt", FieldValue.ServerTimestamp }
        };

        db.Collection("navigation_sessions")
          .Document(sessionId)
          .UpdateAsync(update);

        sessionId = null;
        stepsAtStart = 0;
    }

    // ===============================
    // GET CURRENT SESSION ID (from script #2)
    // ===============================
    public string GetCurrentSessionId()
    {
        if (string.IsNullOrEmpty(sessionId))
            return "NO_SESSION";

        return sessionId;
    }
}
