using UnityEngine;
using TMPro;
using MultiSet;

/**
 * Handles the navigation UI state and input.
 */
public class NavigationUIController : MonoBehaviour
{
    public static NavigationUIController instance;

    [Tooltip("The icon button for opening the Settings scene")]
public GameObject settingsButton;

[Tooltip("The icon button for opening the History scene")]
public GameObject historyButton;

    [Tooltip("Label to show remaining distance")]
    public TextMeshProUGUI remainingDistance;

    [Tooltip("Button to stop navigation")]
    public GameObject stopButton;

    [Tooltip("SelectList where POIs are shown")]
    public SelectList poiList;

    [Tooltip("Parent GameObject of POIs selection UI")]
    public GameObject DestinationSelectUI;

    [Tooltip("Label to show name of current destination")]
    public TextMeshProUGUI destinationName;

    [Tooltip("Parent GameObject of navigation progress slider")]
    public GameObject navigationProgressSlider;

    [Tooltip("Navigation Path Material")]
    public Material material;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ShowNavigationUIElements(false);
        DestinationSelectUI.SetActive(false);

        destinationName.text = "";
    }

    void Update()
    {
        HandleNavigationState();
        UpdateRemainingDistance();
    }

    // handles the 
    void HandleNavigationState()
    {
        if (NavigationController.instance.IsCurrentlyNavigating())
        {
            destinationName.text = NavigationController.instance.currentDestination.poiName;
            return;
        }
        destinationName.text = "";
    }

    /**
     * Toggles visibility of destination select UI.
     */
    public void ToggleDestinationSelectUI()
    {   
        DestinationSelectUI.SetActive(!DestinationSelectUI.activeSelf);
        // Check the new state of the destination panel.
        bool isDestinationPanelActive = DestinationSelectUI.activeSelf;

        // Set the visibility of our icon buttons to the opposite.
        // If the panel is active, the buttons should be inactive, and vice versa.
        if (settingsButton != null) settingsButton.SetActive(!isDestinationPanelActive);
        if (historyButton != null) historyButton.SetActive(!isDestinationPanelActive);
        // --- END OF NEW LOGIC ---
        if (!DestinationSelectUI.activeSelf)
        {
            poiList.ResetPOISearch();
            return;
        }

        poiList.RenderPOIs();
    }

    public void ResetPoiSearch()
    {
        poiList.ResetPOISearch();
    }

    public void RenderPoiCall()
    {
        poiList.RenderPOIs();
    }

    // User clicked to start navigation. Is called from ListItemUI.cs
    public void ClickedStartNavigation(POI poi)
    {
        NavigationController.instance.SetPOIForNavigation(poi);
        ToggleDestinationSelectUI();

        ShowNavigationUIElements(true);
    }

    // User clicked to stop navigation
    public void ClickedStopButton()
    {
        ShowNavigationUIElements(false);
        NavigationController.instance.StopNavigation();
    }

    // toggle visibility of navigation UI elements
    void ShowNavigationUIElements(bool isVisible)
    {
        // for navigation
        navigationProgressSlider.SetActive(isVisible);
        stopButton.SetActive(isVisible);
    }

    // Update info about remaining distance.
    void UpdateRemainingDistance()
    {
        if (!NavigationController.instance.IsCurrentlyNavigating())
        {
            remainingDistance.SetText("");
            return;
        }

        int distance = PathEstimationUtils.instance.getRemainingDistanceMeters();
        string distanceText = distance + "";

        if (distance > 1)
        {
            if (material != null)
                material.SetFloat("_PathLength", distance);
        }
        if (distance <= 1)
        {
            distanceText += " m remaining";
        }
        else
        {
            distanceText += " m remaining";
        }
        remainingDistance.text = distanceText;
    }

    // Show arrival state, is called from NavigationController.cs
    public void ShowArrivedState()
    {
        ShowNavigationUIElements(false);
        ToastManager.Instance.ShowAlert("You arrived at the destination!");
    }
}
