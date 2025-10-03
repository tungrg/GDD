using UnityEngine;
using System.Collections.Generic;

public static class SaveLoadManager
{
    // PlayerPrefs key for skill progress
    private static string skillProgressKey = "SkillProgress";

    public static void SaveSkills(UltimateManager.Skill[] skills)
    {
        SkillSaveData data = new SkillSaveData();

        foreach (var skill in skills)
        {
            data.unlockedSkills.Add(skill.unlock);
        }

        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(skillProgressKey, json);
        PlayerPrefs.Save(); // Force immediate write to disk
        Debug.Log("Skills saved to PlayerPrefs");
    }

    public static void LoadSkills(UltimateManager.Skill[] skills)
    {
        if (!PlayerPrefs.HasKey(skillProgressKey))
        {
            Debug.Log("No save data found in PlayerPrefs. Initializing default skills.");
            InitializeDefaultSkills(skills);
            return;
        }

        string json = PlayerPrefs.GetString(skillProgressKey);
        SkillSaveData data = JsonUtility.FromJson<SkillSaveData>(json);

        if (data != null && data.unlockedSkills.Count > 0)
        {
            int min = Mathf.Min(data.unlockedSkills.Count, skills.Length);
            for (int i = 0; i < min; i++)
            {
                skills[i].unlock = data.unlockedSkills[i];
            }
        }

        // Skill đầu tiên luôn unlock
        if (skills.Length > 0) skills[0].unlock = true;

        Debug.Log("Skills loaded from PlayerPrefs");
    }

    private static void InitializeDefaultSkills(UltimateManager.Skill[] skills)
    {
        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].unlock = (i == 0);
        }
    }

    /// <summary>
    /// Check if skill progress save data exists
    /// </summary>
    public static bool HasSaveData()
    {
        return PlayerPrefs.HasKey(skillProgressKey);
    }

    /// <summary>
    /// Delete all skill progress (reset skills)
    /// </summary>
    public static void ResetSkillProgress()
    {
        if (PlayerPrefs.HasKey(skillProgressKey))
        {
            PlayerPrefs.DeleteKey(skillProgressKey);
            PlayerPrefs.Save();
            Debug.Log("Skill progress reset from PlayerPrefs");
        }
        else
        {
            Debug.Log("No skill progress to reset");
        }
    }

    /// <summary>
    /// Debug: Show skill save data content
    /// </summary>
    public static void ShowSkillSaveData()
    {
        if (PlayerPrefs.HasKey(skillProgressKey))
        {
            string json = PlayerPrefs.GetString(skillProgressKey);
            Debug.Log("=== SKILL PROGRESS DATA ===");
            Debug.Log(json);
            Debug.Log("=== END SKILL DATA ===");
        }
        else
        {
            Debug.Log("No skill progress data found in PlayerPrefs");
        }
    }

    [System.Serializable]
    public class SkillSaveData
    {
        public List<bool> unlockedSkills = new List<bool>();
    }
}
