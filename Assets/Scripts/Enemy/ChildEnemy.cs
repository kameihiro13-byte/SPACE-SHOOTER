using UnityEngine;
using System.Collections;

public class ChildEnemy : MonoBehaviour
{
    [Header("ステータス設定")]
    [SerializeField] private int maxHp = 1;
    [SerializeField] private int scoreValue = 50;

    [Header("演出設定")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;

    [Header("攻撃設定（通常弾）")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float minFireRate = 2.0f;
    [SerializeField] private float maxFireRate = 4.0f;
    [SerializeField] private AudioClip shootSound;

    [Header("特殊行動（カミカゼ）設定")]
    [SerializeField] private bool willAttack = false;
    [SerializeField] private float attackDelay = 6.0f;
    [SerializeField] private float rightSideDelayOffset = 3.0f;
    [SerializeField] private float attackSpeed = 3.0f;
    [SerializeField] private float spinTime = 1.5f;

    // 画面外判定・消滅判定用の境界値（マジックナンバーを排除）
    private readonly float screenBoundX = 4.5f;
    private readonly float screenBoundY = 6.5f;
    private readonly float destroyBoundY = -15.0f;

    private int currentHp;
    private bool isAttacking = false;
    private Color originalColor;

    // キャッシュ用変数
    private Transform player;
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

        FindPlayer();
    }

    /// <summary>
    /// プレイヤーのTransformを検索してキャッシュする
    /// </summary>
    private void FindPlayer()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj == null) pObj = GameObject.Find("Player");
        if (pObj != null) player = pObj.transform;
    }

    /// <summary>
    /// オブジェクトが画面内に存在するかどうかの判定
    /// </summary>
    private bool IsInScreen()
    {
        return transform.position.y < screenBoundY && transform.position.y > -screenBoundY &&
               transform.position.x < screenBoundX && transform.position.x > -screenBoundX;
    }

    /// <summary>
    /// 外部（親機やSpawner）から呼ばれる行動開始のトリガー
    /// </summary>
    public void StartShooting()
    {
        StartCoroutine(ShootRoutine());
        if (willAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    /// <summary>
    /// ランダム間隔での通常弾の発射処理
    /// </summary>
    private IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2.0f));

        while (true)
        {
            if (IsInScreen() && bulletPrefab != null)
            {
                Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                if (shootSound != null)
                {
                    AudioSource.PlayClipAtPoint(shootSound, transform.position);
                }
            }
            yield return new WaitForSeconds(Random.Range(minFireRate, maxFireRate));
        }
    }

    /// <summary>
    /// 待機後、回転しながらプレイヤーへ突撃する特攻処理
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        float finalDelay = attackDelay;

        // 画面右側にいる場合は突撃タイミングを遅らせ、波状攻撃にする
        if (transform.position.x > 0.1f)
        {
            finalDelay += rightSideDelayOffset;
        }

        yield return new WaitForSeconds(finalDelay);

        if (!IsInScreen()) yield break;

        // 編隊（親オブジェクト）の移動から切り離し、単独行動に移行
        transform.SetParent(null);
        isAttacking = true;

        // 回転演出の計算
        float timer = 0f;
        Vector3 startPos = transform.position;
        float spinDir = startPos.x < 0 ? -1f : 1f;
        float radius = 1.0f;

        while (timer < spinTime)
        {
            timer += Time.deltaTime;
            float t = timer / spinTime;

            float angle = t * 360f * Mathf.Deg2Rad;

            float x = (startPos.x + (radius * spinDir)) - Mathf.Cos(angle) * radius * spinDir;
            float y = startPos.y + Mathf.Sin(angle) * radius;

            transform.position = new Vector3(x, y, 0);
            transform.rotation = Quaternion.Euler(0, 0, t * 360f * -spinDir);

            yield return null;
        }

        // 突撃対象の再確認と方向計算
        if (player == null) FindPlayer();
        Vector3 lockedTargetPos = player != null ? player.position : new Vector3(transform.position.x, destroyBoundY, 0);

        Vector3 diveDir = (lockedTargetPos - transform.position).normalized;
        float lookAngle = Mathf.Atan2(diveDir.y, diveDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, lookAngle);

        // 画面外に完全に出るまで突撃を継続
        while (transform.position.y > destroyBoundY && Mathf.Abs(transform.position.x) < 15f)
        {
            transform.position += diveDir * attackSpeed * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
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