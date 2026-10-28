using UnityEngine;

public class SniperRifle : GunBase
{
    public override void Shoot()
    {
        if (!CanShoot() || bulletPrefab == null || firePoint == null) return;

        // 1. สร้างกระสุนทะลุ
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        PenetratingBullet bulletScript = bulletObj.GetComponent<PenetratingBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(damage);
        }

        ConsumeAmmo();
        nextFireTime = Time.time + fireRate;
        PlayShootSound();
    }
}