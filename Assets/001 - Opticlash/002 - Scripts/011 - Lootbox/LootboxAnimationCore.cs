using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class LootboxAnimationCore : MonoBehaviour
{
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }
    [field: SerializeField] private LootboxCore LootboxCore { get; set; }
    [field: SerializeField] public Animator LootboxAnimator { get; set; }

    public void HideLootbox()
    {
        LootboxAnimator.ResetTrigger("open");
        LootboxCore.FragmentReward.SetActive(true);
        gameObject.SetActive(false);
        LobbyCore.GrantAmountTMP.gameObject.SetActive(true);
        LobbyCore.OkBtn.SetActive(true);
        LootboxCore.RewardIndex = 0;
    }
}
