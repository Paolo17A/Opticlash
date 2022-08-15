using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                AdventureCore.SelectedStageImage.sprite = GameManager.Instance.CurrentLevelData.SelectedLevelSprite;
                AdventureCore.StageSelectAnimator.SetBool("ShowStageSelect", true);
                //OpenCombatScene();
            }
            else
            {
                GameManager.Instance.DisplayErrorPanel("You have not yet unlocked this level");
                AdventureCore.LevelWasSelected = false;
            }
        }
    }


    
}
