using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
 
public class test : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Test());
    }
 
    private IEnumerator Test()
    {
        Debug.LogError("[NETTEST] Starting request...");
        using (var req = UnityWebRequest.Get("https://www.youtube.com/"))
        {
            yield return req.SendWebRequest();
 
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[NETTEST] FAILED: = " + req.error +" ," + req.responseCode);
            }
            else
            {
                Debug.LogError("[NETTEST] OK, code = " + req.responseCode);
            }
        }
    }
}