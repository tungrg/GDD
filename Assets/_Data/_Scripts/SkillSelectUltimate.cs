using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillSelectUltimate : MonoBehaviour
{
    public GameObject skillContainer;


    private void Update()
    {
        List<string> names = UltimateManager.Instance.skills.Where(skill => skill.unlock).Select(skill => skill.skillName).ToList();
        foreach (Transform skill in skillContainer.transform)
        {
            skill.gameObject.SetActive(names.Contains(skill.name));
        }
    }
}
