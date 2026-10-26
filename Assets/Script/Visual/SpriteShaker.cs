using System.Collections;
using UnityEngine;

public static class SpriteShaker
{
    /// <summary>
    /// สั่งให้ Transform สั่นตามระยะเวลาและความแรงที่กำหนด (ใช้งานผ่าน Transform ได้ทันที)
    /// </summary>
    /// <param name="target">Transform ที่ต้องการให้สั่น</param>
    /// <param name="runner">MonoBehaviour เจ้าของ Coroutine (ใส่ this ได้เลย)</param>
    /// <param name="duration">ระยะเวลาสั่น (วินาที)</param>
    /// <param name="magnitude">ความแรงในการสั่น (รัศมีระยะทาง)</param>
    public static Coroutine Shake(this Transform target, MonoBehaviour runner, float duration = 0.15f, float magnitude = 0.1f)
    {
        if (target == null || runner == null) return null;
        return runner.StartCoroutine(DoShake(target, duration, magnitude));
    }

    private static IEnumerator DoShake(Transform target, float duration, float magnitude)
    {
        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // สุ่มตำแหน่งกระจายรอบๆ จุดเดิมเฉพาะแกน X และ Y (สำหรับงาน 2D)
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            target.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null; // รอเฟรมถัดไป
        }

        // คืนค่าตำแหน่งเดิมเสมอหลังสั่นเสร็จ กัน Sprite เบี้ยว!
        target.localPosition = originalPosition;
    }
}