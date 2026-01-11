#if UNITY_EDITOR
using UnityEditor;

public class ForceEDMResolve
{
    [MenuItem("Tools/Force EDM Android Resolve")]
    public static void Resolve()
    {
        // This is the correct, version-safe call
        System.Type resolverType =
            System.Type.GetType("GooglePlayServices.PlayServicesResolver, Google.JarResolver");

        if (resolverType == null)
        {
            UnityEngine.Debug.LogError("EDM Resolver not found. Is External Dependency Manager installed?");
            return;
        }

        var method = resolverType.GetMethod(
            "Resolve",
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static
        );

        if (method == null)
        {
            UnityEngine.Debug.LogError("Resolve method not found on PlayServicesResolver.");
            return;
        }

        method.Invoke(null, null);

        UnityEngine.Debug.Log("EDM Android Resolve triggered successfully.");
    }
}
#endif
