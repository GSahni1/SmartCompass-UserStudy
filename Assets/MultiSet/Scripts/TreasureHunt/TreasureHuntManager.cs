using UnityEngine;
using Firebase.Firestore; 
using Firebase.Extensions; 
using Firebase.Auth; 
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

public class TreasureHuntManager : MonoBehaviour
{
    public static TreasureHuntManager Instance;

    [Header("Configuration")]
    // Make sure this matches your Firestore Building ID exactly!
    public string buildingId = "k5DGtOVgeMOefl0f9uXr"; 
    public List<TreasureData> allTreasures; 

    [Header("UI Elements")]
    public GameObject congratsPanel; 
    public TextMeshProUGUI congratsText; 
    public Toggle treasureHuntToggle; 

    // Internal State
    private bool isHuntActive = false;
    private FirebaseFirestore db;
    private string currentUserId;

    void Awake()
    {
        Instance = this;
        db = FirebaseFirestore.DefaultInstance;
        if(congratsPanel != null) congratsPanel.SetActive(false);
    }

    void Start()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            currentUserId = user.UserId;
            LoadUserProgress();
        }
        else
        {
            // Optional: If you want to allow guest play, remove this line.
            Debug.LogWarning("No user logged in for Treasure Hunt.");
        }

        UpdateTreasureVisuals();
        if(treasureHuntToggle != null) 
            treasureHuntToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public void OnToggleChanged(bool isOn)
    {
        isHuntActive = isOn;
        UpdateTreasureVisuals();
        if (!isOn && congratsPanel != null) congratsPanel.SetActive(false);
    }

    private void UpdateTreasureVisuals()
    {
        foreach (var t in allTreasures)
        {
            t.SetActiveState(isHuntActive);
        }
    }

    public void FoundTreasure(TreasureData treasure)
    {
        if (!isHuntActive) return;
        if (treasure.isFound) return;

        // 1. Local Update (Instant Feedback)
        treasure.isFound = true;
        treasure.SetActiveState(isHuntActive); 
        ShowCongratsMessage(treasure.treasureName);

        // 2. Database Update
        SaveToFirebase(treasure);
    }

    public void gautamLog(String msg)
    {
        Debug.Log("gautamLog: "+msg);
    }
    private void SaveToFirebase(TreasureData treasure)
    {
        // 1. Get the Current User info from Auth
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return; 

        string currentUserId = user.UserId;
        DocumentReference userDoc = db.Collection("users").Document(currentUserId);

        // Handle defaults if name/email are empty
        string uName = string.IsNullOrEmpty(user.DisplayName) ? "Guest" : user.DisplayName;
        string uEmail = string.IsNullOrEmpty(user.Email) ? "" : user.Email;

        // --- A. Prepare User Data ---
        var userData = new Dictionary<string, object>
        {
            // Ensure Profile Data is present (Upsert strategy)
            { "displayName", uName },
            { "email", uEmail },
            { "isGuest", user.IsAnonymous },
            
            // The Nested Treasure Map
            { "treasures_found_map", new Dictionary<string, object>
                {
                    { buildingId, new Dictionary<string, object>
                        {
                            { treasure.treasureId, true }
                        }
                    }
                }
            }
        };

        // MergeAll: Creates doc if missing, or merges fields if exists
        userDoc.SetAsync(userData, SetOptions.MergeAll);

        // --- B. Create LOG Document (Analytics) ---
        string sessionId = "0";
        if (NavigationSessionManager.Instance != null)
        {
            sessionId = NavigationSessionManager.Instance.GetCurrentSessionId();
        }

        Dictionary<string, object> logEntry = new Dictionary<string, object>
        {
            { "BuildingID", buildingId },
            { "TreasureID", treasure.treasureId },
            { "UserID", currentUserId },
            { "SessionID", sessionId },
            { "FoundAt", FieldValue.ServerTimestamp }
        };

        db.Collection("treasuresFound").AddAsync(logEntry);
    }

    private void LoadUserProgress()
    {
        if (string.IsNullOrEmpty(currentUserId)) return;

        db.Collection("users").Document(currentUserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snap = task.Result;
                
                // We safely try to get the map. If the user is new, this might be null.
                if (snap.ContainsField("treasures_found_map"))
                {
                    var masterMap = snap.GetValue<Dictionary<string, object>>("treasures_found_map");

                    if (masterMap != null && masterMap.ContainsKey(buildingId))
                    {
                        var buildingTreasures = masterMap[buildingId] as Dictionary<string, object>;
                        
                        foreach (var t in allTreasures)
                        {
                            if (buildingTreasures.ContainsKey(t.treasureId))
                            {
                                t.isFound = true;
                            }
                        }
                        UpdateTreasureVisuals();
                    }
                }
            }
        });
    }

    void ShowCongratsMessage(string itemName)
    {
        if (congratsPanel != null && congratsText != null)
        {
            congratsText.text = $"Congrats! You found the {itemName}!";
            congratsPanel.SetActive(true);
            CancelInvoke("HideCongrats");
            Invoke("HideCongrats", 5f);
        }
    }

    void HideCongrats()
    {
        if (congratsPanel != null) congratsPanel.SetActive(false);
    }
}