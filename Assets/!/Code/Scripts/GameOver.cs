using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameoverCanvas;
    public void StopGame(int score)
    {
        gameoverCanvas.SetActive(true);
    }
    public void RestartLevel()
    {

    }
    public void SubmitScore()
    {

    }
    public void AddXP(int score)
    {

    }
}