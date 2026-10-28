using UnityEngine;

public class ShooterBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 10f;             // ความเร็วของกระสุนศัตรู
    public float damage = 15f;            // ดาเมจที่จะทำใส่ Wall หรือ Player
    public float destroyTime = 5f;        // ทำลายตัวเองตามเวลาถ้าไม่ชนอะไร

    [Header("Target Tags")]
    public string wallTag = "Wall";
    public string meleeTag = "Melee";
    [SerializeField]
    private GameObject explosion;
    [SerializeField]
    private GameObject gone;

    private void Start()
    {
        // ⏳ ทำลายตัวเองอัตโนมัติตามเวลา
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        // 🚀 พุ่งตรงไปตามทิศทางหัวกระสุน (Local Space Vector3.up)
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ⚔️ 1. สัมผัสโดน Melee ของ Player -> ทำลายกระสุนทิ้งทันที! (โดนฟันสลาย)
        if (other.CompareTag(meleeTag))
        {
            Debug.Log("Enemy Bullet destroyed by Player Melee!");
            Instantiate(gone,transform.position,transform.rotation);
            Destroy(gameObject);
            return;
        }
        else if (other.CompareTag(wallTag))
        {
            IDamageable target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
                Instantiate(explosion,transform.position,transform.rotation);
                Destroy(gameObject);
            }
        }

    }
}