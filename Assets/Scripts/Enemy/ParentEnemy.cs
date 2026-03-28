using UnityEngine;
using System.Collections;

public class ParentEnemy : MonoBehaviour
{
    [Header("ステータス設定")]
    [SerializeField] private int maxHp = 5;
    [SerializeField] private int scoreValue = 300;

    [Header("攻撃設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 2.5f;
    [SerializeField] private AudioClip shootSound;

    [Header("演出・ドロップ設定")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private GameObject dropItemPrefab;

    [Header("特殊行動（逃亡）設定")]
    [SerializeField] private bool willEscape = false;
    [SerializeField] private float escapeTime = 5.0f;
    [SerializeField] private float escapeSpeed = 4.0f;

    // 画面外判定用の数値（マジックナンバーを排除し、管理しやすくする）
    private readonly float screenBoundX = 4.5f;
    private readonly float screenBoundY = 6.5f;
    private readonly float destroyPosY = 15f;

    private int currentHp;
    private bool isEscaping = false;
    private Color originalColor;

    // キャッシュ用変数
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    void Start()
    {
        currentHp = maxHp;

        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        ProcessEscapeMovement();
    }

    /// <summary>
    /// 外部（Spawner等）から呼ばれる行動開始のトリガー
    /// </summary>
    public void StartShooting()
    {
        StartCoroutine(ShootRoutine());
        if (willEscape)
        {
            StartCoroutine(EscapeRoutine());
        }
    }

    /// <summary>
    /// 逃亡時の上方向への移動と破棄処理
    /// </summary>
    private void ProcessEscapeMovement()
    {
        if (!isEscaping) return;

        transform.position += Vector3.up * escapeSpeed * Time.deltaTime;

        if (transform.position.y > destroyPosY)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 弾の一定間隔での発射処理
    /// </summary>
    private IEnumerator ShootRoutine()
    {
        while (!isEscaping)
        {
            yield return new WaitForSeconds(fireRate);

            if (!isEscaping && IsInScreen() && bulletPrefab != null)
            {
                Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                if (shootSound != null)
                {
                    AudioSource.PlayClipAtPoint(shootSound, transform.position);
                }
            }
        }
    }

    /// <summary>
    /// 一定時間経過後に逃亡状態へ移行する処理
    /// </summary>
    private IEnumerator EscapeRoutine()
    {
        yield return new WaitForSeconds(escapeTime);
        isEscaping = true;

        // 編隊（親オブジェクト）の移動から切り離し、単独で逃亡させる
        transform.SetParent(null);
    }

    /// <summary>
    /// オブジェクトが画面内に存在するかどうかの判定
    /// </summary>
    private bool IsInScreen()
    {
        return transform.position.y < screenBoundY && transform.position.y > -screenBoundY &&
               transform.position.x < screenBoundX && transform.position.x > -screenBoundX;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            if (!collision.gameObject.activeSelf) return;

            collision.gameObject.SetActive(false);
            TakeDamage(1);
        }
    }

    /// <summary>
    /// ダメージ処理と撃破判定
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
            if (gameManager != null) gameManager.AddScore(scoreValue);

            if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (dropItemPrefab != null) Instantiate(dropItemPrefab, transform.position, Quaternion.identity);

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
            spriteRenderer.color = originalColor;
        }
    }
}