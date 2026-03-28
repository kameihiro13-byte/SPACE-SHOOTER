using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    [Header("ˆÚ“®İ’è")]
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float lifeTime = 3.0f;

    void Start()
    {
        // ƒƒ‚ƒŠ‰ğ•ú‚Ì‚½‚ßAˆê’èŠÔŒo‰ßŒã‚É©“®”jŠü‚·‚é
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        ProcessMovement();
    }

    /// <summary>
    /// ‰º•ûŒü‚Ö‚Ì’¼üˆÚ“®ˆ—
    /// </summary>
    private void ProcessMovement()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
    }
}