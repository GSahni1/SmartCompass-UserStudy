using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;

public class SimpleFirestoreLeaderboard : MonoBehaviour
{
    public Transform content;
    public GameObject rowTemplate;

    FirebaseFirestore db;

    async void Start()
    {
        // If user exists, Firebase is already ready
        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.Log("No logged-in user. Leaderboard not loaded.");
            return;
        }

        db = FirebaseFirestore.DefaultInstance;

        QuerySnapshot snapshot = await db
            .Collection("leaderboard")
            .OrderByDescending("steps")
            .GetSnapshotAsync();

        int rank = 1;

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            GameObject row = Instantiate(rowTemplate, content);
            row.SetActive(true);

            string name = doc.ContainsField("displayName")
                ? doc.GetValue<string>("displayName")
                : "Player";

            int steps = doc.ContainsField("steps")
                ? doc.GetValue<int>("steps")
                : 0;

            row.GetComponent<TMP_Text>().text =
                $"#{rank}   {name}   {steps}";

            rank++;
        }
    }
}
