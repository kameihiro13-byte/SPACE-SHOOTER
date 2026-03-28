using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float baseSpeed = 5.0f;
    [SerializeField] private float speedDownDuration = 4.0f;

    [Header("見た目（バリア）設定")]
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite shieldSprite;

    [Header("演出・サウンド設定")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;

    // 移動制限用の境界値（マジックナンバーの排除）
    private readonly float boundX = 4.0f;
    private readonly float boundY = 8.0f;

    private float currentSpeed;
    private bool hasShield = false;

    // キャッシュ用変数
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    void Start()
    {
        currentSpeed = baseSpeed;

        // 処理負荷対策：毎フレームや衝突時に探さないようStartでキャッシュする
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();

        if (originalSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = originalSprite;
        }

        // 前のステージからのバリア状態の引き継ぎ
        if (GameData.hasShield)
        {
            ActivateShield();
        }
    }

    void Update()
    {
        ProcessMovement();
    }

    /// <summary>
    /// キー入力に応じたプレイヤーの移動と画面外への飛び出し防止処理
    /// </summary>
    private void ProcessMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(x, y, 0).normalized;
        transform.position += moveDirection * currentSpeed * Time.deltaTime;

        // 画面の境界値を超えないように座標をクランプする
        float clampedX = Mathf.Clamp(transform.position.x, -boundX, boundX);
        float clampedY = Mathf.Clamp(transform.position.y, -boundY, boundY);
        transform.position = new Vector3(clampedX, clampedY, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("EnemyLaser"))
        {
            if (hasShield)
            {
                BreakShield(collision.gameObject);
            }
            else
            {
                ProcessDeath();
            }
        }
    }

    /// <summary>
    /// バリア所持時の被弾処理（バリアの解除と敵弾の無効化）
    /// </summary>
    private void BreakShield(GameObject hitObject)
    {
        hasShield = false;
        GameData.hasShield = false;

        if (originalSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = originalSprite;
        }

        if (hitObject.CompareTag("EnemyLaser"))
        {
            Destroy(hitObject);
        }
    }

    /// <summary>
    /// プレイヤーの死亡（ゲームオーバー）処理
    /// </summary>
    private void ProcessDeath()
    {
        if (explosionSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (gameManager != null)
        {
            gameManager.GameOver();
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// アイテム取得時に外部から呼ばれるバリアの展開処理
    /// </summary>
    public void ActivateShield()
    {
        if (!hasShield)
        {
            hasShield = true;
            GameData.hasShield = true;

            if (shieldSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = shieldSprite;
            }
        }
    }

    /// <summary>
    /// デバフアイテム取得時に外部から呼ばれる速度低下処理
    /// </summary>
    public IEnumerator SpeedDownRoutine()
    {
        currentSpeed = baseSpeed / 2f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.8f, 0.5f, 1f); // 視覚的なデバフ表現（紫色）
        }

        yield return new WaitForSeconds(speedDownDuration);

        currentSpeed = baseSpeed;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
}