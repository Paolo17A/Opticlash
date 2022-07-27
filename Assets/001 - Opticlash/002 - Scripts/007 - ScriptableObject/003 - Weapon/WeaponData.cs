using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "Weapon", menuName = "Opticlash/Data/WeaponData")]
public class WeaponData : ScriptableObject
{
    public enum WeaponCode { NONE, C1, C2, C3, C4, C5, B1, B2, B3, B4, A1, A2, A3, S1, S2 };
    public enum SideEffect { NONE, STRONGER, BREAK, WEAK, FREEZE, PARALYZE, CONFUSE, BURN }

    [field: SerializeField] public WeaponCode ThisWeaponCode { get; set; }
    [field: SerializeField] public string WeaponName { get; set; }
    [field: SerializeField] public float BaseDamage { get; set; }
    [field: SerializeField] public int Accuracy { get; set; }
    [field: SerializeField] public int StartingAmmo { get; set; }

    [field: Header("SIDE EFFECTS")]
    [field: SerializeField] public List<SideEffect> SideEffects { get; set; }
    [field: SerializeField] public List<int> SideEffectsRate { get; set; }
    [field: SerializeField] public List<int> SideEffectsFrequency { get; set; }
    [field: SerializeField] public List<int> SideEffectsDuration { get; set; }

    [field: Header("STRONGER CRIT")]
    [field: SerializeField] public bool HasCrit { get; set; }
    [field: SerializeField] public float CritMultiplier { get; set; }
    [field: SerializeField] public int CritRate { get; set; }
    [field: SerializeField] public int CritFrequency { get; set; }

    [field: Header("BONUS BULLETS")]
    [field: SerializeField] public bool HasBonusBullets { get; set; }
    [field: SerializeField] public int BonusBullets { get; set; }
    [field: SerializeField] public int BonusRate { get; set; }
    [field: SerializeField] public int BonusFrequency { get; set; }

    [field: Header("SPRITES")]
    [field: SerializeField] public Sprite BackSprite { get; set; }
    [field: SerializeField] public Sprite MiddleSprite { get; set; }
    [field: SerializeField] public Sprite FrontSprite { get; set; }
    [field: SerializeField] public Sprite EquippedSprite {get; set;}
    [field: SerializeField] public Sprite InfoSprite { get; set; }
    [field: SerializeField] public Sprite Ammo { get; set; }
}
