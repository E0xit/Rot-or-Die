using UnityEngine;
using UnityEngine.Events;

public class Wall : MonoBehaviour, IDamageable
{
    [Header("Wall Health Settings")]
    public float maxHP = 500f;
    private float currentHP;

    [Header("Health Bar Visual (Square Sprite or UI Scale)")]
    public Transform hpBarFill; // SpriteRenderer / RectTransform หลอดเลือดสีแดงกำแพง
    
    [SerializeField] private GameObject explosion;
    
    [Header("Game Over Event")]
    [Tooltip("ลาก WinManager / GameManager มาสั่ง GameOver หรือโหลดฉากตรงนี้ได้เลยเจ้าค่ะ")]
    public UnityEvent onWallDestroyed; 

    private Vector3 originalBarScale;
    private bool isDead = false;

    private void Start()
    {
        currentHP = maxHP;

        if (hpBarFill != null)
        {
            originalBarScale = hpBarFill.localScale;
        }

        UpdateHealthBar();
    }

    // 🩸 ระบบรับความเสียหายจาก Walker หรือ Rocket
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        transform.Shake(this, 0.2f, 0.1f);
        UpdateHealthBar();

        Debug.Log($"Wall Took Damage: {amount} | Remaining HP: {currentHP}/{maxHP}");

        if (currentHP <= 0f)
        {
            WallDestroyed();
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

    // 💥 กำแพงพัง -> เสกเอฟเฟกต์ + ปิด Sprite/Collider + ตะโกนบอก Event ข้างนอก
    private void WallDestroyed()
    {
        if (isDead) return;
        isDead = true;

        if (explosion != null)
        {
            Instantiate(explosion, transform.position, transform.rotation);
        }

        // ปิดการแสดงผลและระบบตรวจจับการชนของกำแพง
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 🔔 เรียกตะโกนบอก Event ข้างนอก (เช่น สั่ง GameOver)
        onWallDestroyed?.Invoke();
    }
}