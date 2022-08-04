using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [field: SerializeField] private List<WeaponData> WeaponData { get; set; }


    public WeaponData GetProperWeaponData(string _value)
    {
        foreach (WeaponData weapon in WeaponData)
            if (weapon.ThisWeaponCode.ToString() == _value)
                return weapon;

        return null;
    }
}
