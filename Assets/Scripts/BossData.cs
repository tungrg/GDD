using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Boss", menuName = "Game/Boss")]
public class BossData : ScriptableObject
{
    [Header("Boss Info")]
    public string nameBoss;
    public Sprite icon;

    [Header("Original Stats")]
    [Range(0f, 1000f)] public float originalHealth = 100;
    [Range(0f, 100f)] public float originalSpeed = 5;
    [Range(0f, 100f)] public float originalDamageAtk = 10;
    [Range(0f, 10f)] public float originalSpeedAtk = 1f;

    [Header("Runtime Stats")]
    [NonSerialized] public float health;
    public float speed;
    [NonSerialized] public float damageAtk;
    [NonSerialized] public float speedAtk;

    [Header("Skills")]
    public SkillBoss[] skills;

    public void ResetRuntimeStats()
    {
        health = originalHealth;
        speed = originalSpeed;
        damageAtk = originalDamageAtk;
        speedAtk = originalSpeedAtk;
    }

    private void OnValidate()
    {
        originalHealth = Math.Max(0, originalHealth);
        originalSpeed = Math.Max(0, originalSpeed);
        originalDamageAtk = Math.Max(0, originalDamageAtk);
        originalSpeedAtk = Math.Max(0, originalSpeedAtk);
    }
}
