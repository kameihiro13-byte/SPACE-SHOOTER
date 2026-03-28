using UnityEngine;

public class ShieldVisual : MonoBehaviour
{
    [Header("バリアの演出設定")]
    [SerializeField] private float rotateSpeed = 30f;
    [SerializeField] private float pulseSpeed = 2.0f;   // 拡縮する速さ
    [SerializeField] private float pulseAmount = 0.1f;  // 拡縮する幅

    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        ProcessRotation();
        ProcessPulsing();
    }

    /// <summary>
    /// シールドを一定速度で回転させる
    /// </summary>
    private void ProcessRotation()
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    /// <summary>
    /// サイン波を使用して、シールドを滑らかに拡縮させる
    /// </summary>
    private void ProcessPulsing()
    {
        // 時間経過に応じて -1.0 ～ 1.0 に変化する値を作り、拡縮幅を掛ける
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        // 元の大きさに加算して適用する
        transform.localScale = baseScale + new Vector3(scaleOffset, scaleOffset, 0f);
    }
}