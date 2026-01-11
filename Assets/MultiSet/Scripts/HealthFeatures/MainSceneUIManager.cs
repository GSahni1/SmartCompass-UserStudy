using UnityEngine;
using TMPro;

public class MainSceneUIManager : MonoBehaviour
{
    // A public reference to the text in this scene
    public TextMeshProUGUI stepCountTextInThisScene;

    void Start()
    {
        // When this scene starts, find the persistent StepCounter instance
        // and give it a reference to our UI text.
        if (StepCounter.instance != null)
        {
            StepCounter.instance.stepCountText = stepCountTextInThisScene;
        }
    }
}