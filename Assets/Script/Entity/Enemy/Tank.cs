using System.Collections;
using UnityEngine;

public class Tank : MonoBehaviour, IDamageable
{
    [Header("Visual & Sprite Reference")]
    public Transform hullSpriteTransform; // ลาก Child GameObject ที่เก็บ Sprite ของ Hull มาใส่
    public float rotationSpeed = 360f;    // ความเร็วในการหมุนตัวถัง (องศา/วินาที)

    [Header("Health System")]
    public float maxHP = 100f;
    private float currentHP;
    private Vector3 originalBarScale;

    [Header("Health Bar Visual (Square Sprite)")]
    public Transform hpBarFill; // ลาก SpriteRenderer หลอดเลือดสีแดงมาใส่

    [Header("Entrance Phase")]
    public float entranceSpeed = 3f;
    public float targetPosY = 3.5f;
    private bool isEntering = true;

    [Header("Movement Boundaries")]
    public Transform leftBoundary;
    public Transform rightBoundary;

    [Header("Movement Behavior Settings")]
    public float moveSpeed = 4f;
    public float moveInterval = 2.5f;
    [Range(0f, 100f)]
    public float moveSuccessRate = 70f;
    public float minStepDistance = 1.5f;
    public float maxStepDistance = 4f;

    [SerializeField]
    private OneShotAnimation explosion;

    private float timer;
    private bool isMoving = false;
    private int currentDirection = 1;

    private void Start()
    {
        timer = moveInterval;
        currentDirection = Random.value > 0.5f ? 1 : -1;

        // บังคับให้ Sprite หันไปทาง Y- (ดิ่งลงล่าง) ตอนเริ่มต้น
        SetHullRotation(0f);
        currentHP = maxHP;

        // บันทึกขนาดเดิมของหลอดเลือดไว้
        if (hpBarFill != null)
        {
            originalBarScale = hpBarFill.localScale;
        }
    }

    private void Update()
    {
        // 🚀 ช่วงที่ 1: ดิ่งลงมาจากฟ้า
        if (isEntering)
        {
            HandleEntrance();
            return;
        }

        // 🤖 ช่วงที่ 2: ลูปพฤติกรรมเดิน
        if (!isMoving)
        {
            // ถ้าหยุดเดิน ให้หมุนตัวถังกลับมาหันลงล่าง (Y-) ตามเดิม
            RotateHullTowards(0f);

            timer += Time.deltaTime;
            if (timer >= moveInterval)
            {
                timer = 0f;
                TryInitiateMove();
            }
        }
    }

    private void HandleEntrance()
    {
        transform.Translate(Vector3.down * entranceSpeed * Time.deltaTime);

        // ตอนดิ่งลงมา ให้ Sprite หันหน้าลงล่าง (Y-)
        SetHullRotation(0f);

        if (transform.position.y <= targetPosY)
        {
            transform.position = new Vector3(transform.position.x, targetPosY, transform.position.z);
            isEntering = false;
        }
    }

    private void TryInitiateMove()
    {
        float roll = Random.Range(0f, 100f);
        if (roll <= moveSuccessRate)
        {
            float randomDistance = Random.Range(minStepDistance, maxStepDistance);
            currentDirection *= -1; // สลับทิศทางเสมอ

            StartCoroutine(RoutineMoveStep(randomDistance, currentDirection));
        }
    }

    private IEnumerator RoutineMoveStep(float distance, int direction)
    {
        isMoving = true;

        float targetX = transform.position.x + (distance * direction);

        if (leftBoundary != null && rightBoundary != null)
        {
            targetX = Mathf.Clamp(targetX, leftBoundary.position.x, rightBoundary.position.x);
        }

        Vector3 destination = new Vector3(targetX, transform.position.y, transform.position.z);

        // 🧭 คำนวณองศาตามทิศทางเคลื่อนที่ (อิง Sprite เริ่มต้นหันลงล่าง Y-)
        // direction > 0 (ไปทางขวา)  -> หมุนทวนเข็ม (+90 องศา หรือ -90 องศาตามทิศ Sprite)
        // direction < 0 (ไปทางซ้าย) -> หมุนตามเข็ม
        float targetRotationAngle = (direction > 0) ? 90f : -90f;

        // เดินเข้าหาจุดหมาย
        while (Vector3.Distance(transform.position, destination) > 0.05f)
        {
            // ขยับพิกัดหลักของบอส
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

            // 🎨 หมุนเฉพาะ Hull Sprite ไปทางทิศที่กำลังขยับอย่างนุ่มนวล
            RotateHullTowards(targetRotationAngle);

            yield return null;
        }

        transform.position = destination;

        // พอถึงจุดหมาย ให้หมุน Sprite ค่อยๆ คืนกลับมาหันลงล่าง (Y-)
        while (Mathf.Abs(Quaternion.Angle(hullSpriteTransform.localRotation, Quaternion.Euler(0, 0, 0))) > 0.1f)
        {
            RotateHullTowards(0f);
            yield return null;
        }

        SetHullRotation(0f);
        isMoving = false;
    }

    // 🔄 Helper Method: ค่อยๆ หมุน Hull Sprite ไปหาองศาเป้าหมายอย่างนุ่มนวล
    private void RotateHullTowards(float targetAngle)
    {
        if (hullSpriteTransform == null) return;

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        hullSpriteTransform.localRotation = Quaternion.RotateTowards(
            hullSpriteTransform.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // 🎯 Helper Method: สั่งหมุนตัดจบไปที่องศานั้นทันที
    private void SetHullRotation(float targetAngle)
    {
        if (hullSpriteTransform == null) return;
        hullSpriteTransform.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }

    private void OnDrawGizmosSelected()
    {
        if (leftBoundary != null && rightBoundary != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(leftBoundary.position, rightBoundary.position);
            Gizmos.DrawWireSphere(leftBoundary.position, 0.5f);
            Gizmos.DrawWireSphere(rightBoundary.position, 0.5f);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        UpdateHealthBar();
        transform.Shake(this);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🧱 ชนขอบล่าง / ชนกำแพง -> ตายทันที
        if (collision.CompareTag("Bullet"))
        {
            TakeDamage(25f);
        }
    }
}