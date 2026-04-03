using UnityEngine;

/// <summary>
/// 2枚の背景画像を用いて無限スクロールを実現する制御クラス
/// </summary>
public class InfiniteBackgroundScroller : MonoBehaviour
{
    [Header("スクロール設定")]
    [SerializeField] private float scrollSpeed = 2.0f;

    // 内部データ
    private Transform[] backgrounds;
    private float imageHeight;

    // 定数
    private readonly int requiredBackgroundCount = 2;

    void Start()
    {
        InitializeBackgrounds();
    }

    void Update()
    {
        ProcessScrolling();
    }

    /// <summary>
    /// 背景オブジェクトの取得と初期配置、画像サイズの計算を行う
    /// </summary>
    private void InitializeBackgrounds()
    {
        // 自身の子オブジェクトから背景を取得
        backgrounds = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            backgrounds[i] = transform.GetChild(i);
        }

        // エラーチェック
        if (backgrounds.Length != requiredBackgroundCount)
        {
            Debug.LogError("[InfiniteBackgroundScroller] 子オブジェクトの数が不正です。背景画像を2枚配置してください。");
            return;
        }

        SpriteRenderer sr = backgrounds[0].GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[InfiniteBackgroundScroller] 背景オブジェクトに SpriteRenderer が見つかりません。");
            return;
        }

        // 画像の縦幅を取得
        imageHeight = sr.bounds.size.y;

        // 1枚目を原点に、2枚目をその直上に配置してシームレスに繋ぐ
        backgrounds[0].localPosition = Vector3.zero;
        backgrounds[1].localPosition = new Vector3(0, imageHeight, 0);
    }

    /// <summary>
    /// 背景の下方向への移動と、画面外に出た際のループ処理を実行する
    /// </summary>
    private void ProcessScrolling()
    {
        // エラー発生時は処理を中断する（NullReferenceException防止）
        if (backgrounds == null || backgrounds.Length != requiredBackgroundCount) return;

        foreach (Transform bg in backgrounds)
        {
            // 背景の移動処理
            bg.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

            // 画面外への流出判定と位置の再設定（ループ処理）
            if (bg.localPosition.y < -imageHeight)
            {
                bg.localPosition += new Vector3(0, imageHeight * requiredBackgroundCount, 0);
            }
        }
    }
}
