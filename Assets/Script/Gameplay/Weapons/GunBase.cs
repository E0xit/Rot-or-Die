using UnityEngine;

public abstract class GunBase : MonoBehaviour
{
    [Header("Gun UI & Info")]
    public string gunName = "Default Gun";
    public Sprite gunIcon;

    [Header("Gun Stats & Balance")]
    public float damage = 20f;
    public float fireRate = 0.2f;

    [Header("Ammo Settings")]
    public bool isInfiniteAmmo = false; // true สำหรับ SingleShot, false สำหรับปืนอื่น
    public int maxAmmo = 30;
    public int currentAmmo;

    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Audio Settings")]
    public AudioClip shootSound;
    [Range(0f, 1f)] public float soundVolume = 0.8f;

    protected float nextFireTime;

    protected virtual void Start()
    {
        currentAmmo = maxAmmo;
    }

    public virtual bool CanShoot()
    {
        // ถ้ากระสุนไม่จำกัด หรือยังมีกระสุนเหลืออยู่ และคูลดาวน์ถึงเวลาแล้ว
        bool hasAmmo = isInfiniteAmmo || currentAmmo > 0;
        return hasAmmo && Time.time >= nextFireTime;
    }

    public abstract void Shoot();

    // 🎯 หักกระสุนเมื่อยิง (เรียกใน Shoot())
    protected void ConsumeAmmo()
    {
        if (!isInfiniteAmmo)
        {
            currentAmmo--;
            currentAmmo = Mathf.Max(0, currentAmmo);
        }
    }

    // 🔄 ฟังก์ชันสำหรับรีเซ็ตสถานะปืน (เช่น เร่งความเร็ว HMG) เมื่อโดนขัดจังหวะ/สลับปืน
    public virtual void ResetGunState()
    {
        // คลาสลูกที่ต้องใช้ (เช่น HMG) จะไป override เขียนต่อเองเจ้าค่ะ
    }

    protected virtual void PlayShootSound()
    {
        if (shootSound != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            AudioSource.PlayClipAtPoint(shootSound, spawnPos, soundVolume);
        }
    }
}