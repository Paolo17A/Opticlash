using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Security.Cryptography;
using System.Text;
using System;
using Newtonsoft.Json;

public class LoginCore : MonoBehaviour
{
    //===============================================================================================
    [field: SerializeField] private EntryCore EntryCore { get; set; }
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    [Header("PLAYFAB VARIABLES")]
    private RegisterPlayFabUserRequest registerPlayFabUser;
    private LoginWithPlayFabRequest loginWithPlayFab;
    private LoginWithEmailAddressRequest loginWithEmailAddress;

    [Header("DEBUGGER")]
    private int failedCallbackCounter;
    private Guid myGUID;
    private string quests;
    //===============================================================================================
    private void Awake()
    {
        registerPlayFabUser = new RegisterPlayFabUserRequest();
        loginWithPlayFab = new LoginWithPlayFabRequest();
        loginWithEmailAddress = new LoginWithEmailAddressRequest();
    }

    #region REGISTRATION
    public void RegisterNewUserPlayfab()
    {
        EntryCore.DisplayLoadingPanel();
        registerPlayFabUser.TitleId = "BB42A";
        registerPlayFabUser.Email = "testuser10@gmail.com";
        registerPlayFabUser.Username = "testuser10";
        registerPlayFabUser.DisplayName = "testuser10";
        registerPlayFabUser.Password = "password";

        PlayFabClientAPI.RegisterPlayFabUser(registerPlayFabUser,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                RegistrationDataInitializationCloudscript();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    FailedAction,
                    RegisterNewUserPlayfab,
                    () => GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage));
            });
    }

    public void RegistrationDataInitializationCloudscript()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "RegistrationDataInitialization",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            EntryCore.HideLoadingPanel();
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                    FailedAction,
                    RegistrationDataInitializationCloudscript,
                    () => ProcessError(errorCallback.ErrorMessage));
        });
    }
    #endregion

    public void LoginUserPlayfab(string username, string password)
    {
        EntryCore.DisplayLoadingPanel();
        loginWithPlayFab.Username = username;
        loginWithPlayFab.Password = "password";

        PlayFabClientAPI.LoginWithPlayFab(loginWithPlayFab,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.PlayfabID = resultCallback.PlayFabId;
                PlayerData.DisplayName = username;
                GetEncrypedPassword(username, password);
            },
            errorCallback =>
            {
                LoginEmailPlayfab(username, password);
            });
    }

    private void LoginEmailPlayfab(string email, string password)
    {
        loginWithEmailAddress.Email = email;
        loginWithEmailAddress.Password = password;

        PlayFabClientAPI.LoginWithEmailAddress(loginWithEmailAddress,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.PlayfabID = resultCallback.PlayFabId;
                PlayerData.DisplayName = "";
                GetEncrypedPassword(email, password);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error, () =>
                {
                    EntryCore.HideLoadingPanel();
                    GameManager.Instance.DisplayErrorPanel("Connectivity error. Please connect to strong internet");
                },
                    () => LoginEmailPlayfab(email, password),
                    () =>
                    {
                        EntryCore.HideLoadingPanel();
                        GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage);
                    });
            });
    }

    public void GetEncrypedPassword(string username, string password)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if(resultCallback.Data.ContainsKey("Password"))
                {
                    Debug.Log(Encrypt(password));
                    if(resultCallback.Data["Password"].Value == Encrypt(password))
                    {
                        LoginCredentials(username, password);
                    }
                    else
                    {
                        EntryCore.HideLoadingPanel();
                        EntryCore.ResetLoginPanel();
                        PlayerData.ResetPlayerData();
                        PlayFabClientAPI.ForgetAllCredentials();
                        GameManager.Instance.DisplayErrorPanel("Incorrect Password");
                    }
                }
                else
                {
                    EntryCore.HideLoadingPanel();
                    GameManager.Instance.DisplayErrorPanel("Please set up your password in the website");
                }
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error, () =>
                {
                    EntryCore.HideLoadingPanel();
                    GameManager.Instance.DisplayErrorPanel("Connectivity error. Please connect to strong internet");
                },
                    () => GetEncrypedPassword(username, password),
                    () =>
                    {
                        EntryCore.HideLoadingPanel();
                        GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage);
                    });
            });
    }

    public void LoginCredentials(string username, string password)
    {
        myGUID = Guid.NewGuid();
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "LoginCredentials",
            FunctionParameter = new { LUID = myGUID.ToString() },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            Debug.Log(resultCallback.FunctionResult);
            failedCallbackCounter = 0;
            if(resultCallback.FunctionResult == null)
            {
                EntryCore.HideLoadingPanel();

            }
            else
            {
                if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
                {
                    PlayerData.LUID = myGUID.ToString();
                    PlayerData.ActiveWeaponID = GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "ActiveCannon");
                    PlayerData.ActiveConstumeInstanceID = GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "ActiveCostume");
                    PlayerData.CurrentStage = int.Parse(GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "CurrentStage"));

                    quests = GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "Quests");
                    PlayerData.DailyCheckIn = int.Parse(GameManager.Instance.DeserializeStringValue(quests, "DailyCheckIn"));
                    PlayerData.SocMedShared = int.Parse(GameManager.Instance.DeserializeStringValue(quests, "SocMedShared"));
                    PlayerData.ItemsUsed = int.Parse(GameManager.Instance.DeserializeStringValue(quests, "ItemsUsed"));
                    PlayerData.MonstersKilled = int.Parse(GameManager.Instance.DeserializeStringValue(quests, "MonstersKilled"));
                    PlayerData.LevelsWon = int.Parse(GameManager.Instance.DeserializeStringValue(quests, "LevelsWon"));
                    PlayerData.DailyQuestClaimed = int.Parse(GameManager.Instance.DeserializeStringValue(quests, "DailyQuestClaimed"));

                    if (EntryCore.RememberMeToggle.isOn)
                    {
                        PlayerPrefs.SetString("Username", username);
                        PlayerPrefs.SetString("Password", password);
                    }
                    
                    EntryCore.HideLoadingPanel();
                    GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
                }
                else
                    EntryCore.HideLoadingPanel();
            }          
        },
        errorCallback =>
        {
            EntryCore.HideLoadingPanel();
            ErrorCallback(errorCallback.Error,
                FailedAction,
                () => LoginCredentials(username, password),
                () => GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage));
        });
    }

    public void LoginWithMetaMask(string wallet)
    {
        LoginWithCustomIDRequest loginWithCustomID = new LoginWithCustomIDRequest();
        loginWithCustomID.TitleId = "BB42A";
        loginWithCustomID.CustomId = wallet;

        PlayFabClientAPI.LoginWithCustomID(loginWithCustomID,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                Debug.Log("Player ID: " + resultCallback.PlayFabId);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    FailedAction,
                    () => LoginWithMetaMask(wallet),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    #region UTILITY
    private string Encrypt(string _password)
    {
        string salt = "CBS";
        string pepper = "OPTIBIT";

        using (SHA256 hash = SHA256.Create())
        {
            byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(_password));

            // Convert byte array to a string   
            StringBuilder firstHash = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                firstHash.Append(bytes[i].ToString("x2"));
            }

            bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(firstHash + salt));
            StringBuilder secondHash = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                secondHash.Append(bytes[i].ToString("x2"));
            }

            bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(secondHash + pepper));
            StringBuilder thirdHash = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                thirdHash.Append(bytes[i].ToString("x2"));
            }

            return thirdHash.ToString();
        }
    }

    private void ErrorCallback(PlayFabErrorCode errorCode, Action failedAction, Action restartAction, Action processError)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                failedAction();
            else
                restartAction();
        }
        else if (errorCode == PlayFabErrorCode.InternalServerError)
            ProcessSpecialError();
        else
        {
            if (processError != null)
                processError();
        }
    }

    private void FailedAction()
    {
        EntryCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel("Connectivity error. Please connect to strong internet");
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        EntryCore.ResetLoginPanel();
    }

    private void ProcessError(string error)
    {
        EntryCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(error);
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        EntryCore.ResetLoginPanel();
    }

    private void ProcessSpecialError()
    {
        EntryCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        EntryCore.ResetLoginPanel();
    }
    #endregion
}
