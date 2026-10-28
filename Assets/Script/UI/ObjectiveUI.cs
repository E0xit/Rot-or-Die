using UnityEngine;
using TMPro;

public class ObjectiveUI : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("ลาก TextMeshProUGUI ที่จะใช้แสดงข้อความภารกิจมาใส่")]
    public TextMeshProUGUI objectiveText;

    [Header("Target Reference")]
    [Tooltip("ลาก GameObject ที่มีสคริปต์ LevelWinCondition มาใส่")]
    public LevelWinCondition winConditionManager;

    [Header("Custom Messages")]
    public string enemyObjectivePrefix = "Enemies Remaining: "; // หรือ "Enemies Defeated: "
    public string bossObjectiveText = "Objective: Defeat the Boss!";
    public string victoryText = "Objective: Complete!";

    private void Start()
    {
        // ถ้าไม่ได้ลาก winConditionManager มา ให้แอบหาใน Scene อัตโนมัติ
        if (winConditionManager == null)
        {
            winConditionManager = FindAnyObjectByType<LevelWinCondition>();
        }

        UpdateObjectiveUI();
    }

    private void Update()
    {
        UpdateObjectiveUI();
    }

    public void UpdateObjectiveUI()
    {
        if (objectiveText == null || winConditionManager == null) return;

        // 🎯 กรณีที่ 1: ด่าน 5 สู้บอส (KillBoss)
        if (winConditionManager.winType == LevelWinCondition.WinType.KillBoss)
        {
            if (winConditionManager.bossObject != null)
            {
                objectiveText.text = bossObjectiveText;
            }
            else
            {
                objectiveText.text = victoryText;
            }
        }
        // 🎯 กรณีที่ 2: ด่าน 1-4 กำจัดมอนสเตอร์ (ClearAllEnemies)
        else
        {
            int current = winConditionManager.CurrentKilledCount;
            int total = winConditionManager.totalEnemiesToSpawn;

            // รูปแบบ A: แสดงจำนวนที่ฆ่าได้แล้วเทียบกับเป้าหมาย (เช่น Enemies Defeated: 5/15)
            objectiveText.text = $"{enemyObjectivePrefix}{current}/{total}";

            // รูปแบบ B (ถ้าชอบแบบถอยหลัง): แสดงมอนที่เหลืออยู่
            // int remaining = Mathf.Max(0, total - current);
            // objectiveText.text = $"Enemies Remaining: {remaining}";
        }
    }
}