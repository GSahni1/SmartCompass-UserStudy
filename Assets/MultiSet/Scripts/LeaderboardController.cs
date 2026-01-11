using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LeaderboardController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public GameObject needsLocalizationMessage;

    void Start()
    {
        needsLocalizationMessage.SetActive(false);
        string buildingId = BuildingManager.instance.CurrentBuildingId;

        if (string.IsNullOrEmpty(buildingId))
        {
            // If no building ID is saved, show a default title and the warning message.
            titleText.text = "Leaderboard";
            needsLocalizationMessage.SetActive(true);
        }
        else
        {
            // --- THIS IS THE CHANGE ---
            // We now set the title text to be the actual mapOrMapsetCode.
            titleText.text = buildingId;
            
            FetchDataFromFirebase(buildingId);
        }
    }

    void FetchDataFromFirebase(string id)
    {
        Debug.Log("Fetching Firebase data for building: " + id);
        // --- YOUR FIREBASE LOGIC GOES HERE ---
    }

    public void GoToMainScene()
    {
        SceneManager.LoadScene("Navigation 1"); // Make sure your main scene name is correct
    }
}