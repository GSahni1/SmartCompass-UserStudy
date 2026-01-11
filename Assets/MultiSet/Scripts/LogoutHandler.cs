using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class LogoutHandler : MonoBehaviour
{
    public string authSceneName = "AuthScene";

    public void Logout()
    {
        FirebaseAuth.DefaultInstance.SignOut();

        if (StepCounter.instance != null)
            StepCounter.instance.OnUserChanged();

        SceneManager.LoadScene(authSceneName);
    }
}
