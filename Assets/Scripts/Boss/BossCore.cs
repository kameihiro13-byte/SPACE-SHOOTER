using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossCore : MonoBehaviour
{
    [Header("本体ステータス設定")]
    [SerializeField] private int maxHp = 30;
    [SerializeField] private int scoreValue = 10000;

    [Header("連携システム")]
    [SerializeField] private List<BossPart> activeParts;
    [SerializeField] private GameObject shieldEffect;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;

    [Header("① 自機狙い弾（通常攻撃）")]
    [SerializeField] private GameObject aimBulletPrefab;
    [SerializeField] private float shootInterval = 0.8f;

    [Header("② 極太センターレーザー（特殊攻撃）")]
    [SerializeField] private GameObject centerWarningLine;
    [SerializeField] private GameObject centerMainLaser;
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private float laserTime = 2.0f;

    [Header("③ 移動設定")]
    [SerializeField] private float normalMoveSpeed = 2.0f;
    [SerializeField] private float angryMoveSpeed = 3.5f;
    [SerializeField] private float moveRange = 3.0f;

    private int currentHp;
    private float shootTimer = 0f;
    private float moveDir = 1f;

    private bool isShieldActive = true;
    private bool isLaserActive = false;
    private bool hasFired50Laser = false;
    private bool hasFired25Laser = false;
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

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        if (gameManager != null)
        {
            gameManager.ShowBossHP();
            gameManager.UpdateBossHP(1.0f);
        }

        if (shieldEffect != null) shieldEffect.SetActive(true);
        if (centerWarningLine != null) centerWarningLine.SetActive(false);
        if (centerMainLaser != null) centerMainLaser.SetActive(false);

        transform.position = new Vector3(0f, 4.0f, 0f);

        // パーツが未登録の場合は最初からシールドを解除する（フェイルセーフ）
        if (activeParts.Count <= 0)
        {
            isShieldActive = false;
            if (shieldEffect != null) shieldEffect.SetActive(false);
        }
    }

    void Update()
    {
        if (isDead) return;

        ProcessMovement();
        ProcessAttack();
    }

    /// <summary>
    /// ボス本体の移動処理
    /// </summary>
    private void ProcessMovement()
    {
        if (isLaserActive) return;

        float currentSpeed = isShieldActive ? normalMoveSpeed : angryMoveSpeed;
        transform.Translate(Vector3.right * moveDir * currentSpeed * Time.deltaTime);

        if (transform.position.x >= moveRange) moveDir = -1f;
        if (transform.position.x <= -moveRange) moveDir = 1f;
    }

    /// <summary>
    /// 通常攻撃（自機狙い弾）の進行管理
    /// </summary>
    private void ProcessAttack()
    {
        if (isShieldActive || isLaserActive || player == null || !player.gameObject.activeInHierarchy) return;

        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            ShootAimBullet();
            shootTimer = 0f;
        }
    }

    /// <summary>
    /// 自機を狙う弾を発射する
    /// </summary>
    private void ShootAimBullet()
    {
        if (aimBulletPrefab != null)
        {
            Vector3 diff = player.position - transform.position;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Instantiate(aimBulletPrefab, transform.position, rotation);
        }
    }

    /// <summary>
    /// 部位破壊時に外部（BossPart）から呼ばれるメソッド
    /// </summary>
    public void OnPartDestroyed(BossPart part)
    {
        if (activeParts.Contains(part))
        {
            activeParts.Remove(part);
        }

        if (activeParts.Count <= 0)
        {
            isShieldActive = false;
            if (shieldEffect != null) shieldEffect.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            collision.gameObject.SetActive(false);

            if (isShieldActive) return;

            TakeDamage(1);
        }
    }

    /// <summary>
    /// ダメージ処理と特殊行動（極太レーザー）のトリガー判定
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

            // HP減少に伴う特殊攻撃のトリガー判定
            if (!hasFired50Laser && currentHp <= (maxHp * 0.5f))
            {
                hasFired50Laser = true;
                StartCoroutine(FireLaserRoutine());
            }
            else if (!hasFired25Laser && currentHp <= (maxHp * 0.25f))
            {
                hasFired25Laser = true;
                StartCoroutine(FireLaserRoutine());
            }
        }
        else
        {
            isDead = true;
            StopAllCoroutines();
            StartCoroutine(DeathRoutine());
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
    /// 中央へ移動し、極太レーザーを発射する一連の処理
    /// </summary>
    private IEnumerator FireLaserRoutine()
    {
        isLaserActive = true;

        Vector3 centerPos = new Vector3(0f, 4.0f, 0f);
        float currentSpeed = isShieldActive ? normalMoveSpeed : angryMoveSpeed;

        // 中央位置へ移動
        while (Vector3.Distance(transform.position, centerPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerPos, currentSpeed * Time.deltaTime);
            yield return null; // 1フレーム待機
        }
        transform.position = centerPos;

        yield return new WaitForSeconds(0.5f);

        // 予告線の表示
        if (centerWarningLine != null) centerWarningLine.SetActive(true);
        yield return new WaitForSeconds(warningTime);

        // レーザーの発射
        if (centerWarningLine != null) centerWarningLine.SetActive(false);
        if (centerMainLaser != null) centerMainLaser.SetActive(true);
        yield return new WaitForSeconds(laserTime);

        // レーザー終了
        if (centerMainLaser != null) centerMainLaser.SetActive(false);

        isLaserActive = false;
    }

    /// <summary>
    /// ボス撃破時の演出とクリア処理
    /// </summary>
    private IEnumerator DeathRoutine()
    {
        // アクティブなレーザー・予告線を強制的に非表示にする
        if (centerWarningLine != null) centerWarningLine.SetActive(false);
        if (centerMainLaser != null) centerMainLaser.SetActive(false);

        // 画面上の敵弾を一掃する
        GameObject[] attacks = GameObject.FindGameObjectsWithTag("EnemyLaser");
        foreach (GameObject attack in attacks)
        {
            Destroy(attack);
        }

        // 連続爆発演出
        float timer = 0f;
        while (timer < 4.0f)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2.0f;
            Vector3 expPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            if (explosionPrefab != null) Instantiate(explosionPrefab, expPos, Quaternion.identity);
            if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, expPos);

            timer += 0.15f;
            yield return new WaitForSeconds(0.15f);
        }

        // 最終の大爆発演出
        if (explosionPrefab != null)
        {
            GameObject finalExp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            finalExp.transform.localScale = new Vector3(3f, 3f, 3f);
        }
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        if (spriteRenderer != null) spriteRenderer.enabled = false;

        // クリア処理への移行待機
        yield return new WaitForSeconds(1.5f);

        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
            gameManager.StageClear();
        }

        Destroy(gameObject);
    }
}