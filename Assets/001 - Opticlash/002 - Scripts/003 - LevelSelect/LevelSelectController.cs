using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using PlayFab;
using PlayFab.ClientModels;

public class LevelSelectController : MonoBehaviour
{
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private AdventureCore AdventureCore { get; set; }

    [field: Header("LEVEL DATA")]
    [field: SerializeField] public LevelData LevelData { get; set; }
    [field: SerializeField] [field: ReadOnly] public bool Accessible { get; set; }

    [field: Header("VISUALS")]
    [field: SerializeField] public SpriteRenderer LevelSprite { get; set; }
    [field: SerializeField] public Sprite LockedSprite { get; set; }
    [field: SerializeField] public Sprite UnlockedSprite { get; set; }

    public void ProcessLevel()
    {
        if(!AdventureCore.LevelWasSelected)
        {
            AdventureCore.LevelWasSelected = true;
            if (Accessible)
            {
                GameManager.Instance.PanelActivated = true;
                GameManager.Instance.CurrentLevelData = LevelData;

                AdventureCore.StageNumberTMP.text = "Stage " + LevelData.LevelIndex;
                foreach (Image monserSlot in AdventureCore.MonsterSlotImages)
                    monserSlot.gameObject.SetActive(false);
                for (int i = 0; i < LevelData.MonsterList.Count; i++)
                {
                    AdventureCore.MonsterSlotImages[i].gameObject.SetActive(true);
                    AdventureCore.MonsterSlotImages[i].sprite = LevelData.MonsterSprites[i];
                }
                AdventureCore.WavesCountMP.text = LevelData.MonsterList.Count.ToString();
                foreach (Image rewardSlot in AdventureCore.RewardSlotImages)
                    rewardSlot.gameObject.SetActive(false);
                for (int i = 0; i < LevelData.RewardSprites.Count; i++)
                {
                    AdventureCore.RewardSlotImages[i].gameObject.SetActive(true);
                    AdventureCore.RewardSlotImages[i].sprite = LevelData.RewardSprites[i];
                }

                AdventureCore.StageSelectAnimator.SetBool("ShowStageSelect", true);
            }
            else
            {
                GameManager.Instance.DisplayErrorPanel("You have not yet unlocked this level");
                AdventureCore.LevelWasSelected = false;
            }
        }
    }
}
