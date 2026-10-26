using System.Collections.Generic;
using UnityEngine;

public class MultiEnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyData
    {
        public string enemyName;          // ชื่อเรียกไว้ดูใน Inspector
        public GameObject enemyPrefab;    // Prefab มอนสเตอร์
        
        [Header("Spawn Controls")]
        public bool isEnabled = true;     // เปิด/ปิด การเกิดของตัวนี้ (ถ้า false จะไม่เกิดเลย)
        public float baseInterval = 3f;   // ระยะเวลาเกิดพื้นฐาน (วินาที)
        [Range(0f, 100f)] 
        public float spawnWeight = 50f;   // โอกาสเกิดเป็น % (0 - 100%)
        
        [Tooltip("ตัวคูณเฉพาะตัว (ใส่ 0 เพื่อปิด, ใส่ 2 เพื่อให้เกิดไวขึ้นสองเท่า)")]
        public float localMultiplier = 1f;

        [HideInInspector] public float timer; // ตัวนับเวลาแยกของแต่ละตัว
    }

    [Header("Enemy Configuration")]
    public List<EnemyData> enemyList;

    [Header("Boundary Markers")]
    public Transform pointA;
    public Transform pointB;

    private void Update()
    {
        if (enemyList == null || pointA == null || pointB == null) return;

        // คำนวณ Global Multiplier (ถ้าตั้งค่าไว้ <= 0 แสดงว่าหยุดสปอว์นทั้งระบบ)
        float globalMult = GlobalSettings.SpawnRateMultiplier;
        if (globalMult <= 0f) return;

        foreach (var enemy in enemyList)
        {
            // ถ้าไม่ได้เปิดใช้งาน หรือไม่มี Prefab ให้ข้ามไปเลย
            if (!enemy.isEnabled || enemy.enemyPrefab == null || enemy.localMultiplier <= 0f) 
                continue;

            enemy.timer += Time.deltaTime;

            // คำนวณ Interval จริงตามความยากรวม (ยิ่งความยากเยอะ Interval จริงยิ่งน้อย = เกิดไวขึ้น)
            float effectiveMultiplier = globalMult * enemy.localMultiplier;
            float currentInterval = enemy.baseInterval / effectiveMultiplier;

            if (enemy.timer >= currentInterval)
            {
                enemy.timer = 0f; // รีเซ็ต тайเมอร์

                // เช็คโอกาสการเกิดตาม Weight (%)
                float randomRoll = Random.Range(0f, 100f);
                if (randomRoll <= enemy.spawnWeight)
                {
                    SpawnEnemy(enemy.enemyPrefab);
                }
            }
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        float t = Random.Range(0f, 1f);
        Vector3 spawnPosition = Vector3.Lerp(pointA.position, pointB.position, t);
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);
        }
    }
}