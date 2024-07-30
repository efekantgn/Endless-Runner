using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadGameScene(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
