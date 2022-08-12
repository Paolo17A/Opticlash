using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [field: Header("UI COMPONENTS")]
    [field: SerializeField] private TextMeshProUGUI EnergyTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI OptibitTMP { get; set; }
    [field: SerializeField] private GameObject SettingsPanel { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool LevelWasSelected { get; set; }
    private int failedCallbackCounter;
    //================================================================================

    public void InitializeLevels()
    {
        EnergyTMP.text = "Energy: " + PlayerData.EnergyCount.ToString();
        OptibitTMP.text = PlayerData.Optibit.ToString();
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
        }
        else
        {
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
                    }
                    else
                    {
                        Debug.Log("Dual log in");
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
        //HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
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
