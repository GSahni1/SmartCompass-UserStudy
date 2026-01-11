using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;

public class FirestoreLeaderboardController : MonoBehaviour
{
    public Transform content;
    public GameObject rowTemplate;

    FirebaseFirestore db;

    class Player
    {
        public string name;
        public int steps;
    }

    async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        await LoadLeaderboard();
    }

    async System.Threading.Tasks.Task LoadLeaderboard()
    {
        QuerySnapshot snapshot = await db
            .Collection("leaderboard")
            .OrderByDescending("steps")
            .GetSnapshotAsync();

        int rank = 1;

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            GameObject row = Instantiate(rowTemplate, content);
            row.SetActive(true);

            Transform back = row.transform.Find("back");

            TMP_Text rankText  = back.Find("RankText").GetComponent<TMP_Text>();
            TMP_Text nameText  = back.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text scoreText = back.Find("ScoreText").GetComponent<TMP_Text>();

            string name = doc.ContainsField("displayName")
                ? doc.GetValue<string>("displayName")
                : "Player";

            int steps = doc.ContainsField("steps")
                ? doc.GetValue<int>("steps")
                : 0;

            rankText.text  = "#" + rank;
            nameText.text  = name;
            scoreText.text = steps.ToString();

            rank++;
        }
    }
}
