using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponUIManager : MonoBehaviour
{
    // 📦 คลาสโครงสร้างสำหรับจับคู่ RectTransform ปืนกับ TextMeshPro กระสุนใน Inspector
    [System.Serializable]
    public class WeaponUISlotData
    {
        public string slotName;             // ตั้งชื่อเพื่อดูง่ายใน Inspector (เช่น Pistol, Shotgun)
        public RectTransform gunIconRect;   // ลาก UI Image ของรูปปืนกระบอกนี้มาใส่ (เพื่อเอาตำแหน่ง Y ให้ Arrow ชี้)
        public TextMeshProUGUI ammoText;    // ลาก Text กระสุนคู่กันมาใส่ (ถ้าไม่มี เช่น ปืน 1 กระสุน Inf ให้ปล่อย null ไว้)
    }

    [Header("Player Reference")]
    public PlayerController player;

    [Header("Arrow Reference")]
    public RectTransform arrowIcon;         // ลาก GameObject 'Arrow' มาใส่

    [Header("Weapon Slots (Mapped in Inspector)")]
    [Tooltip("ใส่ข้อมูลเรียงลำดับตาม Index ปืน 0, 1, 2, 3, 4")]
    public List<WeaponUISlotData> weaponSlots;

    private void Update()
    {
        if (player == null) return;

        UpdateAmmoTexts();
        UpdateArrowPosition();
    }

    // 🎯 1. อัปเดตตัวเลขกระสุนตาม Real-time
    private void UpdateAmmoTexts()
    {
        if (player.gunsList == null) return;

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            // เช็คว่ามีปืนตรงกับ Slot ใน gunsList หรือไม่
            if (i < player.gunsList.Count && player.gunsList[i] != null)
            {
                GunBase currentGun = player.gunsList[i];
                TextMeshProUGUI targetText = weaponSlots[i].ammoText;

                if (targetText != null)
                {
                    // ถ้าเป็นปืนกระสุนไม่จำกัด ไม่โชว์เลขกระสุน (หรือโชว์ว่าง)
                    if (currentGun.isInfiniteAmmo)
                    {
                        targetText.text = "";
                    }
                    else
                    {
                        // แสดงกระสุนปัจจุบัน / กระสุนสูงสุด (เช่น 30/30 หรือ 0/20)
                        targetText.text = $"{currentGun.currentAmmo}/{currentGun.maxAmmo}";
                    }
                }
            }
        }
    }

    // 🎯 2. ย้ายตำแหน่ง Arrow ไปชี้ตรง Y ของปืนที่เลือกอยู่
    private void UpdateArrowPosition()
    {
        int currentIndex = player.CurrentGunIndex;

        if (arrowIcon != null && currentIndex >= 0 && currentIndex < weaponSlots.Count)
        {
            RectTransform activeGunIcon = weaponSlots[currentIndex].gunIconRect;

            if (activeGunIcon != null)
            {
                // ปรับพิกัด Y ของ Arrow ให้ตรงกับ Y ของปืนกระบอกนั้นๆ
                Vector2 arrowPos = arrowIcon.anchoredPosition;
                arrowPos.y = activeGunIcon.anchoredPosition.y;
                arrowIcon.anchoredPosition = arrowPos;
            }
        }
    }
}