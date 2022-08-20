using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using Cinemachine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using TMPro;

public class AdventureCore : MonoBehaviour
{
    //================================================================================
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] public CinemachineVirtualCamera _virtualCamera { get; set; }
    [field: SerializeField] public List<LevelSelectController> Levels { get; set; }

    [field: Header("LOADING")]
    [field: SerializeField] public GameObject LoadingPanel { get; set; }

    [field: Header("UI COMPONENTS")]
    [field: SerializeField] private TextMeshProUGUI EnergyTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI OptibitTMP { get; set; }
    [field: SerializeField] private GameObject SettingsPanel { get; set; }
    [field: SerializeField] public Animator StageSelectAnimator { get; set; }
    [field: SerializeField] public Image SelectedStageImage { get; set; }

    [field: Header("STAGE DATA")]
    [field: SerializeField] public TextMeshProUGUI StageNumberTMP { get; set; }
    [field: SerializeField] public List<Image> MonsterSlotImages { get; set; }
    [field: SerializeField] public TextMeshProUGUI WavesCountMP { get; set; }
    [field: SerializeField] public List<Image> RewardSlotImages { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool LevelWasSelected { get; set; }
    [field: SerializeField][field: ReadOnly] public float MaxYClamp { get; set; }
    private int failedCallbackCounter;
    //================================================================================

    public void InitializeLevels()
    {
        EnergyTMP.text = "Energy: " + PlayerData.EnergyCount.ToString();
        OptibitTMP.text = PlayerData.Optibit.ToString("n0");
        if(GameManager.Instance.DebugMode)
        {
            foreach(LevelSelectController level in Levels)
            {
                if (PlayerData.CurrentStage >= level.LevelData.LevelIndex)
                {
                    level.Accessible = true;
                    level.LevelSprite.sprite = level.UnlockedSprite;
                }
                else
                {
                    level.Accessible = false;
                    level.LevelSprite.sprite = level.LockedSprite;
                }
            }
            if (PlayerData.CurrentStage >= 1 && PlayerData.CurrentStage <= 10)
                MaxYClamp = 20f;
            else if (PlayerData.CurrentStage >= 11 && PlayerData.CurrentStage <= 20)
                MaxYClamp = 57.5f;
            else if (PlayerData.CurrentStage >= 21 && PlayerData.CurrentStage <= 30)
                MaxYClamp = 95.7f;
        }
        else
        {
            DisplayLoadingPanel();
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    if(resultCallback.Data["LUID"].Value == PlayerData.LUID)
                    {
                        PlayerData.CurrentStage = int.Parse(resultCallback.Data["CurrentStage"].Value);
                        foreach (LevelSelectController level in Levels)
                        {
                            if (PlayerData.CurrentStage >= level.LevelData.LevelIndex)
                            {
                                level.Accessible = true;
                                level.LevelSprite.sprite = level.UnlockedSprite;
                            }
                            else
                            {
                                level.Accessible = false;
                                level.LevelSprite.sprite = level.LockedSprite;
                            }
                        }
                        if (PlayerData.CurrentStage >= 1 && PlayerData.CurrentStage <= 10)
                            MaxYClamp = 20f;
                        else if (PlayerData.CurrentStage >= 11 && PlayerData.CurrentStage <= 20)
                            MaxYClamp = 57.5f;
                        else if (PlayerData.CurrentStage >= 21 && PlayerData.CurrentStage <= 30)
                            MaxYClamp = 95.7f;
                        HideLoadingPanel();
                    }
                    else
                    {
                        HideLoadingPanel();
                        GameManager.Instance.DisplayDualLoginErrorPanel();
                    }
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        InitializeLevels,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    public void OpenCombatScene()
    {
        GameManager.Instance.PanelActivated = false;
        GameManager.Instance.SceneController.CurrentScene = "CombatScene";
    }

    public void CloseStageSelect()
    {
        GameManager.Instance.PanelActivated = false;
        LevelWasSelected = false;
        StageSelectAnimator.SetBool("ShowStageSelect", false);
        GameManager.Instance.CurrentLevelData = null;
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
        else
            errorAction();
    }

    private void ProcessError(string errorMessage)
    {
        HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    public void DisplayLoadingPanel()
    {
        LoadingPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
    }

    public void HideLoadingPanel()
    {
        LoadingPanel.SetActive(false);
        GameManager.Instance.PanelActivated = false;
    }

    public void OpenLobbyScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
    }

    public void OpenSettings()
    {
        SettingsPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
    }

    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
        GameManager.Instance.PanelActivated = false;
    }
}
