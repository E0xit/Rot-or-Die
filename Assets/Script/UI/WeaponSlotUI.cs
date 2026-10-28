using UnityEngine;
using TMPro;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI ammoText; // ตัวหนังสือแสดงกระสุน เช่น "100/100"

    // 🎯 อัปเดตตัวเลขกระสุนประจำช่อง
    public void UpdateSlotUI(GunBase gun)
    {
        if (gun == null || ammoText == null) return;

        // ถ้าเป็นปืนกระสุนไม่จำกัด (SingleShot) ไม่ต้องโชว์เลขกระสุน (หรือโชว์เป็น ∞)[cite: 5, 8]
        if (gun.isInfiniteAmmo)
        {
            ammoText.text = ""; // หรือ "Inf"
        }
        else
        {
            // โชว์กระสุนปัจจุบัน / กระสุนสูงสุด (เช่น 30/30 หรือ 0/20)[cite: 5, 8]
            ammoText.text = $"{gun.currentAmmo}/{gun.maxAmmo}";
        }
    }
}