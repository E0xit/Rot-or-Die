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

        // สร้างลูกปรายกระจายออกไปตามจำนวน pelletCount
        for (int i = 0; i < pelletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, currentAngle);
            
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, bulletRotation);

            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(damage);
            }
        }

        // 🎯 1. หักกระสุนลูกซองออก 1 นัดใหญ่ตรงนี้เจ้าค่ะ!
        ConsumeAmmo();

        // 🎯 2. ตั้งคูลดาวน์และเล่นเสียงยิง
        nextFireTime = Time.time + fireRate;
        PlayShootSound();
    }
}