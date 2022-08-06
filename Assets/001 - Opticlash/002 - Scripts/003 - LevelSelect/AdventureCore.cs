using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using Cinemachine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class AdventureCore : MonoBehaviour
{
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] public CinemachineVirtualCamera _virtualCamera { get; set; }
    [field: SerializeField] public List<LevelSelectController> Levels { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool LevelWasSelected { get; set; }
    private int failedCallbackCounter;
    public void InitializeLevels()
    {
        if(GameManager.Instance.DebugMode)
        {
            foreach(LevelSelectController level in Levels)
            {
                if (PlayerData.CurrentStage >= level.LevelData.LevelIndex)
                    level.Accessible = true;
                else
                    level.Accessible = false;
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
                                level.Accessible = true;
                            else
                                level.Accessible = false;
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
}
