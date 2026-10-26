using UnityEngine;

public class SingleShotGun : GunBase
{
    public override void Shoot()
    {
        if (!CanShoot() || bulletPrefab == null || firePoint == null) return;

        // 1. สร้างกระสุน
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. ส่งค่า damage ของปืนกระบอกนี้ไปให้สคริปต์ Bullet
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(damage);
        }

        nextFireTime = Time.time + fireRate;
    }
}