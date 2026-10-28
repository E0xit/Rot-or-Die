using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement & Boundary")]
    public float moveSpeed = 8f;
    public Transform leftBoundary;
    public Transform rightBoundary;

    [Header("Weapon System (Fixed 4 Guns)")]
    [Tooltip("ใส่ปืนเรียงตามลำดับ 4 กระบอก: 0=Single, 1=Shotgun, 2=Sniper, 3=HMG")]
    public List<GunBase> gunsList = new List<GunBase>(4);
    private int currentGunIndex = 0;

    [Header("Melee Combo Settings")]
    public MeleeWeapon meleeObject;
    public Transform meleeSpawnPoint;
    public int CurrentGunIndex => currentGunIndex;
    private bool[] comboPattern = new bool[] { true, true, false };
    private int currentComboIndex = 0;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // กำหนดกระสุนแบบ Infinite ให้กับปืน SingleShot (ปืนกระบอกแรก index 0)
        if (gunsList.Count > 0 && gunsList[0] != null)
        {
            gunsList[0].isInfiniteAmmo = true;
        }

        EquipGun(0);
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleWeaponSwitchingFixed();
        HandleCombat();
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        Vector3 newPosition = transform.position + new Vector3(inputX * moveSpeed * Time.deltaTime, 0, 0);

        if (leftBoundary != null && rightBoundary != null)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary.position.x, rightBoundary.position.x);
        }

        transform.position = newPosition;
    }

    private void HandleRotation()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDirection = (mouseWorldPos - transform.position).normalized;

        float targetAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle - 90f);
    }

    private void HandleCombat()
    {
        bool isMeleeing = meleeObject != null && meleeObject.IsSwinging;

        // ⚔️ กด Melee (คลิกขวา)
        if (Input.GetButtonDown("Fire2"))
        {
            if (!isMeleeing && meleeObject != null)
            {
                // 🛑 โดน Melee ขัดจังหวะ -> สั่ง Reset State ปืนปัจจุบันทันที (เช่น HMG กลับมาช้าเหมือนเดิม)
                if (GetCurrentGun() != null) GetCurrentGun().ResetGunState();

                PerformMeleeCombo();
                return;
            }
        }

        // 🔫 กดยิง (คลิกซ้าย)
        if (Input.GetButton("Fire1") && !isMeleeing)
        {
            GunBase currentGun = GetCurrentGun();
            if (currentGun != null)
            {
                currentGun.Shoot();
            }
        }
    }

    private void PerformMeleeCombo()
    {
        bool isCounterClockwise = comboPattern[currentComboIndex];
        Vector3 spawnPos = meleeSpawnPoint != null ? meleeSpawnPoint.position : transform.position;
        StartCoroutine(meleeObject.ExecuteSwing(spawnPos, transform.rotation, isCounterClockwise));

        currentComboIndex = (currentComboIndex + 1) % comboPattern.Length;
    }

    // 🎯 สลับปืนตายตัว 4 ปุ่ม (ปุ่ม 1, 2, 3, 4 บนคีย์บอร์ด)
    private void HandleWeaponSwitchingFixed()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipGun(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) EquipGun(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) EquipGun(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) EquipGun(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) EquipGun(4);
    }

    private void EquipGun(int index)
    {
        if (gunsList == null || gunsList.Count <= index || index < 0) return;

        // สั่ง Reset ปืนเก่าก่อนสลับไปปืนใหม่
        if (GetCurrentGun() != null) GetCurrentGun().ResetGunState();

        currentGunIndex = index;

        for (int i = 0; i < gunsList.Count; i++)
        {
            if (gunsList[i] != null)
            {
                gunsList[i].gameObject.SetActive(i == currentGunIndex);
            }
        }
    }

    private GunBase GetCurrentGun()
    {
        if (gunsList != null && currentGunIndex < gunsList.Count)
        {
            return gunsList[currentGunIndex];
        }
        return null;
    }

    // เติมใส่ในสคริปต์ PlayerController.cs เจ้าค่ะ
    public void RefillAmmoByPercent(float percent)
    {
        foreach (GunBase gun in gunsList)
        {
            if (gun != null && !gun.isInfiniteAmmo)
            {
                int amountToAdd = Mathf.CeilToInt(gun.maxAmmo * (percent / 100f));
                gun.currentAmmo = Mathf.Min(gun.currentAmmo + amountToAdd, gun.maxAmmo);
            }
        }
    }
}