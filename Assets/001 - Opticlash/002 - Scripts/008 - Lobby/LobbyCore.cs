using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

public class LobbyCore : MonoBehaviour
{
    #region STATE MACHINE
    //==================================================================
    public enum LobbyStates
    {
        NONE,
        CORE,
        RANK,
        SHOP,
        INVENTORY,
        CRAFT,
        QUEST,
        SETTINGS,
        EQUIP,
        COSTUME,
        CANNON,
        CURRENTCANNON
    }

    private event EventHandler lobbyStateChange;
    public event EventHandler onLobbyStateChange
    {
        add
        {
            if (lobbyStateChange == null || !lobbyStateChange.GetInvocationList().Contains(value))
                lobbyStateChange += value;
        }
        remove { lobbyStateChange -= value; }
    }

    public LobbyStates CurrentLobbyState
    {
        get => lobbyStates;
        set
        {
            lobbyStates = value;
            lobbyStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private LobbyStates lobbyStates;
    //==================================================================
    #endregion

    #region VARIABLES
    //==============================================================
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    [field: Header("ANIMATORS")]
    [field: SerializeField] public Animator LobbyAnimator { get; set; }
    [field: SerializeField] public Animator DropdownAnimator { get; set; }
    [field: SerializeField] public GameObject LoadingPanel { get; set; }

    [field: Header("OPTI")]
    [field: SerializeField] private Image OptiLobbyCannon { get; set; }
    [field: SerializeField] private Image OptiEquipCannon { get; set; }
    [field: SerializeField] private Image OptiLobbyCostume { get; set; }
    [field: SerializeField] private Image OptiEquipCostume { get; set; }

    [field: Header("OPTIBIT")]
    [field: SerializeField] private TextMeshProUGUI CoreOptibitTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ShopOptibitTMP { get; set; }

    [field: Header("STAGE")]
    [field: SerializeField] public TextMeshProUGUI StageTMP { get; set; }

    [field: Header("SHOP")]
    [field: SerializeField][field: ReadOnly] private int ShopIndex { get; set; }
    [field: SerializeField] private Image CurrentItemDisplayImage { get; set; }
    [field: SerializeField][field: ReadOnly] private int CurrentItemCost { get; set; }
    [field: SerializeField] private TextMeshProUGUI ItemCostTMP { get; set; }
    [field: SerializeField] private Sprite BreakShopSprite { get; set; }
    [field: SerializeField] private Sprite BurnShopSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseShopSprite { get; set; }
    [field: SerializeField] private Sprite FreezeShopSprite { get; set; }
    [field: SerializeField] private Sprite HealShopSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeShopSprite { get; set; }
    [field: SerializeField] private Sprite WeakShopSprite { get; set; }
    [field: SerializeField] private TextMeshProUGUI ItemNameTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ItemDescriptionTMP { get; set; }
    [field: SerializeField] private Button BuyBtn { get; set; }

    [field: Header("INVENTORY")]
    [field: SerializeField] private Sprite BreakRemoveSprite { get; set; }
    [field: SerializeField] private Sprite BurnRemoveSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseRemoveSprite { get; set; }
    [field: SerializeField] private Sprite FreezeRemoveSprite { get; set; }
    [field: SerializeField] private Sprite HealChargeSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeRemoveSprite { get; set; }
    [field: SerializeField] private Sprite WeakRemoveSprite { get; set; }
    [field: SerializeField] private Image DisplayedInventoryImage { get; set; }
    [field: SerializeField] private TextMeshProUGUI InventoryItemNameTMP{ get; set; }
    [field: SerializeField] private GameObject RectanglePanelInventory { get; set; }
    [field: SerializeField] private TextMeshProUGUI InventoryItemDescriptionTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI InventoryItemCountTMP { get; set; }

    [field: Header("EQUIP")]
    [field: SerializeField] public Sprite ActiveCostumeSprite { get; set; }
    [field: SerializeField] public Sprite InactiveCostumeSprite { get; set; }
    [field: SerializeField] public Sprite ActiveWeaponSprite { get; set; }
    [field: SerializeField] public Sprite InactiveWeaponSprite { get; set; }
    [field: SerializeField] public Button CostumeBtn { get; set; }
    [field: SerializeField] public Button WeaponBtn { get; set; }

    [field: Header("CRAFT")]
    [field: SerializeField] private Image SelectedFragmentImage { get; set; }
    [field: SerializeField] private Sprite NormalFragmentSprite { get; set; }
    [field: SerializeField] private Sprite RareFragmentSprite { get; set; }
    [field: SerializeField] private Sprite EpicFragmentSprite { get; set; }
    [field: SerializeField] private Sprite LegendFragmentSprite { get; set; }
    [field: SerializeField] private TextMeshProUGUI SelectedFragmentTMP { get; set; }
    [field: SerializeField] private Button CraftBtn { get; set; }
    [field: SerializeField] Button NormalFragmentBtn { get; set; }
    [field: SerializeField] TextMeshProUGUI NormalFragmentCountTMP { get; set; }
    [field: SerializeField] Button RareFragmentBtn { get; set; }
    [field: SerializeField] TextMeshProUGUI RareFragmentCountTMP { get; set; }
    [field: SerializeField] Button EpicFragmentBtn { get; set; }
    [field: SerializeField] TextMeshProUGUI EpicFragmentCountTMP { get; set; }
    [field: SerializeField] Button LegendFragmentBtn { get; set; }
    [field: SerializeField] TextMeshProUGUI LegendFragmentCountTMP { get; set; }
    [field: SerializeField] GameObject SquarePanel { get; set; }
    [field: SerializeField] Image SquarePanelImage { get; set; }
    [field: SerializeField] TextMeshProUGUI SquarePanelNameTMP { get; set; }
    [field: SerializeField] GameObject RectanglePanel { get; set; }
    [field: SerializeField] TextMeshProUGUI RectanglePanelDescriptionTMP { get; set; }
    [field: SerializeField] TextMeshProUGUI RectanglePanelFragmentsOwnedTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int FragmentIndex { get; set; }

    [field: Header("COSTUMES")]
    [field: SerializeField] private Image CostumeLeftImage { get; set; }
    [field: SerializeField] private Button EquipLeftCostumeBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI LeftCostumeNameTMP { get; set; }
    [field: SerializeField] private Image CostumeRightImage { get; set; }
    [field: SerializeField] private Button EquipRightCostumeBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI RightCostumeNameTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int CostumePageIndex { get; set; }
    [field: SerializeField] private Button PreviousCostumePageBtn { get; set; }
    [field: SerializeField] private Button NextCostumePageBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI CostumePageTMP { get; set; }
    [field: SerializeField] private Sprite EquipSprite { get; set; }
    [field: SerializeField] private Sprite UnequipSprite { get; set; }

    [field: Header("WEAPONS")]
    [field: SerializeField] private List<WeaponData> NormalWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> RareWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> EpicWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> LegendWeapons { get; set; }
    [field: SerializeField] private Image WeaponLeftImage { get; set; }
    [field: SerializeField] private Button EquipLeftWeaponBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI LeftWeaponNameTMP { get; set; }
    [field: SerializeField] private Image WeaponRightImage { get; set; }
    [field: SerializeField] private Button EquipRightWeaponBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI RightWeaponNameTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int WeaponPageIndex { get; set; }
    [field: SerializeField] private Button PreviousWeaponPageBtn { get; set; }
    [field: SerializeField] private Button NextWeaponPageBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI WeaponPageTMP { get; set; }

    [field: Header("CURRENT CANNON")]
    [field: SerializeField] private Image CurrentCannonImage { get; set; }
    [field: SerializeField] private Image CurrentCannonBigImage { get; set; }
    [field: SerializeField] private TextMeshProUGUI CurrentCannonNameTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI CurrentCannonDamageTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI CurrentCannonAmmoTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI CurrentCannonAccuracyTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI CurrentCannonAbilitiesTMP { get; set; }
    [field: SerializeField] private Button UpgradeCannonBtn { get; set; }

    [Header("DEBUGGER")]
    [ReadOnly] public List<CustomCostumeData> ActualCostumesOwned;
    [ReadOnly] public List<CustomWeaponData> ActualOwnedWeapons;
    [ReadOnly] private int failedCallbackCounter;
    //==============================================================
    #endregion

    public void ToggleDropdown()
    {
        DropdownAnimator.SetBool("Showing", !DropdownAnimator.GetBool("Showing"));
    }

    #region CORE
    public void InitializeLobby()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                int currentDetectedCannon = 0;
                foreach (var item in resultCallback.Inventory)
                {
                    if (item.ItemClass == "CANNON")
                    {
                        PlayerData.OwnedWeapons[currentDetectedCannon].WeaponInstanceID = item.ItemInstanceId;
                        PlayerData.OwnedWeapons[currentDetectedCannon].BaseWeaponData = GameManager.Instance.InventoryManager.GetProperWeaponData(item.ItemId);
                        PlayerData.OwnedWeapons[currentDetectedCannon].BonusDamage = int.Parse(item.CustomData["Bonus"]);
                        currentDetectedCannon++;
                    }
                    else if (item.ItemClass == "COSTUME")
                    {
                        foreach (CustomCostumeData customCostume in PlayerData.OwnedCostumes)
                            if (item.ItemId == customCostume.BaseCostumeData.CostumeID)
                            {
                                customCostume.CostumeIsOwned = true;
                                customCostume.CostumeInstanceID = item.ItemInstanceId;
                                break;
                            }
                    }
                    else if (item.ItemClass == "CONSUMABLE")
                    {
                        switch(item.ItemId)
                        {
                            case "BreakRemoval":
                                PlayerData.BreakRemovalCharges = (int)item.RemainingUses;
                                PlayerData.BreakRemovalInstanceID = item.ItemInstanceId;
                                break;
                            case "BurnRemoval":
                                PlayerData.BurnRemovalCharges = (int)item.RemainingUses;
                                PlayerData.BurnRemovalInstanceID = item.ItemInstanceId;
                                break;
                            case "ConfuseRemoval":
                                PlayerData.ConfuseRemovalCharges = (int)item.RemainingUses;
                                PlayerData.ConfuseRemovalInstanceID = item.ItemInstanceId;
                                break;
                            case "FreezeRemoval":
                                PlayerData.FreezeRemovalCharges = (int)item.RemainingUses;
                                PlayerData.FreezeRemovalInstanceID = item.ItemInstanceId;
                                break;
                            case "HealCharge":
                                PlayerData.HealCharges = (int)item.RemainingUses;
                                PlayerData.HealInstanceID = item.ItemInstanceId;
                                break;
                            case "ParalyzeRemoval":
                                PlayerData.ParalyzeRemovalCharges = (int)item.RemainingUses;
                                PlayerData.ParalyzeRemovalInstanceID = item.ItemInstanceId;
                                break;
                            case "WeakRemoval":
                                PlayerData.WeakRemovalCharges = (int)item.RemainingUses;
                                PlayerData.WeakRemovalInstanceID = item.ItemInstanceId;
                                break;
                        }
                    }
                }
                    
                GetActiveCannon();
                GetActiveCostume();
                StageTMP.text = PlayerData.CurrentStage.ToString();
                PlayerData.Optibit = resultCallback.VirtualCurrency["OP"];
                PlayerData.EnergyCount = resultCallback.VirtualCurrency["EN"];
                PlayerData.NormalFragments = resultCallback.VirtualCurrency["NF"];
                PlayerData.RareFragments = resultCallback.VirtualCurrency["RF"];
                PlayerData.EpicFragments = resultCallback.VirtualCurrency["EF"];
                PlayerData.LegendFragments = resultCallback.VirtualCurrency["LF"];
                DisplayOptibits();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    InitializeLobby,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }    
    public void GetActiveCannon()
    {
        foreach (CustomWeaponData weapon in PlayerData.OwnedWeapons)
            if (weapon.WeaponInstanceID == PlayerData.ActiveWeaponID)
            {
                PlayerData.ActiveCustomWeapon = weapon;
                CurrentCannonImage.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.CurrentSprite;
                CurrentCannonBigImage.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.CurrentBigSprite;
                CurrentCannonNameTMP.text = PlayerData.ActiveCustomWeapon.BaseWeaponData.WeaponName;
                CurrentCannonDamageTMP.text = (PlayerData.ActiveCustomWeapon.BaseWeaponData.BaseDamage + PlayerData.ActiveCustomWeapon.BonusDamage).ToString();
                CurrentCannonAbilitiesTMP.text = PlayerData.ActiveCustomWeapon.BaseWeaponData.BaseDamage.ToString();
                CurrentCannonAmmoTMP.text = PlayerData.ActiveCustomWeapon.BaseWeaponData.StartingAmmo.ToString();
                CurrentCannonAccuracyTMP.text = PlayerData.ActiveCustomWeapon.BaseWeaponData.Accuracy.ToString();
                CurrentCannonAbilitiesTMP.text = PlayerData.ActiveCustomWeapon.BaseWeaponData.Abilities;
                break;
            }
        SetOptiCannon(PlayerData.ActiveCustomWeapon.BaseWeaponData.EquippedSprite);
    }
    public void GetActiveCostume()
    {
        if(PlayerData.ActiveConstumeInstanceID != "NONE")
        {
            foreach (CustomCostumeData costume in PlayerData.OwnedCostumes)
                if (costume.CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
                {
                    PlayerData.ActiveCostume = costume.BaseCostumeData;
                    break;
                }
            SetOptiCostume(PlayerData.ActiveCostume.LobbyCostumeSprite);
        }
        else
        {
            OptiLobbyCostume.gameObject.SetActive(false);
            OptiEquipCostume.gameObject.SetActive(false);
        }
    }

    public void DisplayOptibits()
    {
        CoreOptibitTMP.text = PlayerData.Optibit.ToString();
        ShopOptibitTMP.text = PlayerData.Optibit.ToString();
        if (PlayerData.Optibit >= 100 + (PlayerData.ActiveCustomWeapon.BonusDamage * 50))
            UpgradeCannonBtn.interactable = true;
        else
            UpgradeCannonBtn.interactable = false;
    }
    #endregion

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
                CurrentItemDisplayImage.sprite = HealShopSprite;
                ItemNameTMP.text = "HEAL";
                ItemDescriptionTMP.text = "HEAL 15 Health Points";
                CurrentItemCost = 1000;
                break;
            case 5:
                CurrentItemDisplayImage.sprite = ParalyzeShopSprite;
                ItemNameTMP.text = "PARALYZE REMOVAL";
                ItemDescriptionTMP.text = "Remove PARALYZE status";
                CurrentItemCost = 450;
                break;
            case 6:
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
                    PlayerData.HealCharges++;
                    break;
                case 5:
                    PlayerData.ParalyzeRemovalCharges++;
                    break;
                case 6:
                    PlayerData.WeakRemovalCharges++;
                    break;
            }
            PlayerData.Optibit -= CurrentItemCost;
            ProcessBuyButton();
            DisplayOptibits();
        }
        else
        {
            DisplayLoadingPanel();

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
                            break;
                        case 1:
                            PlayerData.BurnRemovalCharges++;
                            break;
                        case 2:
                            PlayerData.ConfuseRemovalCharges++;
                            break;
                        case 3:
                            PlayerData.FreezeRemovalCharges++;
                            break;
                        case 4:
                            PlayerData.HealCharges++;
                            break;
                        case 5:
                            PlayerData.ParalyzeRemovalCharges++;
                            break;
                        case 6:
                            PlayerData.WeakRemovalCharges++;
                            break;
                    }
                    PlayerData.Optibit -= CurrentItemCost;
                    ProcessBuyButton();
                    DisplayOptibits();
                    HideLoadingPanel();
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

    #region INVENTORY
    public void InitializeInventory()
    {
        DisplayedInventoryImage.gameObject.SetActive(false);
        InventoryItemNameTMP.gameObject.SetActive(false);
        RectanglePanelInventory.SetActive(false);
    }

    public void SelectBreakRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = BreakRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "BREAK REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff BREAK";
        InventoryItemCountTMP.text = PlayerData.BreakRemovalCharges.ToString();
    }

    public void SelectBurnRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = BurnRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "BURN REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff BURN";
        InventoryItemCountTMP.text = PlayerData.BurnRemovalCharges.ToString();
    }

    public void SelectConfuseRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = ConfuseRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "CONFUSE REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff CONFUSE";
        InventoryItemCountTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
    }

    public void SelectFreezeRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = FreezeRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "FREEZE REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff FREEZE";
        InventoryItemCountTMP.text = PlayerData.FreezeRemovalCharges.ToString();
    }

    public void SelectHealRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = HealChargeSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "HEAL REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to recover 15% Health Points";
        InventoryItemCountTMP.text = PlayerData.HealCharges.ToString();
    }

    public void SelectParalyzeRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = ParalyzeRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "PARALYZE REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff PARALYZE";
        InventoryItemCountTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
    }

    public void SelectWeakRemove()
    {
        DisplayedInventoryImage.gameObject.SetActive(true);
        DisplayedInventoryImage.sprite = WeakRemoveSprite;
        InventoryItemNameTMP.gameObject.SetActive(true);
        InventoryItemNameTMP.text = "WEAK REMOVAL";
        RectanglePanelInventory.SetActive(true);
        InventoryItemDescriptionTMP.text = "Use to remove negative buff WEAK";
        InventoryItemCountTMP.text = PlayerData.WeakRemovalCharges.ToString();
    }
    #endregion

    #region CRAFT
    public void InitializeCrafting()
    {
        SelectedFragmentImage.gameObject.SetActive(false);
        SquarePanel.SetActive(false);
        RectanglePanel.SetActive(false);
        CraftBtn.interactable = false;
        FragmentIndex = 0;

        NormalFragmentCountTMP.text = PlayerData.NormalFragments.ToString();
        if (PlayerData.NormalFragments > 0)
            NormalFragmentBtn.interactable = true;
        else
            NormalFragmentBtn.interactable = false;

        RareFragmentCountTMP.text = PlayerData.RareFragments.ToString();
        if (PlayerData.RareFragments > 0)
            RareFragmentBtn.interactable = true;
        else
            RareFragmentBtn.interactable = false;

        EpicFragmentCountTMP.text = PlayerData.EpicFragments.ToString();
        if (PlayerData.EpicFragments > 0)
            EpicFragmentBtn.interactable = true;
        else
            EpicFragmentBtn.interactable = false;

        LegendFragmentCountTMP.text = PlayerData.LegendFragments.ToString();
        if (PlayerData.LegendFragments > 0)
            LegendFragmentBtn.interactable = true;
        else
            LegendFragmentBtn.interactable = false;
    }
    public void SelectNormalFragment()
    {
        FragmentIndex = 1;
        SelectedFragmentImage.gameObject.SetActive(true);
        SelectedFragmentImage.sprite = NormalFragmentSprite;
        SquarePanelImage.sprite = NormalFragmentSprite;
        SquarePanelNameTMP.text = "Normal Grade Fragment";
        SelectedFragmentTMP.text = PlayerData.NormalFragments + "/100";
        RectanglePanelFragmentsOwnedTMP.text = PlayerData.NormalFragments + "/100";
        RectanglePanelDescriptionTMP.text = "Used to create a Random Normal grade cannon (C Rank)";
        ProcessCraftBtn(PlayerData.NormalFragments, GetFragmentPrice(FragmentIndex));
    }
    public void SelectRareFragment()
    {
        FragmentIndex = 2;
        SelectedFragmentImage.gameObject.SetActive(true);
        SelectedFragmentImage.sprite = RareFragmentSprite;
        SquarePanelImage.sprite = RareFragmentSprite;
        SquarePanelNameTMP.text = "Rare Grade Fragment";
        SelectedFragmentTMP.text = PlayerData.RareFragments + "/125";
        RectanglePanelFragmentsOwnedTMP.text = PlayerData.RareFragments + "/125";
        RectanglePanelDescriptionTMP.text = "Used to create a Rare grade cannon (B Rank)";
        ProcessCraftBtn(PlayerData.RareFragments, GetFragmentPrice(FragmentIndex));
    }
    public void SelectEpicFragment()
    {
        FragmentIndex = 3;
        SelectedFragmentImage.gameObject.SetActive(true);
        SelectedFragmentImage.sprite = EpicFragmentSprite;
        SquarePanelImage.sprite = EpicFragmentSprite;
        SquarePanelNameTMP.text = "Epic Grade Fragment";
        SelectedFragmentTMP.text = PlayerData.EpicFragments + "/150";
        RectanglePanelFragmentsOwnedTMP.text = PlayerData.EpicFragments + "/150";
        RectanglePanelDescriptionTMP.text = "Used to create an Epi grade cannon (A Rank)";
        ProcessCraftBtn(PlayerData.NormalFragments, GetFragmentPrice(FragmentIndex));
    }
    public void SelectLegendFragment()
    {
        FragmentIndex = 4;
        SelectedFragmentImage.gameObject.SetActive(true);
        SelectedFragmentImage.sprite = LegendFragmentSprite;
        SquarePanelImage.sprite = LegendFragmentSprite;
        SquarePanelNameTMP.text = "Legend Grade Fragment";
        SelectedFragmentTMP.text = PlayerData.LegendFragments + "/200";
        RectanglePanelFragmentsOwnedTMP.text = PlayerData.LegendFragments + "/200";
        RectanglePanelDescriptionTMP.text = "Used to create a Legend grade cannon (S Rank)";
        ProcessCraftBtn(PlayerData.LegendFragments, GetFragmentPrice(FragmentIndex));
    }

    public void CraftNewWeapon()
    {
        if (GameManager.Instance.DebugMode)
        {
            switch (FragmentIndex)
            {
                case 1:
                    PlayerData.NormalFragments -= GetFragmentPrice(FragmentIndex);
                    SelectedFragmentTMP.text = PlayerData.NormalFragments + "/100";
                    RectanglePanelFragmentsOwnedTMP.text = PlayerData.NormalFragments + "/100";
                    ProcessCraftBtn(PlayerData.NormalFragments, GetFragmentPrice(FragmentIndex));
                    for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                    {
                        if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                        {
                            PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                            PlayerData.OwnedWeapons[i].BaseWeaponData = NormalWeapons[UnityEngine.Random.Range(0, NormalWeapons.Count)];
                            Debug.Log("Your new weapon is " + PlayerData.OwnedWeapons[i].BaseWeaponData.name);
                            break;
                        }
                    }
                    break;
                case 2:
                    PlayerData.RareFragments -= GetFragmentPrice(FragmentIndex);
                    SelectedFragmentTMP.text = PlayerData.RareFragments + "/125";
                    RectanglePanelFragmentsOwnedTMP.text = PlayerData.RareFragments + "/125";
                    ProcessCraftBtn(PlayerData.RareFragments, GetFragmentPrice(FragmentIndex));
                    for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                    {
                        if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                        {
                            PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                            PlayerData.OwnedWeapons[i].BaseWeaponData = RareWeapons[UnityEngine.Random.Range(0, RareWeapons.Count)];
                            Debug.Log("Your new weapon is " + PlayerData.OwnedWeapons[i].BaseWeaponData.name);
                            break;
                        }
                    }
                    break;
                case 3:
                    PlayerData.EpicFragments -= GetFragmentPrice(FragmentIndex);
                    SelectedFragmentTMP.text = PlayerData.EpicFragments + "/150";
                    RectanglePanelFragmentsOwnedTMP.text = PlayerData.EpicFragments + "/150";
                    ProcessCraftBtn(PlayerData.EpicFragments, GetFragmentPrice(FragmentIndex));
                    for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                    {
                        if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                        {
                            PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                            PlayerData.OwnedWeapons[i].BaseWeaponData = EpicWeapons[UnityEngine.Random.Range(0, EpicWeapons.Count)];
                            Debug.Log("Your new weapon is " + PlayerData.OwnedWeapons[i].BaseWeaponData.name);
                            break;
                        }
                    }
                    break;
                case 4:
                    PlayerData.LegendFragments -= GetFragmentPrice(FragmentIndex);
                    SelectedFragmentTMP.text = PlayerData.LegendFragments + "/200";
                    RectanglePanelFragmentsOwnedTMP.text = PlayerData.LegendFragments + "/200";
                    ProcessCraftBtn(PlayerData.LegendFragments, GetFragmentPrice(FragmentIndex));
                    for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                    {
                        if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                        {
                            PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                            PlayerData.OwnedWeapons[i].BaseWeaponData = LegendWeapons[UnityEngine.Random.Range(0, LegendWeapons.Count)];
                            Debug.Log("Your new weapon is " + PlayerData.OwnedWeapons[i].BaseWeaponData.name);
                            break;
                        }
                    }
                    break;
            }
        }
        else
        {
            DisplayLoadingPanel();
            PurchaseItemRequest purchaseItem = new PurchaseItemRequest();
            purchaseItem.CatalogVersion = "Cannons";
            purchaseItem.ItemId = GetFragmentOutcome(FragmentIndex).ThisWeaponCode.ToString();
            purchaseItem.Price = GetFragmentPrice(FragmentIndex);
            purchaseItem.VirtualCurrency = GetFragmentId(FragmentIndex);

            PlayFabClientAPI.PurchaseItem(purchaseItem,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    UpdateFragmentsDisplay();
                    for(int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                    {
                        if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                        {
                            PlayerData.OwnedWeapons[i].WeaponInstanceID = resultCallback.Items[0].ItemInstanceId;
                            PlayerData.OwnedWeapons[i].BaseWeaponData = GameManager.Instance.InventoryManager.GetProperWeaponData(resultCallback.Items[0].ItemId);
                            break;
                        }
                    }
                    CraftNewWeaponCloudscript(resultCallback.Items[0].ItemInstanceId);
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        CraftNewWeapon,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }    
    }

    private void CraftNewWeaponCloudscript(string newCannonId)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest() 
        {
            FunctionName = "CraftNewWeapon",
            FunctionParameter = new { localLUID = PlayerData.LUID, newCannonId = newCannonId},
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            if (GameManager.Instance.DeserializeStringValue(JsonConvert.SerializeObject(resultCallback.FunctionResult), "messageValue") == "Success")
            {
                HideLoadingPanel();

            }
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                        () => CraftNewWeaponCloudscript(newCannonId),
                        () => ProcessError(errorCallback.ErrorMessage));
        });
    }
    #endregion

    #region EQUIP
    public void InitializeWeapons()
    {
        ActualOwnedWeapons.Clear();
        foreach (CustomWeaponData ownedWeapon in PlayerData.OwnedWeapons)
        {
            if (ownedWeapon.BaseWeaponData != null)
                ActualOwnedWeapons.Add(ownedWeapon);
            else
                break;
        }
        WeaponPageIndex = 1;
        PreviousWeaponPageBtn.interactable = false;
        if (ActualOwnedWeapons.Count > 2)
            NextWeaponPageBtn.interactable = true;
        else
            NextWeaponPageBtn.interactable = false;
        DisplayCurrentPageWeapons();
    }

    public void InitializeCostumes()
    {
        ActualCostumesOwned.Clear();
        foreach (CustomCostumeData ownedCostume in PlayerData.OwnedCostumes)
            if (ownedCostume.CostumeIsOwned)
                ActualCostumesOwned.Add(ownedCostume);
        
        CostumePageIndex = 1;
        PreviousCostumePageBtn.interactable = false;
        if (ActualCostumesOwned.Count > 2)
            NextCostumePageBtn.interactable = true;
        else
            NextCostumePageBtn.interactable = false;
        DisplayCurrentPageCostumes();
    }

    #region COSTUME
    public void NextCostumePage()
    {
        CostumePageIndex++;
        CostumePageTMP.text = "Page " + CostumePageIndex;
        PreviousCostumePageBtn.interactable = true;
        if (ActualCostumesOwned.Count > 2 * CostumePageIndex)
            NextCostumePageBtn.interactable = true;
        else
            NextCostumePageBtn.interactable = false;
        DisplayCurrentPageCostumes();
    }
    public void PreviousCostumePage()
    {
        CostumePageIndex--;
        CostumePageTMP.text = "Page " + CostumePageIndex;
        if (CostumePageIndex == 1)
            PreviousCostumePageBtn.interactable = false;
        else
            PreviousCostumePageBtn.interactable = true;
        NextCostumePageBtn.interactable = true;
        DisplayCurrentPageCostumes();
    }

    private void DisplayCurrentPageCostumes()
    {
        // LEFT
        CostumeLeftImage.sprite = ActualCostumesOwned[(2 * CostumePageIndex) - 2].BaseCostumeData.InfoSprite;
        LeftCostumeNameTMP.text = ActualCostumesOwned[(2 * CostumePageIndex) - 2].BaseCostumeData.CostumeName;
        if (ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
            EquipLeftCostumeBtn.GetComponent<Image>().sprite = UnequipSprite;
        else
            EquipLeftCostumeBtn.GetComponent<Image>().sprite = EquipSprite;

        //RIGHT
        if (ActualCostumesOwned.Count >= (2 * CostumePageIndex))
        {
            CostumeRightImage.gameObject.SetActive(true);
            CostumeRightImage.sprite = ActualCostumesOwned[(2 * CostumePageIndex) - 1].BaseCostumeData.InfoSprite;
            RightCostumeNameTMP.text = ActualCostumesOwned[(2 * CostumePageIndex) - 1].BaseCostumeData.CostumeName;
            if (ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
                EquipRightCostumeBtn.GetComponent<Image>().sprite = UnequipSprite;
            else
                EquipRightCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
        }
        else
            CostumeRightImage.gameObject.SetActive(false);
    }

    public void EquipLeftCostume()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
                UnequipCostume();
            else
            {
                PlayerData.ActiveConstumeInstanceID = ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID;
                PlayerData.ActiveCostume = ActualCostumesOwned[(2 * CostumePageIndex) - 2].BaseCostumeData;
                EquipLeftCostumeBtn.GetComponent<Image>().sprite = UnequipSprite;
                EquipRightCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
                SetOptiCostume(PlayerData.ActiveCostume.LobbyCostumeSprite);
            }
        }
        else
        {

        }
    }

    public void EquipRightCostume()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
                UnequipCostume();
            else
            {
                PlayerData.ActiveConstumeInstanceID = ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID;
                PlayerData.ActiveCostume = ActualCostumesOwned[(2 * CostumePageIndex) - 1].BaseCostumeData;
                EquipLeftCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
                EquipRightCostumeBtn.GetComponent<Image>().sprite = UnequipSprite;
                SetOptiCostume(PlayerData.ActiveCostume.LobbyCostumeSprite);
            }
        }
        else
        {

        }
    }
    #endregion

    #region WEAPON
    public void NextWeaponPage()
    {
        WeaponPageIndex++;
        WeaponPageTMP.text = "Page " + WeaponPageIndex;
        PreviousWeaponPageBtn.interactable = true;
        if (ActualOwnedWeapons.Count > 2 * WeaponPageIndex)
            NextWeaponPageBtn.interactable = true;
        else
            NextWeaponPageBtn.interactable = false;
        DisplayCurrentPageWeapons();
    }
    public void PreviousWeaponPage()
    {
        WeaponPageIndex--;
        WeaponPageTMP.text = "Page " + WeaponPageIndex;
        if (WeaponPageIndex == 1)
            PreviousWeaponPageBtn.interactable = false;
        else
            PreviousWeaponPageBtn.interactable = true;
        NextWeaponPageBtn.interactable = true;
        DisplayCurrentPageWeapons();
    }
    private void DisplayCurrentPageWeapons()
    {
        // LEFT
        WeaponLeftImage.sprite = ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].BaseWeaponData.InfoSprite;
        LeftWeaponNameTMP.text = ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].BaseWeaponData.WeaponName;
        if (ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].BonusDamage > 0)
            LeftWeaponNameTMP.text += " + " + ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].BonusDamage;
        if (ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].WeaponInstanceID == PlayerData.ActiveWeaponID)
            EquipLeftWeaponBtn.interactable = false;
        else
            EquipLeftWeaponBtn.interactable = true;

        //RIGHT
        if (ActualOwnedWeapons.Count >= (2 * WeaponPageIndex))
        {
            WeaponRightImage.gameObject.SetActive(true);
            WeaponRightImage.sprite = ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].BaseWeaponData.InfoSprite;
            RightWeaponNameTMP.text = ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].BaseWeaponData.WeaponName;
            if (ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].BonusDamage > 0)
                RightWeaponNameTMP.text += " + " + ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].BonusDamage;
            if (ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].WeaponInstanceID == PlayerData.ActiveWeaponID)
                EquipRightWeaponBtn.interactable = false;
            else
                EquipRightWeaponBtn.interactable = true;
        }
        else
            WeaponRightImage.gameObject.SetActive(false);
    }

    public void EquipLeftWeapon()
    {
        if (GameManager.Instance.DebugMode)
        {
            PlayerData.ActiveWeaponID = ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].WeaponInstanceID;
            GetActiveCannon();
            EquipLeftWeaponBtn.interactable = false;
            EquipRightWeaponBtn.interactable = true;
        }
        else
        {

        }
    }

    public void EquipRightWeapon()
    {
        if (GameManager.Instance.DebugMode)
        {
            PlayerData.ActiveWeaponID = ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].WeaponInstanceID;
            GetActiveCannon();
            EquipLeftWeaponBtn.interactable = true;
            EquipRightWeaponBtn.interactable = false;
        }
        else
        {

        }
    }
    #endregion
    #endregion

    #region CURRENT CANNON
    public void UpgradeCannon()
    {
        if(GameManager.Instance.DebugMode)
        {
            PlayerData.Optibit -= 100 + (PlayerData.ActiveCustomWeapon.BonusDamage * 50);
            PlayerData.ActiveCustomWeapon.BonusDamage++;
            CurrentCannonNameTMP.text = PlayerData.ActiveCustomWeapon.BaseWeaponData.WeaponName + " + " + PlayerData.ActiveCustomWeapon.BonusDamage;
            CurrentCannonDamageTMP.text = (PlayerData.ActiveCustomWeapon.BaseWeaponData.BaseDamage + PlayerData.ActiveCustomWeapon.BonusDamage).ToString();
        }
        DisplayOptibits();
    }
    #endregion 

    #region UTILITY
    public void OpenCombatScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "CombatScene";
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

    private void ProcessBuyButton()
    {
        if (PlayerData.Optibit >= CurrentItemCost)
            BuyBtn.interactable = true;
        else
            BuyBtn.interactable = false;
    }
    private void ProcessCraftBtn(int fragments, int quota)
    {
        if (fragments >= quota)
            CraftBtn.interactable = true;
        else
            CraftBtn.interactable = false;
        SquarePanel.SetActive(true);
        RectanglePanel.SetActive(true);
    }

    private void SetOptiCannon(Sprite _sprite)
    {
        OptiLobbyCannon.sprite = _sprite;
        OptiEquipCannon.sprite = _sprite;
    }

    private void SetOptiCostume(Sprite _sprite)
    {
        OptiLobbyCostume.gameObject.SetActive(true);
        OptiLobbyCostume.sprite = _sprite;
        OptiEquipCostume.gameObject.SetActive(true);
        OptiEquipCostume.sprite = _sprite;
    }

    private void UnequipCostume()
    {
        PlayerData.ActiveConstumeInstanceID = "";
        PlayerData.ActiveCostume = null;
        EquipLeftCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
        EquipRightCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
        OptiLobbyCostume.gameObject.SetActive(false);
        OptiEquipCostume.gameObject.SetActive(false);
    }

    private string GetConsumableId(int _value)
    {
        switch(_value)
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
                return "HealCharge";
            case 5:
                return "ParalyzeRemoval";
            case 6:
                return "WeakRemoval";
            default:
                return null;
        }
    }
    
    private string GetFragmentId(int _value)
    {
        switch(_value)
        {
            case 1:
                return "NF";
            case 2:
                return "RF";
            case 3:
                return "EF";
            case 4:
                return "LF";
            default:
                return null;
        }
    }
    private int GetFragmentPrice(int _value)
    {
        switch (_value)
        {
            case 1:
                return 100;
            case 2:
                return 125;
            case 3:
                return 150;
            case 4:
                return 200;
            default:
                return 0;
        }
    }
    private WeaponData GetFragmentOutcome(int _value)
    {
        switch(_value)
        {
            case 1:
                return NormalWeapons[UnityEngine.Random.Range(0, NormalWeapons.Count)];
            case 2:
                return RareWeapons[UnityEngine.Random.Range(0, RareWeapons.Count)];
            case 3:
                return EpicWeapons[UnityEngine.Random.Range(0, EpicWeapons.Count)];
            case 4:
                return LegendWeapons[UnityEngine.Random.Range(0, LegendWeapons.Count)];
            default:
                return null;
        }
    }

    private void UpdateFragmentsDisplay()
    {
        switch (FragmentIndex)
        {
            case 1:
                PlayerData.NormalFragments -= GetFragmentPrice(FragmentIndex);
                SelectedFragmentTMP.text = PlayerData.NormalFragments + "/100";
                RectanglePanelFragmentsOwnedTMP.text = PlayerData.NormalFragments + "/100";
                ProcessCraftBtn(PlayerData.NormalFragments, GetFragmentPrice(FragmentIndex));

                break;
            case 2:
                PlayerData.RareFragments -= GetFragmentPrice(FragmentIndex);
                SelectedFragmentTMP.text = PlayerData.RareFragments + "/125";
                RectanglePanelFragmentsOwnedTMP.text = PlayerData.RareFragments + "/125";
                ProcessCraftBtn(PlayerData.RareFragments, GetFragmentPrice(FragmentIndex));
                break;
            case 3:
                PlayerData.EpicFragments -= GetFragmentPrice(FragmentIndex);
                SelectedFragmentTMP.text = PlayerData.EpicFragments + "/150";
                RectanglePanelFragmentsOwnedTMP.text = PlayerData.EpicFragments + "/150";
                ProcessCraftBtn(PlayerData.EpicFragments, GetFragmentPrice(FragmentIndex));
                break;
            case 4:
                PlayerData.LegendFragments -= GetFragmentPrice(FragmentIndex);
                SelectedFragmentTMP.text = PlayerData.LegendFragments + "/200";
                RectanglePanelFragmentsOwnedTMP.text = PlayerData.LegendFragments + "/200";
                ProcessCraftBtn(PlayerData.LegendFragments, GetFragmentPrice(FragmentIndex));
                break;
        }
    }
    #endregion
}
