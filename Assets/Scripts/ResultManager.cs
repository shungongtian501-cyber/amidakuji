using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public void Retry()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}