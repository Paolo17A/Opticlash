using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Opticlash/Data/CustomWeaponData")]
public class CustomWeaponData : ScriptableObject
{
    [field: SerializeField] public string WeaponInstanceID { get; set; }
    [field: SerializeField] public WeaponData BaseWeaponData { get; set; }
    [field: SerializeField] public int BonusDamage { get; set; }

}
