using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System;
using Newtonsoft.Json;

public class ClaimCore : MonoBehaviour
{
    //=====================================================================
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }
    [field: SerializeField] private GameObject RewardedCostume { get; set; }
    [field: SerializeField] private SpriteRenderer CostumeSprite { get; set; }

    [field: Header("INPUT")]
    [field: SerializeField] private TMP_InputField CodeInputField { get; set; }

    [field: Header("DEBUGGER")]
    private int failedCallbackCounter;
    private bool mayClaimFreeCostume;
    private string unownedCostume;
    //=====================================================================

    public void ProcessInputCode()
    {
        if (GameManager.Instance.DebugMode)
        {
            GameManager.Instance.DisplayErrorPanel("This does not work in debug mode");
        }
        else
        {
            GameManager.Instance.DisplayErrorPanel("Code is invalid");
            /*if (CodeInputField.text == "CBS4L1FE0PT1CL4SHB3T4")
            {
                mayClaimFreeCostume = false;
                unownedCostume = "";
                foreach (CustomCostumeData costume in PlayerData.OwnedCostumes)
                    if (!costume.CostumeIsOwned)
                    {
                        mayClaimFreeCostume = true;
                        unownedCostume = costume.BaseCostumeData.CostumeID;
                        break;
                    }

                if (mayClaimFreeCostume)
                    GetUserData(CodeInputField.text, unownedCostume);
                else
                    GameManager.Instance.DisplayErrorPanel("You already own every possible costume");

            }
            else
            {
                GameManager.Instance.DisplayErrorPanel("Code is invalid");
            }*/
        }
    }

    #region PLAYFAB API
    private void GetUserData(string _inputCode, string _unownedCostume)
    {
        LobbyCore.DisplayLoadingPanel();
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Data["LUID"].Value == PlayerData.LUID)
                {
                    if(resultCallback.Data.ContainsKey("RedeemCode"))
                    {
                        List<string> claimedCodes = JsonConvert.DeserializeObject<List<string>>(resultCallback.Data["RedeemCode"].Value);
                        if (claimedCodes.Contains(_inputCode))
                        {
                            LobbyCore.HideLoadingPanel();
                            GameManager.Instance.DisplayErrorPanel("You have already claimed this code before");
                            CodeInputField.text = "";
                        }
                        else
                            UpdateUserData(claimedCodes, _inputCode, _unownedCostume);
                    }
                    else
                    {
                        List<string> claimedCodes = new List<string>();
                        UpdateUserData(claimedCodes, _inputCode, _unownedCostume);
                    }
                }
                else
                {
                    LobbyCore.HideLoadingPanel();
                    GameManager.Instance.DisplayDualLoginErrorPanel();
                }
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => GetUserData(_inputCode, _unownedCostume),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void UpdateUserData(List<string> _claimedCodes, string _inputCode, string _unownedCostume)
    {
        _claimedCodes.Add(_inputCode);
        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
        updateUserData.Data.Add("RedeemCode", JsonConvert.SerializeObject(_claimedCodes));

        PlayFabClientAPI.UpdateUserData(updateUserData,
        resultCallback =>
        {
            failedCallbackCounter = 0;
            GrantCostumeCloudscript(_unownedCostume);
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                    () => UpdateUserData(_claimedCodes, _inputCode, _unownedCostume),
                    () => ProcessError(errorCallback.ErrorMessage));
        });
    }

    private void GrantCostumeCloudscript(string _unownedCostume)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GrantCostume",
            FunctionParameter = new { costume = _unownedCostume },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            Debug.Log(resultCallback.FunctionResult);
            CodeInputField.text = "";
            GameManager.Instance.DisplayErrorPanel("A new costume has been added to your inventory");
            LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CORE;
            LobbyCore.InitializeLobby();
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                        () => GrantCostumeCloudscript(_unownedCostume),
                        () => ProcessError(errorCallback.ErrorMessage));
        });
    }
    #endregion

    #region UTILITY
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
