using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaboomController : MonoBehaviour
{
    [field: SerializeField] PlayerData PlayerData { get; set; }

    private void Start()
    {
        if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C3 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C4 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C5)
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B3 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B4)
            gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A3)
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.S1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.S2)
            gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;
    }

    public void HideKaboom()
    {
        gameObject.SetActive(false);
    }
}
