using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "Weapon", menuName = "Opticlash/Data/CustomWeaponData")]
public class CustomWeaponData : ScriptableObject
{
    [field: SerializeField] public string WeaponInstanceID { get; set; }
    [field: SerializeField] public WeaponData BaseWeaponData { get; set; }
    [field: SerializeField] public float Level { get; set; }

    [field: SerializeField][field: ReadOnly] public float Attack { get; set; }
    [field: SerializeField][field: ReadOnly] public float Accuracy { get; set; }
    [field: SerializeField][field: ReadOnly] public int FragmentUpgradeCost { get; set; }
    [field: SerializeField][field: ReadOnly] public int OptibitUpgradeCost { get; set; }

    public void CalculateCannonStats()
    {
        Attack = (5 * (Level * 0.6f)) + 10;
        Accuracy = (5 * (Level * 0.4f)) + 10;
        FragmentUpgradeCost = Mathf.CeilToInt(BaseWeaponData.FragmentUpgradeCostConstant * Level);
        OptibitUpgradeCost = Mathf.CeilToInt(BaseWeaponData.OptibitUpgradeCostConstant * Level);
    }
}
