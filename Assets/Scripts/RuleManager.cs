using UnityEngine;
using UnityEngine.SceneManagement;

public class RuleManager : MonoBehaviour
{
    // ゲーム開始
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // タイトルへ戻る
    public void BackTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}