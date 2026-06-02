using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text highScoreText;

    [SerializeField]
    private GameObject newRecordText;

    void Start()
    {
        Debug.Log(
        "受け取った:"
        + ScoreManager.score
    );
        // 現在スコア取得
        int currentScore =
            ScoreManager.score;

        // スコア表示
        scoreText.text =
            "SCORE : "
            + currentScore;

        // 保存済みハイスコア取得
        int highScore =
            PlayerPrefs.GetInt(
                "HighScore",
                0
            );

        bool isNewRecord = false;

        // 更新判定
        if (currentScore > highScore)
        {
            highScore =
                currentScore;

            PlayerPrefs.SetInt(
                "HighScore",
                highScore
            );

            PlayerPrefs.Save();

            isNewRecord = true;
        }

        // ハイスコア表示
        highScoreText.text =
            "HIGH SCORE : "
            + highScore;

        // NEW RECORD表示
        newRecordText.SetActive(
            isNewRecord
        );
    }

    public void Retry()
    {
        SceneManager.LoadScene(
            "GameScene"
        );
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene(
            "TitleScene"
        );
    }
}