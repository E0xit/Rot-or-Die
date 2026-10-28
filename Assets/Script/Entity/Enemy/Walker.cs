using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Walker : MonoBehaviour, IDamageable
{
    [Header("Movement Speed")]
    public float speed = 2f; // ความเร็วในการเดินดิ่งลง

    [Header("Health System")]
    public float maxHP = 100f;
    private float currentHP;

    [Header("Health Bar Visual (Square Sprite)")]
    public Transform hpBarFill; // ลาก SpriteRenderer หลอดเลือดสีแดงมาใส่

    [Header("Frame Settings")]
    [Tooltip("ใส่รายการรูปภาพแยกตามลำดับเฟรมที่นี่เจ้าค่ะ")]
    [SerializeField] private Sprite[] frames;

    [Tooltip("จำนวนเฟรมต่อวินาที (FPS)")]
    [SerializeField] private float frameRate = 12f;

    [SerializeField] private bool loop = true;
    [SerializeField] private OneShotAnimation explosion;

    [Header("Wall Charge Settings")]
    [Tooltip("ระยะเวลาชาร์จสั่นก่อนระเบิด (วินาที)")]
    [SerializeField] private float chargeDuration = 1.0f;
    [Tooltip("ความแรงสั่นเริ่มต้น")]
    [SerializeField] private float startShakeMagnitude = 0.05f;
    [Tooltip("ความแรงสั่นสูงสุดก่อนระเบิด")]
    [SerializeField] private float maxShakeMagnitude = 0.3f;
    [Tooltip("ดาเมจที่จะทำใส่กำแพงตอนระเบิดสำเร็จ")]
    [SerializeField] private float wallDamage = 50f;

    [SerializeField]
    private OneShotAnimation death;
    [SerializeField]
    private AudioSource audio;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;

    private Vector3 originalBarScale;
    private bool isChargingExplosion = false; // เช็คว่ากำลังชาร์จระเบิดอยู่หรือไม่
    private Coroutine chargeCoroutine;        // เก็บตัวแปรไว้สั่งหยุดเมื่อโดนยิงตายกลางทาง!
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

        if (hpBarFill != null)
        {
            originalBarScale = hpBarFill.localScale;
        }
    }

    private void Update()
    {
        // 🛑 ถ้ากำลังชาร์จระเบิดอยู่ ให้หยุดเดินทันที!
        if (!isChargingExplosion)
        {
            transform.Translate(Vector2.down * speed * Time.deltaTime);
        }

        HandleSpriteAnimation();
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🧱 ชนกำแพง -> เริ่มสั่นชาร์จระเบิด
        if (collision.CompareTag("Wall"))
        {
            if (!isChargingExplosion)
            {
                // บันทึก Coroutine ไว้ เผื่อสั่งหยุดเมื่อโดนยิงตายกลางคัน!
                chargeCoroutine = StartCoroutine(ChargeAndExplodeRoutine(collision));
            }
        }
        else if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
        // ⚔️ โดน Melee -> โดนดาเมจจาก Melee
        else if (collision.CompareTag("Melee"))
        {
            TakeDamage(50f);
        }
    }

    /// <summary>
    /// 💣 Coroutine สั่นแรงขึ้นเรื่อยๆ ถ้าชาร์จเสร็จโดยไม่โดนยิงตาย จะไปเรียก AttackWall()
    /// </summary>
    private IEnumerator ChargeAndExplodeRoutine(Collider2D wallCollider)
    {
        isChargingExplosion = true;

        Vector3 originalSpriteLocalPos = spriteRenderer.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;

            // เพิ่มระดับความสั่นเรื่อยๆ ตามเวลา
            float currentMagnitude = Mathf.Lerp(startShakeMagnitude, maxShakeMagnitude, elapsed / chargeDuration);

            float offsetX = UnityEngine.Random.Range(-1f, 1f) * currentMagnitude;
            float offsetY = UnityEngine.Random.Range(-1f, 1f) * currentMagnitude;

            spriteRenderer.transform.localPosition = originalSpriteLocalPos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        // คืนค่าตำแหน่ง Sprite
        spriteRenderer.transform.localPosition = originalSpriteLocalPos;

        // 💥 ชาร์จจนจบสำเร็จโดยไม่โดนยิงตาย -> ตีกำแพง!
        AttackWall(wallCollider);
    }

    // 🩸 ระบบลดเลือด & ขัดจังหวะการชาร์จ
    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        // สั่นกระแทกตอนโดนยิง
        transform.Shake(this);
        UpdateHealthBar();

        // ❌ ถ้าเลือดหมด (ตายก่อนชาร์จเสร็จ) -> ขัดจังหวะการชาร์จทันที!
        if (currentHP <= 0f)
        {
            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine); // หยุดการชาร์จระเบิดกำแพง!
            }

            Death(); // ตายจากการโดนยิง (ไม่ตีกำแพง)
        }
    }

    /// <summary>
    /// 🧱 ฟังก์ชันตีกำแพงเมื่อชาร์จสำเร็จ
    /// </summary>
    private void AttackWall(Collider2D wallCollider)
    {
        if (wallCollider != null)
        {
            // ดึงสคริปต์กำแพงมาทำดาเมจ (ถ้ามีสคริปต์กำแพง)
            IDamageable wall = wallCollider.GetComponent<IDamageable>();
            if (wall != null)
            {
                wall.TakeDamage(wallDamage);
            }
        }

        if (explosion != null) Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void UpdateHealthBar()
    {
        if (hpBarFill != null)
        {
            float hpRatio = currentHP / maxHP;
            hpBarFill.localScale = new Vector3(originalBarScale.x * hpRatio, originalBarScale.y, originalBarScale.z);
        }
    }

    /// <summary>
    /// ☠️ ฟังก์ชันตายทั่วไป (โดนยิงตาย หรือ ระเบิดเสร็จแล้วลบทิ้ง)
    /// </summary>
    private void Death()
    {
        // 🎲 สุ่ม Roll ดรอปไอเทมตอนตาย
    if (itemDropPrefab != null && UnityEngine.Random.Range(0f, 100f) <= dropChance)
    {
        Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
    }
        if (explosion != null)
        {
            Instantiate(death, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}