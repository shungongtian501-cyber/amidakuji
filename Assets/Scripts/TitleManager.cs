using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // ルール説明へ
    public void Rule()
    {
        SceneManager.LoadScene("RuleScene");
    }

    // 直接ゲームへ
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // 終了
    public void QuitGame()
    {
        Application.Quit();
    }
}