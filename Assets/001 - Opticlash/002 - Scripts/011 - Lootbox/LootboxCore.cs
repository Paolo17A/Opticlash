using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using System;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

public class LootboxCore : MonoBehaviour
{
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }
    [field: SerializeField] private GameObject GrantPanel { get; set; }

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

    [field: Header("REWARDS")]
    [field: SerializeField] public GameObject OptibitReward { get; set; }
    [field: SerializeField] public GameObject FragmentReward { get; set; }
    [field: SerializeField] private SpriteRenderer FragmentSprite { get; set; }
    [field: SerializeField] public GameObject CannonReward { get; set; }
    [field: SerializeField] private SpriteRenderer CannonSprite { get; set; }
    [field: SerializeField] public GameObject CostumeReward { get; set; }
    [field: SerializeField] private SpriteRenderer CostumeRewardSprite { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField] private int failedCallbackCounter { get; set; }

    private void Start()
    {

    }
    public void InitializeLootbox()
    {
        if (GameManager.Instance.DebugMode)
        {
            LootboxIndex = 1;
            PreviousLootboxBtn.interactable = false;
            NextLootboxBtn.interactable = true;
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
                    PreviousLootboxBtn.interactable = false;
                    NextLootboxBtn.interactable = true;
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
        LootboxIndex--;
        if (LootboxIndex == 1)
            PreviousLootboxBtn.interactable = false;
        else
            PreviousLootboxBtn.interactable = true;
        NextLootboxBtn.interactable = true;
        DisplayCurrentLootbox();
    }

    public void NextLootbox()
    {
        LootboxIndex++;
        PreviousLootboxBtn.interactable = true;
        if (LootboxIndex == 5)
            NextLootboxBtn.interactable = false;
        else
            NextLootboxBtn.interactable = true;
        DisplayCurrentLootbox();
    }

    public void OpenLootboxButton()
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

    #region COMMON LOOTBOX
    private void OpenCommonLootbox()
    {
        int randomNum = UnityEngine.Random.Range(0, 100);
        int randomAmount = 0;
        if (GameManager.Instance.DebugMode)
        {
            if (randomNum >= 0 && randomNum <= 39)
            {
                randomAmount = UnityEngine.Random.Range(50000, 100000);
                PlayerData.Optibit += randomAmount;
                CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 1;
                LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " OPTIBIT";
            }
            else if (randomNum >= 40 && randomNum <= 79)
            {
                randomAmount = UnityEngine.Random.Range(600, 1200);
                PlayerData.NormalFragments += randomAmount;
                CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 2;
                LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " NORMAL FRAGMENTS";
            }

            else if (randomNum >= 80 && randomNum <= 99)
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
                CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
            }

            PlayerData.CommonLootboxCount--;
            CommonLootboxOwned.text = PlayerData.CommonLootboxCount.ToString();
            DisplayCurrentLootbox();
            LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
            LobbyCore.OkBtn.SetActive(false);
            LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
            LobbyCore.GrantPanel.SetActive(true);
            CommonAnimatedBox.SetActive(true);
            CommonAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");

        }
        else
        {
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
        string cloudScriptCommand = "";
        int randomAmount = 0;
        if (randomNum >= 0 && randomNum <= 39)
        {
            cloudScriptCommand = "AddOptibit";
            randomAmount = UnityEngine.Random.Range(50000, 100000);
        }
        else if (randomNum >= 40 && randomNum <= 79)
        {
            cloudScriptCommand = "AddNormalFragment";
            randomAmount = UnityEngine.Random.Range(600, 1200);
        }
        else if (randomNum >= 80 && randomNum <= 99)
        {
            int cannonIndex = UnityEngine.Random.Range(0, NormalWeapons.Count);
            switch (cannonIndex)
            {
                case 0:
                    cloudScriptCommand = "GrantC1Cannon";
                    break;
                case 1:
                    cloudScriptCommand = "GrantC2Cannon";
                    break;
                case 2:
                    cloudScriptCommand = "GrantC3Cannon";
                    break;
                case 3:
                    cloudScriptCommand = "GrantC4Cannon";
                    break;
                case 4:
                    cloudScriptCommand = "GrantC5Cannon";
                    break;
            }
        }


        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = cloudScriptCommand,
            FunctionParameter = new { localLUID = PlayerData.LUID, amount = randomAmount },
            GeneratePlayStreamEvent = true
        },
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
                {
                    PlayerData.CommonLootboxCount--;
                    CommonLootboxOwned.text = PlayerData.CommonLootboxCount.ToString();
                    DisplayCurrentLootbox();
                    if (PlayerData.CommonLootboxCount == 0)
                    {
                        PlayerData.CommonLootboxInstanceID = "";
                        OpenLootboxBtn.interactable = false;
                    }
                    LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                    LobbyCore.OkBtn.SetActive(false);
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    CommonAnimatedBox.SetActive(true);
                    if (cloudScriptCommand == "AddOptibit")
                    {
                        CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 1;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " OPTIBIT";
                    }
                    else if (cloudScriptCommand == "AddNormalFragment")
                    {
                        FragmentSprite.sprite = NormalFragment;
                        CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 2;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " NORMAL FRAGMENTS";
                    }
                    else if (cloudScriptCommand == "GrantC1Cannon")
                    {
                        CannonSprite.sprite = NormalWeapons[0].AnimatedSprite;
                        CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED C1 CANNON";

                    }
                    else if (cloudScriptCommand == "GrantC2Cannon")
                    {
                        CannonSprite.sprite = NormalWeapons[1].AnimatedSprite;
                        CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED C2 CANNON";
                    }
                    else if (cloudScriptCommand == "GrantC3Cannon")
                    {
                        CannonSprite.sprite = NormalWeapons[2].AnimatedSprite;
                        CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED C3 CANNON";
                    }
                    else if (cloudScriptCommand == "GrantC4Cannon")
                    {
                        CannonSprite.sprite = NormalWeapons[3].AnimatedSprite;
                        CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED C4 CANNON";
                    }
                    else if (cloudScriptCommand == "GrantC5Cannon")
                    {
                        CannonSprite.sprite = NormalWeapons[4].AnimatedSprite;
                        CommonAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED C5 CANNON";
                    }
                    CommonAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
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
        int randomAmount = 0;
        if (GameManager.Instance.DebugMode)
        {
            if (randomNum >= 0 && randomNum <= 39)
            {
                randomAmount = UnityEngine.Random.Range(250000, 500000);
                PlayerData.Optibit += randomAmount;
                RareAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 1;
                LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " OPTIBIT";
            }
            else if (randomNum >= 40 && randomNum <= 79)
            {
                randomAmount = UnityEngine.Random.Range(700, 1300);
                PlayerData.RareFragments += randomAmount;
                RareAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 2;
                LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " RARE FRAGMENTS";
            }
            else if (randomNum >= 80 && randomNum <= 99)
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
            PlayerData.RareLootboxCount--;
            RareLootboxOwned.text = PlayerData.RareLootboxCount.ToString();
            DisplayCurrentLootbox();
            LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
            LobbyCore.OkBtn.SetActive(false);
            LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
            LobbyCore.GrantPanel.SetActive(true);
            RareAnimatedBox.SetActive(true);
            RareAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
        }
        else
        {
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
        string cloudScriptCommand = "";
        int randomAmount = 0;
        if (randomNum >= 0 && randomNum <= 39)
        {
            cloudScriptCommand = "AddOptibit";
            randomAmount = UnityEngine.Random.Range(250000, 500000);
        }
        else if (randomNum >= 40 && randomNum <= 79)
        {
            cloudScriptCommand = "AddRareFragment";
            randomAmount = UnityEngine.Random.Range(700, 1300);
        }
        else if (randomNum >= 80 && randomNum <= 99)
        {
            int cannonIndex = UnityEngine.Random.Range(0, RareWeapons.Count);
            switch (cannonIndex)
            {
                case 0:
                    cloudScriptCommand = "GrantB1Cannon";
                    break;
                case 1:
                    cloudScriptCommand = "GrantB2Cannon";
                    break;
                case 2:
                    cloudScriptCommand = "GrantB3Cannon";
                    break;
                case 3:
                    cloudScriptCommand = "GrantB4Cannon";
                    break;
            }
        }

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = cloudScriptCommand,
            FunctionParameter = new { localLUID = PlayerData.LUID, amount = randomAmount },
            GeneratePlayStreamEvent = true
        },
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
                {
                    PlayerData.RareLootboxCount--;
                    RareLootboxOwned.text = PlayerData.RareLootboxCount.ToString();
                    if (PlayerData.RareLootboxCount == 0)
                    {
                        PlayerData.RareLootboxInstanceID = "";
                        OpenLootboxBtn.interactable = false;
                    }
                    LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                    LobbyCore.OkBtn.SetActive(false);
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    RareAnimatedBox.SetActive(true);
                    if (cloudScriptCommand == "AddOptibit")
                    {
                        RareAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 1;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " OPTIBIT";
                    }
                    else if (cloudScriptCommand == "AddRareFragment")
                    {
                        FragmentSprite.sprite = RareFragment;
                        RareAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 2;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " RARE FRAGMENTS";
                    }
                    else if (cloudScriptCommand == "GrantB1Cannon")
                    {
                        CannonSprite.sprite = RareWeapons[0].AnimatedSprite;
                        RareAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED B1 CANNON";

                    }
                    else if (cloudScriptCommand == "GrantB2Cannon")
                    {
                        CannonSprite.sprite = RareWeapons[1].AnimatedSprite;
                        RareAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED B2 CANNON";
                    }
                    else if (cloudScriptCommand == "GrantB3Cannon")
                    {
                        CannonSprite.sprite = RareWeapons[2].AnimatedSprite;
                        RareAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED B3 CANNON";
                    }
                    else if (cloudScriptCommand == "GrantB4Cannon")
                    {
                        CannonSprite.sprite = RareWeapons[3].AnimatedSprite;
                        RareAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED B4 CANNON";
                    }
                    RareAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
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
            if (randomNum >= 0 && randomNum <= 39)
                PlayerData.Optibit += UnityEngine.Random.Range(500000, 1000000);
            else if (randomNum >= 40 && randomNum <= 79)
                PlayerData.EpicFragments += UnityEngine.Random.Range(800, 1400);
            else if (randomNum >= 80 && randomNum <= 99)
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
            PlayerData.EpicLootboxCount--;
            EpicLootboxOwned.text = PlayerData.EpicLootboxCount.ToString();
            DisplayCurrentLootbox();
        }
        else
        {
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
        string cloudScriptCommand = "";
        int randomAmount = 0;
        if (randomNum >= 0 && randomNum <= 39)
        {
            cloudScriptCommand = "AddOptibit";
            randomAmount = UnityEngine.Random.Range(500000, 1000000);
        }
        else if (randomNum >= 40 && randomNum <= 79)
        {
            cloudScriptCommand = "AddEpicFragment";
            randomAmount = UnityEngine.Random.Range(800, 1400);
        }
        else if (randomNum >= 80 && randomNum <= 99)
        {
            int cannonIndex = UnityEngine.Random.Range(0, EpicWeapons.Count);
            switch (cannonIndex)
            {
                case 0:
                    cloudScriptCommand = "GrantA1Cannon";
                    break;
                case 1:
                    cloudScriptCommand = "GrantA2Cannon";
                    break;
                case 2:
                    cloudScriptCommand = "GrantA3Cannon";
                    break;
            }
        }

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = cloudScriptCommand,
            FunctionParameter = new { localLUID = PlayerData.LUID, amount = randomAmount },
            GeneratePlayStreamEvent = true
        },
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
                {
                    PlayerData.EpicLootboxCount--;
                    EpicLootboxOwned.text = PlayerData.EpicLootboxCount.ToString();
                    if (PlayerData.EpicLootboxCount == 0)
                    {
                        PlayerData.EpicLootboxInstanceID = "";
                        OpenLootboxBtn.interactable = false;
                    }
                    LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                    LobbyCore.OkBtn.SetActive(false);
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    EpicAnimatedBox.SetActive(true);
                    if (cloudScriptCommand == "AddOptibit")
                    {
                        EpicAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 1;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " OPTIBIT";
                    }
                    else if (cloudScriptCommand == "AddEpicFragment")
                    {
                        FragmentSprite.sprite = EpicFragment;
                        EpicAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 2;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " EPIC FRAGMENTS";
                    }
                    else if (cloudScriptCommand == "GrantA1Cannon")
                    {
                        CannonSprite.sprite = EpicWeapons[0].AnimatedSprite;
                        EpicAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED A1 CANNON";

                    }
                    else if (cloudScriptCommand == "GrantA2Cannon")
                    {
                        CannonSprite.sprite = EpicWeapons[1].AnimatedSprite;
                        EpicAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED A2 CANNON";
                    }
                    else if (cloudScriptCommand == "GrantA3Cannon")
                    {
                        CannonSprite.sprite = EpicWeapons[2].AnimatedSprite;
                        EpicAnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED A3 CANNON";
                    }
                    EpicAnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
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
            if (randomNum >= 0 && randomNum <= 39)
                PlayerData.Optibit += UnityEngine.Random.Range(1000000, 2000000);
            else if (randomNum >= 40 && randomNum <= 79)
                PlayerData.LegendFragments += UnityEngine.Random.Range(900, 1500);
            else if (randomNum >= 80 && randomNum <= 89)
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = LegendWeapons[0];
                        Debug.Log("Your new weapon is " + PlayerData.OwnedWeapons[i].BaseWeaponData.name);
                        break;
                    }
                }
            else if (randomNum >= 90 && randomNum <= 99)
            {
                bool mayGrantCostume = false;
                foreach (CustomCostumeData costume in PlayerData.OwnedCostumes)
                    if (!costume.CostumeIsOwned)
                    {
                        mayGrantCostume = true;
                        break;
                    }

                if (mayGrantCostume)
                {
                    int randomCostumeIndex = UnityEngine.Random.Range(0, PlayerData.OwnedCostumes.Count);
                    while (PlayerData.OwnedCostumes[randomCostumeIndex].CostumeIsOwned)
                        randomCostumeIndex = UnityEngine.Random.Range(0, PlayerData.OwnedCostumes.Count);
                    PlayerData.OwnedCostumes[randomCostumeIndex].CostumeIsOwned = true;
                }
                else
                {
                    for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                    {
                        if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                        {
                            PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                            PlayerData.OwnedWeapons[i].BaseWeaponData = LegendWeapons[0];
                            Debug.Log("Your new weapon is " + PlayerData.OwnedWeapons[i].BaseWeaponData.name);
                            break;
                        }
                    }
                }
            }
            PlayerData.LegendaryLootbox1Count--;
            LegendLootbox1Owned.text = PlayerData.LegendaryLootbox1Count.ToString();
            DisplayCurrentLootbox();
        }
        else
        {
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
        string cloudScriptCommand = "";
        int randomAmount = 0;
        if (randomNum >= 0 && randomNum <= 39)
        {
            cloudScriptCommand = "AddOptibit";
            randomAmount = UnityEngine.Random.Range(1000000, 2000000);
        }
        else if (randomNum >= 40 && randomNum <= 79)
        {
            cloudScriptCommand = "AddLegendFragment";
            randomAmount = UnityEngine.Random.Range(900, 1500);
        }
        else if (randomNum >= 80 && randomNum <= 99)
            cloudScriptCommand = "GrantS1Cannon";

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = cloudScriptCommand,
            FunctionParameter = new { localLUID = PlayerData.LUID, amount = randomAmount },
            GeneratePlayStreamEvent = true
        },
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
                {
                    PlayerData.LegendaryLootbox1Count--;
                    LegendLootbox1Owned.text = PlayerData.LegendaryLootbox1Count.ToString();
                    if (PlayerData.LegendaryLootbox1Count == 0)
                    {
                        PlayerData.LegendaryLootbox1InstanceID = "";
                        OpenLootboxBtn.interactable = false;
                    }
                    LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                    LobbyCore.OkBtn.SetActive(false);
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    Legend1AnimatedBox.SetActive(true);
                    if (cloudScriptCommand == "AddOptibit")
                    {
                        Legend1AnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 1;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " OPTIBIT";
                    }
                    else if (cloudScriptCommand == "AddLegendFragment")
                    {
                        FragmentSprite.sprite = LegendFragment;
                        Legend1AnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 2;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " LEGEND FRAGMENTS";
                    }
                    else if (cloudScriptCommand == "GrantS1Cannon")
                    {
                        CannonSprite.sprite = LegendWeapons[0].AnimatedSprite;
                        Legend1AnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED S1 CANNON";

                    }
                    Legend1AnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
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
            if (randomNum >= 0 && randomNum <= 39)
                PlayerData.Optibit += UnityEngine.Random.Range(1000000, 2000000);
            else if (randomNum >= 40 && randomNum <= 79)
                PlayerData.LegendFragments += UnityEngine.Random.Range(900, 1500);
            else if (randomNum >= 80 && randomNum <= 89)
                for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                {
                    if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                    {
                        PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                        PlayerData.OwnedWeapons[i].BaseWeaponData = LegendWeapons[1];
                        Debug.Log("Your new weapon is " + PlayerData.OwnedWeapons[i].BaseWeaponData.name);
                        break;
                    }
                }
            else if (randomNum >= 90 && randomNum <= 99)
            {
                bool mayGrantCostume = false;
                foreach (CustomCostumeData costume in PlayerData.OwnedCostumes)
                    if (!costume.CostumeIsOwned)
                    {
                        mayGrantCostume = true;
                        break;
                    }

                if (mayGrantCostume)
                {
                    int randomCostumeIndex = UnityEngine.Random.Range(0, PlayerData.OwnedCostumes.Count);
                    while (PlayerData.OwnedCostumes[randomCostumeIndex].CostumeIsOwned)
                        randomCostumeIndex = UnityEngine.Random.Range(0, PlayerData.OwnedCostumes.Count);
                    PlayerData.OwnedCostumes[randomCostumeIndex].CostumeIsOwned = true;
                }
                else
                {
                    for (int i = 0; i < PlayerData.OwnedWeapons.Count; i++)
                    {
                        if (PlayerData.OwnedWeapons[i].BaseWeaponData == null)
                        {
                            PlayerData.OwnedWeapons[i].WeaponInstanceID = "newlyCraftedWeapon" + i;
                            PlayerData.OwnedWeapons[i].BaseWeaponData = LegendWeapons[1];
                            Debug.Log("Your new weapon is " + PlayerData.OwnedWeapons[i].BaseWeaponData.name);
                            break;
                        }
                    }
                }
            }
            PlayerData.LegendaryLootbox2Count--;
            LegendLootbox2Owned.text = PlayerData.LegendaryLootbox2Count.ToString();
            DisplayCurrentLootbox();
        }
        else
        {
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
        string cloudScriptCommand = "";
        int randomAmount = 0;
        if (randomNum >= 0 && randomNum <= 39)
        {
            cloudScriptCommand = "AddOptibit";
            randomAmount = UnityEngine.Random.Range(1000000, 2000000);
        }
        else if (randomNum >= 40 && randomNum <= 79)
        {
            cloudScriptCommand = "AddLegendFragment";
            randomAmount = UnityEngine.Random.Range(900, 1500);
        }
        else if (randomNum >= 80 && randomNum <= 99)
            cloudScriptCommand = "GrantS2Cannon";

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = cloudScriptCommand,
            FunctionParameter = new { localLUID = PlayerData.LUID, amount = randomAmount },
            GeneratePlayStreamEvent = true
        },
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (GameManager.Instance.DeserializeStringValue(resultCallback.FunctionResult.ToString(), "messageValue") == "Success")
                {
                    PlayerData.LegendaryLootbox1Count--;
                    LegendLootbox1Owned.text = PlayerData.LegendaryLootbox1Count.ToString();
                    if (PlayerData.LegendaryLootbox1Count == 0)
                    {
                        PlayerData.LegendaryLootbox1InstanceID = "";
                        OpenLootboxBtn.interactable = false;
                    }
                    LobbyCore.GrantAmountTMP.gameObject.SetActive(false);
                    LobbyCore.OkBtn.SetActive(false);
                    LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.NEWGRANT;
                    LobbyCore.GrantPanel.SetActive(true);
                    Legend2AnimatedBox.SetActive(true);
                    if (cloudScriptCommand == "AddOptibit")
                    {
                        Legend2AnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 1;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " OPTIBIT";
                    }
                    else if (cloudScriptCommand == "AddLegendFragment")
                    {
                        FragmentSprite.sprite = LegendFragment;
                        Legend2AnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 2;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED " + randomAmount + " LEGEND FRAGMENTS";
                    }
                    else if (cloudScriptCommand == "GrantS1Cannon")
                    {
                        CannonSprite.sprite = LegendWeapons[1].AnimatedSprite;
                        Legend2AnimatedBox.GetComponent<LootboxAnimationCore>().RewardIndex = 3;
                        LobbyCore.GrantAmountTMP.text = "YOU GAINED S2 CANNON";

                    }
                    Legend2AnimatedBox.GetComponent<LootboxAnimationCore>().LootboxAnimator.SetTrigger("open");
                    LobbyCore.InitializeLobby();
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
                    OpenLootboxBtn.interactable = true;
                else
                    OpenLootboxBtn.interactable = false;
                break;
            case 2:
                TopBoxImage.sprite = TopRareBox;
                BotBoxImage.sprite = BotRareBox;
                RareLootboxPanel.SetActive(true);
                if (PlayerData.RareLootboxCount > 0)
                    OpenLootboxBtn.interactable = true;
                else
                    OpenLootboxBtn.interactable = false;
                break;
            case 3:
                TopBoxImage.sprite = TopEpicBox;
                BotBoxImage.sprite = BotEpicBox;
                EpicLootboxPanel.SetActive(true);
                if (PlayerData.EpicLootboxCount > 0)
                    OpenLootboxBtn.interactable = true;
                else
                    OpenLootboxBtn.interactable = false;
                break;
            case 4:
                TopBoxImage.sprite = TopLegend1Box;
                BotBoxImage.sprite = BotLegend1Box;
                LegendLootbox1Panel.SetActive(true);
                if (PlayerData.LegendaryLootbox1Count > 0)
                    OpenLootboxBtn.interactable = true;
                else
                    OpenLootboxBtn.interactable = false;
                break;
            case 5:
                TopBoxImage.sprite = TopLegend2Box;
                BotBoxImage.sprite = BotLegend2Box;
                LegendLootbox2Panel.SetActive(true);
                if (PlayerData.LegendaryLootbox2Count > 0)
                    OpenLootboxBtn.interactable = true;
                else
                    OpenLootboxBtn.interactable = false;
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
