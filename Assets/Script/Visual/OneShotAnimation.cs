using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OneShotAnimation : MonoBehaviour
{
    public Sprite[] frames;
    public float frameRate = 12f;

    private IEnumerator Start()
    {
        if (frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float delay = 1f / frameRate;

        // วนลูปเปลี่ยนรูปตามเฟรม
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(delay);
        }

        // เล่นเสร็จแล้ว ทำลายตัวเองทิ้งทันที!
        Destroy(gameObject);
    }
}