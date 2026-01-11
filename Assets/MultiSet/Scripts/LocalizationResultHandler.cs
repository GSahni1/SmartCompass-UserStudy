using UnityEngine;
using MultiSet;
using System; 

public class LocalizationResultHandler : MonoBehaviour
{
    [Header("SDK Reference")]
    [Tooltip("CRITICAL: Drag the GameObject that has the MapLocalizationManager script here.")]
    public MapLocalizationManager mapLocalizationManager; // We need this reference to ask for the result.

    /// <summary>
    /// This is the PUBLIC function with NO parameters that the UnityEvent will call.
    /// </summary>
    public void HandleLocalizationSuccess()
    {
        Debug.Log("✅ Localization Success Event Fired!");

        if (mapLocalizationManager == null)
        {
            Debug.LogError("FATAL ERROR: The MapLocalizationManager reference is not set in the Inspector on the LocalizationResultHandler script! Cannot get building ID.");
            return;
        }

        // Now that we know localization succeeded, we ask the manager for the result.
        string buildingId = mapLocalizationManager.mapOrMapsetCode;

        if (!string.IsNullOrEmpty(buildingId))
        {
            // We found the data! Now pass it to the BuildingManager.
            BuildingManager.instance.SetCurrentBuilding(buildingId);
        }
        else
        {
            Debug.LogWarning("Success event fired, but the 'mapOrMapsetCode' on the manager was empty.");
        }
    }
}