using UnityEngine;

public class NotificationTester : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Showing first notification.");
        NotificationManager.Show("Hello", "This is your first popup.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NotificationManager.Show("New Message", "You pressed Space.");
        }
    }
}
