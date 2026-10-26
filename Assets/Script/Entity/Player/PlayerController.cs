using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement & Boundary")]
    public float moveSpeed = 8f;
    public Transform leftBoundary;
    public Transform rightBoundary;

    [Header("Weapon System")]
    public List<GunBase> gunsList;
    private int currentGunIndex = 0;

    [Header("Melee Combo Settings")]
    public MeleeWeapon meleeObject;    // ลาก GameObject สี่เหลี่ยมผืนผ้า (ที่มีสคริปต์ MeleeWeapon) มาวาง
    public Transform meleeSpawnPoint;  // จุดโผล่ตรงหน้า Player
    
    // Pattern คอมโบ: true = ทวนเข็ม (CCW), false = ตามเข็ม (CW)
    // ลำดับ: ทวนเข็ม -> ทวนเข็ม -> ตามเข็ม
    private bool[] comboPattern = new bool[] { true, true, false };
    private int currentComboIndex = 0;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        EquipGun(0);
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleWeaponSwitching();
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
        // เช็คว่ากำลังฟันอยู่หรือไม่
        bool isMeleeing = meleeObject != null && meleeObject.IsSwinging;

        // ⚔️ กด Melee (คลิกขวาอย่างเดียว เอา E ออกเพื่อไม่ให้ตีกับปุ่มสลับปืน)
        if (Input.GetButtonDown("Fire2"))
        {
            if (!isMeleeing && meleeObject != null)
            {
                PerformMeleeCombo();
                return;
            }
        }

        // 🔫 กดยิง (คลิกซ้าย) -> จะยิงได้ก็ต่อเมื่อ "ไม่ได้กำลังฟันมีดอยู่"
        if (Input.GetButton("Fire1") && !isMeleeing)
        {
            if (gunsList.Count > 0 && gunsList[currentGunIndex] != null)
            {
                gunsList[currentGunIndex].Shoot();
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

    private void HandleWeaponSwitching()
    {
        // ถ้าไม่มีปืน หรือมีแค่ปืนเดียว ไม่ต้องสลับให้เสียเวลา
        if (gunsList == null || gunsList.Count <= 1) return;

        int previousIndex = currentGunIndex;

        // 1. ตรวจจับ Input จาก Scroll Wheel และปุ่ม Q / E
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // ถอนลูกกลิ้งขึ้น (Scroll Up) หรือ กด E -> สลับไปปืนถัดไป (Next Weapon)
        if (scroll > 0f || Input.GetKeyDown(KeyCode.E))
        {
            currentGunIndex = (currentGunIndex + 1) % gunsList.Count;
        }
        // เลื่อนลูกกลิ้งลง (Scroll Down) หรือ กด Q -> สลับไปปืนก่อนหน้า (Previous Weapon)
        else if (scroll < 0f || Input.GetKeyDown(KeyCode.Q))
        {
            // บวก gunsList.Count เพิ่มเพื่อกันไม่ให้ค่าติดลบก่อน Modulo เจ้าค่ะ
            currentGunIndex = (currentGunIndex - 1 + gunsList.Count) % gunsList.Count;
        }

        // 2. ถ้ามีการเปลี่ยน Index จริงๆ ถึงค่อยเรียก EquipGun สลับภาพ/โมเดล
        if (previousIndex != currentGunIndex)
        {
            EquipGun(currentGunIndex);
        }
    }

    private void EquipGun(int index)
    {
        if (gunsList == null || gunsList.Count == 0) return;
        if (index < 0 || index >= gunsList.Count) return;

        currentGunIndex = index;

        // เปิดเฉพาะปืนที่เลือกอยู่ ปิดปืนที่เหลือให้หมด
        for (int i = 0; i < gunsList.Count; i++)
        {
            if (gunsList[i] != null)
            {
                gunsList[i].gameObject.SetActive(i == currentGunIndex);
            }
        }
    }
}