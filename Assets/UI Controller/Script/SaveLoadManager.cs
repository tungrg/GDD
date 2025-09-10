using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveLoadManager
{
    private static string fileName = "SkillProgress.json";
    private static string path = Path.Combine(Application.persistentDataPath, fileName);

    public static void SaveSkills(UltimateManager.Skill[] skills)
    {
        SkillSaveData data = new SkillSaveData();

        foreach (var skill in skills)
        {
            data.unlockedSkills.Add(skill.unlock);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Skills saved to: " + path);
    }

    public static void LoadSkills(UltimateManager.Skill[] skills)
    {
        if (!File.Exists(path))
        {
            Debug.Log("No save file found. Initializing default skills.");
            InitializeDefaultSkills(skills);
            return;
        }

        string json = File.ReadAllText(path);
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

        Debug.Log("Skills loaded from: " + path);
    }

    private static void InitializeDefaultSkills(UltimateManager.Skill[] skills)
    {
        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].unlock = (i == 0);
        }
    }

    [System.Serializable]
public class SkillSaveData
{
    public List<bool> unlockedSkills = new List<bool>();
}
}
