using UnityEngine;
using TMPro;

public class HistoryRowUI : MonoBehaviour
{
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI stepCountText;

    // A public method to easily set up the row's data
    public void SetRowData(DailyStepData data)
    {
        dateText.text = data.date;
        stepCountText.text = data.steps.ToString() + " steps";
    }
}