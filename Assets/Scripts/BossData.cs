using System;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "New Boss", menuName = "Game/Boss")]
public class BossData : ScriptableObject
{
    [Header("Boss Data")]
    public string nameBoss;
    public Sprite icon;
    //public GameObject prefabBoss3D;

    [Header("Offensive Stats")]
    [Range(0f, 1000f)]
    public float health;                            //Máu
    [Range(0f, 100f)]
    public float speed;                            //Tốc độ di chuyển
    [Range(0f, 100f)]
    public float damageAtk;                        //Sát thương tấn công
    [Range(0f, 10f)]
    public float speedAtk;                         //Tốc độ tấn công

    [Header("Skills")]
    public SkillBoss[] skills;
    public void OnValidate()
    {
        health = Math.Max(0, health);
        speed = Math.Max(0, speed);
        damageAtk = Math.Max(0, damageAtk);
        speedAtk = Math.Max(0, speedAtk);
    }
}
