using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // コルーチンを使うために追加

public class GameManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private TextMeshProUGUI scoreTextUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject clearText;
    [SerializeField] private GameObject bossHpUI;
    [SerializeField] private Image bossHpFill;

    [Header("ステージ進行設定")]
    [SerializeField] private int stageNumber = 1;
    [SerializeField] private string nextSceneName = "";

    [Header("遷移ディレイ設定")]
    [SerializeField] private float clearTransitionDelay = 3.0f;
    [SerializeField] private float gameOverDisplayDelay = 1.5f;

    // 内部データ
    private int currentScore = 0; // 画面に表示するトータルスコア
    private int stageScore = 0;   // 現在のステージのみで獲得したスコア
    private bool isGameOver = false;

    void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// ステージ開始時のデータ引き継ぎとUIの初期化
    /// </summary>
    private void InitializeGame()
    {
        // 前のステージからのトータルスコアを引き継ぐ
        currentScore = GameData.totalScore;
        UpdateScoreDisplay();

        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (clearText != null) clearText.SetActive(false);
        if (bossHpUI != null) bossHpUI.SetActive(false);
    }

    /// <summary>
    /// 敵撃破時などに外部から呼ばれるスコア加算処理
    /// </summary>
    public void AddScore(int amount)
    {
        if (isGameOver) return;

        currentScore += amount;
        stageScore += amount;

        // 加算と同時にグローバルなデータ領域にも保存しておく
        GameData.totalScore = currentScore;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreTextUI != null)
        {
            scoreTextUI.text = "SCORE: " + currentScore.ToString();
        }
    }

    /// <summary>
    /// ボス撃破時に呼ばれるステージクリア処理
    /// </summary>
    public void StageClear()
    {
        if (clearText != null) clearText.SetActive(true);
        if (bossHpUI != null) bossHpUI.SetActive(false);

        // リザルト画面で個別に表示できるよう、ステージ単体のスコアを保存する
        if (stageNumber == 1) ScoreData.stage1Score = stageScore;
        else if (stageNumber == 2) ScoreData.stage2Score = stageScore;
        else if (stageNumber == 3) ScoreData.stage3Score = stageScore;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            StartCoroutine(TransitionToNextStageRoutine());
        }
    }

    private IEnumerator TransitionToNextStageRoutine()
    {
        yield return new WaitForSeconds(clearTransitionDelay);
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// プレイヤー被弾時に呼ばれるゲームオーバー処理
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;

        // コンティニュー時のデータ不整合を防ぐため、進行データを初期化する
        GameData.ResetData();

        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDisplayDelay);

        if (gameOverUI != null) gameOverUI.SetActive(true);
        Time.timeScale = 0f; // ゲーム進行の完全停止
    }

    /// <summary>
    /// ゲームオーバー画面のボタン等から呼ばれるリトライ処理
    /// </summary>
    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ==========================================
    // ボスHP表示関連
    // ==========================================

    public void ShowBossHP()
    {
        if (bossHpUI != null) bossHpUI.SetActive(true);
    }

    public void UpdateBossHP(float percentage)
    {
        if (bossHpFill != null) bossHpFill.fillAmount = percentage;
    }
}
