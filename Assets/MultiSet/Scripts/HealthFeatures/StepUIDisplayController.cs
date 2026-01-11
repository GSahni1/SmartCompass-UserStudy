using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepUIDisplayController : MonoBehaviour
{
    [Header("UI Elements")]
    public Image progressRingImage; // The orange rim that will fill up
    public TextMeshProUGUI stepCountText; // The big text for the current count

    private int userStepTarget;

    void Start()
    {
        // Load the user's step target from PlayerPrefs.
        userStepTarget = PlayerPrefs.GetInt(SettingsManager.StepTargetKey, 10000);
    }

    void Update()
    {
        if (StepCounter.instance == null) return;

        int currentSteps = StepCounter.instance.GetTodaysSteps();

        // Calculate progress (0.0 to 1.0)
        float progress = (userStepTarget > 0) ? (float)currentSteps / (float)userStepTarget : 0f;

        // Update the fill amount of the orange rim
        progressRingImage.fillAmount = progress;

        // Update the step count text
        stepCountText.text = currentSteps.ToString("N0");
    }
}