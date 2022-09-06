using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class LootboxCore : MonoBehaviour
{
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }
    [field: SerializeField] private LobbyController LobbyController { get; set; }

    [field: Header("LOOTBOX")]
    [field: SerializeField] private Image TopBoxImage { get; set; }
    [field: SerializeField] private Image BotBoxImage { get; set; }
    [field: SerializeField] private Button OpenLootboxBtn { get; set; }
    [field: SerializeField] private Button PreviousLootboxBtn { get; set; }
    [field: SerializeField] private Button NextLootboxBtn { get; set; }
    [field: SerializeField] private GameObject CommonLootboxPanel { get; set; }
    [field: SerializeField] private GameObject RareLootboxPanel { get; set; }
    [field: SerializeField] private GameObject EpicLootboxPanel { get; set; }
    [field: SerializeField] private GameObject LegendLootbox1Panel { get; set; }
    [field: SerializeField] private GameObject LegendLootbox2Panel { get; set; }
    [field: SerializeField] private TextMeshProUGUI CommonLootboxOwned { get; set; }
    [field: SerializeField] private TextMeshProUGUI RareLootboxOwned { get; set; }
    [field: SerializeField] private TextMeshProUGUI EpicLootboxOwned { get; set; }
    [field: SerializeField] private TextMeshProUGUI LegendLootbox1Owned { get; set; }
    [field: SerializeField] private TextMeshProUGUI LegendLootbox2Owned { get; set; }
    [field: SerializeField][field: ReadOnly] private int LootboxIndex { get; set; }

    [field: Header("LOOTBOX ROSTER")]
    [field: SerializeField] private Sprite TopCommonBox { get; set; }
    [field: SerializeField] private Sprite BotCommonBox { get; set; }
    [field: SerializeField] private GameObject CommonAnimatedBox { get; set; }
    [field: SerializeField] private Sprite TopRareBox { get; set; }
    [field: SerializeField] private Sprite BotRareBox { get; set; }
    [field: SerializeField] private GameObject RareAnimatedBox { get; set; }
    [field: SerializeField] private Sprite TopEpicBox { get; set; }
    [field: SerializeField] private Sprite BotEpicBox { get; set; }
    [field: SerializeField] private GameObject EpicAnimatedBox { get; set; }
    [field: SerializeField] private Sprite TopLegend1Box { get; set; }
    [field: SerializeField] private Sprite BotLegend1Box { get; set; }
    [field: SerializeField] private GameObject Legend1AnimatedBox { get; set; }
    [field: SerializeField] private Sprite TopLegend2Box { get; set; }
    [field: SerializeField] private Sprite BotLegend2Box { get; set; }
    [field: SerializeField] private GameObject Legend2AnimatedBox { get; set; }

    [field: Header("WEAPONS")]
    [field: SerializeField] private List<WeaponData> NormalWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> RareWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> EpicWeapons { get; set; }
    [field: SerializeField] private List<WeaponData> LegendWeapons { get; set; }

    [field: Header("FRAGMENTS")]
    [field: SerializeField] private Sprite NormalFragment { get; set; }
    [field: SerializeField] private Sprite RareFragment { get; set; }
    [field: SerializeField] private Sprite EpicFragment { get; set; }
    [field: SerializeField] private Sprite LegendFragment { get; set; }

    [field: Header("CONSUMABLES")]
    [field: SerializeField] private Sprite SmallHeal { get; set; }
    [field: SerializeField] private Sprite MediumHeal { get; set; }
    [field: SerializeField] private Sprite LargeHeal { get; set; }
    [field: SerializeField] private Sprite BreakRemoveSprite { get; set; }
    [field: SerializeField] private Sprite BurnRemoveSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseRemoveSprite { get; set; }
    [field: SerializeField] private Sprite FreezeRemoveSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeRemoveSprite { get; set; }
    [field: SerializeField] private Sprite WeakRemoveSprite { get; set; }

    [field: Header("REWARDS")]
    [field: SerializeField] public GameObject ConsumableReward { get; set; }
    [field: SerializeField] public SpriteRenderer ConsumableSprite { get; set; }
    [field: SerializeField] public GameObject FragmentReward { get; set; }
    [field: SerializeField] private SpriteRenderer FragmentSprite { get; set; }
    [field: SerializeField] public GameObject CannonReward { get; set; }
    [field: SerializeField] private SpriteRenderer CannonSprite { get; set; }
    [field: SerializeField] public GameObject CostumeReward { get; set; }
    [field: SerializeField] private SpriteRenderer CostumeRewardSprite { get; set; }

    [field: Header("OPEN")]
    [field: SerializeField] private Sprite MayOpenSprite { get; set; }
    [field: SerializeField] private Sprite MayNotOpenSprite { get; set; }
    [field: SerializeField] private Sprite MayGoLeftSprite { get; set; }
    [field: SerializeField] private Sprite MayNotGoLeftSprite { get; set; }
    [field: SerializeField] private Sprite MayGoRightSprite { get; set; }
    [field: SerializeField] private Sprite MayNotGoRightSprite { get; set; }


    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] private int failedCallbackCounter { get; set; }
    [field: SerializeField][field: ReadOnly] private bool HasThisLootbox { get; set; }
    [field: SerializeField][field: ReadOnly] private int NormalFragmentsDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int RareFragmentsDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int EpicFragmentsDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int LegendFragmentsDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int SmallHealSkillDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int MediumHealSkillDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int LargeHealSkillDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int BreakRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int WeakRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int FreezeRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int ParalyzeRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int ConfuseRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int BurnRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private List<string> CannonStrings { get; set; }
    [field: SerializeField][field: ReadOnly] public List<string> RewardsToDisplay { get; set; }
    [field: SerializeField][field: ReadOnly] public int RewardIndex { get; set; }

    private void Awake()
    {
        CannonStrings = new List<string>();
        RewardsToDisplay = new List<string>();
    }

    public void InitializeLootbox()
    {
        if (GameManager.Instance.DebugMode)
        {
            LootboxIndex = 1;
            PreviousLootboxBtn.GetComponent<Image>().sprite = MayNotGoLeftSprite;
            NextLootboxBtn.GetComponent<Image>().sprite = MayGoRightSprite;
            CommonLootboxOwned.text = PlayerData.CommonLootboxCount.ToString();
            RareLootboxOwned.text = PlayerData.RareLootboxCount.ToString();
            EpicLootboxOwned.text = PlayerData.EpicLootboxCount.ToString();
            LegendLootbox1Owned.text = PlayerData.LegendaryLootbox1Count.ToString();
            LegendLootbox2Owned.text = PlayerData.LegendaryLootbox2Count.ToString();
            DisplayCurrentLootbox();
        }
        else
        {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    foreach (ItemInstance item in resultCallback.Inventory)
                    {
                        if (item.ItemClass == "LOOTBOX")
                        {
                            switch (item.ItemId)
                            {
                                case "CommonLootbox":
                                    PlayerData.CommonLootboxCount = (int)item.RemainingUses;
                                    PlayerData.CommonLootboxInstanceID = item.ItemInstanceId;
                                    break;
                                case "RareLootbox":
                                    PlayerData.RareLootboxCount = (int)item.RemainingUses;
                                    PlayerData.RareLootboxInstanceID = item.ItemInstanceId;
                                    break;
                                case "EpicLootbox":
                                    PlayerData.EpicLootboxCount = (int)item.RemainingUses;
                                    PlayerData.EpicLootboxInstanceID = item.ItemInstanceId;
                                    break;
                                case "LegendLootbox1":
                                    PlayerData.LegendaryLootbox1Count = (int)item.RemainingUses;
                                    PlayerData.LegendaryLootbox1InstanceID = item.ItemInstanceId;
                                    break;
                                case "LegendLootbox2":
                                    PlayerData.LegendaryLootbox2Count = (int)item.RemainingUses;
                                    PlayerData.LegendaryLootbox2InstanceID = item.ItemInstanceId;
                                    break;
                            }
                        }
                    }
                    LootboxIndex = 1;
                    PreviousLootboxBtn.GetComponent<Image>().sprite = MayNotGoLeftSprite;
                    NextLootboxBtn.GetComponent<Image>().sprite = MayGoRightSprite;
                    CommonLootboxOwned.text = PlayerData.CommonLootboxCount.ToString();
                    RareLootboxOwned.text = PlayerData.RareLootboxCount.ToString();
                    EpicLootboxOwned.text = PlayerData.EpicLootboxCount.ToString();
                    LegendLootbox1Owned.text = PlayerData.LegendaryLootbox1Count.ToString();
                    LegendLootbox2Owned.text = PlayerData.LegendaryLootbox2Count.ToString();
                    DisplayCurrentLootbox();
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        InitializeLootbox,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    public void PreviousLootbox()
    {
        if(LootboxIndex != 1)
        {
            LootboxIndex--;
            if (LootboxIndex == 1)
                PreviousLootboxBtn.GetComponent<Image>().sprite = MayNotGoLeftSprite;
            else
                PreviousLootboxBtn.GetComponent<Image>().sprite = MayGoLeftSprite;
            NextLootboxBtn.GetComponent<Image>().sprite = MayGoRightSprite;
            DisplayCurrentLootbox();
        }
    }

    public void NextLootbox()
    {if(LootboxIndex != 5)
        {
            LootboxIndex++;
            PreviousLootboxBtn.GetComponent<Image>().sprite = MayGoLeftSprite;
            if (LootboxIndex == 5)
                NextLootboxBtn.GetComponent<Image>().sprite = MayNotGoRightSprite;
            else
                NextLootboxBtn.GetComponent<Image>().sprite = MayGoRightSprite;
            DisplayCurrentLootbox();
        }
    }

    public void OpenLootboxButton()
    {
        if(HasThisLootbox)
        {
            switch (LootboxIndex)
            {
                case 1:
                    OpenCommonLootbox();
                    break;
                case 2:
                    OpenRareLootbox();
                    break;
                case 3:
                    OpenEpicLootbox();
                    break;
                case 4:
                    OpenLegendaryLootbox1();
                    break;
                case 5:
                    OpenLegendaryLootbox2();
                    break;
            }
        }
        else
        {
            switch (LootboxIndex)
            {
                case 1:
                    GameManager.Instance.DisplayErrorPanel("You do not have any Common Lootboxes");
                    break;
                case 2:
                    GameManager.Instance.DisplayErrorPanel("You do not have any Rare Lootboxes");
                    break;
                case 3:
                    GameManager.Instance.DisplayErrorPanel("You do not have any Epic Lootboxes");
                    break;
                case 4:
                    GameManager.Instance.DisplayErrorPanel("You do not have any Legend 1 Lootboxes");
                    break;
                case 5:
                    GameManager.Instance.DisplayErrorPanel("You do not have any Legend 2 Lootboxes");
                    break;
            }
        }
    }

    #region COMMON LOOTBOX
    private void OpenCommonLootbox()
    {
        int randomNum = UnityEngine.Random.Range(0, 100);
        if (GameManager.Instance.DebugMode)
        {
            if(randomNum >= 50)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = NormalWeapons[UnityEngine.Random.Range(0, NormalWeapons.Count)];
                        break;
                    }
                }
            }
            ResetDrops();
            DropRandomConsumables(3);
            UpdatePlayerInventory();

            PlayerData.CommonLootboxCount--;
            CommonLootboxOwned.text = PlayerData.CommonLootboxCount.ToString();
            DisplayCurrentLootbox();
            LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
            LobbyCore.OkBtn.SetActive(false);
            LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
            LobbyCore.GrantPanel.SetActive(true);
            CommonAnimatedBox.SetActive(true);
            GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
            CommonAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");

        }
        else
        {
            LobbyCore.DisplayLoadingPanel();
            ConsumeItemRequest consumeItem = new ConsumeItemRequest();
            consumeItem.ItemInstanceId = PlayerData.CommonLootboxInstanceID;
            consumeItem.ConsumeCount = 1;

            PlayFabClientAPI.ConsumeItem(consumeItem,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    GetNormalBoxContent(randomNum);
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        OpenCommonLootbox,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    private void GetNormalBoxContent(int randomNum)
    {
        RewardsToDisplay.Clear();
        ResetDrops();
        DropRandomConsumables(3);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GrantRewardedItems",
            FunctionParameter = new
            {
                localLUID = PlayerData.LUID,

                optibit = 0,
                normalFragment = 500,
                rareFragment = 0,
                epicFragment = 0,
                legendFragment = 0,
                smallHeal = SmallHealSkillDropped,
                mediumHeal = MediumHealSkillDropped,
                largeHeal = LargeHealSkillDropped,
                breakRemove = BreakRemoveDropped,
                burnRemove = BurnRemoveDropped,
                confuseRemove = ConfuseRemoveDropped,
                freezeRemove = FreezeRemoveDropped,
                paralyzeRemove = ParalyzeRemoveDropped,
                weakRemove = WeakRemoveDropped
            },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            Debug.Log(resultCallback.FunctionResult);
            if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
            {
                PlayerData.CommonLootboxCount--;
                CommonLootboxOwned.text = PlayerData.CommonLootboxCount.ToString();
                DisplayCurrentLootbox();
                if (PlayerData.CommonLootboxCount > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                    PlayerData.CommonLootboxInstanceID = "";
                }
                LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                LobbyCore.OkBtn.SetActive(false);

                LobbyCore.GrantAmountTMP.text = "YOU GAINED 500 NORMAL FRAGMENTS";
                FragmentSprite.sprite = NormalFragment;
                NormalFragmentsDropped = 500;
                UpdatePlayerInventory();
                CannonStrings.Clear();
                RewardsToDisplay.Insert(0, "NORMAL");

                if (randomNum >= 50)
                {
                    AddCommonCannon();
                    DropCannons(0);
                }
                else
                {
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    CommonAnimatedBox.SetActive(true);
                    LobbyCore.HideLoadingPanel();

                    GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
                    CommonAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
                }
            }     
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                () => GetNormalBoxContent(randomNum),
                () => ProcessError(errorCallback.ErrorMessage));
        });
    }
    #endregion

    #region RARE LOOTBOX
    private void OpenRareLootbox()
    {
        int randomNum = UnityEngine.Random.Range(0, 100);
        if (GameManager.Instance.DebugMode)
        {
            if (randomNum >= 60)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = NormalWeapons[UnityEngine.Random.Range(0, NormalWeapons.Count)];
                        break;
                    }
                }
            }

            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 30)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = RareWeapons[UnityEngine.Random.Range(0, RareWeapons.Count)];
                        break;
                    }
                }
            }

            ResetDrops();
            DropRandomConsumables(5);
            UpdatePlayerInventory();

            PlayerData.RareLootboxCount--;
            RareLootboxOwned.text = PlayerData.RareLootboxCount.ToString();
            DisplayCurrentLootbox();
            LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
            LobbyCore.OkBtn.SetActive(false);
            LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
            LobbyCore.GrantPanel.SetActive(true);
            RareAnimatedBox.SetActive(true);
            GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
            RareAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
        }
        else
        {
            LobbyCore.DisplayLoadingPanel();
            ConsumeItemRequest consumeItem = new ConsumeItemRequest();
            consumeItem.ItemInstanceId = PlayerData.RareLootboxInstanceID;
            consumeItem.ConsumeCount = 1;

            PlayFabClientAPI.ConsumeItem(consumeItem,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    GetRareBoxContent(randomNum);
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        OpenRareLootbox,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    private void GetRareBoxContent(int randomNum)
    {
        RewardsToDisplay.Clear();
        ResetDrops();
        DropRandomConsumables(5);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GrantRewardedItems",
            FunctionParameter = new
            {
                localLUID = PlayerData.LUID,

                optibit = 0,
                normalFragment = 1000,
                rareFragment = 500,
                epicFragment = 0,
                legendFragment = 0,
                smallHeal = SmallHealSkillDropped,
                mediumHeal = MediumHealSkillDropped,
                largeHeal = LargeHealSkillDropped,
                breakRemove = BreakRemoveDropped,
                burnRemove = BurnRemoveDropped,
                confuseRemove = ConfuseRemoveDropped,
                freezeRemove = FreezeRemoveDropped,
                paralyzeRemove = ParalyzeRemoveDropped,
                weakRemove = WeakRemoveDropped
            },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            Debug.Log(resultCallback.FunctionResult);
            if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
            {
                PlayerData.RareLootboxCount--;
                RareLootboxOwned.text = PlayerData.RareLootboxCount.ToString();
                DisplayCurrentLootbox();
                if (PlayerData.RareLootboxCount > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                    PlayerData.RareLootboxInstanceID = "";
                }
                LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                LobbyCore.OkBtn.SetActive(false);

                LobbyCore.GrantAmountTMP.text = "YOU GAINED 500 RARE FRAGMENTS";
                FragmentSprite.sprite = RareFragment;
                NormalFragmentsDropped = 1000;
                RareFragmentsDropped = 500;
                UpdatePlayerInventory();

                CannonStrings.Clear();
                RewardsToDisplay.Insert(0, "RARE");
                RewardsToDisplay.Insert(1, "NORMAL");
                if (randomNum >= 60)
                    AddCommonCannon();
                
                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 30)
                    AddRareCannon();

                if (CannonStrings.Count > 0)
                    DropCannons(1);
                else
                {
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    RareAnimatedBox.SetActive(true);
                    LobbyCore.HideLoadingPanel();

                    GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
                    RareAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
                }
            }
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                () => GetRareBoxContent(randomNum),
                () => ProcessError(errorCallback.ErrorMessage));
        });
    }
    #endregion

    #region EPIC LOOTBOX
    public void OpenEpicLootbox()
    {
        int randomNum = UnityEngine.Random.Range(0, 100);
        if (GameManager.Instance.DebugMode)
        {
            if (randomNum >= 70)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = NormalWeapons[UnityEngine.Random.Range(0, NormalWeapons.Count)];
                        break;
                    }
                }
            }
            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 40)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = RareWeapons[UnityEngine.Random.Range(0, RareWeapons.Count)];
                        break;
                    }
                }
            }
            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 30)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = EpicWeapons[UnityEngine.Random.Range(0, EpicWeapons.Count)];
                        break;
                    }
                }
            }

            PlayerData.EpicLootboxCount--;
            EpicLootboxOwned.text = PlayerData.EpicLootboxCount.ToString();
            DisplayCurrentLootbox();
            LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
            LobbyCore.OkBtn.SetActive(false);
            LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
            LobbyCore.GrantPanel.SetActive(true);
            EpicAnimatedBox.SetActive(true);
            GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
            EpicAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
        }
        else
        {
            LobbyCore.DisplayLoadingPanel();
            ConsumeItemRequest consumeItem = new ConsumeItemRequest();
            consumeItem.ItemInstanceId = PlayerData.EpicLootboxInstanceID;
            consumeItem.ConsumeCount = 1;

            PlayFabClientAPI.ConsumeItem(consumeItem,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    GetEpicBoxContent(randomNum);
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        OpenEpicLootbox,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    private void GetEpicBoxContent(int randomNum)
    {
        RewardsToDisplay.Clear();
        ResetDrops();
        DropRandomConsumables(10);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GrantRewardedItems",
            FunctionParameter = new
            {
                localLUID = PlayerData.LUID,

                optibit = 0,
                normalFragment = 1500,
                rareFragment = 750,
                epicFragment = 500,
                legendFragment = 0,
                smallHeal = SmallHealSkillDropped,
                mediumHeal = MediumHealSkillDropped,
                largeHeal = LargeHealSkillDropped,
                breakRemove = BreakRemoveDropped,
                burnRemove = BurnRemoveDropped,
                confuseRemove = ConfuseRemoveDropped,
                freezeRemove = FreezeRemoveDropped,
                paralyzeRemove = ParalyzeRemoveDropped,
                weakRemove = WeakRemoveDropped
            },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            Debug.Log(resultCallback.FunctionResult);
            if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
            {
                PlayerData.EpicLootboxCount--;
                EpicLootboxOwned.text = PlayerData.EpicLootboxCount.ToString();
                DisplayCurrentLootbox();
                if (PlayerData.EpicLootboxCount > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                    PlayerData.EpicLootboxInstanceID = "";
                }
                LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                LobbyCore.OkBtn.SetActive(false);

                LobbyCore.GrantAmountTMP.text = "YOU GAINED 500 EPIC FRAGMENTS";
                FragmentSprite.sprite = EpicFragment;
                NormalFragmentsDropped = 1500;
                RareFragmentsDropped = 750;
                EpicFragmentsDropped = 500;
                UpdatePlayerInventory();

                CannonStrings.Clear();
                RewardsToDisplay.Insert(0, "EPIC");
                RewardsToDisplay.Insert(1, "RARE");
                RewardsToDisplay.Insert(2, "NORMAL");
                if (randomNum >= 70)
                    AddCommonCannon();

                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 40)
                    AddRareCannon();

                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 30)
                    AddEpicCannon();

                if (CannonStrings.Count > 0)
                    DropCannons(2);
                else
                {
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    EpicAnimatedBox.SetActive(true);
                    LobbyCore.HideLoadingPanel();

                    GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
                    EpicAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
                }
            }
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                () => GetEpicBoxContent(randomNum),
                () => ProcessError(errorCallback.ErrorMessage));
        });
    }
    #endregion

    #region LEGEND LOOTBOX 1
    private void OpenLegendaryLootbox1()
    {
        int randomNum = UnityEngine.Random.Range(0, 100);
        if (GameManager.Instance.DebugMode)
        {
            if (randomNum >= 80)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = NormalWeapons[UnityEngine.Random.Range(0, NormalWeapons.Count)];
                        break;
                    }
                }
            }
            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 50)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = RareWeapons[UnityEngine.Random.Range(0, RareWeapons.Count)];
                        break;
                    }
                }
            }
            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 40)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = EpicWeapons[UnityEngine.Random.Range(0, EpicWeapons.Count)];
                        break;
                    }
                }
            }
            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 30)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = LegendWeapons[UnityEngine.Random.Range(0, LegendWeapons.Count)];
                        break;
                    }
                }
            }
            PlayerData.LegendaryLootbox1Count--;
            LegendLootbox1Owned.text = PlayerData.LegendaryLootbox1Count.ToString();
            DisplayCurrentLootbox();
            LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
            LobbyCore.OkBtn.SetActive(false);
            LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
            LobbyCore.GrantPanel.SetActive(true);
            Legend1AnimatedBox.SetActive(true);
            GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
            Legend1AnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
        }
        else
        {
            LobbyCore.DisplayLoadingPanel();
            ConsumeItemRequest consumeItem = new ConsumeItemRequest();
            consumeItem.ItemInstanceId = PlayerData.LegendaryLootbox1InstanceID;
            consumeItem.ConsumeCount = 1;

            PlayFabClientAPI.ConsumeItem(consumeItem,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    GetLegendBox1Content(randomNum);
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        OpenLegendaryLootbox1,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    private void GetLegendBox1Content(int randomNum)
    {
        RewardsToDisplay.Clear();
        ResetDrops();
        DropRandomConsumables(20);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GrantRewardedItems",
            FunctionParameter = new
            {
                localLUID = PlayerData.LUID,

                optibit = 0,
                normalFragment = 3000,
                rareFragment = 1500,
                epicFragment = 1000,
                legendFragment = 500,
                smallHeal = SmallHealSkillDropped,
                mediumHeal = MediumHealSkillDropped,
                largeHeal = LargeHealSkillDropped,
                breakRemove = BreakRemoveDropped,
                burnRemove = BurnRemoveDropped,
                confuseRemove = ConfuseRemoveDropped,
                freezeRemove = FreezeRemoveDropped,
                paralyzeRemove = ParalyzeRemoveDropped,
                weakRemove = WeakRemoveDropped
            },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            Debug.Log(resultCallback.FunctionResult);
            if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
            {
                PlayerData.LegendaryLootbox1Count--;
                LegendLootbox1Owned.text = PlayerData.LegendaryLootbox1Count.ToString();
                DisplayCurrentLootbox();
                if (PlayerData.LegendaryLootbox1Count > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                    PlayerData.LegendaryLootbox1InstanceID = "";
                }
                LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                LobbyCore.OkBtn.SetActive(false);

                LobbyCore.GrantAmountTMP.text = "YOU GAINED 500 LEGEND FRAGMENTS";
                FragmentSprite.sprite = LegendFragment;
                NormalFragmentsDropped = 3000;
                RareFragmentsDropped = 1500;
                EpicFragmentsDropped = 1000;
                LegendFragmentsDropped = 500;
                UpdatePlayerInventory();
                RewardsToDisplay.Insert(0, "LEGEND");
                RewardsToDisplay.Insert(1, "EPIC");
                RewardsToDisplay.Insert(2, "RARE");
                RewardsToDisplay.Insert(3, "NORMAL");

                CannonStrings.Clear();
                if (randomNum >= 80)
                    AddCommonCannon();

                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 50)
                    AddRareCannon();

                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 40)
                    AddEpicCannon();

                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 30)
                    AddLegendCannon();

                if (CannonStrings.Count > 0)
                    DropCannons(3);
                else
                {
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    Legend1AnimatedBox.SetActive(true);
                    LobbyCore.HideLoadingPanel();

                    GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
                    Legend1AnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
                }
            }
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                () => GetLegendBox1Content(randomNum),
                () => ProcessError(errorCallback.ErrorMessage));
        });
    }
    #endregion

    #region LEGEND LOOTBOX 2
    private void OpenLegendaryLootbox2()
    {
        int randomNum = UnityEngine.Random.Range(0, 100);
        if (GameManager.Instance.DebugMode)
        {
            if (randomNum >= 90)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = NormalWeapons[UnityEngine.Random.Range(0, NormalWeapons.Count)];
                        break;
                    }
                }
            }
            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 60)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = RareWeapons[UnityEngine.Random.Range(0, RareWeapons.Count)];
                        break;
                    }
                }
            }
            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 50)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = EpicWeapons[UnityEngine.Random.Range(0, EpicWeapons.Count)];
                        break;
                    }
                }
            }
            randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum >= 40)
            {
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = LegendWeapons[UnityEngine.Random.Range(0, LegendWeapons.Count)];
                        break;
                    }
                }
            }

            PlayerData.LegendaryLootbox2Count--;
            LegendLootbox2Owned.text = PlayerData.LegendaryLootbox2Count.ToString();
            DisplayCurrentLootbox();
            LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
            LobbyCore.OkBtn.SetActive(false);
            LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
            LobbyCore.GrantPanel.SetActive(true);
            Legend2AnimatedBox.SetActive(true);
            GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
            Legend2AnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
        }
        else
        {
            LobbyCore.DisplayLoadingPanel();
            ConsumeItemRequest consumeItem = new ConsumeItemRequest();
            consumeItem.ItemInstanceId = PlayerData.LegendaryLootbox2InstanceID;
            consumeItem.ConsumeCount = 1;

            PlayFabClientAPI.ConsumeItem(consumeItem,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    GetLegendBox2Content(randomNum);
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        OpenLegendaryLootbox2,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    private void GetLegendBox2Content(int randomNum)
    {
        RewardsToDisplay.Clear();
        ResetDrops();
        DropRandomConsumables(50);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GrantRewardedItems",
            FunctionParameter = new
            {
                localLUID = PlayerData.LUID,

                optibit = 0,
                normalFragment = 5000,
                rareFragment = 2500,
                epicFragment = 1500,
                legendFragment = 1000,
                smallHeal = SmallHealSkillDropped,
                mediumHeal = MediumHealSkillDropped,
                largeHeal = LargeHealSkillDropped,
                breakRemove = BreakRemoveDropped,
                burnRemove = BurnRemoveDropped,
                confuseRemove = ConfuseRemoveDropped,
                freezeRemove = FreezeRemoveDropped,
                paralyzeRemove = ParalyzeRemoveDropped,
                weakRemove = WeakRemoveDropped
            },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            Debug.Log(resultCallback.FunctionResult);
            if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
            {
                PlayerData.LegendaryLootbox2Count--;
                LegendLootbox2Owned.text = PlayerData.LegendaryLootbox2Count.ToString();
                DisplayCurrentLootbox();
                if (PlayerData.LegendaryLootbox2Count > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                    PlayerData.LegendaryLootbox2InstanceID = "";
                }
                LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                LobbyCore.OkBtn.SetActive(false);

                LobbyCore.GrantAmountTMP.text = "YOU GAINED 1000 LEGEND FRAGMENTS";
                FragmentSprite.sprite = LegendFragment;
                NormalFragmentsDropped = 5000;
                RareFragmentsDropped = 2500;
                EpicFragmentsDropped = 1500;
                LegendFragmentsDropped = 1000;
                UpdatePlayerInventory();

                CannonStrings.Clear();
                RewardsToDisplay.Insert(0, "LEGEND");
                RewardsToDisplay.Insert(1, "EPIC");
                RewardsToDisplay.Insert(2, "RARE");
                RewardsToDisplay.Insert(3, "NORMAL");

                if (randomNum >= 90)
                    AddCommonCannon();

                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 60)
                    AddRareCannon();

                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 50)
                    AddEpicCannon();

                randomNum = UnityEngine.Random.Range(0, 100);
                if (randomNum >= 40)
                    AddLegendCannon();

                if (CannonStrings.Count > 0)
                    DropCannons(4);
                else
                {
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    Legend2AnimatedBox.SetActive(true);
                    LobbyCore.HideLoadingPanel();

                    GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
                    Legend2AnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
                }
            }
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                () => GetLegendBox2Content(randomNum),
                () => ProcessError(errorCallback.ErrorMessage));
        });
    }

    #endregion

    #region REWARD DISPLAY
    public void ProcessOkayPress()
    {
        RewardIndex++;
        if (RewardIndex != RewardsToDisplay.Count)
        {
            ConsumableReward.SetActive(false);
            FragmentReward.SetActive(false);
            CannonReward.SetActive(false);
            CostumeReward.SetActive(false);
            switch (RewardsToDisplay[RewardIndex])
            {
                #region FRAGMENTS
                case "NORMAL":
                    FragmentSprite.sprite = NormalFragment;
                    FragmentReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + NormalFragmentsDropped + "  NORMAL FRAGMENTS";
                    break;
                case "RARE":
                    FragmentSprite.sprite = RareFragment;
                    FragmentReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + RareFragmentsDropped + "  RARE FRAGMENTS";
                    break;
                case "EPIC":
                    FragmentSprite.sprite = EpicFragment;
                    FragmentReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + EpicFragmentsDropped + "  EPIC FRAGMENTS";
                    break;
                case "LEGEND":
                    FragmentSprite.sprite = LegendFragment;
                    FragmentReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + LegendFragmentsDropped + "  LEGEND FRAGMENTS";
                    break;
                #endregion
                #region CONSUMABLES
                case "SMALL":
                    ConsumableSprite.sprite = SmallHeal;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + SmallHealSkillDropped +  "  SMALL HEAL CHARGES";
                    break;
                case "MEDIUM":
                    ConsumableSprite.sprite = MediumHeal;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + MediumHealSkillDropped + "  MEDIUM HEAL CHARGES";
                    break;
                case "LARGE":
                    ConsumableSprite.sprite = LargeHeal;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + LargeHealSkillDropped + "  LARGE HEAL CHARGES";
                    break;
                case "BREAK":
                    ConsumableSprite.sprite = BreakRemoveSprite;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + BreakRemoveDropped + "  BREAK REMOVE ITEMS";
                    break;
                case "BURN":
                    ConsumableSprite.sprite = BurnRemoveSprite;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + BurnRemoveDropped + "  BURN REMOVE ITEMS";
                    break;
                case "CONFUSE":
                    ConsumableSprite.sprite = ConfuseRemoveSprite;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + ConfuseRemoveDropped + "  CONFUSE REMOVE ITEMS";
                    break;
                case "FREEZE":
                    ConsumableSprite.sprite = FreezeRemoveSprite;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + FreezeRemoveDropped + "  FREEZE REMOVE ITEMS";
                    break;
                case "PARALYZE":
                    ConsumableSprite.sprite = ParalyzeRemoveSprite;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + ParalyzeRemoveDropped + "  PARALYZE REMOVE ITEMS";
                    break;
                case "WEAK":
                    ConsumableSprite.sprite = WeakRemoveSprite;
                    ConsumableReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + WeakRemoveDropped + "  WEAK REMOVE ITEMS";
                    break;
                #endregion
                #region CANNONS
                case "C1":
                    CannonSprite.sprite = NormalWeapons[0].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + NormalWeapons[0].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "C2":
                    CannonSprite.sprite = NormalWeapons[1].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + NormalWeapons[1].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "C3":
                    CannonSprite.sprite = NormalWeapons[2].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + NormalWeapons[2].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "C4":
                    CannonSprite.sprite = NormalWeapons[3].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + NormalWeapons[3].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "C5":
                    CannonSprite.sprite = NormalWeapons[4].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + NormalWeapons[4].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "B1":
                    CannonSprite.sprite = RareWeapons[0].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + RareWeapons[0].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "B2":
                    CannonSprite.sprite = RareWeapons[1].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + RareWeapons[1].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "B3":
                    CannonSprite.sprite = RareWeapons[2].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + RareWeapons[2].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "B4":
                    CannonSprite.sprite = RareWeapons[3].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + RareWeapons[3].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "A1":
                    CannonSprite.sprite = EpicWeapons[0].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + EpicWeapons[0].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "A2":
                    CannonSprite.sprite = EpicWeapons[1].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + EpicWeapons[1].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "A3":
                    CannonSprite.sprite = EpicWeapons[2].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + EpicWeapons[2].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "S1":
                    CannonSprite.sprite = LegendWeapons[0].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + LegendWeapons[0].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                case "S2":
                    CannonSprite.sprite = LegendWeapons[1].AnimatedSprite;
                    CannonReward.SetActive(true);
                    LobbyCore.GrantAmountTMP.text = "YOU GAINED " + LegendWeapons[1].ThisWeaponCode.ToString() + "  CANNON";
                    break;
                #endregion
            }
        }
        else
        {
            LobbyController.LobbyStateToIndex(1);
        }
    }
    #endregion

    #region UTILITY
    private void DisplayCurrentLootbox()
    {
        CommonLootboxPanel.SetActive(false);
        RareLootboxPanel.SetActive(false);
        EpicLootboxPanel.SetActive(false);
        LegendLootbox1Panel.SetActive(false);
        LegendLootbox2Panel.SetActive(false);
        switch (LootboxIndex)
        {
            case 1:
                TopBoxImage.sprite = TopCommonBox;
                BotBoxImage.sprite = BotCommonBox;
                CommonLootboxPanel.SetActive(true);
                if (PlayerData.CommonLootboxCount > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                }
                break;
            case 2:
                TopBoxImage.sprite = TopRareBox;
                BotBoxImage.sprite = BotRareBox;
                RareLootboxPanel.SetActive(true);
                if (PlayerData.RareLootboxCount > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                }
                break;
            case 3:
                TopBoxImage.sprite = TopEpicBox;
                BotBoxImage.sprite = BotEpicBox;
                EpicLootboxPanel.SetActive(true);
                if (PlayerData.EpicLootboxCount > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                }
                break;
            case 4:
                TopBoxImage.sprite = TopLegend1Box;
                BotBoxImage.sprite = BotLegend1Box;
                LegendLootbox1Panel.SetActive(true);
                if (PlayerData.LegendaryLootbox1Count > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                }
                break;
            case 5:
                TopBoxImage.sprite = TopLegend2Box;
                BotBoxImage.sprite = BotLegend2Box;
                LegendLootbox2Panel.SetActive(true);
                if (PlayerData.LegendaryLootbox2Count > 0)
                {
                    HasThisLootbox = true;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayOpenSprite;
                }
                else
                {
                    HasThisLootbox = false;
                    OpenLootboxBtn.GetComponent<Image>().sprite = MayNotOpenSprite;
                }
                break;
        }
    }

    private void DropRandomConsumables(int _count)
    {
        for(int i = 0; i < _count; i++)
        {
            switch(UnityEngine.Random.Range(0,9))
            {
                case 0:
                    SmallHealSkillDropped++;
                    if (!RewardsToDisplay.Contains("SMALL"))
                        RewardsToDisplay.Add("SMALL");
                    break;
                case 1:
                    MediumHealSkillDropped++;
                    if (!RewardsToDisplay.Contains("MEDIUM"))
                        RewardsToDisplay.Add("MEDIUM");
                    break;
                case 2:
                    LargeHealSkillDropped++;
                    if (!RewardsToDisplay.Contains("LARGE"))
                        RewardsToDisplay.Add("LARGE");
                    break;
                case 3:
                    BreakRemoveDropped++;
                    if (!RewardsToDisplay.Contains("BREAK"))
                        RewardsToDisplay.Add("BREAK");
                    break;
                case 4:
                    BurnRemoveDropped++;
                    if (!RewardsToDisplay.Contains("BURN"))
                        RewardsToDisplay.Add("BURN");
                    break;
                case 5:
                    ConfuseRemoveDropped++;
                    if (!RewardsToDisplay.Contains("CONFUSE"))
                        RewardsToDisplay.Add("CONFUSE");
                    break;
                case 6:
                    FreezeRemoveDropped++;
                    if (!RewardsToDisplay.Contains("FREEZE"))
                        RewardsToDisplay.Add("FREEZE");
                    break;
                case 7:
                    ParalyzeRemoveDropped++;
                    if (!RewardsToDisplay.Contains("PARALYZE"))
                        RewardsToDisplay.Add("PARALYZE");
                    break;
                case 8:
                    WeakRemoveDropped++;
                    if (!RewardsToDisplay.Contains("WEAK"))
                        RewardsToDisplay.Add("WEAK");
                    break;
            }
        }
    }

    private void ResetDrops()
    {
        NormalFragmentsDropped = 0;
        RareFragmentsDropped = 0;
        EpicFragmentsDropped = 0;
        LegendFragmentsDropped = 0;
        SmallHealSkillDropped = 0;
        MediumHealSkillDropped = 0;
        LargeHealSkillDropped = 0;
        BurnRemoveDropped = 0;
        BreakRemoveDropped = 0;
        ConfuseRemoveDropped = 0;
        FreezeRemoveDropped = 0;
        ParalyzeRemoveDropped = 0;
        WeakRemoveDropped = 0;
    }    

    private void UpdatePlayerInventory()
    {
        PlayerData.NormalFragments += NormalFragmentsDropped;
        PlayerData.RareFragments += RareFragmentsDropped;
        PlayerData.EpicFragments += EpicFragmentsDropped;
        PlayerData.LegendFragments += LegendFragmentsDropped;
        PlayerData.SmallHealCharges += SmallHealSkillDropped;
        PlayerData.MediumHealCharges += MediumHealSkillDropped;
        PlayerData.LargeHealCharges += LargeHealSkillDropped;
        PlayerData.BreakRemovalCharges += BreakRemoveDropped;
        PlayerData.BurnRemovalCharges += BurnRemoveDropped;
        PlayerData.ConfuseRemovalCharges += ConfuseRemoveDropped;
        PlayerData.FreezeRemovalCharges += FreezeRemoveDropped;
        PlayerData.ParalyzeRemovalCharges += ParalyzeRemoveDropped;
        PlayerData.WeakRemovalCharges += WeakRemoveDropped;
    }

    private void DropCannons(int lootboxIndex)
    {
        int grantedCannons = 0;
        for(int i = 0; i < CannonStrings.Count; i++)
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = CannonStrings[i],
                GeneratePlayStreamEvent = true
            },
            resultCallback =>
            {
                grantedCannons++;
                if(grantedCannons == CannonStrings.Count)
                {
                    LobbyCore.HideLoadingPanel();
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);

                    GameManager.Instance.SFXAudioManager.PlayUpgradeSFX();
                    switch (lootboxIndex)
                    {
                        case 0:
                            CommonAnimatedBox.SetActive(true);
                            CommonAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                            break;
                        case 1:
                            RareAnimatedBox.SetActive(true);
                            RareAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                            break;
                        case 2:
                            EpicAnimatedBox.SetActive(true);
                            EpicAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                            break;
                        case 3:
                            Legend1AnimatedBox.SetActive(true);
                            Legend1AnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                            break;
                        case 4:
                            Legend2AnimatedBox.SetActive(true);
                            Legend2AnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                            break;
                    }

                    LobbyCore.InitializeLobby();
                }
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => DropCannons(lootboxIndex),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
        }
    }

    private void AddCommonCannon()
    {
        int cannonIndex = UnityEngine.Random.Range(0, NormalWeapons.Count);
        switch (cannonIndex)
        {
            case 0:
                CannonStrings.Add("GrantC1Cannon");
                RewardsToDisplay.Add("C1");
                break;
            case 1:
                CannonStrings.Add("GrantC2Cannon");
                RewardsToDisplay.Add("C2");
                break;
            case 2:
                CannonStrings.Add("GrantC3Cannon");
                RewardsToDisplay.Add("C3");
                break;
            case 3:
                CannonStrings.Add("GrantC4Cannon");
                RewardsToDisplay.Add("C4");
                break;
            case 4:
                CannonStrings.Add("GrantC5Cannon");
                RewardsToDisplay.Add("C5");
                break;
        }
    }  
    
    private void AddRareCannon()
    {
        int cannonIndex = UnityEngine.Random.Range(0, RareWeapons.Count);
        switch (cannonIndex)
        {
            case 0:
                CannonStrings.Add("GrantB1Cannon");
                RewardsToDisplay.Add("B1");
                break;
            case 1:
                CannonStrings.Add("GrantB2Cannon");
                RewardsToDisplay.Add("B2");
                break;
            case 2:
                CannonStrings.Add("GrantB3Cannon");
                RewardsToDisplay.Add("B3");
                break;
            case 3:
                CannonStrings.Add("GrantB4Cannon");
                RewardsToDisplay.Add("B4");
                break;
        }
    }

    private void AddEpicCannon()
    {
        int cannonIndex = UnityEngine.Random.Range(0, EpicWeapons.Count);
        switch (cannonIndex)
        {
            case 0:
                CannonStrings.Add("GrantA1Cannon");
                RewardsToDisplay.Add("A1");
                break;
            case 1:
                CannonStrings.Add("GrantA2Cannon");
                RewardsToDisplay.Add("A2");
                break;
            case 2:
                CannonStrings.Add("GrantA3Cannon");
                RewardsToDisplay.Add("A3");
                break;
        }
    }

    private void AddLegendCannon()
    {
        int cannonIndex = UnityEngine.Random.Range(0, LegendWeapons.Count);
        switch (cannonIndex)
        {
            case 0:
                CannonStrings.Add("GrantS1Cannon");
                RewardsToDisplay.Add("S1");

                break;
            case 1:
                CannonStrings.Add("GrantS2Cannon");
                RewardsToDisplay.Add("S2");
                break;
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
