using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
 
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
 
[Serializable]
public class DailyStepData
{
    public string date;
    public int steps;
}
 
[Serializable]
public class StepDataHistory
{
    public List<DailyStepData> history = new List<DailyStepData>();
}
 
public class StepCounter : MonoBehaviour
{
    public static StepCounter instance;
 
    public TextMeshProUGUI stepCountText;
 
    private long phoneStepsAtDayStart = -1;
    private long lastReceivedTotalSteps = 0;
 
    private StepDataHistory stepHistory = new StepDataHistory();
    private bool isSensorAvailable = false;
    private AndroidJavaObject stepDetector;
 
    private string HistoryKey =>
        $"StepHistory_{FirebaseAuth.DefaultInstance.CurrentUser?.UserId}";
 
    private string BaselineKey =>
        $"StepBaseline_{FirebaseAuth.DefaultInstance.CurrentUser?.UserId}";
 
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
 
    void Start()
    {
        LoadStepData();
        LoadBaseline();
        CheckForNewDay();
 
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION"))
            Permission.RequestUserPermission("android.permission.ACTIVITY_RECOGNITION");
        else
            InitializeSensor();
#endif
    }
 
    void Update()
    {
        if (stepCountText != null)
            stepCountText.text = "Steps Today: " + GetTodayStepData().steps;
    }
 
    public void OnStepCounterDataReceived(string message)
    {
        if (!long.TryParse(message, out long totalSteps)) return;
 
        if (phoneStepsAtDayStart == -1)
        {
            phoneStepsAtDayStart = totalSteps - GetTodayStepData().steps;
            SaveBaseline();
        }
 
        lastReceivedTotalSteps = totalSteps;
        int newSteps = (int)(lastReceivedTotalSteps - phoneStepsAtDayStart);
 
        GetTodayStepData().steps = newSteps;
        SaveStepData();
        UpdateStepsInFirestore(newSteps);
    }
 
    private void InitializeSensor()
    {
        if (isSensorAvailable) return;
 
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject sensorManager = activity.Call<AndroidJavaObject>("getSystemService", "sensor");
 
        int SENSOR_TYPE_STEP_COUNTER = 19;
        AndroidJavaObject sensor = sensorManager.Call<AndroidJavaObject>("getDefaultSensor", SENSOR_TYPE_STEP_COUNTER);
 
        if (sensor != null)
        {
            stepDetector = new AndroidJavaObject("com.PersuasiveComputing.SmartComp.StepDetectorListener");
            stepDetector.Call("setUnityObjectName", gameObject.name);
            stepDetector.Call("register", sensorManager, sensor);
            isSensorAvailable = true;
        }
    }
 
    void UpdateStepsInFirestore(int steps)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;
 
        FirebaseFirestore.DefaultInstance
            .Collection("leaderboard")
            .Document(user.UserId)
            .SetAsync(new Dictionary<string, object>
            {
                { "steps", steps }
            }, SetOptions.MergeAll);
    }
 
    public void OnUserChanged()
    {
        LoadStepData();
        LoadBaseline();
    }
 
    void LoadStepData()
    {
        if (PlayerPrefs.HasKey(HistoryKey))
            stepHistory = JsonUtility.FromJson<StepDataHistory>(PlayerPrefs.GetString(HistoryKey));
        else
            stepHistory = new StepDataHistory();
    }
 
    void SaveStepData()
    {
        PlayerPrefs.SetString(HistoryKey, JsonUtility.ToJson(stepHistory));
        PlayerPrefs.Save();
    }
 
    void LoadBaseline()
    {
        if (PlayerPrefs.HasKey(BaselineKey))
            phoneStepsAtDayStart = long.Parse(PlayerPrefs.GetString(BaselineKey));
        else
            phoneStepsAtDayStart = -1;
    }
 
    void SaveBaseline()
    {
        if (phoneStepsAtDayStart >= 0)
            PlayerPrefs.SetString(BaselineKey, phoneStepsAtDayStart.ToString());
    }
 
    void CheckForNewDay()
    {
        if (GetTodayStepData().date != DateTime.Now.ToString("yyyy-MM-dd"))
            phoneStepsAtDayStart = -1;
    }
 
    DailyStepData GetTodayStepData()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        foreach (var d in stepHistory.history)
            if (d.date == today) return d;
 
        var newDay = new DailyStepData { date = today, steps = 0 };
        stepHistory.history.Add(newDay);
        return newDay;
    }
 
    // Compatibility
    public int GetTodaysSteps() => GetTodayStepData().steps;
    public List<DailyStepData> GetEntireHistory() => stepHistory.history;
}