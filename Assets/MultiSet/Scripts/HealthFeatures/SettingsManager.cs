using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public TMP_InputField targetInputField;
    public Toggle backgroundTrackingToggle;

    public const string StepTargetKey = "UserStepTarget";
    public const string BackgroundTrackingKey = "BackgroundTrackingEnabled";

    void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Load the saved step target. If none is saved, default to 10000.
        int currentTarget = PlayerPrefs.GetInt(StepTargetKey, 10000);
        targetInputField.text = currentTarget.ToString();

        // Load the background tracking preference. Default to false (off).
        bool isBackgroundTrackingOn = PlayerPrefs.GetInt(BackgroundTrackingKey, 0) == 1;
        backgroundTrackingToggle.isOn = isBackgroundTrackingOn;
    }

    public void SaveSettings()
    {
        // Save the step target from the input field.
        if (int.TryParse(targetInputField.text, out int newTarget))
        {
            PlayerPrefs.SetInt(StepTargetKey, newTarget);
        }

        // Save the toggle's on/off state (as a 1 or 0).
        int backgroundTrackingValue = backgroundTrackingToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt(BackgroundTrackingKey, backgroundTrackingValue);

        PlayerPrefs.Save();
        Debug.Log("Settings have been saved!");
    }
}