using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("移動・生存設定")]
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float maxLifeTime = 3.0f;

    private float currentLifeTime = 0f;

    /// <summary>
    /// オブジェクトプールから再利用（アクティブ化）される直前に呼ばれる初期化処理
    /// </summary>
    private void OnEnable()
    {
        // プールから取り出されるたびに生存時間タイマーをリセットする
        currentLifeTime = 0f;
    }

    private void Update()
    {
        ProcessMovement();
        CheckLifeTime();
    }

    /// <summary>
    /// 上方向への直進移動処理
    /// </summary>
    private void ProcessMovement()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }

    /// <summary>
    /// 生存時間の管理と、制限時間到達時のプールへの返却（非アクティブ化）処理
    /// </summary>
    private void CheckLifeTime()
    {
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= maxLifeTime)
        {
            // Destroyではなく非アクティブ化することでプールに返却する
            gameObject.SetActive(false);
        }
    }
}