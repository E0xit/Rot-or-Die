using System.Collections;
using UnityEngine;

public class Charger : MonoBehaviour, IDamageable
{
    private enum ChargerState
    {
        Entering,   // กำลังเดินลงมาจุดจอด
        Charging,   // กำลังยืนสั่นสะสมพลัง
        Dashing,    // พุ่งพรวดลงล่างใส่กำแพง
        Dead        // ตายแล้ว
    }

    private ChargerState currentState = ChargerState.Entering;

    [Header("Entrance Phase")]
    public float entranceSpeed = 2.5f;     // ความเร็วตอนเดินลงมาจากฟ้า
    public float targetPosY = 2.0f;         // พิกัด Y ที่จะหยุดเพื่อเตรียมชาร์จ

    [Header("Charge Settings")]
    public float chargeDuration = 1.5f;    // ระยะเวลาสั่นชาร์จพลังก่อนพุ่ง (วินาที)
    public float startShakeMagnitude = 0.05f; // ความแรงสั่นเริ่มต้น
    public float maxShakeMagnitude = 0.35f;   // ความแรงสั่นสูงสุดก่อนพุ่ง

    [Header("Dash & Explosion Settings")]
    public float chargeDashSpeed = 12f;    // ความเร็วตอนพุ่งชนกำแพง (ยิ่งเยอะยิ่งไว!)
    public float wallDamage = 75f;         // ดาเมจที่จะทำใส่กำแพงตอนชนระเบิด
    public OneShotAnimation explosion;     // Effect ตอนระเบิดชนกำแพง

    [Header("Health System")]
    public float maxHP = 120f;
    private float currentHP;

    [Header("Health Bar Visual (Square Sprite)")]
    public Transform hpBarFill;            // SpriteRenderer หลอดเลือดสีแดง

    [Header("Frame Settings")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 12f;
    [SerializeField] private bool loop = true;

    [Header("Death Effects")]
    public OneShotAnimation death;
    public AudioSource audioSource;

    [Header("Item Drop Settings")]
    public GameObject itemDropPrefab;
    [Range(0f, 100f)] public float dropChance = 30f;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float animationTimer;
    private Vector3 originalBarScale;

    private Coroutine chargeCoroutine;

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
        if (currentState == ChargerState.Dead) return;

        // 🚀 1. ช่วงเดินลงมาจากฟ้ามาตั้งหลัก
        if (currentState == ChargerState.Entering)
        {
            HandleEntrance();
        }
        // ⚡ 2. ช่วงพุ่งพรวดใส่กำแพง
        else if (currentState == ChargerState.Dashing)
        {
            transform.Translate(Vector2.down * chargeDashSpeed * Time.deltaTime);
        }

        HandleSpriteAnimation();
    }

    // 1. เดินลงมาตามแกน Y- จนถึงจุดหมาย
    private void HandleEntrance()
    {
        transform.Translate(Vector3.down * entranceSpeed * Time.deltaTime);

        if (transform.position.y <= targetPosY)
        {
            transform.position = new Vector3(transform.position.x, targetPosY, transform.position.z);
            currentState = ChargerState.Charging;

            // เริ่มทำการชาร์จสั่นสะสมพลัง!
            chargeCoroutine = StartCoroutine(RoutineChargeAndDash());
        }
    }

    // 2. Coroutine สั่นสะสมพลังแรงขึ้นเรื่อยๆ แล้วเปลี่ยน State เป็น Dashing
    private IEnumerator RoutineChargeAndDash()
    {
        Vector3 originalSpriteLocalPos = spriteRenderer.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;

            // คำนวณความสั่นเร่งขึ้นเรื่อยๆ ตามเวลาแบบ Linear
            float currentMagnitude = Mathf.Lerp(startShakeMagnitude, maxShakeMagnitude, elapsed / chargeDuration);

            float offsetX = Random.Range(-1f, 1f) * currentMagnitude;
            float offsetY = Random.Range(-1f, 1f) * currentMagnitude;

            spriteRenderer.transform.localPosition = originalSpriteLocalPos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        // คืนตำแหน่ง Sprite
        spriteRenderer.transform.localPosition = originalSpriteLocalPos;

        // 🚀 สั่นชาร์จเสร็จแล้ว -> สั่งเปลี่ยนโหมดเป็นพุ่งชน!
        currentState = ChargerState.Dashing;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == ChargerState.Dead) return;

        // 🧱 ชนกำแพง ( Wall ) -> ระเบิดทำดาเมจใส่กำแพงทันที!
        if (collision.CompareTag("Wall"))
        {
            AttackWall(collision);
        }
        // ⚔️ โดน Melee -> โดนดาเมจ
        else if (collision.CompareTag("Melee"))
        {
            TakeDamage(50f);
        }
        else if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    // 🧱 ฟังก์ชันทำดาเมจระเบิดกำแพง
    private void AttackWall(Collider2D wallCollider)
    {
        if (wallCollider != null)
        {
            IDamageable wall = wallCollider.GetComponent<IDamageable>();
            if (wall != null)
            {
                wall.TakeDamage(wallDamage);
            }
        }

        if (explosion != null) Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    // 🩸 ระบบลดเลือด & ขัดจังหวะการชาร์จ
    public void TakeDamage(float amount)
    {
        if (currentState == ChargerState.Dead) return;

        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        transform.Shake(this);
        UpdateHealthBar();

        if (currentHP <= 0f)
        {
            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine); // ขัดจังหวะการชาร์จทันทีถ้ายิงตายก่อน!
            }

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
        currentState = ChargerState.Dead;

        if (itemDropPrefab != null && Random.Range(0f, 100f) <= dropChance)
        {
            Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
        }
        if (death != null)
        {
            Instantiate(death, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    private void HandleSpriteAnimation()
    {
        if (frames == null || frames.Length == 0) return;

        animationTimer += Time.deltaTime;

        if (animationTimer >= 1f / frameRate)
        {
            animationTimer -= 1f / frameRate;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                currentFrame = loop ? 0 : frames.Length - 1;
            }

            spriteRenderer.sprite = frames[currentFrame];
        }
    }
}