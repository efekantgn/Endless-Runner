using System;
using System.Collections;
using LootLocker.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TMP_InputField inputfield;
    [SerializeField] private TextMeshProUGUI leaderboardScoreText;
    [SerializeField] private TextMeshProUGUI leaderboardNameText;
    private int score = 0;
    private string leaderboardID = "23485";
    private int leaderboardTopCount = 10;

    public void StopGame(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        GetLeaderboard();
    }
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void SubmitScore()
    {
        StartCoroutine(SubmitScoreToLeaderbord());
    }

    private IEnumerator SubmitScoreToLeaderbord()
    {
        bool? nameSet = null;
        LootLockerSDKManager.SetPlayerName(inputfield.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("PlayerNameSetted");
                nameSet = true;
            }
            else
            {
                Debug.Log("PlayerNameError");
                nameSet = false;
            }
        });
        yield return new WaitUntil(() => nameSet.HasValue);

        //if (!nameSet.Value) yield break;
        bool? scoreSubmited = null;
        LootLockerSDKManager.SubmitScore("", score, leaderboardID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Score Submited ");
                scoreSubmited = true;
            }
            else
            {
                Debug.Log("Error on Submiting Score");
                scoreSubmited = false;
            }
        });
        yield return new WaitUntil(() => scoreSubmited.HasValue);

        if (!nameSet.Value) yield break;

        GetLeaderboard();
    }

    private void GetLeaderboard()
    {
        LootLockerSDKManager.GetScoreList(leaderboardID, leaderboardTopCount, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Leaderboard Updated.");
                string leaderboardName = "";
                string leaderboardScore = "";
                LootLockerLeaderboardMember[] members = response.items;
                for (int i = 0; i < members.Length; i++)
                {
                    LootLockerPlayer player = members[i].player;

                    if (player == null) continue;

                    if (player.name != "")
                    {
                        leaderboardName += player.name + "\n";
                    }
                    else
                    {
                        leaderboardName += player.id + "\n";
                    }
                    leaderboardScore += members[i].score + "\n";
                }
                leaderboardNameText.SetText(leaderboardName);
                leaderboardScoreText.SetText(leaderboardScore);
            }
            else
            {
                Debug.Log("Error on updating Leaderboard");

            }
        });
    }

    public void AddXP(int score)
    {

    }
}