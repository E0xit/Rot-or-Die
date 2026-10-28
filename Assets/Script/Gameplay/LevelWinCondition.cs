using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelWinCondition : MonoBehaviour
{
    public enum WinType
    {
        ClearAllEnemies, // ด่าน 1-4: ฆ่ามอนสเตอร์ให้ครบตามจำนวนที่กำหนด
        KillBoss         // ด่าน 5: ฆ่าบอสตัวเดียวจบ!
    }

    [Header("🎯 Win Condition Mode")]
    public WinType winType = WinType.ClearAllEnemies;

    [Header("📊 Settings for ClearAllEnemies (ด่าน 1-4)")]
    [Tooltip("จำนวนมอนสเตอร์ทั้งหมดที่จะให้เกิดในด่านนี้")]
    public int totalEnemiesToSpawn = 15;
    private int currentSpawnedCount = 0;
    private int currentKilledCount = 0;

    [Header("🤖 Settings for KillBoss (ด่าน 5)")]
    [Tooltip("ลาก GameObject บอสในฉากมาใส่ (หรือปล่อย null ไว้ถ้าบอสเสกมาทีหลัง)")]
    public GameObject bossObject;

    [Header("🚪 Next Scene Settings")]
    [Tooltip("ชื่อ Scene ถัดไปที่จะให้วาร์ปไป เช่น 'Level02' หรือ 'MoveToNextLevel'")]
    public string nextSceneName = "MoveToNextLevel";

    [Tooltip("ถ้าเป็นด่านสุดท้าย (ด่าน 5) ให้ติ๊กถูก ช่องนี้จะบังคับวาร์ปไป 'WinScreen'")]
    public bool isFinalLevel = false;
    public string winScreenSceneName = "WinScreen";

    [Header("⏳ Delay before Scene Transition")]
    public float delayBeforeLoad = 1.5f;
    public int CurrentKilledCount => currentKilledCount;
    public int CurrentSpawnedCount => currentSpawnedCount;

    private bool isGameWon = false;
    public GameObject playerObject;
    public float delayBeforeGameOver = 3f; // ปรับเวลาหน่วงก่อนวาร์ปไปฉาก GameOver
    public string gameOverSceneName = "GameOver";
    private void Start()
    {
        // 📝 บันทึกชื่อ Scene ปัจจุบันเก็บไว้เป็นด่านล่าสุด!
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastPlayedLevel", currentSceneName);
        PlayerPrefs.Save();
    }
    private void Update()
    {
        if (isGameWon) return;

        // 🤖 กรณีด่าน 5: เช็คบอสโดนทำลาย
        if (winType == WinType.KillBoss)
        {
            if (bossObject == null)
            {
                // ถ้าบอสโดน Destroy จนกลายเป็น null แล้ว -> ชนะทันที!
                TriggerWin();
            }
        }
    }

    /// <summary>
    /// 👾 เรียกฟังก์ชันนี้ตอน Spawner เสกมอนสเตอร์แต่ละตัวออกมา (ด่าน 1-4)
    /// </summary>
    public void OnEnemySpawned()
    {
        currentSpawnedCount++;
    }

    /// <summary>
    /// ☠️ เรียกฟังก์ชันนี้ตอนมอนสเตอร์แต่ละตัวตาย (ด่าน 1-4)
    /// </summary>
    public void OnEnemyKilled()
    {
        if (winType == WinType.KillBoss) return;

        currentKilledCount++;

        // 🎯 ฆ่าศัตรูครบตามจำนวนเป้าหมายที่ตั้งไว้เมื่อไหร่ -> ชนะทันที!
        if (currentKilledCount >= totalEnemiesToSpawn)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        if (isGameWon) return;
        isGameWon = true;

        Debug.Log("🎉 VICTORY! Level Cleared!");

        StartCoroutine(RoutineLoadNextScene());
    }

    private IEnumerator RoutineLoadNextScene()
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        // ถ้าเป็นด่านสุดท้าย ให้ไปหน้า WinScreen แต่ถ้าไม่ใช่ ให้ไปหน้า nextSceneName ที่ระบุไว้ใน Inspector
        string targetScene = isFinalLevel ? winScreenSceneName : nextSceneName;

        SceneManager.LoadScene(targetScene);
    }

    public void TriggerGameOver()
    {
        if (playerObject != null)
        {
            playerObject.SetActive(false); // ปิดตัว Player
        }

        StartCoroutine(RoutineGameOver());
    }

    private IEnumerator RoutineGameOver()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);
        SceneManager.LoadScene(gameOverSceneName);
    }
}