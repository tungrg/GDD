using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Boss", menuName = "Game/Boss")]
public class BossData : ScriptableObject
{
    [Header("Boss Info")]
    public string nameBoss;
    public Sprite icon;

    [Header("Stats")]
    [Range(0f, 1000f)]
    public float health = 100;
    [Range(0f, 100f)]
    public float speed = 5;
    [Range(0f, 100f)]
    public float damageAtk = 10;
    [Range(0f, 10f)]
    public float speedAtk = 1f;

    [Header("Skills")]
    public SkillBoss[] skills;

    private void OnValidate()
    {
        health = Math.Max(0, health);
        speed = Math.Max(0, speed);
        damageAtk = Math.Max(0, damageAtk);
        speedAtk = Math.Max(0, speedAtk);
    }
}
