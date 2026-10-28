using UnityEngine;

public class PlayerRocket : MonoBehaviour
{
    [Header("Rocket Speed Settings")]
    public float speed = 18f;             // ความเร็วของจรวด
    public float destroyTime = 4f;        // ทำลายตัวเองตามเวลาถ้าไม่ชนอะไร
    public string enemyTag = "Enemy";     // Tag ของศัตรู

    [Header("AOE Explosion Settings")]
    public GameObject explosionVFX;       // Particle / Animation เอฟเฟกต์ระเบิด
    public LayerMask enemyLayer;          // Layer ของศัตรู (ไว้ใช้ตรวจจับวงกว้าง)

    // ตัวแปรที่สคริปต์ปืนจะส่งมาให้ตอน Instantiate
    [HideInInspector] public float damage = 80f;
    [HideInInspector] public float explosionRadius = 2.5f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        // 🚀 พุ่งตรงไปตามทิศทางหัวจรวด (Vector3.up)
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    public void SetRocketStats(float damageAmount, float radius)
    {
        damage = damageAmount;
        explosionRadius = radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 💥 เมื่อปะทะศัตรูตัวแรก หรือ ชนฉาก/กำแพง -> ระเบิด AOE ทันที!
        if (other.CompareTag(enemyTag))
        {
            Explode();
        }
    }

    // 💣 ฟังก์ชันระเบิดทำดาเมจเป็นวงกว้าง (AOE)
    private void Explode()
    {
        // 1. ตรวจจับ Collider ของศัตรูทั้งหมดที่อยู่ในรัศมีวงกลม ณ จุดระเบิด
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            IDamageable damageable = enemyCollider.GetComponent<IDamageable>();
            if (damageable != null && damageable is not Wall)
            {
                // สามารถเพิ่ม System คำนวณความเสียหายตามระยะห่าง (Damage Falloff) เพิ่มเติมตรงนี้ได้เจ้าค่ะ
                damageable.TakeDamage(damage);
            }
        }

        // 2. แสดง Animation / Particle ระเบิด (ถ้ามี)
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        // 3. สั่งทำลายตัวจรวดทิ้ง
        Destroy(gameObject);
    }

    // 🎨 วาดรัศมีวงระเบิดให้เห็นชัดๆ ใน Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}