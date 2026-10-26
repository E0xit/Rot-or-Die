using UnityEngine;

public abstract class GunBase : MonoBehaviour
{
    [Header("Gun UI & Info")]
    public string gunName = "Default Gun";
    public Sprite gunIcon;

    [Header("Gun Stats & Balance")]
    public float damage = 20f;   // 🎯 ปรับดาเมจของปืนเล่มนี้ตรงนี้!
    public float fireRate = 0.2f;

    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    protected float nextFireTime;

    public virtual bool CanShoot()
    {
        return Time.time >= nextFireTime;
    }

    public abstract void Shoot();
}