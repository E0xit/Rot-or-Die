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
    [SerializeField]
    private OneShotAnimation explosion;
    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;

    private Vector3 originalBarScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHP = maxHP;

        // บันทึกขนาดเดิมของหลอดเลือดไว้
        if (hpBarFill != null)
        {
            originalBarScale = hpBarFill.localScale;
        }
    }

    private void Update()
    {
        // เดินลงล่าง (Vector2.down)
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;

        // คำนวณช่วงเวลาเปลี่ยนเฟรมตาม FPS
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1; // ค้างที่เฟรมสุดท้าย
                }
            }

            // เปลี่ยนรูปใน Sprite Renderer
            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🧱 ชนขอบล่าง / กำแพง -> ตายทันที
        if (collision.CompareTag("Wall"))
        {
            Death();
        }
        else if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
        // ⚔️ โดน Melee -> โดนดาเมจจาก Melee
        else if (collision.CompareTag("Melee"))
        {
            TakeDamage(50f); // หรือจะดึง damage จาก MeleeWeapon script มาก็ได้เจ้าค่ะ
        }
    }

    // 🩸 ระบบลดเลือด & อัปเดตหลอดเลือดสี่เหลี่ยม
    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
        transform.Shake(this);
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
            // คำนวณสัดส่วน HP (0.0 ถึง 1.0)
            float hpRatio = currentHP / maxHP;

            // ย่อ/ขยายแกน X ของหลอดเลือดสี่เหลี่ยม
            hpBarFill.localScale = new Vector3(originalBarScale.x * hpRatio, originalBarScale.y, originalBarScale.z);
        }
    }

    private void Death()
    {
        if (explosion != null) Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}