using UnityEngine;
using System.Collections;

public class BossPart : MonoBehaviour
{
    [Header("ステータス設定")]
    [SerializeField] private int maxHp = 15;
    [SerializeField] private int scoreValue = 1000;

    [Header("連携設定")]
    [SerializeField] private BossCore core;

    [Header("通常攻撃（自機狙い弾）設定")]
    [SerializeField] private GameObject aimBulletPrefab;
    [SerializeField] private float shootInterval = 1.0f;

    [Header("特殊攻撃（レーザー）設定")]
    [SerializeField] private float laserCooldown = 6.0f;
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private float laserTime = 2.0f;
    [SerializeField] private GameObject warningLineObj;
    [SerializeField] private GameObject mainLaserObj;

    [Header("回転アニメーション設定")]
    [SerializeField] private float targetLaserAngle = -45f;
    [SerializeField] private float rotationSpeed = 150f;

    [Header("演出・サウンド設定")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;

    private int currentHp;
    private float shootTimer = 0f;
    private float originalAngle;
    private bool isLaserActive = false;

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

        if (warningLineObj != null) warningLineObj.SetActive(false);
        if (mainLaserObj != null) mainLaserObj.SetActive(false);

        originalAngle = transform.eulerAngles.z;

        StartCoroutine(LaserRoutine());
    }

    void Update()
    {
        ProcessAttack();
    }

    /// <summary>
    /// 通常攻撃（自機狙い弾）の進行管理
    /// </summary>
    private void ProcessAttack()
    {
        if (isLaserActive || player == null || !player.gameObject.activeInHierarchy) return;

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
    /// 回転、予告線、レーザー照射を制御する一連のルーチン
    /// </summary>
    private IEnumerator LaserRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(laserCooldown);

            isLaserActive = true;

            // レーザー発射角度への回転処理
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetLaserAngle);
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }

            // 予告線の表示
            if (warningLineObj != null) warningLineObj.SetActive(true);
            yield return new WaitForSeconds(warningTime);

            // レーザーの照射
            if (warningLineObj != null) warningLineObj.SetActive(false);
            if (mainLaserObj != null) mainLaserObj.SetActive(true);
            yield return new WaitForSeconds(laserTime);

            // レーザーの終了
            if (mainLaserObj != null) mainLaserObj.SetActive(false);

            // 元の角度への復帰処理
            Quaternion defaultRotation = Quaternion.Euler(0, 0, originalAngle);
            while (Quaternion.Angle(transform.rotation, defaultRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, defaultRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }

            isLaserActive = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            collision.gameObject.SetActive(false);
            TakeDamage(1);
        }
    }

    /// <summary>
    /// パーツのダメージ処理と破壊判定
    /// </summary>
    private void TakeDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp > 0)
        {
            StartCoroutine(DamageColorRoutine());
        }
        else
        {
            // 破壊時、本体（BossCore）へ通知を行う
            if (core != null) core.OnPartDestroyed(this);
            if (gameManager != null) gameManager.AddScore(scoreValue);

            if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
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
}