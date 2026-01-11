using UnityEngine;

public class TreasureData : MonoBehaviour
{
    [Header("Settings")]
    public string treasureId; 
    public string treasureName; 
    
    [HideInInspector]
    public bool isFound = false;

    // CHANGED: We now store an array of renderers to handle multiple children (Chest + Coins)
    private Renderer[] allRenderers;
    private Collider col;

    void Awake()
    {
        // OLD: meshRenderer = GetComponent<MeshRenderer>(); 
        // NEW: Find ALL renderers in this object AND its children
        allRenderers = GetComponentsInChildren<Renderer>();
        
        col = GetComponent<Collider>();
    }

    public void SetActiveState(bool isHuntActive)
    {
        // Only show if Hunt is ON AND it hasn't been found yet
        bool shouldShow = isHuntActive && !isFound;
        
        // Toggle the Collider on the Parent
        if(col != null) col.enabled = shouldShow;

        // Toggle Visuals for ALL children (Chest, Coins, Lid, etc.)
        if (allRenderers != null)
        {
            foreach (var r in allRenderers)
            {
                r.enabled = shouldShow;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TreasureHuntManager.Instance.FoundTreasure(this);
    }
}