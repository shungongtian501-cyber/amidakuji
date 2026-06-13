using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField]
    private Text mountainRankText;

    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text highScoreText;

    [SerializeField]
    private GameObject newRecordText;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip resultBGM;

    [SerializeField]
    private AudioClip newRecordSE;

    void Start()
    {

        // 現在スコア
        int currentScore =
            ScoreManager.score;

        scoreText.text =
            "SCORE : "
            + (currentScore * 10) + "m";

        int mountainHeight =
    currentScore * 10;

        mountainRankText.text =
    mountainHeight
    + "m\n"
    + "🏔 "
    + GetMountainRank(
        mountainHeight
    );
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

        // 表示更新
        highScoreText.text =
            "HIGH SCORE : "
            + (highScore * 10 )
            + "m";

        newRecordText.SetActive(
            isNewRecord
        );

        // 音再生
        if (isNewRecord)
        {
            StartCoroutine(
                PlayNewRecordFlow()
            );
        }
        else
        {
            audioSource.clip =
                resultBGM;

            audioSource.Play();
        }
    }

    IEnumerator PlayNewRecordFlow()
    {
        // 壮大SE再生
        audioSource.PlayOneShot(
            newRecordSE
        );

        // SE終わるまで待つ
        yield return new WaitForSeconds(
            newRecordSE.length
        );

        // BGM再生
        audioSource.clip =
            resultBGM;

        audioSource.loop = true;

        audioSource.Play();
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
    string GetMountainRank(int height)
    {
        if (height < 500)
        {
            return "散歩レベル";
        }
        else if (height < 1000)
        {
            return "高尾山級";
        }
        else if (height < 2000)
        {
            return "阿蘇山級";
        }
        else if (height < 3000)
        {
            return "北岳級";
        }
        else if (height < 3776)
        {
            return "富士山目前！";
        }
        else if (height < 5000)
        {
            return "富士山級";
        }
        else if (height < 6000)
        {
            return "キリマンジャロ級";
        }
        else if (height < 7000)
        {
            return "デナリ級";
        }
        else if (height < 8000)
        {
            return "アコンカグア級";
        }
        else if (height < 8849)
        {
            return "エベレスト目前！";
        }
        else if (height < 10000)
        {
            return "エベレスト級";
        }
        else if (height < 12000)
        {
            return "成層圏級";
        }
        else
        {
            return "宇宙到達";
        }
    }
}