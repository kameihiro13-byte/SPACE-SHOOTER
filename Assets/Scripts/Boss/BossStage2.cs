using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossStage2 : MonoBehaviour
{
    [Header("ステータス設定")]
    [SerializeField] private int maxHp = 30;
    [SerializeField] private int scoreValue = 8000;

    [Header("移動・攻撃設定")]
    [SerializeField] private GameObject bossLaserPrefab;
    [SerializeField] private Transform muzzlePos;
    [SerializeField] private float attackInterval = 3.0f;

    [Header("演出・サウンド設定")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip clearSound;
    [SerializeField] private string nextSceneName = "Stage3Scene";
    [SerializeField] private float clearWaitTime = 4.0f;

    private int currentHp;
    private float attackTimer = 0f;
    private bool isDead = false;

    // キャッシュ用変数
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    void Start()
    {
        currentHp = maxHp;

        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            gameManager.ShowBossHP();
            gameManager.UpdateBossHP(1.0f);
        }

        // プレイヤーのTransformをキャッシュ
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj == null) pObj = GameObject.Find("Player");
        if (pObj != null) player = pObj.transform;
    }

    void Update()
    {
        if (isDead) return;

        ProcessMovement();
        ProcessAttack();
    }

    /// <summary>
    /// ボスの8の字移動処理
    /// </summary>
    private void ProcessMovement()
    {
        // サイン波を組み合わせてリサージュ図形（8の字）の軌道を描画する
        float x = Mathf.Sin(Time.time * 1.5f) * 3f;
        float y = 3.5f + Mathf.Sin(Time.time * 3.0f) * 0.5f;
        transform.position = new Vector3(x, y, 0f);
    }

    /// <summary>
    /// ボスの攻撃パターンの進行管理
    /// </summary>
    private void ProcessAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            int attackPattern = Random.Range(0, 2);
            if (attackPattern == 0)
            {
                Fire3WayAim();
            }
            else
            {
                StartCoroutine(FireSpiralRoutine());
            }

            attackTimer = 0f;
        }
    }

    /// <summary>
    /// プレイヤーを狙う3方向弾を発射する
    /// </summary>
    private void Fire3WayAim()
    {
        Vector3 spawnPos = muzzlePos != null ? muzzlePos.position : transform.position;
        Vector3 targetPos = player != null ? player.position : spawnPos + Vector3.down;
        Vector3 dirToPlayer = (targetPos - spawnPos).normalized;

        float centerAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        float[] angles = { centerAngle - 20f, centerAngle, centerAngle + 20f };

        foreach (float ang in angles)
        {
            float rad = ang * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;

            GameObject laser = Instantiate(bossLaserPrefab, spawnPos, Quaternion.identity);

            BossLaser laserScript = laser.GetComponent<BossLaser>();
            if (laserScript != null)
            {
                laserScript.direction = dir;
            }
        }

        PlayShootSound();
    }

    /// <summary>
    /// 回転しながら連続で弾を発射する（スパイラル攻撃）
    /// </summary>
    private IEnumerator FireSpiralRoutine()
    {
        float currentAngle = 90f;
        int bulletCount = 20;

        for (int i = 0; i < bulletCount; i++)
        {
            if (isDead) break;

            Vector3 spawnPos = muzzlePos != null ? muzzlePos.position : transform.position;
            float radian = currentAngle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0).normalized;

            GameObject laser = Instantiate(bossLaserPrefab, spawnPos, Quaternion.identity);

            BossLaser laserScript = laser.GetComponent<BossLaser>();
            if (laserScript != null)
            {
                laserScript.direction = dir;
            }

            PlayShootSound();

            currentAngle += 18f;
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void PlayShootSound()
    {
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position, 0.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            if (!collision.gameObject.activeSelf) return;

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

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("EnemyLaser");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }

        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (clearSound != null) AudioSource.PlayClipAtPoint(clearSound, Camera.main.transform.position);

        yield return new WaitForSeconds(clearWaitTime);

        SceneManager.LoadScene(nextSceneName);
    }
}