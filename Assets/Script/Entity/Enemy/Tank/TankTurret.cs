using System.Collections;
using UnityEngine;

public class BossTurret : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;         // จุดปล่อยจรวด
    public GameObject rocketPrefab;     // Prefab จรวด Rocket
    public Transform targetToHit;       // เป้าหมายที่จรวดโดนสะท้อนจะพุ่งกลับมาชน (Tank/Hull)

    [Header("Dynamic Angle Limits (Based on Objects)")]
    public Transform leftLimitObject;   // Marker ขอบเขตหมุนซ้ายสุด
    public Transform rightLimitObject;  // Marker ขอบเขตหมุนขวาสุด
    public float rotateSpeed = 120f;    // ความเร็วในการหมุน (องศา/วินาที)

    [Header("Behavior Intervals")]
    public float decisionInterval = 2f;
    [Range(0f, 100f)]
    public float rotateSuccessRate = 80f;

    [Header("Shooting Settings")]
    public float fireInterval = 3f;
    private float fireTimer;
    private float decisionTimer;

    private Coroutine rotateCoroutine;

    private void Start()
    {
        decisionTimer = decisionInterval;
        fireTimer = fireInterval;

        if (targetToHit == null && transform.parent != null)
        {
            targetToHit = transform.parent;
        }

        transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        HandleTurretDecision();
        HandleShooting();
    }

    // 🤖 1. สุ่มตัดสินใจหมุนป้อมปืน + Clamp มุมจาก Target Objects
    private void HandleTurretDecision()
    {
        decisionTimer += Time.deltaTime;

        if (decisionTimer >= decisionInterval)
        {
            decisionTimer = 0f;

            if (Random.Range(0f, 100f) <= rotateSuccessRate)
            {
                // 1. คำนวณช่วงมุม Min / Max จากตำแหน่งของ Target Objects ทั้ง 2 จุด
                float minAngle = GetAngleToTarget(leftLimitObject);
                float maxAngle = GetAngleToTarget(rightLimitObject);

                // สลับ min/max ให้ถูกต้องกรณีองศาฝั่งซ้ายมากกว่าฝั่งขวา
                float minBound = Mathf.Min(minAngle, maxAngle);
                float maxBound = Mathf.Max(minAngle, maxAngle);

                // 2. สุ่มองศาอิสระ (เช่น สุ่มช่วง -180 ถึง 180 องศา)
                float rawRandomAngle = Random.Range(-180f, 180f);

                // 3. 🎯 Clamp ค่าทันที! ไม่ให้เกินขอบเขตของ Left/Right Objects
                float clampedAngle = Mathf.Clamp(rawRandomAngle, minBound, maxBound);

                if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
                rotateCoroutine = StartCoroutine(RoutineRotateTurret(clampedAngle));
            }
        }
    }

    // 📐 Helper Function: คำนวณองศาจาก Turret ไปยัง GameObject เป้าหมาย (เทียบกับทิศ Y- ลงล่าง)
    private float GetAngleToTarget(Transform targetObj)
    {
        if (targetObj == null) return 0f;

        // ดึงทิศทางจาก Turret ไปหา Target Object
        Vector2 dir = (targetObj.position - transform.position).normalized;

        // คำนวณมุมองศา (เทียบกับแกน Y- ดิ่งลงล่าง)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;

        // แปลงช่วงให้อยู่ใน [-180, 180]
        if (angle > 180f) angle -= 360f;

        return angle;
    }

    // 🔄 Coroutine ค่อยๆ หมุน Turret ไปยังมุมที่ Clamp แล้ว
    private IEnumerator RoutineRotateTurret(float angle)
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.5f)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.localRotation = targetRotation;
    }

    // 🚀 2. ยิงจรวด
    private void HandleShooting()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            ShootRocket();
        }
    }

    private void ShootRocket()
    {
        if (rocketPrefab == null || firePoint == null) return;

        GameObject rocketObj = Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);

        Rocket rocketScript = rocketObj.GetComponent<Rocket>();
        if (rocketScript != null)
        {
            rocketScript.SetBossTarget(targetToHit);
        }
    }

    // 🎨 Gizmos แสดงเส้นขอบเขตซ้าย-ขวาจริงใน Scene View
    private void OnDrawGizmosSelected()
    {
        if (leftLimitObject != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, leftLimitObject.position);
        }
        if (rightLimitObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, rightLimitObject.position);
        }
    }
}