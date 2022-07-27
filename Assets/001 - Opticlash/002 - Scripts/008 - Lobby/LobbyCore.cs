using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.Linq;
using TMPro;
using UnityEngine.UI;

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
        CANNON
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

    [field: Header("OPTIBIT")]
    [field: SerializeField] private TextMeshProUGUI CoreOptibitTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ShopOptibitTMP { get; set; }

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
    [field: SerializeField] private TextMeshProUGUI BreakRemoveTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI BurnRemoveTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ConfuseRemoveTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI FreezeRemoveTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI HealChargeTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ParalyzeRemoveTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI WeakRemoveTMP { get; set; }

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
    [field: SerializeField][field: ReadOnly] private int FragmentIndex { get; set; }

    [field: Header("COSTUMES")]
    [field: SerializeField] private Image CostumeLeftImage { get; set; }
    [field: SerializeField] private Button EquipLeftCostumeBtn { get; set; }
    [field: SerializeField] private Image CostumeRightImage { get; set; }
    [field: SerializeField] private Button EquipRightCostumeBtn { get; set; }
    [field: SerializeField][field: ReadOnly] private int CostumePageIndex { get; set; }
    [field: SerializeField] private Button PreviousCostumePageBtn { get; set; }
    [field: SerializeField] private Button NextCostumePageBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI CostumePageTMP { get; set; }

    [field: Header("WEAPONS")]
    [field: SerializeField] private List<WeaponData> NormalWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> RareWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> EpicWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> LegendWeapons { get; set; }
    [field: SerializeField] private Image WeaponLeftImage { get; set; }
    [field: SerializeField] private Button EquipLeftWeaponBtn { get; set; }
    [field: SerializeField] private Image WeaponRightImage { get; set; }
    [field: SerializeField] private Button EquipRightWeaponBtn { get; set; }
    [field: SerializeField][field: ReadOnly] private int WeaponPageIndex { get; set; }
    [field: SerializeField] private Button PreviousWeaponPageBtn { get; set; }
    [field: SerializeField] private Button NextWeaponPageBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI WeaponPageTMP { get; set; }

    [Header("DEBUGGER")]
    [ReadOnly] public List<CustomCostumeData> ActualCostumesOwned;
    [ReadOnly] public List<CustomWeaponData> ActualOwnedWeapons;
    //==============================================================
    #endregion

    public void ToggleDropdown()
    {
        DropdownAnimator.SetBool("Showing", !DropdownAnimator.GetBool("Showing"));
    }

    #region CORE
    public void GetActiveCannon()
    {
        foreach (CustomWeaponData weapon in PlayerData.OwnedWeapons)
            if (weapon.WeaponInstanceID == PlayerData.ActiveWeaponID)
            {
                PlayerData.ActiveWeapon = weapon.BaseWeaponData;
                break;
            }
    }
    public void GetActiveCostume()
    {
        foreach(CustomCostumeData costume in PlayerData.OwnedCostumes)
            if(costume.CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
            {
                PlayerData.ActiveCostume = costume.BaseCostumeData;
                break;
            }
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
    }
    #endregion

    #region INVENTORY
    public void InitializeInventory()
    {
        if (GameManager.Instance.DebugMode)
        {
            BreakRemoveTMP.text = PlayerData.BreakRemovalCharges.ToString();
            BurnRemoveTMP.text = PlayerData.BurnRemovalCharges.ToString();
            ConfuseRemoveTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
            FreezeRemoveTMP.text = PlayerData.FreezeRemovalCharges.ToString();
            HealChargeTMP.text = PlayerData.HealCharges.ToString();
            ParalyzeRemoveTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
            WeakRemoveTMP.text = PlayerData.WeakRemovalCharges.ToString();
        }
    }
    #endregion

    #region CRAFT
    public void InitializeCrafting()
    {
        SelectedFragmentImage.gameObject.SetActive(false);
        CraftBtn.interactable = false;
        FragmentIndex = 0;
        if (GameManager.Instance.DebugMode)
        {
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
        else
        {

        }
    }
    public void SelectNormalFragment()
    {
        FragmentIndex = 1;
        SelectedFragmentImage.gameObject.SetActive(true);
        SelectedFragmentImage.sprite = NormalFragmentSprite;
        SelectedFragmentTMP.text = PlayerData.NormalFragments + "/100";
        ProcessCraftBtn(PlayerData.NormalFragments, 100);
    }
    public void SelectRareFragment()
    {
        FragmentIndex = 2;
        SelectedFragmentImage.gameObject.SetActive(true);
        SelectedFragmentImage.sprite = RareFragmentSprite;
        SelectedFragmentTMP.text = PlayerData.RareFragments + "/125";
        ProcessCraftBtn(PlayerData.RareFragments, 125);
    }
    public void SelectEpicFragment()
    {
        FragmentIndex = 3;
        SelectedFragmentImage.gameObject.SetActive(true);
        SelectedFragmentImage.sprite = EpicFragmentSprite;
        SelectedFragmentTMP.text = PlayerData.EpicFragments + "/150";
        ProcessCraftBtn(PlayerData.NormalFragments, 150);
    }
    public void SelectLegendFragment()
    {
        FragmentIndex = 4;
        SelectedFragmentImage.gameObject.SetActive(true);
        SelectedFragmentImage.sprite = LegendFragmentSprite;
        SelectedFragmentTMP.text = PlayerData.LegendFragments + "/200";
        ProcessCraftBtn(PlayerData.LegendFragments, 200);
    }

    public void CraftNewWeapon()
    {
        if (GameManager.Instance.DebugMode)
        {
            switch (FragmentIndex)
            {
                case 1:
                    PlayerData.NormalFragments -= 100;
                    SelectedFragmentTMP.text = PlayerData.NormalFragments + "/100";
                    ProcessCraftBtn(PlayerData.NormalFragments, 100);
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
                    PlayerData.RareFragments -= 125;
                    SelectedFragmentTMP.text = PlayerData.RareFragments + "/125";
                    ProcessCraftBtn(PlayerData.RareFragments, 125);
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
                    PlayerData.EpicFragments -= 150;
                    SelectedFragmentTMP.text = PlayerData.EpicFragments + "/150";
                    ProcessCraftBtn(PlayerData.EpicFragments, 150);
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
                    PlayerData.LegendFragments -= 200;
                    SelectedFragmentTMP.text = PlayerData.LegendFragments + "/200";
                    ProcessCraftBtn(PlayerData.LegendFragments, 200);
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
        if (ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
            EquipLeftCostumeBtn.interactable = false;
        else
            EquipLeftCostumeBtn.interactable = true;

        //RIGHT
        if (ActualCostumesOwned.Count >= (2 * CostumePageIndex))
        {
            CostumeRightImage.gameObject.SetActive(true);
            CostumeRightImage.sprite = ActualCostumesOwned[(2 * CostumePageIndex) - 1].BaseCostumeData.InfoSprite;

            if (ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
                EquipRightCostumeBtn.interactable = false;
            else
                EquipRightCostumeBtn.interactable = true;
        }
        else
            CostumeRightImage.gameObject.SetActive(false);
    }

    public void EquipLeftCostume()
    {
        if (GameManager.Instance.DebugMode)
        {
            PlayerData.ActiveConstumeInstanceID = ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID;
            PlayerData.ActiveCostume = ActualCostumesOwned[(2 * CostumePageIndex) - 2].BaseCostumeData;
            EquipLeftCostumeBtn.interactable = false;
            EquipRightCostumeBtn.interactable = true;
        }
        else
        {

        }
    }

    public void EquipRightCostume()
    {
        if (GameManager.Instance.DebugMode)
        {
            PlayerData.ActiveConstumeInstanceID = ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID;
            PlayerData.ActiveCostume = ActualCostumesOwned[(2 * CostumePageIndex) - 1].BaseCostumeData;
            EquipLeftCostumeBtn.interactable = true;
            EquipRightCostumeBtn.interactable = false;
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
        if (ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].WeaponInstanceID == PlayerData.ActiveWeaponID)
            EquipLeftWeaponBtn.interactable = false;
        else
            EquipLeftWeaponBtn.interactable = true;

        //RIGHT
        if (ActualOwnedWeapons.Count >= (2 * WeaponPageIndex))
        {
            WeaponRightImage.gameObject.SetActive(true);
            WeaponRightImage.sprite = ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].BaseWeaponData.InfoSprite;

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
            PlayerData.ActiveWeapon = ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].BaseWeaponData;
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
            PlayerData.ActiveWeapon = ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].BaseWeaponData;
            EquipLeftWeaponBtn.interactable = true;
            EquipRightWeaponBtn.interactable = false;
        }
        else
        {

        }
    }
    #endregion
    #endregion

    #region UTILITY
    public void OpenCombatScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "CombatScene";
    }

    public void DisplayOptibits()
    {
        if (GameManager.Instance.DebugMode)
        {
            CoreOptibitTMP.text = PlayerData.Optibit.ToString();
            ShopOptibitTMP.text = PlayerData.Optibit.ToString();
        }
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
    }
    #endregion
}
