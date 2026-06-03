using System.Collections;
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

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip resultBGM;

    [SerializeField]
    private AudioClip newRecordSE;

    void Start()
    {
        Debug.Log(
            "受け取った:"
            + ScoreManager.score
        );

        // 現在スコア
        int currentScore =
            ScoreManager.score;

        scoreText.text =
            "SCORE : "
            + currentScore;

        // 保存済みハイスコア
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
            + highScore;

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
}