using System.Collections.Generic;
using UnityEngine;

public class PenetratingBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 25f; // สไนเปอร์กระสุนต้องวิ่งไว!
    public float destroyTime = 4f;
    public string enemyTag = "Enemy";

    [HideInInspector] public float damage = 100f;

    // ลิสต์จำว่าเคยทำดาเมจใส่ศัตรูตัวไหนไปแล้วบ้าง (กันไม่ให้เฟรมซ้ำทำดาเมจเบิ้ลตัวเดิม)
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    public void SetDamage(float damageAmount)
    {
        damage = damageAmount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag) && !hitEnemies.Contains(other))
        {
            hitEnemies.Add(other); // บันทึกว่าชนตัวนี้แล้ว

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // 💥 ไม่สั่ง Destroy(gameObject) ทันที เพื่อให้กระสุนพุ่งทะลุตัวถัดไปได้!
        }
        else if (other.CompareTag("Wall"))
        {
            // ถ้าชนกำแพงฉาก ค่อยทำลายกระสุน
            Destroy(gameObject);
        }
    }
}