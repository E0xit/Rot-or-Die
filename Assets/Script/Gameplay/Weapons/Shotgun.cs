using UnityEngine;

public class Shotgun : GunBase
{
    public int pelletCount = 5;
    public float spreadAngle = 20f;

    public override void Shoot()
    {
        if (!CanShoot() || bulletPrefab == null || firePoint == null) return;

        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (pelletCount - 1);

        for (int i = 0; i < pelletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, currentAngle);
            
            // สร้างลูกปรายแต่ละนัด
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, bulletRotation);

            // ส่งค่า damage ของ shotgun (เช่น 8 ดาเมจต่อ 1 เม็ด) ไปให้กระสุนทุกเม็ด
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(damage);
            }
        }

        nextFireTime = Time.time + fireRate;
    }
}