using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("サウンド設定")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip clearSound;

    [Header("遷移設定")]
    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private float transitionDelay = 0.3f;

    [Header("ランク基準スコア")]
    [SerializeField] private int rankS_Threshold = 50000;
    [SerializeField] private int rankA_Threshold = 30000;
    [SerializeField] private int rankB_Threshold = 10000;

    private AudioSource audioSource;
    private bool isTransitioning = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        PlayClearSound();
        ProcessScoreAndRank();
    }

    /// <summary>
    /// リザルト画面表示時のクリア音再生
    /// </summary>
    private void PlayClearSound()
    {
        if (clearSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clearSound);
        }
    }

    /// <summary>
    /// スコアの集計とUIへの反映、ランクの計算を行う
    /// </summary>
    private void ProcessScoreAndRank()
    {
        int totalScore = ScoreData.stage1Score + ScoreData.stage2Score + ScoreData.stage3Score;

        if (scoreText != null)
        {
            scoreText.text = "TOTAL : " + totalScore.ToString();
        }

        if (rankText != null)
        {
            DetermineRank(totalScore);
        }
    }

    /// <summary>
    /// 合計スコアに基づいたランク判定とUIの色設定
    /// </summary>
    private void DetermineRank(int totalScore)
    {
        if (totalScore >= rankS_Threshold)
        {
            rankText.text = "RANK: S (Perfect!!)";
            rankText.color = Color.yellow;
        }
        else if (totalScore >= rankA_Threshold)
        {
            rankText.text = "RANK: A";
            rankText.color = new Color(1f, 0.5f, 0f); // オレンジ
        }
        else if (totalScore >= rankB_Threshold)
        {
            rankText.text = "RANK: B";
            rankText.color = Color.green;
        }
        else
        {
            rankText.text = "RANK: C";
            rankText.color = Color.gray;
        }
    }

    /// <summary>
    /// タイトルへ戻るボタン押下時にUIから呼び出されるメソッド
    /// </summary>
    public void GoToTitle()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(TransitionToTitleRoutine());
    }

    /// <summary>
    /// SE再生後、スコアをリセットしてタイトル画面へ遷移する
    /// </summary>
    private IEnumerator TransitionToTitleRoutine()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // 次のプレイに向けてスコアデータを初期化
        ScoreData.stage1Score = 0;
        ScoreData.stage2Score = 0;
        ScoreData.stage3Score = 0;

        yield return new WaitForSeconds(transitionDelay);

        SceneManager.LoadScene(titleSceneName);
    }
}