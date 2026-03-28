using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Stage2WaveManager : MonoBehaviour
{
    [Header("エネミープレハブ設定")]
    [SerializeField] private GameObject childHp1Prefab;
    [SerializeField] private GameObject childHp2Prefab;
    [SerializeField] private GameObject parentNormalPrefab;
    [SerializeField] private GameObject parentRarePrefab;
    [SerializeField] private GameObject bossPrefab;

    [Header("編隊・登場演出設定")]
    [SerializeField] private float flyInSpeed = 9.0f;
    [SerializeField] private float swayWidth = 1.0f;

    [Header("ボス登場演出設定")]
    [SerializeField] private GameObject warningUI;
    [SerializeField] private Image redScreen;
    [SerializeField] private AudioClip sirenSound;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bossBgm;

    // 定数（マジックナンバーの排除）
    private readonly float waveClearCheckInterval = 0.5f;
    private readonly float initialSpawnWait = 2.5f;
    private readonly Vector3 bossSpawnPos = new Vector3(0, 8.0f, 0f);

    void Start()
    {
        StartCoroutine(WaveTimelineRoutine());
    }

    /// <summary>
    /// ステージ全体の進行（ウェーブ管理とボス登場）を制御するメインルーチン
    /// </summary>
    private IEnumerator WaveTimelineRoutine()
    {
        // ==========================================
        // 第1ウェーブ
        // ==========================================
        yield return new WaitForSeconds(2.0f);

        GameObject anchor1 = new GameObject("Wave2_1_Anchor");
        anchor1.transform.position = new Vector3(0, 4.5f, 0);
        FormationAnchor fa1 = anchor1.AddComponent<FormationAnchor>();

        // ★修正ポイント：変数を直接書き換えず、Initializeメソッド経由で値を渡す！
        fa1.Initialize(15.0f, swayWidth);

        Vector3[] w1_hp1_pos = {
            new Vector3(-2.5f, -1.5f, 0f), new Vector3(-1.8f, -1.0f, 0f), new Vector3(-1.1f, -1.5f, 0f),
            new Vector3( 2.5f, -1.5f, 0f), new Vector3( 1.8f, -1.0f, 0f), new Vector3( 1.1f, -1.5f, 0f)
        };
        foreach (Vector3 pos in w1_hp1_pos)
        {
            SpawnAndFlyIn(childHp1Prefab, anchor1.transform, pos);
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.5f);

        Vector3[] w1_hp2_pos = { new Vector3(-1.8f, 0.0f, 0f), new Vector3(1.8f, 0.0f, 0f) };
        foreach (Vector3 pos in w1_hp2_pos)
        {
            SpawnAndFlyIn(childHp2Prefab, anchor1.transform, pos);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1.0f);
        SpawnAndFlyIn(parentNormalPrefab, anchor1.transform, Vector3.zero);

        // 敵の全滅監視（タイムアウト18秒）
        yield return StartCoroutine(WaitForWaveClear(18.0f));


        // ==========================================
        // 第2ウェーブ
        // ==========================================
        GameObject anchor2 = new GameObject("Wave2_2_Anchor");
        anchor2.transform.position = new Vector3(0, 4.5f, 0);
        FormationAnchor fa2 = anchor2.AddComponent<FormationAnchor>();

        // ★修正ポイント
        fa2.Initialize(20.0f, swayWidth);

        Vector3[] w2_hp1_pos = {
            new Vector3(-2.0f, -2.0f, 0f), new Vector3(-1.0f, -2.5f, 0f), new Vector3(0.0f, -2.0f, 0f), new Vector3(1.0f, -2.5f, 0f), new Vector3(2.0f, -2.0f, 0f),
            new Vector3(-2.0f, -1.3f, 0f), new Vector3(-1.0f, -1.8f, 0f), new Vector3(0.0f, -1.3f, 0f), new Vector3(1.0f, -1.8f, 0f), new Vector3(2.0f, -1.3f, 0f)
        };
        foreach (Vector3 pos in w2_hp1_pos)
        {
            SpawnAndFlyIn(childHp1Prefab, anchor2.transform, pos);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        Vector3[] w2_hp2_pos = {
            new Vector3(-1.5f, -0.6f, 0f), new Vector3(-0.75f, -0.6f, 0f), new Vector3(0.0f, -0.6f, 0f), new Vector3(0.75f, -0.6f, 0f), new Vector3(1.5f, -0.6f, 0f)
        };
        foreach (Vector3 pos in w2_hp2_pos)
        {
            SpawnAndFlyIn(childHp2Prefab, anchor2.transform, pos);
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(1.0f);
        SpawnAndFlyIn(parentNormalPrefab, anchor2.transform, Vector3.zero);

        // 敵の全滅監視（タイムアウト25秒）
        yield return StartCoroutine(WaitForWaveClear(25.0f));


        // ==========================================
        // 第3ウェーブ
        // ==========================================
        GameObject anchor3 = new GameObject("Wave2_3_Anchor");
        anchor3.transform.position = new Vector3(0, 4.5f, 0);
        FormationAnchor fa3 = anchor3.AddComponent<FormationAnchor>();

        // ★修正ポイント
        fa3.Initialize(20.0f, swayWidth);

        foreach (Vector3 pos in w2_hp1_pos)
        {
            SpawnAndFlyIn(childHp1Prefab, anchor3.transform, pos);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        foreach (Vector3 pos in w2_hp2_pos)
        {
            SpawnAndFlyIn(childHp2Prefab, anchor3.transform, pos);
            yield return new WaitForSeconds(0.15f);
        }
        yield return new WaitForSeconds(1.0f);
        SpawnAndFlyIn(parentRarePrefab, anchor3.transform, Vector3.zero);

        // 敵の全滅監視（タイムアウト25秒）
        yield return StartCoroutine(WaitForWaveClear(25.0f));


        // ==========================================
        // ボス戦
        // ==========================================
        yield return StartCoroutine(WarningRoutine());

        if (bossPrefab != null)
        {
            Instantiate(bossPrefab, bossSpawnPos, Quaternion.identity);
        }
    }

    /// <summary>
    /// 画面内の敵の数を監視し、全滅した場合は最大待機時間をスキップして進行する
    /// </summary>
    private IEnumerator WaitForWaveClear(float maxWaitTime)
    {
        // 敵のスポーン演出中の誤判定を防ぐための初期待機
        yield return new WaitForSeconds(initialSpawnWait);

        float timer = 0f;
        while (timer < maxWaitTime)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length == 0)
            {
                // 次のウェーブへの移行インターバル
                yield return new WaitForSeconds(2.0f);
                break;
            }

            // 毎フレームの検索は負荷が高いため、一定間隔（0.5秒）で判定を行う
            timer += waveClearCheckInterval;
            yield return new WaitForSeconds(waveClearCheckInterval);
        }
    }

    /// <summary>
    /// ボス登場前の警告演出とBGMの切り替え
    /// </summary>
    private IEnumerator WarningRoutine()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (sirenSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(sirenSound, Camera.main.transform.position, 1.0f);
        }

        if (warningUI != null) warningUI.SetActive(true);
        if (redScreen != null) redScreen.gameObject.SetActive(true);

        for (int i = 0; i < 3; i++)
        {
            if (redScreen != null) redScreen.color = new Color(1f, 0f, 0f, 0.4f);
            yield return new WaitForSeconds(0.5f);
            if (redScreen != null) redScreen.color = new Color(1f, 0f, 0f, 0f);
            yield return new WaitForSeconds(0.5f);
        }

        if (redScreen != null) redScreen.gameObject.SetActive(false);
        if (warningUI != null) warningUI.SetActive(false);

        if (bgmSource != null && bossBgm != null)
        {
            bgmSource.clip = bossBgm;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 指定されたアンカーの子オブジェクトとして敵を生成し、登場アニメーションを開始する
    /// </summary>
    private void SpawnAndFlyIn(GameObject prefab, Transform anchor, Vector3 localPos)
    {
        GameObject obj = Instantiate(prefab, anchor.position, Quaternion.identity);
        obj.transform.SetParent(anchor);
        StartCoroutine(FlyInRoutine(obj.transform, localPos));
    }

    /// <summary>
    /// スパイラル軌道を描いて所定のフォーメーション位置へ移動する登場アニメーション
    /// </summary>
    private IEnumerator FlyInRoutine(Transform obj, Vector3 targetLocalPos)
    {
        float startX = targetLocalPos.x < 0 ? -15f : 15f;
        obj.localPosition = new Vector3(startX, -5f, 0);
        Vector3 startLocal = obj.localPosition;

        float time = 0;
        float duration = 1.5f;

        while (obj != null && time < duration)
        {
            time += Time.deltaTime * (flyInSpeed / 5f);
            float t = time / duration;
            if (t > 1f) t = 1f;

            Vector3 basePos = Vector3.Lerp(startLocal, targetLocalPos, t);

            // サイン波を用いてループ軌道（スパイラル）のオフセットを計算
            float loopAngle = t * Mathf.PI * 2f;
            float radius = Mathf.Sin(t * Mathf.PI) * 3.0f;
            float offsetX = Mathf.Cos(loopAngle) * radius;
            float offsetY = Mathf.Sin(loopAngle) * radius;

            obj.localPosition = basePos + new Vector3(offsetX, offsetY, 0);
            obj.Rotate(0, 0, 360f * Time.deltaTime * 1.5f);

            yield return null;
        }

        if (obj != null)
        {
            // アニメーション完了時に座標と回転を正確な値に補正する
            obj.localRotation = Quaternion.identity;
            obj.localPosition = targetLocalPos;

            // 攻撃開始のトリガーを送信
            ChildEnemy child = obj.GetComponent<ChildEnemy>();
            if (child != null) child.StartShooting();

            ParentEnemy parent = obj.GetComponent<ParentEnemy>();
            if (parent != null) parent.StartShooting();
        }
    }
}