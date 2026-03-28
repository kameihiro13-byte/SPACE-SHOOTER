using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("射撃基本設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 40;
    [SerializeField] private AudioClip shootSound;

    [Header("武器レベル・連射間隔設定")]
    [SerializeField] private float level1ShootInterval = 0.15f;
    [SerializeField] private float level2ShootInterval = 0.10f;

    // 弾の発射位置オフセット（マジックナンバーの排除）
    private readonly Vector3 singleShotOffset = new Vector3(0f, 0.5f, 0f);
    private readonly Vector3 doubleShotOffsetLeft = new Vector3(-0.3f, 0.5f, 0f);
    private readonly Vector3 doubleShotOffsetRight = new Vector3(0.3f, 0.5f, 0f);

    private List<GameObject> bulletPool;
    private int weaponLevel = 1;
    private float shootTimer = 0f;

    void Start()
    {
        weaponLevel = GameData.weaponLevel;
        InitializeObjectPool();
    }

    void Update()
    {
        ProcessShootingInput();
    }

    /// <summary>
    /// オブジェクトプール（弾の事前生成とキャッシュ）の初期化
    /// </summary>
    private void InitializeObjectPool()
    {
        bulletPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            bulletPool.Add(obj);
        }
    }

    /// <summary>
    /// キー入力の検知と連射間隔（クーリングタイム）の管理
    /// </summary>
    private void ProcessShootingInput()
    {
        shootTimer += Time.deltaTime;

        float currentInterval = (weaponLevel == 1) ? level1ShootInterval : level2ShootInterval;

        if (Input.GetKey(KeyCode.Space) && shootTimer >= currentInterval)
        {
            ExecuteShoot();
            shootTimer = 0f;
        }
    }

    /// <summary>
    /// 武器レベルに応じた弾の発射処理とサウンド再生
    /// </summary>
    private void ExecuteShoot()
    {
        bool hasShot = false;

        if (weaponLevel == 1)
        {
            hasShot = FireSingleShot();
        }
        else if (weaponLevel >= 2)
        {
            hasShot = FireDoubleShot();
        }

        if (hasShot && shootSound != null && Camera.main != null)
        {
            // 連射時に音が途切れないよう、PlayClipAtPointを使用
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, 0.4f);
        }
    }

    private bool FireSingleShot()
    {
        GameObject bullet = GetBulletFromPool();
        if (bullet != null)
        {
            bullet.transform.position = transform.position + singleShotOffset;
            bullet.SetActive(true);
            return true;
        }
        return false;
    }

    private bool FireDoubleShot()
    {
        bool shotFired = false;

        GameObject bullet1 = GetBulletFromPool();
        if (bullet1 != null)
        {
            bullet1.transform.position = transform.position + doubleShotOffsetLeft;
            bullet1.SetActive(true);
            shotFired = true;
        }

        GameObject bullet2 = GetBulletFromPool();
        if (bullet2 != null)
        {
            bullet2.transform.position = transform.position + doubleShotOffsetRight;
            bullet2.SetActive(true);
            shotFired = true;
        }

        return shotFired;
    }

    /// <summary>
    /// プール内から非アクティブ（未使用）な弾オブジェクトを検索して返す
    /// </summary>
    private GameObject GetBulletFromPool()
    {
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].activeInHierarchy)
            {
                return bulletPool[i];
            }
        }
        return null; // プールが枯渇している場合
    }

    /// <summary>
    /// アイテム取得時に外部から呼ばれる武器のパワーアップ処理
    /// </summary>
    public void PowerUpWeapon()
    {
        weaponLevel = 2;
        GameData.weaponLevel = weaponLevel;
    }
}