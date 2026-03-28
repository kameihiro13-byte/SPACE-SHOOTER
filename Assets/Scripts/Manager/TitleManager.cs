using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("シーン遷移設定")]
    [SerializeField] private string nextSceneName = "GameScene";
    [SerializeField] private float transitionDelay = 0.5f;

    [Header("サウンド設定")]
    [SerializeField] private AudioClip startSound;

    // ボタンの連打による多重ロードを防ぐためのフラグ
    private bool isTransitioning = false;

    /// <summary>
    /// スタートボタン押下時にUIから呼び出されるメソッド
    /// </summary>
    public void GameStart()
    {
        // 既に遷移処理が始まっていたら、2回目以降のクリックは無視する
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(TransitionRoutine());
    }

    /// <summary>
    /// 決定音を再生し、指定時間待機した後にシーンをロードする
    /// </summary>
    private IEnumerator TransitionRoutine()
    {
        if (startSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(startSound, Camera.main.transform.position);
        }

        yield return new WaitForSeconds(transitionDelay);

        SceneManager.LoadScene(nextSceneName);
    }
}