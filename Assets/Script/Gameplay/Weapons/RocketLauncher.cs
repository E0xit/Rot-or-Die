using UnityEngine;

public class RocketLauncher : GunBase
{
    [Header("Rocket Launcher Unique Stats")]
    [Tooltip("รัศมีวงระเบิด AOE")]
    public float explosionRadius = 2.5f;

    public override void Shoot()
    {
        if (!CanShoot() || bulletPrefab == null || firePoint == null) return;

        // 1. สร้างกระสุนจรวด PlayerRocket
        GameObject rocketObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. ส่งค่า damage และ explosionRadius ไปให้กระสุน
        PlayerRocket rocketScript = rocketObj.GetComponent<PlayerRocket>();
        if (rocketScript != null)
        {
            rocketScript.SetRocketStats(damage, explosionRadius);
        }

        // 3. หักกระสุน + ตั้งคูลดาวน์
        ConsumeAmmo();
        nextFireTime = Time.time + fireRate;

        // 4. เล่นเสียงยิง
        PlayShootSound();
    }
}