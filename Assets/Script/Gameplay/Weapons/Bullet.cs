using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 15f;
    public float destroyTime = 3f;
    public string enemyTag = "Enemy";

    // 💥 ตัวแปรเก็บ Damage (ไม่ต้องโชว์ใน Inspector เพราะให้ปืนเป็นคนยัดค่ามาให้)
    [HideInInspector]
    public float damage = 10f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    // ฟังก์ชันสำหรับให้สคริปต์ปืนเป็นคนเรียกเพื่อตั้งค่า Damage
    public void SetDamage(float damageAmount)
    {
        damage = damageAmount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag))
        {
            // 🎯 ดึง IDamageable แทน ไม่สนว่าข้างในจะเป็น Walker, Boss หรืออะไรก็ช่าง!
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // ทำลายกระสุนทิ้ง
            Destroy(gameObject);
        }
    }
}