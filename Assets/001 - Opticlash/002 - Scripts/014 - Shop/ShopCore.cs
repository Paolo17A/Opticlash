using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class ShopCore : MonoBehaviour
{
    //===============================================================================================
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }

    [field: Header("SHOP")]
    [field: SerializeField][field: ReadOnly] private int ShopIndex { get; set; }
    [field: SerializeField] private Image CurrentItemDisplayImage { get; set; }
    [field: SerializeField][field: ReadOnly] private int CurrentItemCost { get; set; }
    [field: SerializeField] private TextMeshProUGUI ItemCostTMP { get; set; }
    [field: SerializeField] private Sprite BreakShopSprite { get; set; }
    [field: SerializeField] private Sprite BurnShopSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseShopSprite { get; set; }
    [field: SerializeField] private Sprite FreezeShopSprite { get; set; }
    [field: SerializeField] private Sprite SmallHealShopSprite { get; set; }
    [field: SerializeField] private Sprite MediumHealShopSprite { get; set; }
    [field: SerializeField] private Sprite LargeHealShopSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeShopSprite { get; set; }
    [field: SerializeField] private Sprite WeakShopSprite { get; set; }
    [field: SerializeField] private TextMeshProUGUI ItemNameTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ItemDescriptionTMP { get; set; }
    [field: SerializeField] private Button BuyBtn { get; set; }

    [Header("DEBUGGER")]
    private int failedCallbackCounter;
    //===============================================================================================

    #region SHOP
    public void PreviousShopItem()
    {
        if (ShopIndex == 0)
            ShopIndex = 6;
        else
            ShopIndex--;
        DisplayShopItem();
    }
    public void NextShopItem()
    {
        if (ShopIndex == 6)
            ShopIndex = 0;
        else
            ShopIndex++;
        DisplayShopItem();
    }

    public void DisplayShopItem()
    {
        switch (ShopIndex)
        {
            case 0:
                CurrentItemDisplayImage.sprite = BreakShopSprite;
                ItemNameTMP.text = "BREAK REMOVAL";
                ItemDescriptionTMP.text = "Remove BREAK status";
                CurrentItemCost = 500;
                break;
            case 1:
                CurrentItemDisplayImage.sprite = BurnShopSprite;
                ItemNameTMP.text = "BURN REMOVAL";
                ItemDescriptionTMP.text = "Remove BURN status";
                CurrentItemCost = 750;
                break;
            case 2:
                CurrentItemDisplayImage.sprite = ConfuseShopSprite;
                ItemNameTMP.text = "CONFUSE REMOVAL";
                ItemDescriptionTMP.text = "Remove CONFUSE status";
                CurrentItemCost = 450;
                break;
            case 3:
                CurrentItemDisplayImage.sprite = FreezeShopSprite;
                ItemNameTMP.text = "FREEZE REMOVAL";
                ItemDescriptionTMP.text = "Remove FREEZE status";
                CurrentItemCost = 500;
                break;
            case 4:
                CurrentItemDisplayImage.sprite = SmallHealShopSprite;
                ItemNameTMP.text = "SMALL HEAL";
                ItemDescriptionTMP.text = "HEAL 10 Health Points";
                CurrentItemCost = 800;
                break;
            case 5:
                CurrentItemDisplayImage.sprite = MediumHealShopSprite;
                ItemNameTMP.text = "MEDIUM HEAL";
                ItemDescriptionTMP.text = "HEAL 15 Health Points";
                CurrentItemCost = 900;
                break;
            case 6:
                CurrentItemDisplayImage.sprite = LargeHealShopSprite;
                ItemNameTMP.text = "LARGE HEAL";
                ItemDescriptionTMP.text = "HEAL 25 Health Points";
                CurrentItemCost = 1000;
                break;
            case 7:
                CurrentItemDisplayImage.sprite = ParalyzeShopSprite;
                ItemNameTMP.text = "PARALYZE REMOVAL";
                ItemDescriptionTMP.text = "Remove PARALYZE status";
                CurrentItemCost = 450;
                break;
            case 8:
                CurrentItemDisplayImage.sprite = WeakShopSprite;
                ItemNameTMP.text = "WEAK REMOVAL";
                ItemDescriptionTMP.text = "Remove WEAK status";
                CurrentItemCost = 450;
                break;
        }
        ItemCostTMP.text = CurrentItemCost.ToString();
        ProcessBuyButton();
    }

    public void PurchaseCurrentItem()
    {
        if (GameManager.Instance.DebugMode)
        {
            switch (ShopIndex)
            {
                case 0:
                    PlayerData.BurnRemovalCharges++;
                    break;
                case 1:
                    PlayerData.BreakRemovalCharges++;
                    break;
                case 2:
                    PlayerData.ConfuseRemovalCharges++;
                    break;
                case 3:
                    PlayerData.FreezeRemovalCharges++;
                    break;
                case 4:
                    PlayerData.SmallHealCharges++;
                    break;
                case 5:
                    PlayerData.MediumHealCharges++;
                    break;
                case 6:
                    PlayerData.LargeHealCharges++;
                    break;
                case 7:
                    PlayerData.ParalyzeRemovalCharges++;
                    break;
                case 8:
                    PlayerData.WeakRemovalCharges++;
                    break;
            }
            PlayerData.Optibit -= CurrentItemCost;
            ProcessBuyButton();
            LobbyCore.DisplayOptibits();
        }
        else
        {
            LobbyCore.DisplayLoadingPanel();

            PurchaseItemRequest purchaseItem = new PurchaseItemRequest();
            purchaseItem.CatalogVersion = "Consumables";
            purchaseItem.ItemId = GetConsumableId(ShopIndex);
            purchaseItem.Price = CurrentItemCost;
            purchaseItem.VirtualCurrency = "OP";

            PlayFabClientAPI.PurchaseItem(purchaseItem,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    switch (ShopIndex)
                    {
                        case 0:
                            PlayerData.BreakRemovalCharges++;
                            PlayerData.BreakRemovalInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                        case 1:
                            PlayerData.BurnRemovalCharges++;
                            PlayerData.BurnRemovalInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                        case 2:
                            PlayerData.ConfuseRemovalCharges++;
                            PlayerData.ConfuseRemovalInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                        case 3:
                            PlayerData.FreezeRemovalCharges++;
                            PlayerData.FreezeRemovalInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                        case 4:
                            PlayerData.SmallHealCharges++;
                            PlayerData.SmallHealInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                        case 5:
                            PlayerData.MediumHealCharges++;
                            PlayerData.MediumHealInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                        case 6:
                            PlayerData.LargeHealCharges++;
                            PlayerData.LargeHealInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                        case 7:
                            PlayerData.ParalyzeRemovalCharges++;
                            PlayerData.ParalyzeRemovalInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                        case 8:
                            PlayerData.WeakRemovalCharges++;
                            PlayerData.WeakRemovalInstanceID = resultCallback.Items[0].ItemInstanceId;
                            break;
                    }
                    PlayerData.Optibit -= CurrentItemCost;
                    ProcessBuyButton();
                    LobbyCore.DisplayOptibits();
                    LobbyCore.HideLoadingPanel();
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                    PurchaseCurrentItem,
                    () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }
    #endregion

    #region UTILITY
    private void ProcessBuyButton()
    {
        if (PlayerData.Optibit >= CurrentItemCost)
            BuyBtn.interactable = true;
        else
            BuyBtn.interactable = false;
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

    private string GetConsumableId(int _value)
    {
        switch (_value)
        {
            case 0:
                return "BreakRemoval";
            case 1:
                return "BurnRemoval";
            case 2:
                return "ConfuseRemoval";
            case 3:
                return "FreezeRemoval";
            case 4:
                return "SmallHealCharge";
            case 5:
                return "HealCharge";
            case 6:
                return "LargeHealCharge";
            case 7:
                return "ParalyzeRemoval";
            case 8:
                return "WeakRemoval";
            default:
                return null;
        }
    }

    private void ProcessError(string errorMessage)
    {
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }
    #endregion
}
