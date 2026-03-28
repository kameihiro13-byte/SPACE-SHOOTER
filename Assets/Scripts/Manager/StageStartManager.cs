using UnityEngine;
using System.Collections;
using TMPro;

public class StageStartManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TextMeshProUGUI stageTextUI;
    [SerializeField] private string stageName = "STAGE 1";

    [Header("演出設定")]
    [SerializeField] private float displayTime = 2.0f;

    void Start()
    {
        InitializeStageStart();
        StartCoroutine(WaitAndStartRoutine());
    }

    /// <summary>
    /// ステージ開始時のUI表示と、ゲーム進行の一時停止処理
    /// </summary>
    private void InitializeStageStart()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (stageTextUI != null) stageTextUI.text = stageName;

        // ゲーム内の時間経過と全てのオーディオを一時停止する
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    /// <summary>
    /// 指定時間待機後、ゲームの進行と音声を再開する
    /// </summary>
    private IEnumerator WaitAndStartRoutine()
    {
        // Time.timeScale = 0 の影響を受けないよう、現実時間（Realtime）で待機する
        yield return new WaitForSecondsRealtime(displayTime);

        if (startPanel != null) startPanel.SetActive(false);

        // ゲーム内の時間経過とオーディオの再生を再開
        Time.timeScale = 1.0f;
        AudioListener.pause = false;
    }
}