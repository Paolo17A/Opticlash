using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using System;

public class QuestCore : MonoBehaviour
{
    #region VARIABLES
    //=========================================================================
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }

    [field: Header("QUEST DATA")]
    [field: SerializeField] private Slider QuestProgressSlider { get; set; }
    [field: SerializeField] private Button ClaimDailyQuest { get; set; }
    [field: SerializeField] private TextMeshProUGUI DailyLogInTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI SocMedSharedTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ItemsUsedTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI MonstersKilledTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI LevelsWonTMP { get; set; }
    [field: SerializeField] private Button ClaimQuestRewardBtn { get; set; }

    [Header("DEBUGGER")]
    private int failedCallbackCounter;
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
            LevelsWonTMP.text = PlayerData.LevelsWon + "/10";
            if (PlayerData.LevelsWon >= 10)
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
        else
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    if (resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        PlayerData.DailyCheckIn = int.Parse(GameManager.Instance.DeserializeStringValue(resultCallback.Data["Quests"].Value, "DailyCheckIn"));
                        PlayerData.SocMedShared = int.Parse(GameManager.Instance.DeserializeStringValue(resultCallback.Data["Quests"].Value, "SocMedShared"));
                        PlayerData.ItemsUsed = int.Parse(GameManager.Instance.DeserializeStringValue(resultCallback.Data["Quests"].Value, "ItemsUsed"));
                        PlayerData.MonstersKilled = int.Parse(GameManager.Instance.DeserializeStringValue(resultCallback.Data["Quests"].Value, "MonstersKilled"));
                        PlayerData.LevelsWon = int.Parse(GameManager.Instance.DeserializeStringValue(resultCallback.Data["Quests"].Value, "LevelsWon"));
                        PlayerData.DailyQuestClaimed = int.Parse(GameManager.Instance.DeserializeStringValue(resultCallback.Data["Quests"].Value, "DailyQuestClaimed"));

                        QuestProgressSlider.value = 0;

                        #region QUEST 1
                        DailyLogInTMP.text = PlayerData.DailyCheckIn + "/1";
                        if (PlayerData.DailyCheckIn >= 1)
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
                        LevelsWonTMP.text = PlayerData.LevelsWon + "/10";
                        if (PlayerData.LevelsWon >= 10)
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
                    else
                    {
                        Debug.Log("Double log in");
                    }
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        InitializeQuest,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    public void DailyLogIn()
    {
        PlayerData.DailyCheckIn++;
        if (GameManager.Instance.DebugMode)
            InitializeQuest();
        else
            UpdateQuest();
    }

    public void ShareToSocMed()
    {
        new NativeShare().SetText("Start playing Opticlash!").SetUrl("https://marketplace.optibit.tech/home/customer/dashboard")
            .SetCallback((result, shareTarget) => ProcessShareResult(result))
            .Share();
    }

    private void ProcessShareResult(NativeShare.ShareResult result)
    {
        if (result == NativeShare.ShareResult.Shared)
        {
            PlayerData.SocMedShared++;
            if (GameManager.Instance.DebugMode)
                InitializeQuest();
            else
                UpdateQuest();
        }
    }

    public void ClaimDailyQuestReward()
    {
        if (PlayerData.DailyQuestClaimed == 0)
        {
            PlayerData.DailyQuestClaimed++;
            if (!GameManager.Instance.DebugMode)
            {
                LobbyCore.DisplayLoadingPanel();
                Dictionary<string, int> quests = new Dictionary<string, int>();
                quests.Add("DailyCheckIn", PlayerData.DailyCheckIn);
                quests.Add("SocMedShared", PlayerData.SocMedShared);
                quests.Add("ItemsUsed", PlayerData.ItemsUsed);
                quests.Add("MonstersKilled", PlayerData.MonstersKilled);
                quests.Add("LevelsWon", PlayerData.LevelsWon);
                quests.Add("DailyQuestClaimed", PlayerData.DailyQuestClaimed);
                string serializedQuest = JsonConvert.SerializeObject(quests);

                PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
                {
                    FunctionName = "ClaimDailyReward",
                    FunctionParameter = new { localLUID = PlayerData.LUID, quests = serializedQuest },
                    GeneratePlayStreamEvent = true
                },
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    LobbyCore.HideLoadingPanel();
                    PlayerData.Optibit += 100;
                    LobbyCore.DisplayOptibits();
                    GameManager.Instance.DisplayErrorPanel("You gained 100 Optibits. Your new balance is " + PlayerData.Optibit.ToString("n0"));
                },
                errorCallback =>
                {
                    PlayerData.DailyQuestClaimed--;
                    ErrorCallback(errorCallback.Error,
                        ClaimDailyQuestReward,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
            }
        }
        else
        {
            LobbyCore.HideLoadingPanel();
            GameManager.Instance.DisplayErrorPanel("Daily quest has already been claimed");
        }
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

    #region UTILITY

    private void UpdateQuest()
    {
        Dictionary<string, int> quests = new Dictionary<string, int>();
        quests.Add("DailyCheckIn", PlayerData.DailyCheckIn);
        quests.Add("SocMedShared", PlayerData.SocMedShared);
        quests.Add("ItemsUsed", PlayerData.ItemsUsed);
        quests.Add("MonstersKilled", PlayerData.MonstersKilled);
        quests.Add("LevelsWon", PlayerData.LevelsWon);
        quests.Add("DailyQuestClaimed", PlayerData.DailyQuestClaimed);

        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
        updateUserData.Data.Add("Quests", JsonConvert.SerializeObject(quests));
        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                InitializeQuest();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    UpdateQuest,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    private void ErrorCallback(PlayFabErrorCode errorCode, Action restartAction, Action errorAction)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                ProcessError("Connectivity error. Please connect to strong internet");
            else
                restartAction();
        }
        else if (errorCode == PlayFabErrorCode.InternalServerError)
            ProcessSpecialError();
        else
            errorAction();
    }

    private void ProcessError(string errorMessage)
    {
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }
    private void ProcessSpecialError()
    {
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
