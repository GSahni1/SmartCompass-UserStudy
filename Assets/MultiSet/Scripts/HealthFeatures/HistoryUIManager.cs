using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class HistoryUIManager : MonoBehaviour
{
    public GameObject historyRowPrefab;
    public Transform contentParent;

    void Start()
    {
        foreach (Transform child in contentParent) { Destroy(child.gameObject); }

        List<DailyStepData> history = StepCounter.instance.GetEntireHistory();
        history.Reverse();

        foreach (DailyStepData dayData in history)
        {
            // Create the new row
            GameObject newRowObject = Instantiate(historyRowPrefab, contentParent);
            
            // Get the HistoryRowUI component from it and set the data
            newRowObject.GetComponent<HistoryRowUI>().SetRowData(dayData);
        }
    }
}