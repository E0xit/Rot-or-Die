using System.Collections;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingAngle = 90f;      // องศากว้างในการแกว่ง (เช่น 90 องศา)
    public float swingDuration = 0.15f;  // ความเร็วในการแกว่ง 1 ครั้ง (วินาที)
    public LayerMask enemyLayer;

    public bool IsSwinging { get; private set; } = false;

    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Collider2D meleeCollider;

    private void Awake()
    {
        // ซ่อนไว้ก่อนตอนยังไม่ฟัน
        SetWeaponActive(false);
    }

    // สั่งฟันตามทิศทางที่กำหนด (isCounterClockwise = true คือทวนเข็ม, false คือตามเข็ม)
    public IEnumerator ExecuteSwing(Vector3 spawnPosition, Quaternion startRotation, bool isCounterClockwise)
    {
        IsSwinging = true;

        // วางพิกัดและหมุนองศาตาม Player ในเฟรมนั้น (แต่จะไม่เลี้ยวตาม Player หลังจากนี้)
        transform.position = spawnPosition;
        transform.rotation = startRotation;

        SetWeaponActive(true);

        // คำนวณมุมองศาการแกว่ง
        // ทวนเข็ม (Counter-Clockwise) = บวกองศา / ตามเข็ม (Clockwise) = ลบองศา
        float direction = isCounterClockwise ? 1f : -1f;
        float halfAngle = swingAngle / 2f;

        // จุดเริ่มต้นและจุดสิ้นสุดการแกว่ง
        Quaternion startRot = startRotation * Quaternion.Euler(0, 0, -halfAngle * direction);
        Quaternion endRot = startRotation * Quaternion.Euler(0, 0, halfAngle * direction);

        float elapsed = 0f;
        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swingDuration;

            // หมุนแกว่งจากมุมเริ่มไปมุมจบ
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        SetWeaponActive(false);
        IsSwinging = false;
    }

    private void SetWeaponActive(bool active)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = active;
        if (meleeCollider != null) meleeCollider.enabled = active;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจจับชนศัตรูระหว่างแกว่ง
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Debug.Log("Melee Hit Enemy: " + other.name);
            // สั่งลดเลือดศัตรูตรงนี้ได้เลยเจ้าค่ะ!
        }
    }
}