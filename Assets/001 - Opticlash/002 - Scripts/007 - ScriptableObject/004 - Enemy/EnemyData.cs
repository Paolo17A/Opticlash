using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Opticlash/Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    public enum StatusEffect { NONE, POISONED, ELECTROCUTED, BURNED, FROZEN, SCARED, DECAYED, DIZZY, HALLUCINATING,  SPICED};
    public enum Buff { NONE, BLESSED, TIMEBOMB };

    [field: Header("BASE ENEMY")]
    [field: SerializeField] public GameObject EnemyPrefab { get; set; }
    [field: SerializeField] public int Health { get; set; }
    [field: SerializeField] public int Damage { get; set; }

    [Header("BOSS VALUES")]
    public int bonusDamage;
    public int bonusDroppedXP;
    //public bool hasStatusEffect;

    [Header("STATUS EFFECT")]
    public StatusEffect statusEffect;
    public int statusEffectDamage;

    [Header("BUFFS")]
    public Buff buff;
}
