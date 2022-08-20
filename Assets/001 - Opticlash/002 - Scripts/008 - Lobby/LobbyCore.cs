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
        LOOTBOX,
        QUEST,
        SETTINGS,
        EQUIP,
        COSTUME,
        CANNON,
        CURRENTCANNON,
        CLAIM,
        NEWGRANT
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
    [field: SerializeField] private UpgradeCannonCore UpgradeCannonCore { get; set; }

    [field: Header("ANIMATORS")]
    [field: SerializeField] public Animator LobbyAnimator { get; set; }
    [field: SerializeField] public Animator DropdownAnimator { get; set; }
    [field: SerializeField] public GameObject LoadingPanel { get; set; }

    [field: Header("OPTI")]
    [field: SerializeField] private SpriteRenderer OptiLobbyCannon { get; set; }
    [field: SerializeField] private Image OptiEquipCannon { get; set; }
    [field: SerializeField] private SpriteRenderer OptiLobbyCostume { get; set; }
    [field: SerializeField] private Image OptiEquipCostume { get; set; }

    [field: Header("ENERGY")]
    [field: SerializeField] private TextMeshProUGUI EnergyTMP { get; set; }

    [field: Header("OPTIBIT")]
    [field: SerializeField] private TextMeshProUGUI CoreOptibitTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI ShopOptibitTMP { get; set; }

    [field: Header("STAGE")]
    [field: SerializeField] public TextMeshProUGUI StageTMP { get; set; }

    [field: Header("EQUIP")]
    [field: SerializeField] public Sprite ActiveCostumeSprite { get; set; }
    [field: SerializeField] public Sprite InactiveCostumeSprite { get; set; }
    [field: SerializeField] public Sprite ActiveWeaponSprite { get; set; }
    [field: SerializeField] public Sprite InactiveWeaponSprite { get; set; }
    [field: SerializeField] public Button CostumeBtn { get; set; }
    [field: SerializeField] public Button WeaponBtn { get; set; }
    [field: SerializeField] private Image EquippedWeapon { get; set; }
    [field: SerializeField] private TextMeshProUGUI AttackTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI AccuracyTMP { get; set; }
    [field: SerializeField] private Image EquippedCostume { get; set; }
    [field: SerializeField] private TextMeshProUGUI DefenseTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI EvasionTMP { get; set; }
    [field: SerializeField] private Sprite NoWeaponSprite { get; set; }
    [field: SerializeField] private Sprite NoCostumeSprite { get; set; }
    [field: SerializeField] private Sprite MayGoLeftSprite { get; set; }
    [field: SerializeField] private Sprite MayNotGoLeftSprite { get; set; }
    [field: SerializeField] private Sprite MayGoRightSprite { get; set; }
    [field: SerializeField] private Sprite MayNotGoRightSprite { get; set; }

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
    [field: SerializeField] private Image CostumeRightImage { get; set; }
    [field: SerializeField] private Button EquipRightCostumeBtn { get; set; }
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
    [field: SerializeField] private Image WeaponRightImage { get; set; }
    [field: SerializeField] private Button EquipRightWeaponBtn { get; set; }
    [field: SerializeField][field: ReadOnly] private int WeaponPageIndex { get; set; }
    [field: SerializeField] private Button PreviousWeaponPageBtn { get; set; }
    [field: SerializeField] private Button NextWeaponPageBtn { get; set; }
    [field: SerializeField] private TextMeshProUGUI WeaponPageTMP { get; set; }

    [field: Header("GRANT")]
    [field: SerializeField] public GameObject GrantPanel { get; set; }
    [field: SerializeField] public GameObject GrantCanvas { get; set; }
    [field: SerializeField] public TextMeshProUGUI GrantAmountTMP { get; set; }
    [field: SerializeField] public GameObject OkBtn { get; set; }

    [field: Header("PLAY BUTTONS")]
    [field: SerializeField] private Button AdventureBtn { get; set; }
    [field: SerializeField] private Sprite MayAdventureSprite { get; set; }
    [field: SerializeField] private Sprite MayNotAdventureSprite { get; set; }

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
                        PlayerData.OwnedWeapons[currentDetectedCannon].Level = float.Parse(item.CustomData["Level"]);
                        PlayerData.OwnedWeapons[currentDetectedCannon].CalculateCannonStats();
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
                            case "SmallHealCharge":
                                PlayerData.SmallHealCharges = (int)item.RemainingUses;
                                PlayerData.SmallHealInstanceID = item.ItemInstanceId;
                                break;
                            case "HealCharge":
                                PlayerData.MediumHealCharges = (int)item.RemainingUses;
                                PlayerData.MediumHealInstanceID = item.ItemInstanceId;
                                break;
                            case "LargeHealCharge":
                                PlayerData.LargeHealCharges = (int)item.RemainingUses;
                                PlayerData.LargeHealInstanceID = item.ItemInstanceId;
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
                EnergyTMP.text = "Energy: " + PlayerData.EnergyCount.ToString();
                PlayerData.NormalFragments = resultCallback.VirtualCurrency["NF"];
                UpgradeCannonCore.NormalFragmentTMP.text = PlayerData.NormalFragments.ToString("n0");
                PlayerData.RareFragments = resultCallback.VirtualCurrency["RF"];
                UpgradeCannonCore.RareFragmentTMP.text = PlayerData.RareFragments.ToString("n0");
                PlayerData.EpicFragments = resultCallback.VirtualCurrency["EF"];
                UpgradeCannonCore.EpicFragmentTMP.text = PlayerData.EpicFragments.ToString("n0");
                PlayerData.LegendFragments = resultCallback.VirtualCurrency["LF"];
                UpgradeCannonCore.LegendFragmentTMP.text = PlayerData.LegendFragments.ToString("n0");
                DisplayOptibits();
                HideLoadingPanel();
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
        if(PlayerData.ActiveWeaponID == "NONE")
        {
            UpgradeCannonCore.CurrentCannonImage.GetComponent<Button>().interactable = false;
            OptiEquipCannon.gameObject.SetActive(false);
            OptiLobbyCannon.gameObject.SetActive(false);
            AttackTMP.gameObject.SetActive(false);
            AccuracyTMP.gameObject.SetActive(false);
            EquippedWeapon.sprite = NoWeaponSprite;
            //AdventureBtn.interactable = false;
            AdventureBtn.GetComponent<Image>().sprite = MayNotAdventureSprite;
            UpgradeCannonCore.UpgradeSuccessRateTMP.text = "NONE";
        }
        else
        {
            UpgradeCannonCore.CurrentCannonImage.GetComponent<Button>().interactable = true;
            OptiEquipCannon.gameObject.SetActive(true);
            OptiLobbyCannon.gameObject.SetActive(true);
            //AdventureBtn.interactable = true;
            AdventureBtn.GetComponent<Image>().sprite = MayAdventureSprite;
            foreach (CustomWeaponData weapon in PlayerData.OwnedWeapons)
            {
                weapon.CalculateCannonStats();
                if (weapon.WeaponInstanceID == PlayerData.ActiveWeaponID)
                {
                    PlayerData.ActiveCustomWeapon = weapon;
                    EquippedWeapon.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.EquippedSprite;
                    AttackTMP.text = "ATK: " + PlayerData.ActiveCustomWeapon.Attack;
                    AccuracyTMP.text = "ACC: " + PlayerData.ActiveCustomWeapon.Accuracy;
                    UpgradeCannonCore.CurrentCannonImage.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.CurrentSprite;
                    UpgradeCannonCore.CurrentCannonBigImage.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.CurrentBigSprite;
                    UpgradeCannonCore.CurrentCannonDamageTMP.text = PlayerData.ActiveCustomWeapon.Attack.ToString("n0");
                    UpgradeCannonCore.CurrentCannonAccuracyTMP.text = PlayerData.ActiveCustomWeapon.Accuracy.ToString("n0");
                    if (PlayerData.ActiveCustomWeapon.BaseWeaponData.Abilities != "")
                        UpgradeCannonCore.CurrentCannonAbilitiesTMP.text = PlayerData.ActiveCustomWeapon.BaseWeaponData.Abilities;
                    else
                        UpgradeCannonCore.CurrentCannonAbilitiesTMP.text = "NONE";

                    if(PlayerData.ActiveCustomWeapon.Level == PlayerData.ActiveCustomWeapon.BaseWeaponData.StartingLevel)
                    {
                        UpgradeCannonCore.SuccessChance = 100;
                        UpgradeCannonCore.UpgradeSuccessRateTMP.text = UpgradeCannonCore.SuccessChance.ToString("n0");
                    }
                    else
                    {
                        UpgradeCannonCore.SuccessChance = 100;
                        float dummyLevel = PlayerData.ActiveCustomWeapon.BaseWeaponData.StartingLevel;
                        while(dummyLevel <= PlayerData.ActiveCustomWeapon.Level)
                        {
                            UpgradeCannonCore.SuccessChance -= 10;
                            dummyLevel += PlayerData.ActiveCustomWeapon.BaseWeaponData.UpgradeValue;
                        }
                        UpgradeCannonCore.UpgradeSuccessRateTMP.text = UpgradeCannonCore.SuccessChance.ToString("n0") + "%";

                    }
                    break;
                }
            }
            SetOptiCannon(PlayerData.ActiveCustomWeapon.BaseWeaponData.AnimatedSprite);
        }
        
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
            EquippedCostume.sprite = PlayerData.ActiveCostume.EquippedSprite;
            DefenseTMP.gameObject.SetActive(true);
            EvasionTMP.gameObject.SetActive(true);
            DefenseTMP.text = "DEF: " + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
            EvasionTMP.text = "EVA" + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
            SetOptiCostume(PlayerData.ActiveCostume.LobbyCostumeSprite);
        }
        else
        {
            OptiLobbyCostume.gameObject.SetActive(false);
            OptiEquipCostume.gameObject.SetActive(false);
            EquippedCostume.sprite = NoCostumeSprite;
            DefenseTMP.text = "DEF: 10";
            EvasionTMP.text = "EVA: 10";
        }
    }

    public void DisplayOptibits()
    {
        CoreOptibitTMP.text = PlayerData.Optibit.ToString("n0");
        ShopOptibitTMP.text = PlayerData.Optibit.ToString("n0");
        if(PlayerData.ActiveWeaponID != "NONE")
        {
            UpgradeCannonCore.CurrentCannonDamageTMP.text = PlayerData.ActiveCustomWeapon.Attack.ToString("n0");
            UpgradeCannonCore.CurrentCannonRequiredFragments.text = GetFragmentBasis().ToString("n0") + " / " + PlayerData.ActiveCustomWeapon.FragmentUpgradeCost.ToString("n0");
            UpgradeCannonCore.CurrentCannonRequiredOptibit.text = PlayerData.Optibit.ToString("n0") + " / " + PlayerData.ActiveCustomWeapon.OptibitUpgradeCost.ToString("n0");
            if (PlayerData.Optibit >= PlayerData.ActiveCustomWeapon.OptibitUpgradeCost && PlayerData.ActiveCustomWeapon.FragmentUpgradeCost <= GetFragmentBasis())
                UpgradeCannonCore.UpgradeCannonBtn.GetComponent<Image>().sprite = UpgradeCannonCore.MayUpgradeSprite;
            else
                UpgradeCannonCore.UpgradeCannonBtn.GetComponent<Image>().sprite = UpgradeCannonCore.MayNotUpgradeSprite;
        }
    }

    public void LogOut()
    {
        if(GameManager.Instance.DebugMode)
        {
            PlayerPrefs.DeleteAll();
            PlayerData.ResetPlayerData();
            GameManager.Instance.SceneController.CurrentScene = "EntryScene";
        }
        else
        {
            DisplayLoadingPanel();
            UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
            updateUserData.Data = new Dictionary<string, string>();
            updateUserData.Data.Add("LUID", "NONE");

            PlayFabClientAPI.UpdateUserData(updateUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    PlayerPrefs.DeleteAll();
                    PlayerData.ResetPlayerData();
                    PlayFabClientAPI.ForgetAllCredentials();
                    HideLoadingPanel();
                    GameManager.Instance.SceneController.CurrentScene = "EntryScene";
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        LogOut,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
        
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
        GameManager.Instance.SFXAudioManager.PlayShowOptiSFX();
        ActualOwnedWeapons.Clear();
        foreach (CustomWeaponData ownedWeapon in PlayerData.OwnedWeapons)
        {
            if (ownedWeapon.BaseWeaponData != null)
                ActualOwnedWeapons.Add(ownedWeapon);
            else
                break;
        }
        WeaponPageIndex = 1;
        PreviousWeaponPageBtn.GetComponent<Image>().sprite = MayNotGoLeftSprite;
        if (ActualOwnedWeapons.Count > 2)
            NextWeaponPageBtn.GetComponent<Image>().sprite = MayGoRightSprite;
        else
            NextWeaponPageBtn.GetComponent<Image>().sprite = MayNotGoRightSprite;

        if(ActualOwnedWeapons.Count > 0)
            DisplayCurrentPageWeapons();
        else
        {
            Debug.Log("You do not own any weapons");
            WeaponLeftImage.gameObject.SetActive(false);
            WeaponRightImage.gameObject.SetActive(false);
        }
    }

    public void InitializeCostumes()
    {
        GameManager.Instance.SFXAudioManager.PlayShowOptiSFX();
        ActualCostumesOwned.Clear();
        foreach (CustomCostumeData ownedCostume in PlayerData.OwnedCostumes)
            if (ownedCostume.CostumeIsOwned)
                ActualCostumesOwned.Add(ownedCostume);
        
        CostumePageIndex = 1;
        PreviousCostumePageBtn.GetComponent<Image>().sprite = MayNotGoLeftSprite;
        if (ActualCostumesOwned.Count > 2)
            NextCostumePageBtn.GetComponent<Image>().sprite = MayGoRightSprite;
        else
            NextCostumePageBtn.GetComponent<Image>().sprite = MayNotGoRightSprite;
        if(ActualCostumesOwned.Count > 0)
            DisplayCurrentPageCostumes();
        else
        {
            CostumeLeftImage.gameObject.SetActive(false);
            CostumeRightImage.gameObject.SetActive(false);
        }
    }

    #region COSTUME
    public void NextCostumePage()
    {
        if(ActualCostumesOwned.Count > 2 * CostumePageIndex)
        {
            CostumePageIndex++;
            CostumePageTMP.text = "Page " + CostumePageIndex;
            PreviousCostumePageBtn.GetComponent<Image>().sprite = MayGoLeftSprite;
            if (ActualCostumesOwned.Count > 2 * CostumePageIndex)
                NextCostumePageBtn.GetComponent<Image>().sprite = MayGoRightSprite;
            else
                NextCostumePageBtn.GetComponent<Image>().sprite = MayNotGoRightSprite;
            DisplayCurrentPageCostumes();
        }
        
    }
    public void PreviousCostumePage()
    {
        if(CostumePageIndex != 1)
        {
            CostumePageIndex--;
            CostumePageTMP.text = "Page " + CostumePageIndex;
            if (CostumePageIndex == 1)
                PreviousCostumePageBtn.GetComponent<Image>().sprite = MayNotGoLeftSprite;
            else
                PreviousCostumePageBtn.GetComponent<Image>().sprite = MayGoLeftSprite;
            NextCostumePageBtn.GetComponent<Image>().sprite = MayGoRightSprite;
            DisplayCurrentPageCostumes();
        }
    }

    private void DisplayCurrentPageCostumes()
    {
        // LEFT
        CostumeLeftImage.sprite = ActualCostumesOwned[(2 * CostumePageIndex) - 2].BaseCostumeData.InfoSprite;
        if (ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
        {
            EquippedCostume.sprite = PlayerData.ActiveCostume.EquippedSprite;
            EquipLeftCostumeBtn.GetComponent<Image>().sprite = UnequipSprite;
        }
        else
            EquipLeftCostumeBtn.GetComponent<Image>().sprite = EquipSprite;

        //RIGHT
        if (ActualCostumesOwned.Count >= (2 * CostumePageIndex))
        {
            CostumeRightImage.gameObject.SetActive(true);
            CostumeRightImage.sprite = ActualCostumesOwned[(2 * CostumePageIndex) - 1].BaseCostumeData.InfoSprite;
            if (ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
            {
                EquippedCostume.sprite = PlayerData.ActiveCostume.EquippedSprite;
                EquipRightCostumeBtn.GetComponent<Image>().sprite = UnequipSprite;
            }
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
                EquippedCostume.sprite = PlayerData.ActiveCostume.EquippedSprite;
                SetOptiCostume(PlayerData.ActiveCostume.LobbyCostumeSprite);
                DefenseTMP.gameObject.SetActive(true);
                EvasionTMP.gameObject.SetActive(true);
                DefenseTMP.text = "DEF: " + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
                EvasionTMP.text = "EVA" + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
            }
        }
        else
        {
            if (ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
                UnequipCostume();
            else
            {
                UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
                updateUserData.Data = new Dictionary<string, string>();
                updateUserData.Data.Add("ActiveCostume", PlayerData.ActiveConstumeInstanceID = ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID);
                PlayFabClientAPI.UpdateUserData(updateUserData,
                    resultCallback =>
                    {
                        PlayerData.ActiveConstumeInstanceID = ActualCostumesOwned[(2 * CostumePageIndex) - 2].CostumeInstanceID;
                        PlayerData.ActiveCostume = ActualCostumesOwned[(2 * CostumePageIndex) - 2].BaseCostumeData;
                        EquipLeftCostumeBtn.GetComponent<Image>().sprite = UnequipSprite;
                        EquipRightCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
                        EquippedCostume.sprite = PlayerData.ActiveCostume.EquippedSprite;
                        SetOptiCostume(PlayerData.ActiveCostume.LobbyCostumeSprite);
                        DefenseTMP.gameObject.SetActive(true);
                        EvasionTMP.gameObject.SetActive(true);
                        DefenseTMP.text = "DEF: " + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
                        EvasionTMP.text = "EVA" + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            EquipLeftCostume,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
                
            }
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
                DefenseTMP.gameObject.SetActive(true);
                EvasionTMP.gameObject.SetActive(true);
                DefenseTMP.text = "DEF: " + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
                EvasionTMP.text = "EVA: " + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
            }
        }
        else
        {
            if (ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID == PlayerData.ActiveConstumeInstanceID)
                UnequipCostume();
            else
            {
                UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
                updateUserData.Data = new Dictionary<string, string>();
                updateUserData.Data.Add("ActiveCostume", PlayerData.ActiveConstumeInstanceID = ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID);
                PlayFabClientAPI.UpdateUserData(updateUserData,
                    resultCallback =>
                    {
                        PlayerData.ActiveConstumeInstanceID = ActualCostumesOwned[(2 * CostumePageIndex) - 1].CostumeInstanceID;
                        PlayerData.ActiveCostume = ActualCostumesOwned[(2 * CostumePageIndex) - 1].BaseCostumeData;
                        EquipLeftCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
                        EquipRightCostumeBtn.GetComponent<Image>().sprite = UnequipSprite;
                        SetOptiCostume(PlayerData.ActiveCostume.LobbyCostumeSprite);
                        DefenseTMP.gameObject.SetActive(true);
                        EvasionTMP.gameObject.SetActive(true);
                        DefenseTMP.text = "DEF: " + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
                        EvasionTMP.text = "EVA: " + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            EquipRightCostume,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });

            }
        }
    }
    #endregion

    #region WEAPON
    public void NextWeaponPage()
    {
        if(ActualOwnedWeapons.Count > 2 * WeaponPageIndex)
        {
            WeaponPageIndex++;
            WeaponPageTMP.text = "Page " + WeaponPageIndex;
            PreviousWeaponPageBtn.GetComponent<Image>().sprite = MayGoLeftSprite;
            if (ActualOwnedWeapons.Count > 2 * WeaponPageIndex)
                NextWeaponPageBtn.GetComponent<Image>().sprite = MayGoRightSprite;
            else
                NextWeaponPageBtn.GetComponent<Image>().sprite = MayNotGoRightSprite;
            DisplayCurrentPageWeapons();
        }
    }
    public void PreviousWeaponPage()
    {
        if(WeaponPageIndex != 1)
        {
            WeaponPageIndex--;
            WeaponPageTMP.text = "Page " + WeaponPageIndex;
            if (WeaponPageIndex == 1)
                PreviousWeaponPageBtn.GetComponent<Image>().sprite = MayNotGoLeftSprite;
            else
                PreviousWeaponPageBtn.interactable = MayGoLeftSprite;
            NextWeaponPageBtn.GetComponent<Image>().sprite = MayGoRightSprite;
            DisplayCurrentPageWeapons();
        }
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
            GetActiveCannon();
            EquipLeftWeaponBtn.interactable = false;
            EquipRightWeaponBtn.interactable = true;
        }
        else
        {
            UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
            updateUserData.Data = new Dictionary<string, string>();
            updateUserData.Data.Add("ActiveCannon", PlayerData.ActiveWeaponID = ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].WeaponInstanceID);

            PlayFabClientAPI.UpdateUserData(updateUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    PlayerData.ActiveWeaponID = ActualOwnedWeapons[(2 * WeaponPageIndex) - 2].WeaponInstanceID;
                    GetActiveCannon();
                    EquipLeftWeaponBtn.interactable = false;
                    EquipRightWeaponBtn.interactable = true;
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        EquipLeftWeapon,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
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
            UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
            updateUserData.Data = new Dictionary<string, string>();
            updateUserData.Data.Add("ActiveCannon", PlayerData.ActiveWeaponID = ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].WeaponInstanceID);

            PlayFabClientAPI.UpdateUserData(updateUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    PlayerData.ActiveWeaponID = ActualOwnedWeapons[(2 * WeaponPageIndex) - 1].WeaponInstanceID;
                    GetActiveCannon();
                    EquipLeftWeaponBtn.interactable = true;
                    EquipRightWeaponBtn.interactable = false;
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        EquipRightWeapon,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }
    #endregion
    #endregion

    #region UTILITY
    public void DisplayComingSoon()
    {
        GameManager.Instance.DisplayErrorPanel("Coming Soon");
    }
    public void OpenAdventureScene()
    {
        if (PlayerData.ActiveWeaponID == "NONE")
            GameManager.Instance.DisplayErrorPanel("You do not own any cannons");
        else
            GameManager.Instance.SceneController.CurrentScene = "AdventureScene";
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
        if(GameManager.Instance.DebugMode)
        {
            PlayerData.ActiveConstumeInstanceID = "NONE";
            PlayerData.ActiveCostume = null;
            EquipLeftCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
            EquipRightCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
            OptiLobbyCostume.gameObject.SetActive(false);
            OptiEquipCostume.gameObject.SetActive(false);
            DefenseTMP.gameObject.SetActive(true);
            EvasionTMP.gameObject.SetActive(true);
            DefenseTMP.text = "Defense: " + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
            EvasionTMP.text = "Evasion" + ((5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10).ToString();
        }
        else
        {
            UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
            updateUserData.Data = new Dictionary<string, string>();
            updateUserData.Data.Add("ActiveCostume", "NONE");
            PlayFabClientAPI.UpdateUserData(updateUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    PlayerData.ActiveConstumeInstanceID = "NONE";
                    PlayerData.ActiveCostume = null;
                    EquipLeftCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
                    EquipRightCostumeBtn.GetComponent<Image>().sprite = EquipSprite;
                    EquippedCostume.sprite = NoCostumeSprite;
                    OptiLobbyCostume.gameObject.SetActive(false);
                    OptiEquipCostume.gameObject.SetActive(false);
                    DefenseTMP.text = "DEF: 10";
                    EvasionTMP.text = "EVA: 10";
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        UnequipCostume,
                        () => ProcessError(errorCallback.ErrorMessage));   
                });
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

    public int GetFragmentBasis()
    {
        if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C4 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C5)
            return PlayerData.NormalFragments;
        else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B4)
            return PlayerData.RareFragments;
        else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A3)
            return PlayerData.EpicFragments;
        else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S2)
            return PlayerData.LegendFragments;
        else
            return 0;
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
