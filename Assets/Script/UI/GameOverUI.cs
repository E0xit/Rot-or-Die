using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Fallback Level")]
    [Tooltip("ชื่อด่านสำรองกรณีหาด่านล่าสุดไม่เจอ")]
    public string defaultLevelName = "Level01";

    /// <summary>
    /// 🔄 ผูกฟังก์ชันนี้กับปุ่ม "Try Again / Restart"
    /// </summary>
    public void OnClickRestartLevel()
    {
        // อ่านชื่อด่านล่าสุดที่เคยบันทึกไว้ใน PlayerPrefs
        string lastLevel = PlayerPrefs.GetString("LastPlayedLevel", defaultLevelName);

        Debug.Log($"🔄 Restarting Level: {lastLevel}");

        // โหลดฉากด่านปัจจุบันกลับมาเล่นใหม่ทันที!
        SceneManager.LoadScene(lastLevel);
    }

    /// <summary>
    /// 🏠 ผูกฟังก์ชันนี้กับปุ่ม "Main Menu" (ถ้ามี)
    /// </summary>
    public void OnClickMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickPlay()
    {
        SceneManager.LoadScene("Level1");
    }
}