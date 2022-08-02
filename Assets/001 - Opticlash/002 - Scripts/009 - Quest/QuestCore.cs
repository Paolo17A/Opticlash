using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestCore : MonoBehaviour
{
    #region VARIABLES
    //=========================================================================
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    [field: Header("QUEST DATA")]
    [field: SerializeField] private Slider QuestProgressSlider { get; set; }
    [field: SerializeField] private Button ClaimDailyQuest { get; set; }
    [field: SerializeField] private TextMeshProUGUI DailyLogInTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI SocMedSharedTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ItemsUsedTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI MonstersKilledTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI BossesKilledTMP { get; set; }
    [field: SerializeField] private Button ClaimQuestRewardBtn { get; set; }
    //=========================================================================
    #endregion

    public void InitializeQuest()
    {
        if(GameManager.Instance.DebugMode)
        {
            QuestProgressSlider.value = 0;

            #region QUEST 1
            DailyLogInTMP.text = PlayerData.DailyCheckIn + "/1";
            if(PlayerData.DailyCheckIn >= 1)
            {
                PlayerData.Quest1Claimed = true;
                ClaimDailyQuest.interactable = false;
                QuestProgressSlider.value += 0.2f;
            }
            else
            {
                PlayerData.Quest1Claimed = false;
                ClaimDailyQuest.interactable = true;
            }
            #endregion
            #region QUEST 2
            SocMedSharedTMP.text = PlayerData.SocMedShared + "/1";
            if (PlayerData.SocMedShared >= 1)
            {
                PlayerData.Quest2Claimed = true;
                QuestProgressSlider.value += 0.2f;
            }
            else
                PlayerData.Quest2Claimed = false;
            #endregion
            #region QUEST 3
            ItemsUsedTMP.text = PlayerData.ItemsUsed + "/5";
            if (PlayerData.ItemsUsed >= 5)
            {
                PlayerData.Quest3Claimed = true;
                QuestProgressSlider.value += 0.2f;
            }
            else
                PlayerData.Quest3Claimed = false;
            #endregion
            #region QUEST 4
            MonstersKilledTMP.text = PlayerData.MonstersKilled + "/20";
            if (PlayerData.MonstersKilled >= 20)
            {
                PlayerData.Quest4Claimed = true;
                QuestProgressSlider.value += 0.2f;
            }
            else
                PlayerData.Quest4Claimed = false;
            #endregion
            #region QUEST 5
            BossesKilledTMP.text = PlayerData.BossesKilled + "/2";
            if (PlayerData.BossesKilled >= 2)
            {
                PlayerData.Quest5Claimed = true;
                QuestProgressSlider.value += 0.2f;
            }
            else
                PlayerData.Quest5Claimed = false;
            #endregion

            if (QuestProgressSlider.value == 1)
                ClaimQuestRewardBtn.interactable = true;
            else
                ClaimQuestRewardBtn.interactable = false;
        }
    }

    public void DailyLogIn()
    {
        if(GameManager.Instance.DebugMode)
        {
            PlayerData.DailyCheckIn++;
            InitializeQuest();
        }
    }

    public void ShareToSocMed()
    {
        new NativeShare().SetText("Start playing Opticlash with my referral link!").SetUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ")
            .SetCallback((result, shareTarget) => ProcessShareResult(result))
            .Share();
    }

    private void ProcessShareResult(NativeShare.ShareResult result)
    {
        if (result == NativeShare.ShareResult.Shared)
        {
            if(GameManager.Instance.DebugMode)
            {
                PlayerData.SocMedShared++;
                InitializeQuest();
            }
        }
    }

    public void ClaimDailyQuestReward()
    {
        PlayerData.DailyQuestClaimed++;
            //Advertisements.Instance.ShowRewardedVideo(CompleteMethod);
    }
    private void CompleteMethod(bool completed)
    {
        if (completed)
        {
            Debug.Log("Quest 3 will be accomplished");
            if(GameManager.Instance.DebugMode)
                PlayerData.DailyQuestClaimed++;
            else
            {

            }
        }
        else
        {
            Debug.Log("Ad was not finished");
        }
    }
}
