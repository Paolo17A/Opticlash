using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using System;
using MyBox;

public class UpgradeCannonCore : MonoBehaviour
{
    #region VARIABLES
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }

    [field: Header("CURRENT CANNON")]
    [field: SerializeField] public Image CurrentCannonImage { get; set; }
    [field: SerializeField] public Image CurrentCannonBigImage { get; set; }
    [field: SerializeField] public TextMeshProUGUI CurrentCannonDamageTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI CurrentCannonAccuracyTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI CurrentCannonAbilitiesTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI CurrentCannonRequiredFragments { get; set; }
    [field: SerializeField] public TextMeshProUGUI CurrentCannonRequiredOptibit { get; set; }
    [field: SerializeField] public Button UpgradeCannonBtn { get; set; }

    [field: Header("FRAGMENTS")]
    [field: SerializeField] public TextMeshProUGUI NormalFragmentTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI RareFragmentTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI EpicFragmentTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI LegendFragmentTMP { get; set; }

    [field: Header("UPGRADE")]
    [field: SerializeField] public GameObject UpgradeSuccessAnimation { get; set; }
    [field: SerializeField] public TextMeshProUGUI UpgradeSuccessRateTMP { get; set; }
    [field: SerializeField][field: ReadOnly] public int SuccessChance { get; set; }

    [field: Header("DEBUGGER")]
    private int failedCallbackCounter;
    private string virtualCurrencyCode;
    #endregion

    public void UpgradeCannon()
    {
        if (GameManager.Instance.DebugMode)
        {
            PlayerData.Optibit -= PlayerData.ActiveCustomWeapon.OptibitUpgradeCost;
            if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C4 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C5)
                PlayerData.NormalFragments -= PlayerData.ActiveCustomWeapon.FragmentUpgradeCost;
            else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B4)
                PlayerData.RareFragments -= PlayerData.ActiveCustomWeapon.FragmentUpgradeCost;
            else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A3)
                PlayerData.EpicFragments -= PlayerData.ActiveCustomWeapon.FragmentUpgradeCost;
            else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S2)
                PlayerData.LegendFragments -= PlayerData.ActiveCustomWeapon.FragmentUpgradeCost;
            PlayerData.ActiveCustomWeapon.Level += PlayerData.ActiveCustomWeapon.BaseWeaponData.UpgradeValue;
            CurrentCannonDamageTMP.text = (PlayerData.ActiveCustomWeapon.BaseWeaponData.BaseDamage + PlayerData.ActiveCustomWeapon.Level).ToString();
            NormalFragmentTMP.text = PlayerData.NormalFragments.ToString();
            RareFragmentTMP.text = PlayerData.RareFragments.ToString();
            EpicFragmentTMP.text = PlayerData.EpicFragments.ToString();
            LegendFragmentTMP.text = PlayerData.LegendFragments.ToString();
            LobbyCore.DisplayOptibits();
        }
        else
        {
            LobbyCore.DisplayLoadingPanel();
            if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C4 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C5)
                virtualCurrencyCode = "NF";
            else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B4)
                virtualCurrencyCode = "RF";
            else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A3)
                virtualCurrencyCode = "EF";
            else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S2)
                virtualCurrencyCode = "LF";
            
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "UpgradeActiveWeapon",
                FunctionParameter = new
                {
                    localLUID = PlayerData.LUID,
                    optibitUpgradeCost = PlayerData.ActiveCustomWeapon.OptibitUpgradeCost,
                    virtualCurrency = virtualCurrencyCode,
                    fragmentUpgradeCost = PlayerData.ActiveCustomWeapon.FragmentUpgradeCost,
                    cannonId = PlayerData.ActiveWeaponID,
                    newLevelValue = (PlayerData.ActiveCustomWeapon.Level + PlayerData.ActiveCustomWeapon.BaseWeaponData.UpgradeValue).ToString(),
                },
                GeneratePlayStreamEvent = true
            },
            resultCallback =>
            {
                Debug.Log(resultCallback.FunctionResult);
                if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
                {
                    failedCallbackCounter = 0;
                    PlayerData.Optibit -= PlayerData.ActiveCustomWeapon.OptibitUpgradeCost;
                    PlayerData.ActiveCustomWeapon.Level += PlayerData.ActiveCustomWeapon.BaseWeaponData.UpgradeValue;
                    PlayerData.ActiveCustomWeapon.CalculateCannonStats();
                    //CurrentCannonDamageTMP.text = PlayerData.ActiveCustomWeapon.Attack.ToString();
                    CurrentCannonAccuracyTMP.text = PlayerData.ActiveCustomWeapon.Accuracy.ToString();
                    if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C4 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C5)
                    {
                        PlayerData.NormalFragments -= PlayerData.ActiveCustomWeapon.FragmentUpgradeCost;
                        NormalFragmentTMP.text = PlayerData.NormalFragments.ToString();
                        CurrentCannonDamageTMP.text = PlayerData.NormalFragments + "/" + PlayerData.ActiveCustomWeapon.Attack.ToString();
                    }
                    else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B4)
                    {
                        PlayerData.RareFragments -= PlayerData.ActiveCustomWeapon.FragmentUpgradeCost;
                        RareFragmentTMP.text = PlayerData.RareFragments.ToString();
                        CurrentCannonDamageTMP.text = PlayerData.RareFragments + "/" + PlayerData.ActiveCustomWeapon.Attack.ToString();

                    }
                    else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A3)
                    {
                        PlayerData.EpicFragments -= PlayerData.ActiveCustomWeapon.FragmentUpgradeCost;
                        EpicFragmentTMP.text = PlayerData.EpicFragments.ToString();
                        CurrentCannonDamageTMP.text = PlayerData.EpicFragments + "/" + PlayerData.ActiveCustomWeapon.Attack.ToString();
                    }
                    else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S2)
                    {
                        PlayerData.LegendFragments -= PlayerData.ActiveCustomWeapon.FragmentUpgradeCost;
                        LegendFragmentTMP.text = PlayerData.LegendFragments.ToString();
                        CurrentCannonDamageTMP.text = PlayerData.LegendFragments + "/" + PlayerData.ActiveCustomWeapon.Attack.ToString();
                    }
                    LobbyCore.DisplayOptibits();
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.HideLoadingPanel();
                    LobbyCore.GrantPanel.SetActive(true);
                    LobbyCore.OkBtn.SetActive(false);
                    UpgradeSuccessAnimation.SetActive(true);
                    UpgradeSuccessAnimation.GetComponent<UpgradeAnimationCore>().UpgradeCannonAnimator.SetTrigger("success");
                }
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    UpgradeCannon,
                    () => ProcessError(errorCallback.ErrorMessage));
            }); ;
        }
    }

    #region UTILITY
    public void ProcessUpgradeCannonButton()
    {
        if (PlayerData.ActiveWeaponID == "NONE")
        {
            Debug.Log("invi");
            CurrentCannonImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Dis");
            CurrentCannonImage.gameObject.SetActive(true);
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
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }
    #endregion
}
