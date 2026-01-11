using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance;
    public string CurrentBuildingId { get; private set; } = "";

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCurrentBuilding(string buildingId)
    {
        if (!string.IsNullOrEmpty(buildingId))
        {
            CurrentBuildingId = buildingId;
            Debug.Log("✅ BUILDING CONTEXT SAVED: " + CurrentBuildingId);
        }
    }
}