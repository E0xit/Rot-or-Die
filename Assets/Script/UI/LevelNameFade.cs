using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LevelNameFade : MonoBehaviour
{
    [Header("⏳ Fade Timing Settings")]
    [Tooltip("เวลารอ (วินาที) ก่อนที่ข้อความจะเริ่มค่อยๆ จางหายไป")]
    public float displayDuration = 2.0f;

    [Tooltip("ระยะเวลา (วินาที) ในการค่อยๆ จางหายไปจนมองไม่เห็น")]
    public float fadeDuration = 1.5f;

    [Header("🗑️ Auto Destroy Options")]
    [Tooltip("ถ้าติ๊กถูก จะสั่ง Destroy ตัวเองทิ้งอัตโนมัติเมื่อ Fade จบ เพื่อไม่ให้รก Hierarchy")]
    public bool destroyOnComplete = true;

    private TextMeshProUGUI levelText;

    private void Awake()
    {
        levelText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        StartCoroutine(RoutineFadeOut());
    }

    private IEnumerator RoutineFadeOut()
    {
        // 1. นั่งรอตามเวลาที่กำหนด
        yield return new WaitForSeconds(displayDuration);

        // 2. ค่อยๆ ปรับ Alpha จาก 1 (ชัด) ลงไปหา 0 (ใส)
        float elapsed = 0f;
        Color originalColor = levelText.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            levelText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            yield return null;
        }

        // บังคับ Alpha เป็น 0 สนิท
        levelText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // 3. ทำลายทิ้ง หรือปิดตัวเองเมื่อแสดงผลจบ
        if (destroyOnComplete)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}