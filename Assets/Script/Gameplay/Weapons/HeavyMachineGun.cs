using UnityEngine;

public class HeavyMachineGun : GunBase
{
    [Header("Exponential Spin-Up Settings")]
    public float baseFireRate = 0.4f;     // ความเร็วเริ่มต้น (ช้า หนักๆ)
    public float maxFireRate = 0.04f;    // ความเร็วสูงสุดเมื่อยิงค้าง (รัวเป็นสายน้ำ!)
    
    [Tooltip("ค่าความเร่งแบบ Exponential (ยิ่งค่าเยอะ ยิ่งเร่งความเร็วรัวไวมนุษย์ขึ้นอย่างเห็นได้ชัด)")]
    public float accelerationFactor = 3.5f; 

    private float firingDuration = 0f;    // ระยะเวลาสะสมที่กดยิงค้างไว้
    private bool isShootingThisFrame = false;

    protected override void Start()
    {
        base.Start();
        // กำหนดกระสุนแบบไม่จำกัดหรือมีจำกัดตาม Inspector
    }

    private void Update()
    {
        // 🛑 ถ้าเฟรมนี้ไม่ได้กดยิง (หรือปล่อยคลิกซ้าย) ให้รีเซ็ตเวลาสะสมทันที!
        if (!isShootingThisFrame)
        {
            ResetGunState();
        }

        // รีเซ็ต Flag เคลียร์รอบต่อไป
        isShootingThisFrame = false;
    }

    public override void Shoot()
    {
        if (!CanShoot() || bulletPrefab == null || firePoint == null) return;

        isShootingThisFrame = true;

        // 1. สะสมระยะเวลาที่ยิงค้างต่อเนื่อง
        firingDuration += Time.deltaTime;

        // 2. 🚀 คำนวณ FireRate แบบ Exponential Decay!
        // สมการ: max + (base - max) * e^(-k * t)
        float dynamicFireRate = maxFireRate + (baseFireRate - maxFireRate) * Mathf.Exp(-accelerationFactor * firingDuration);

        // 3. สร้างกระสุน
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(damage);
        }

        ConsumeAmmo();

        // ตั้งเวลาคูลดาวน์นัดถัดไปตามความเร็ว Exponential ที่คำนวณได้
        nextFireTime = Time.time + dynamicFireRate;

        PlayShootSound();
    }

    // 🔄 ขัดจังหวะปุ๊บ (Melee / สลับปืน / ปล่อยยิง) ล้างเวลาสะสม กลับมาช้าหนักเหมือนเดิมทันที!
    public override void ResetGunState()
    {
        firingDuration = 0f;
    }
}