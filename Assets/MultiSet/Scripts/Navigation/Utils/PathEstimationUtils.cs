using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/**
 * Estimate remaining path to selected POI
 * + Progress-based notifications (25%, 50%, 75%)
 */
public class PathEstimationUtils : MonoBehaviour
{
    public static PathEstimationUtils instance;

    // remaining distance
    float remainingDistance = 0;

    // ARCamera of the scene
    Camera ARCamera;

    // collider radius of the ARCamera
    float arCameraColliderRadius;

    // current destination
    POI destination;

    // collider radius of the current destination
    float destinationColliderRadius;

    [Tooltip("Slider to visualize progress")]
    public Slider progressSlider;

    // TOTAL distance when navigation started
    float startingDistance = 0;

    bool estimationStarted = false;

    // ===============================
    // 🔔 NOTIFICATION FLAGS
    // ===============================
    bool notified25 = false;
    bool notified50 = false;
    bool notified75 = false;

    void Awake()
    {
        instance = this;
        ARCamera = Camera.main;
    }

    void Start()
    {
        arCameraColliderRadius =
            ARCamera.gameObject.GetComponent<SphereCollider>().radius;
    }

    void FixedUpdate()
    {
        HandleProgress();
    }

    // ===============================
    // PROGRESS HANDLING
    // ===============================
    void HandleProgress()
    {
        if (!estimationStarted || startingDistance <= 0)
            return;

        float travelled = startingDistance - remainingDistance;
        float progress = Mathf.Clamp01(travelled / startingDistance);

        // UI slider update
        if (progressSlider != null)
            progressSlider.value = progress + 0.03f;

        // 🔔 progress notifications
        CheckProgressNotifications(progress);
    }

    // ===============================
    // 🔔 PROGRESS NOTIFICATIONS
    // ===============================
    void CheckProgressNotifications(float progress)
    {
        if (progress >= 0.25f && !notified25)
        {
            notified25 = true;
            NotificationManager.Show(
                "Good start!",
                "You’ve completed 25% of your route"
            );
        }

        if (progress >= 0.50f && !notified50)
        {
            notified50 = true;
            NotificationManager.Show(
                "Halfway there!",
                "You’re 50% of the way to your destination"
            );
        }

        if (progress >= 0.75f && !notified75)
        {
            notified75 = true;
            NotificationManager.Show(
                "Almost there!",
                "Just 25% left — keep going"
            );
        }
    }

    /**
     * Updates remaining distance from NavMesh path
     */
    public void UpdateEstimation(Vector3[] path)
    {
        if (destination == null)
        {
            destination = NavigationController.instance.currentDestination;
            destinationColliderRadius =
                destination.poiCollider.GetComponent<SphereCollider>().radius;
        }

        if (path.Length > 1)
        {
            float remainingPathTotal = 0f;

            for (int i = 0; i < path.Length - 1; i++)
            {
                remainingPathTotal +=
                    Vector3.Distance(path[i], path[i + 1]);
            }

            remainingDistance =
                remainingPathTotal - arCameraColliderRadius - destinationColliderRadius;

            // reset starting distance if path recalculates
            if (!estimationStarted || remainingDistance > startingDistance)
            {
                startingDistance = remainingDistance;
                estimationStarted = true;
            }
        }
    }

    // ===============================
    // RESET (NEW SESSION / CANCEL)
    // ===============================
    public void ResetEstimation()
    {
        destination = null;
        estimationStarted = false;
        remainingDistance = 0;
        startingDistance = 0;

        // reset notification flags
        notified25 = false;
        notified50 = false;
        notified75 = false;
    }

    // ===============================
    // 🔥 USED BY FIREBASE (TOTAL PATH)
    // ===============================
    public float GetTotalPathDistance()
    {
        return startingDistance;
    }

    // ===============================
    // 🔥 USED BY UI LIST (LIVE ESTIMATE)
    // ===============================
    public float EstimateDistanceToPosition(POI destination)
    {
        if (!NavigationController.instance.agent.isOnNavMesh)
            return -1;

        NavMeshPath path = new NavMeshPath();

        NavMesh.CalculatePath(
            NavigationController.instance.agent.transform.position,
            destination.poiCollider.transform.position,
            NavMesh.AllAreas,
            path
        );

        if (path.status == NavMeshPathStatus.PathInvalid ||
            path.status == NavMeshPathStatus.PathPartial)
        {
            return -2;
        }

        if (path.corners.Length > 1)
        {
            float distance = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                distance +=
                    Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            return distance;
        }

        return -1;
    }

    // ===============================
    // HELPERS
    // ===============================
    public int getRemainingDistanceMeters()
    {
        return (int)remainingDistance;
    }

    public float GetProgress01()
    {
        if (!estimationStarted || startingDistance <= 0)
            return 0f;

        return Mathf.Clamp01(
            (startingDistance - remainingDistance) / startingDistance
        );
    }

    public bool IsEstimationActive()
    {
        return estimationStarted && startingDistance > 0;
    }
}
