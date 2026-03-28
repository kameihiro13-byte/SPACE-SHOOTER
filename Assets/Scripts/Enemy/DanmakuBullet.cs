using UnityEngine;

public class DanmakuBullet : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float speed = 4.0f;

    // 画面外判定用の境界値（マジックナンバーを排除）
    private readonly float boundX = 8.0f;
    private readonly float boundY = 10.0f;

    void Update()
    {
        ProcessMovement();
        CheckOutOfBounds();
    }

    /// <summary>
    /// 自身の向いている方向への直進移動処理
    /// </summary>
    private void ProcessMovement()
    {
        transform.position += transform.up * speed * Time.deltaTime;
    }

    /// <summary>
    /// 画面外に出た際のオブジェクト破棄処理
    /// </summary>
    private void CheckOutOfBounds()
    {
        if (transform.position.y < -boundY || transform.position.y > boundY ||
            transform.position.x < -boundX || transform.position.x > boundX)
        {
            Destroy(gameObject);
        }
    }
}