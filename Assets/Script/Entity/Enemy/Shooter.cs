using System.Collections;
using UnityEngine;

public class Shooter : MonoBehaviour, IDamageable
{
    [Header("Entrance Phase")]
    public float entranceSpeed = 2.5f;   // ความเร็วตอนเดินลงมาจุดจอด
    public float targetPosY = 2.0f;       // พิกัด Y ที่จะลงมาจอดเพื่อตั้งป้อมยิง
    private bool isEntering = true;      // สถานะว่ากำลังเดินลงมาหรือไม่

    [Header("Shooting Behavior Settings")]
    public GameObject bulletPrefab;      // Prefab กระสุนของศัตรู
    public Transform firePoint;          // จุดปล่อยกระสุน
    public float shootInterval = 2f;     // รอบเวลาสุ่มยิงแบบตายตัว (วินาที)
    [Range(0f, 100f)]
    public float shootSuccessRate = 60f; // โอกาสยิงสำเร็จ (%) ในแต่ละรอบเวลา

    [Header("Health System")]
    public float maxHP = 80f;
    private float currentHP;

    [Header("Frame Settings")]
    [Tooltip("ใส่รายการรูปภาพแยกตามลำดับเฟรมที่นี่เจ้าค่ะ")]
    [SerializeField] private Sprite[] frames;

    [Tooltip("จำนวนเฟรมต่อวินาที (FPS)")]
    [SerializeField] private float frameRate = 12f;

    [SerializeField] private bool loop = true;

    [Header("Health Bar Visual (Square Sprite)")]
    public Transform hpBarFill;          // SpriteRenderer หลอดเลือดสีแดง

    [Header("Death Effects")]
    public OneShotAnimation deathEffect;
    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;
    private float shootTimer;
    private Vector3 originalBarScale;
    [Header("Item Drop Settings")]
    public GameObject itemDropPrefab;      // Prefab ไอเทมที่จะดรอป
    [Range(0f, 100f)]
    public float dropChance = 30f;          // โอกาสดรอปไอเทม (%) เช่น 30%
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHP = maxHP;
        shootTimer = shootInterval;

        if (hpBarFill != null)
        {
            originalBarScale = hpBarFill.localScale;
        }
    }

    private void Update()
    {
        // 🚀 ช่วงที่ 1: เดินลงมาจากฟ้ามายังตำแหน่งจอด
        if (isEntering)
        {
            HandleEntrance();
            return;
        }

        // 🔫 ช่วงที่ 2: ยืนประจำตำแหน่ง แล้วนับเวลาตาม Same Time Interval
        HandleShootingInterval();
        HandleSpriteAnimation();
    }

    // 1. จัดการการเดินลงมาจากข้างบน
    private void HandleEntrance()
    {
        transform.Translate(Vector3.down * entranceSpeed * Time.deltaTime);

        // พอถึงพิกัด Y ที่กำหนด ให้หยุดเดินแล้วเริ่มโหมดยิง
        if (transform.position.y <= targetPosY)
        {
            transform.position = new Vector3(transform.position.x, targetPosY, transform.position.z);
            isEntering = false;
        }
    }

    // 2. นับเวลารอบยิง + Roll สุ่มโอกาสสำเร็จ
    private void HandleShootingInterval()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            shootTimer = 0f; // รีเซ็ตรอบเวลา

            // 🎲 Roll สุ่มโอกาสยิงสำเร็จตาม % ที่กำหนด
            float roll = Random.Range(0f, 100f);
            if (roll <= shootSuccessRate)
            {
                Shoot();
            }
        }
    }

    // 🎯 3. สั่งปล่อยกระสุน
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        // ถ้าไม่มี firePoint ให้ใช้ตำแหน่งของตัวเอง
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRot = firePoint != null ? firePoint.rotation : Quaternion.Euler(0, 0, 180f); // หันลงล่าง (Y-)

        Instantiate(bulletPrefab, spawnPos, spawnRot);
    }

    // 🩸 ระบบรับความเสียหาย (IDamageable)
    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        transform.Shake(this, 0.2f, 0.1f);
        UpdateHealthBar();

        if (currentHP <= 0f)
        {
            Death();
        }
    }

    private void UpdateHealthBar()
    {
        if (hpBarFill != null)
        {
            float hpRatio = currentHP / maxHP;
            hpBarFill.localScale = new Vector3(originalBarScale.x * hpRatio, originalBarScale.y, originalBarScale.z);
        }
    }

    private void Death()
    {
        if (itemDropPrefab != null && Random.Range(0f, 100f) <= dropChance)
        {
            Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
        }
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ⚔️ โดน Melee ของ Player -> โดนดาเมจ
        if (collision.CompareTag("Melee"))
        {
            TakeDamage(50f);
        }
        if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    private void HandleSpriteAnimation()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                currentFrame = loop ? 0 : frames.Length - 1;
            }

            spriteRenderer.sprite = frames[currentFrame];
        }
    }
}