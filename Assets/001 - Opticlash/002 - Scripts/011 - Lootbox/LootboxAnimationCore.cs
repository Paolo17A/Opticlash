using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class LootboxAnimationCore : MonoBehaviour
{
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }
    [field: SerializeField] private LootboxCore LootboxCore { get; set; }
    [field: SerializeField] public Animator LootboxAnimator { get; set; }

    [field: SerializeField] [field: ReadOnly] public int RewardIndex { get; set; }

    public void HideLootbox()
    {
        LootboxAnimator.ResetTrigger("open");
        switch (RewardIndex)
        {
            case 1:
                LootboxCore.OptibitReward.SetActive(true);
                break;
            case 2:
                LootboxCore.FragmentReward.SetActive(true);
                break;
            case 3:
                LootboxCore.CannonReward.SetActive(true);
                break;
        }
        gameObject.SetActive(false);
        LobbyCore.GrantAmountTMP.gameObject.SetActive(true);
        LobbyCore.OkBtn.SetActive(true);
    }
}
