using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour
{
    [Header("ステータス設定")]
    [SerializeField] private int maxHp = 20;
    [SerializeField] private int scoreValue = 5000;

    [Header("移動・攻撃設定")]
    [SerializeField] private GameObject bossLaserPrefab;
    [SerializeField] private Transform muzzlePos;
    [SerializeField] private float attackInterval = 2.0f;
    [SerializeField] private float moveRange = 6f;

    [Header("演出・サウンド設定")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip clearSound;
    [SerializeField] private string nextSceneName = "Stage2Scene";
    [SerializeField] private float clearWaitTime = 4.0f;

    private int currentHp;
    private float attackTimer = 0f;
    private bool isDead = false;

    // キャッシュ用変数
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    void Start()
    {
        currentHp = maxHp;

        // 毎フレームの検索負荷を避けるため、Start時にコンポーネントをキャッシュする
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            gameManager.ShowBossHP();
            gameManager.UpdateBossHP(1.0f);
        }
    }

    void Update()
    {
        // 死亡時は処理を行わない
        if (isDead) return;

        ProcessMovement();
        ProcessAttack();
    }

    /// <summary>
    /// ボスの左右移動処理
    /// </summary>
    private void ProcessMovement()
    {
        // Mathf.PingPongを使用して指定範囲内を往復移動させる
        float x = Mathf.PingPong(Time.time * 2f, moveRange) - (moveRange / 2f);
        transform.position = new Vector3(x, 3.0f, 0f);
    }

    /// <summary>
    /// ボスの攻撃パターンの進行管理
    /// </summary>
    private void ProcessAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            // 銃口が設定されていない場合は、自身の中心座標を基準にする
            Vector3 firePos = muzzlePos != null ? muzzlePos.position : transform.position;

            // ランダムに攻撃パターンを選択
            int attackPattern = Random.Range(0, 2);
            if (attackPattern == 0)
            {
                FireDoubleLaser(firePos);
            }
            else
            {
                FireOmniLaser(firePos);
            }

            attackTimer = 0f;
        }
    }

    /// <summary>
    /// 2発のレーザーを並行に発射する
    /// </summary>
    private void FireDoubleLaser(Vector3 firePos)
    {
        Vector3 leftPos = firePos + new Vector3(-1.0f, 0f, 0);
        Vector3 rightPos = firePos + new Vector3(1.0f, 0f, 0);

        Instantiate(bossLaserPrefab, leftPos, Quaternion.identity);
        Instantiate(bossLaserPrefab, rightPos, Quaternion.identity);

        PlayShootSound();
    }

    /// <summary>
    /// 全方位（円形）にレーザーを発射する
    /// </summary>
    private void FireOmniLaser(Vector3 firePos)
    {
        int bulletCount = 12;
        for (int i = 0; i < bulletCount; i++)
        {
            // 三角関数を使用して円周上の発射方向を計算する
            float angle = i * (360f / bulletCount);
            float radian = angle * Mathf.Deg2Rad;
            Vector3 fireDirection = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0).normalized;

            GameObject laser = Instantiate(bossLaserPrefab, firePos, Quaternion.identity);

            BossLaser laserScript = laser.GetComponent<BossLaser>();
            if (laserScript != null)
            {
                laserScript.direction = fireDirection;
            }
        }

        PlayShootSound();
    }

    private void PlayShootSound()
    {
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position, 0.8f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            collision.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ボスのダメージ処理とHP管理
    /// </summary>
    private void TakeDamage(int damage)
    {
        currentHp -= damage;

        if (gameManager != null)
        {
            float hpPercentage = (float)currentHp / maxHp;
            gameManager.UpdateBossHP(hpPercentage);
        }

        if (currentHp > 0)
        {
            StartCoroutine(DamageColorRoutine());
        }
        else
        {
            isDead = true;
            StartCoroutine(ClearRoutine());
        }
    }

    /// <summary>
    /// 被弾時の赤色点滅処理
    /// </summary>
    private IEnumerator DamageColorRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// ボス撃破時の演出とステージクリア処理
    /// </summary>
    private IEnumerator ClearRoutine()
    {
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
            gameManager.StageClear();

            AudioSource bgmSource = gameManager.GetComponent<AudioSource>();
            if (bgmSource != null) bgmSource.Stop();
        }

        // 画面に残っている敵の弾を一掃する
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("EnemyLaser");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }

        // 爆発演出とサウンドの再生
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // ボス自身の見た目と当たり判定を消す
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (clearSound != null) AudioSource.PlayClipAtPoint(clearSound, Camera.main.transform.position);

        yield return new WaitForSeconds(clearWaitTime);

        SceneManager.LoadScene(nextSceneName);
    }
}