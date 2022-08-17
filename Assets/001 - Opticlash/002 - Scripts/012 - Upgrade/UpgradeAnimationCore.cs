using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeAnimationCore : MonoBehaviour
{
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }
    [field: SerializeField] private GameObject ShiningCannon { get; set; }
    [field: SerializeField] private SpriteRenderer CannonSprite { get; set; }
    [field: SerializeField] public Animator UpgradeCannonAnimator { get; set; }

    public void HideUpgrade()
    {
        UpgradeCannonAnimator.ResetTrigger("success");
        ShiningCannon.SetActive(true);
        CannonSprite.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.AnimatedSprite;
        gameObject.SetActive(false);
        LobbyCore.GrantAmountTMP.gameObject.SetActive(true);
        LobbyCore.GrantAmountTMP.text = "YOU HAVE UPGRADED " + PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode.ToString() + " CANNON TO LEVEL " + PlayerData.ActiveCustomWeapon.Level;
        LobbyCore.OkBtn.SetActive(true);
    }
}
