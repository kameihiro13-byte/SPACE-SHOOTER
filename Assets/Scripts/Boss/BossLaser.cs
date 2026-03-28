using UnityEngine;

public class BossLaser : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float lifeTime = 4.0f;

    // 外部スクリプト（Boss本体など）から生成直後に発射方向を上書きするため public とする
    public Vector3 direction = Vector3.down;

    void Start()
    {
        // 画面外に飛んでいった弾がメモリを圧迫しないよう、一定時間後に自動破棄する
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        ProcessMovement();
    }

    /// <summary>
    /// 指定された方向への直線移動処理
    /// </summary>
    private void ProcessMovement()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}