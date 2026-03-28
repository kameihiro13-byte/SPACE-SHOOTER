using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Stage1WaveManager : MonoBehaviour
{
    [Header("エネミープレハブ設定")]
    [SerializeField] private GameObject childHp1Prefab;
    [SerializeField] private GameObject childHp2Prefab;
    [SerializeField] private GameObject parentNormalPrefab;
    [SerializeField] private GameObject parentRarePrefab;
    [SerializeField] private GameObject bossPrefab;

    [Header("編隊・登場演出設定")]
    [SerializeField] private float flyInSpeed = 8.0f;
    [SerializeField] private float swayWidth = 0.7f;

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
        // 第1ウェーブ：手慣らし
        // ==========================================
        yield return new WaitForSeconds(2.0f);

        GameObject anchor1 = new GameObject("Wave1_Anchor");
        anchor1.transform.position = new Vector3(0, 4.5f, 0);
        FormationAnchor fa1 = anchor1.AddComponent<FormationAnchor>();
        fa1.Initialize(10.0f, swayWidth); // 【変更点】メソッド経由で安全に設定値を渡す

        Vector3[] w1_hp1_pos = {
            new Vector3(-0.8f, -1.2f, 0f), new Vector3(0.0f, -1.2f, 0f), new Vector3(0.8f, -1.2f, 0f)
        };
        foreach (Vector3 pos in w1_hp1_pos)
        {
            SpawnAndFlyIn(childHp1Prefab, anchor1.transform, pos);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1.0f);
        SpawnAndFlyIn(parentNormalPrefab, anchor1.transform, Vector3.zero);

        // 敵の全滅監視（タイムアウト12秒）
        yield return StartCoroutine(WaitForWaveClear(12.0f));


        // ==========================================
        // 第2ウェーブ：5×3のブロック陣形
        // ==========================================
        GameObject anchor2 = new GameObject("Wave2_Anchor");
        anchor2.transform.position = new Vector3(0, 4.5f, 0);
        FormationAnchor fa2 = anchor2.AddComponent<FormationAnchor>();
        fa2.Initialize(20.0f, swayWidth);

        Vector3[] w2_hp1_pos = {
            new Vector3(-1.4f, -2.6f, 0f), new Vector3(-0.7f, -2.6f, 0f), new Vector3( 0.0f, -2.6f, 0f), new Vector3( 0.7f, -2.6f, 0f), new Vector3( 1.4f, -2.6f, 0f),
            new Vector3(-1.4f, -1.9f, 0f), new Vector3(-0.7f, -1.9f, 0f), new Vector3( 0.0f, -1.9f, 0f), new Vector3( 0.7f, -1.9f, 0f), new Vector3( 1.4f, -1.9f, 0f)
        };
        foreach (Vector3 pos in w2_hp1_pos)
        {
            SpawnAndFlyIn(childHp1Prefab, anchor2.transform, pos);
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.5f);

        Vector3[] w2_hp2_pos = {
            new Vector3(-1.4f, -1.2f, 0f), new Vector3(-0.7f, -1.2f, 0f), new Vector3( 0.0f, -1.2f, 0f), new Vector3( 0.7f, -1.2f, 0f), new Vector3( 1.4f, -1.2f, 0f)
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
        // 第3ウェーブ：レア親玉の最終陣形
        // ==========================================
        GameObject anchor3 = new GameObject("Wave3_Anchor");
        anchor3.transform.position = new Vector3(0, 4.5f, 0);
        FormationAnchor fa3 = anchor3.AddComponent<FormationAnchor>();
        fa3.Initialize(20.0f, swayWidth);

        foreach (Vector3 pos in w2_hp1_pos)
        {
            SpawnAndFlyIn(childHp1Prefab, anchor3.transform, pos);
            yield return new WaitForSeconds(0.15f);
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
        // ボス戦：WARNING演出＆BGM切り替え
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
        yield return new WaitForSeconds(initialSpawnWait);

        float timer = 0f;
        while (timer < maxWaitTime)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length == 0)
            {
                yield return new WaitForSeconds(2.0f);
                break;
            }

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
    /// ステージ1専用：上空から直線的に所定のフォーメーション位置へ降下する演出
    /// </summary>
    private IEnumerator FlyInRoutine(Transform obj, Vector3 targetLocalPos)
    {
        // 画面上部から出現させるための初期オフセット
        obj.localPosition = targetLocalPos + new Vector3(0, 10f, 0);

        while (obj != null && Vector3.Distance(obj.localPosition, targetLocalPos) > 0.01f)
        {
            obj.localPosition = Vector3.MoveTowards(obj.localPosition, targetLocalPos, flyInSpeed * Time.deltaTime);
            yield return null;
        }

        if (obj != null)
        {
            obj.localPosition = targetLocalPos;

            ChildEnemy child = obj.GetComponent<ChildEnemy>();
            if (child != null) child.StartShooting();

            ParentEnemy parent = obj.GetComponent<ParentEnemy>();
            if (parent != null) parent.StartShooting();
        }
    }
}

// ==========================================
// 編隊全体の揺れ（Sway）と画面外への退避を管理するアンカークラス
// ==========================================
public class FormationAnchor : MonoBehaviour
{
    private float swayWidth = 0.7f;
    private float swaySpeed = 2.0f;
    private float waitTime = 15.0f;
    private float escapeSpeed = 4.0f;

    private float startX;
    private float timer = 0f;

    /// <summary>
    /// マネージャーから生成された直後に設定値を流し込むための初期化メソッド
    /// </summary>
    public void Initialize(float wait, float sway)
    {
        this.waitTime = wait;
        this.swayWidth = sway;
    }

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < waitTime)
        {
            // サイン波を用いて左右の揺れ（Sway）を計算
            float newX = startX + Mathf.Sin(timer * swaySpeed) * swayWidth;
            transform.position = new Vector3(newX, transform.position.y, 0);
        }
        else
        {
            // 待機時間終了後、上方向へ一斉退避
            transform.position += Vector3.up * escapeSpeed * Time.deltaTime;
            if (transform.position.y > 15f)
            {
                Destroy(gameObject);
            }
        }
    }
}