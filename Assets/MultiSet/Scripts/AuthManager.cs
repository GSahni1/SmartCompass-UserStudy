using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    [Header("Scene")]
    public string nextSceneName = "MainScene";

    private FirebaseAuth auth;
    public static bool FirebaseReady = false;

    void Awake()
    {
        // 🔒 Singleton + persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        SetStatus("Initializing...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                FirebaseReady = true;

                Debug.Log("Firebase initialized and Auth ready");
                SetStatus("");
            }
            else
            {
                Debug.LogError("Firebase dependency error: " + task.Result);
                SetStatus("Firebase error");
            }
        });
    }

    // 🔘 Called by CONTINUE button
    public void OnContinuePressed()
    {
        if (!FirebaseReady)
        {
            SetStatus("Firebase not ready yet");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();
        string name = nameInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("Email & password required");
            return;
        }

        SetStatus("Signing in...");

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("Auth Success");
                    LoadNextScene();
                }
                else
                {
                    CreateAccount(email, password, name);
                }
            });
    }

    void CreateAccount(string email, string password, string name)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError(task.Exception);
                    SetStatus("Account creation failed");
                    return;
                }

                FirebaseUser user = task.Result.User;

                if (!string.IsNullOrEmpty(name))
                {
                    user.UpdateUserProfileAsync(
                        new UserProfile { DisplayName = name }
                    );
                }

                Debug.Log("Account created");
                LoadNextScene();
            });
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
