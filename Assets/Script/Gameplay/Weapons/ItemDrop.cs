using System.Collections;
using UnityEngine;

public enum ItemType
{
    AmmoPack,   // เติมกระสุนปืน
    HealthPack  // เติมเลือดกำแพง
}

public class ItemDrop : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemType itemType = ItemType.AmmoPack;
    
    [Range(0f, 100f)]
    [Tooltip("เปอร์เซ็นต์กระสุนที่จะเติมให้ปืน (เช่น 30 หมายถึงเติม 30% ของ MaxAmmo)")]
    public float ammoPercent = 30f;        // 🎯 เปลี่ยนเป็น % กระสุนตรงนี้เจ้าค่ะ!

    public float fallSpeed = 3f;            // ความเร็วในการดิ่งลงมา (Y-)
    public float targetPosY = -4f;          // พิกัด Y เป้าหมายที่จะหยุดรอผู้เล่น

    [Header("Lifetime Settings")]
    public float lifeTimeAfterStop = 8f;   // เวลาที่ไอเทมจะลอยรอเก็บ ก่อนสลายตัวเอง (วินาที)
    public float fadeDuration = 1f;        // ระยะเวลาตอนค่อยๆ จางหายไป

    private bool isStopped = false;
    private bool isPickedUp = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (isPickedUp) return;

        // 🚀 ช่วงที่ 1: ดิ่งลงมาตามแกน Y- (พิกัด X ไม่เปลี่ยน)
        if (!isStopped)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            if (transform.position.y <= targetPosY)
            {
                transform.position = new Vector3(transform.position.x, targetPosY, transform.position.z);
                isStopped = true;
                StartCoroutine(DespawnRoutine());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;

        if (other.CompareTag("Player"))
        {
            isPickedUp = true;
            ApplyItemEffect(other.gameObject);
            Destroy(gameObject);
        }
    }

    // 🎁 ฟังก์ชันแจกของให้ Player (ส่งค่าเปอร์เซ็นต์ไปแทน)
    private void ApplyItemEffect(GameObject playerObj)
    {
        if (itemType == ItemType.AmmoPack)
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RefillAmmoByPercent(ammoPercent);
                Debug.Log($"🎁 Picked up Ammo Pack! Refilled +{ammoPercent}% ammo!");
            }
        }
    }

    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(lifeTimeAfterStop - fadeDuration);

        float elapsed = 0f;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (spriteRenderer != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}