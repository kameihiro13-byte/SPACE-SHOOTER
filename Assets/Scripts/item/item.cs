using UnityEngine;

/// <summary>
/// 落下アイテムの挙動と、プレイヤー取得時の効果付与を管理するクラス
/// </summary>
public class Item : MonoBehaviour
{
    // ==========================================
    // ★【超重要】列挙型（enum）の定義
    // 数字ではなく「名前」で管理することで、圧倒的に読みやすくバグの出にくい設計になります
    // ==========================================
    public enum ItemType
    {
        WeaponPowerUp, // 武器の強化（ダブルショットなど）
        Shield         // バリアの展開
    }

    [Header("アイテム設定")]
    [SerializeField] private ItemType type = ItemType.WeaponPowerUp;
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private int scoreValue = 500;

    // 画面外判定用の境界値（マジックナンバーの排除）
    private readonly float destroyBoundY = -9.0f;

    // キャッシュ用変数
    private GameManager gameManager;

    void Start()
    {
        // アイテム取得時に毎回検索すると一瞬フリーズする原因になるため、Startでキャッシュする
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        ProcessMovement();
        CheckOutOfBounds();
    }

    /// <summary>
    /// 下方向への直進移動処理
    /// </summary>
    private void ProcessMovement()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    /// <summary>
    /// 画面外に出た際のオブジェクト破棄処理
    /// </summary>
    private void CheckOutOfBounds()
    {
        if (transform.position.y < destroyBoundY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 文字列比較（== "Player"）より軽量で安全なタグ比較（CompareTag）を使用する
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyItemEffect(collision.gameObject);

            if (gameManager != null)
            {
                gameManager.AddScore(scoreValue);
            }

            Destroy(gameObject);
        }
    }

    /// <summary>
    /// アイテムの種類（enum）に応じたプレイヤーへの効果付与処理
    /// </summary>
    private void ApplyItemEffect(GameObject playerObj)
    {
        // switch文とenumを組み合わせることで、条件分岐が非常に美しく可読性の高いものになる
        switch (type)
        {
            case ItemType.WeaponPowerUp:
                PlayerShooter shooter = playerObj.GetComponent<PlayerShooter>();
                if (shooter != null) shooter.PowerUpWeapon();
                break;

            case ItemType.Shield:
                PlayerController controller = playerObj.GetComponent<PlayerController>();
                if (controller != null) controller.ActivateShield();
                break;
        }
    }
}