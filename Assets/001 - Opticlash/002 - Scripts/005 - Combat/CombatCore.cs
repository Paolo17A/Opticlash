using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;

public class CombatCore : MonoBehaviour
{
    #region STATE MACHINE
    public enum CombatState
    {
        NONE,
        SPAWNING,
        TIMER,
        PLAYERTURN,
        ENEMYTURN,
        GAMEOVER,
        WALKING,
        WARPING,
        STAGECLEAR
    }

    private event EventHandler combatStateChange;
    public event EventHandler onCombatStateChange
    {
        add
        {
            if (combatStateChange == null || !combatStateChange.GetInvocationList().Contains(value))
            {
                combatStateChange += value;
            }
        }
        remove
        {
            combatStateChange -= value;
        }
    }

    public CombatState CurrentCombatState
    {
        get { return currentCombatState; }
        set
        {
            currentCombatState = value;
            combatStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion
    //=================================================================================
    [SerializeField][ReadOnly] private CombatState currentCombatState;
    [field: SerializeField] public PlayerData PlayerData { get; set; }
    [field: SerializeField] public Animator UIAnimator { get; set; }
    [field: SerializeField] private BoardCore BoardCore { get; set; }

    [Header("LOADING")]
    [SerializeField] private GameObject LoadingPanel;
    
    [field: Header("GAME VARIABLES")]
    [field: SerializeField][field: ReadOnly] public int QuestionTimerLeft { get; set; }
    [field: SerializeField] private TextMeshProUGUI TimerTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private float CurrentCountdownNumber { get; set; }
    [field: SerializeField] public TextMeshProUGUI AmmoTMP { get; set; }
    [field: SerializeField][field: ReadOnly] public int AmmoCount { get; set; }
    [field: SerializeField][field: ReadOnly] public int RoundCounter { get; set; }
    [field: SerializeField] public TextMeshProUGUI RoundTMP { get; set; }
    [field: SerializeField][field: ReadOnly] public int StageCounter { get; set; }
    [field: SerializeField] public TextMeshProUGUI StageTMP { get; set; }
    [field: SerializeField] [field: ReadOnly] public int MonstersKilled { get; set; }
    [field: SerializeField] public SpriteRenderer MissedSprite { get; set; }

    [field: Header("POWER UPS")]
    [field: SerializeField] public Button DoubleDamageBtn { get; set; }
    [field: SerializeField] public GameObject DoubleDamageImage { get; set; }
    [field: SerializeField] public TextMeshProUGUI DoubleDamageTMP { get; set; }
    [field: SerializeField] public Button ShieldBtn { get; set; }
    [field: SerializeField] public GameObject ShieldImage { get; set; }
    [field: SerializeField] public TextMeshProUGUI ShieldTMP { get; set; }
    [field: SerializeField] public Button LifestealBtn { get; set; }
    [field: SerializeField] public GameObject Portal { get; set; }
    [field: SerializeField] public Transform PortalEndPoint { get; set; }
    [field: SerializeField] public Animator FlashAnimator { get; set; }
    [field: SerializeField] public int MonstersSkipped { get; set; }

    [field: Header("SKILLS")]
    [field: SerializeField] public Button HealBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI HealChargesTMP { get; set; }
    [field: SerializeField] public Button MediumHealBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI MediumHealChargesTMP { get; set; }
    [field: SerializeField] public Button LargeHealBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI LargeHealChargesTMP { get; set; }
    [field: SerializeField] public Button BreakRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI BreakChargesTMP { get; set; }
    [field: SerializeField] public Button WeakRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI WeakChargesTMP { get; set; }
    [field: SerializeField] public Button FreezeRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI FreezeChargesTMP { get; set; }
    [field: SerializeField] public Button ParalyzeRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI ParalyzeChargesTMP { get; set; }
    [field: SerializeField] public Button ConfuseRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI ConfuseChargesTMP { get; set; }

    [field: Header("LOCAL PLAYER DATA")]
    [field: SerializeField] public CharacterCombatController SpawnedPlayer { get; set; }
    [field: SerializeField] public TextMeshPro PlayerTakenDamage { get; set; }
    [field: SerializeField] public Animator PlayerTakenDamageAnimator { get; set; }

    [field: Header("LOCAL ENEMY DATA")]
    [field: SerializeField] public GameObject MonsterParent { get; set; }
    [field: SerializeField][field: ReadOnly] public int EnemyIndex { get; set; }
    [field: SerializeField][field: ReadOnly] public EnemyCombatController CurrentEnemy { get; set; }
    [field: SerializeField] public TextMeshPro EnemyTakenDamage { get; set; }
    [field: SerializeField] public Animator EnemyTakenDamageAnimator { get; set; }
    [field: SerializeField] public GameObject EnemyProjectile { get; set; }
    [field: SerializeField] public Vector3 EnemyProjectileStartingPoint { get; set; }
    [field: SerializeField] public GameObject EnemyLaser { get; set; }
    [field: SerializeField] public Vector3 EnemyLaserStartingPoint { get; set; }

    [field: Header("GAME OVER")]
    [field: SerializeField] private int OptibitDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI OptibitDroppedTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int NormalFragmentDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int RareFragmentDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int EpicFragmentDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int LegendFragmentDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int SmallHealSkillDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int MediumHealSkillDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int LargeHealSkillDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int BreakRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int WeakRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int FreezeRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int ParalyzeRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int ConfuseRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int BurnRemoveDropped { get; set; }
    [field: SerializeField] public Image OptiCostume { get; set; }
    [field: SerializeField] public Image OptiCannon { get; set; }

    [field: Header("STAGE CLEAR")]
    [field: SerializeField] private List<RewardController> FragmentRewards { get; set; }
    [field: SerializeField] private List<RewardController> ConsumableRewards { get; set; }
    [field: SerializeField] private Sprite NormalFragmentSprite { get; set; }
    [field: SerializeField] private Sprite RareFragmentSprite { get; set; }
    [field: SerializeField] private Sprite EpicFragmentSprite { get; set; }
    [field: SerializeField] private Sprite LegendFragmentSprite { get; set; }
    [field: SerializeField] private Sprite SmallHealSprite { get; set; }
    [field: SerializeField] private Sprite MediumHealSprite { get; set; }
    [field: SerializeField] private Sprite LargeHealSprite { get; set; }
    [field: SerializeField] private Sprite BreakRemoveSprite { get; set; }
    [field: SerializeField] private Sprite BurnRemoveSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseRemoveSprite { get; set; }
    [field: SerializeField] private Sprite FreezeRemoveSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeRemoveSprite { get; set; }
    [field: SerializeField] private Sprite WeakRemoveSprite { get; set; }


    [field: Header("SETTINGS")]
    [field: SerializeField] private GameObject SettingsPanel { get; set; }


    [Header("DEBUGGER")]
    public Coroutine timerCoroutine;
    private int failedCallbackCounter;
    public bool isPlaying;
    //=================================================================================

    #region SPAWNING
    public void SpawnEnemies()
    {
        StageCounter = 0;
        StageTMP.text = "WAVE " + StageCounter.ToString();
        RoundCounter = 0;
        RoundTMP.text = "Round 1";
        EnemyIndex = 0;
    }

    public void SpawnNextEnemy()
    {
        EnemyIndex++;
        if(EnemyIndex == GameManager.Instance.CurrentLevelData.MonsterLevels.Count)
            CurrentCombatState = CombatState.STAGECLEAR;
        else
        {
            StageCounter++;
            StageTMP.text = StageCounter.ToString();
            foreach (Transform child in MonsterParent.transform)
            {
                if (GameManager.Instance.CurrentLevelData.MonsterList[EnemyIndex] == child.GetComponent<EnemyCombatController>().MonsterID)
                {
                    child.gameObject.SetActive(true);
                    CurrentEnemy = child.GetComponent<EnemyCombatController>();
                    CurrentEnemy.MonsterLevel = GameManager.Instance.CurrentLevelData.MonsterLevels[EnemyIndex];
                    MonsterParent.transform.position = new Vector3(MonsterParent.transform.position.x, CurrentEnemy.OriginalEnemyPosition.y, MonsterParent.transform.position.z);
                    break;
                }
            }
            SpawnedPlayer.ShotAccuracy = PlayerData.ActiveCustomWeapon.BaseWeaponData.Accuracy - CurrentEnemy.EvasionValue;
            CurrentEnemy.InitializeEnemy();
        }
    }
    #endregion

    #region TIMER
    public IEnumerator StartQuestionTimer()
    {
        CurrentCountdownNumber = 60f;
        QuestionTimerLeft = 60;
        TimerTMP.color = Color.white;
        TimerTMP.text = "Time Left: " + QuestionTimerLeft.ToString();
        while (QuestionTimerLeft > 0f && CurrentCombatState == CombatState.TIMER)
        {
            if(!GameManager.Instance.PanelActivated)
            {
                CurrentCountdownNumber -= Time.deltaTime;
                QuestionTimerLeft = (int)CurrentCountdownNumber;
                if (QuestionTimerLeft == 10)
                    TimerTMP.color = Color.red;
                TimerTMP.text = "Time Left: " + QuestionTimerLeft.ToString();
                if (GameManager.Instance.CheatsActivated && QuestionTimerLeft == 57)
                {
                    StopTimerCoroutine();
                    SpawnedPlayer.DamageDeal = 15;
                    BoardCore.ShotsEarned = 3;
                    CurrentCombatState = CombatState.PLAYERTURN;
                    SpawnedPlayer.CurrentCombatState = CharacterCombatController.CombatState.ATTACKING;
                }

                    
            }
            yield return null;
        }

        CurrentCombatState = CombatState.ENEMYTURN;
    }
    public void StopTimerCoroutine()
    {
        StopCoroutine(timerCoroutine);
    }
    #endregion

    #region WARPING
    public void SetCorrectStage()
    {
        StageCounter++;
        StageTMP.text = StageCounter.ToString();
        foreach (Transform child in MonsterParent.transform)
        {
            if (GameManager.Instance.CurrentLevelData.MonsterList[EnemyIndex] == child.GetComponent<EnemyCombatController>().MonsterID)
            {
                Debug.Log("Current monster should be" + child.name);
                child.gameObject.SetActive(true);
                CurrentEnemy = child.GetComponent<EnemyCombatController>();
                CurrentEnemy.MonsterLevel = GameManager.Instance.CurrentLevelData.MonsterLevels[EnemyIndex];
                MonsterParent.transform.position = new Vector3(MonsterParent.transform.position.x, CurrentEnemy.OriginalEnemyPosition.y, MonsterParent.transform.position.z);
                break;
            }
        }
        CurrentEnemy.InitializeEnemy();
    }
    #endregion

    #region UI
    public void ProcessPowerUpInteractability()
    {
        if (SpawnedPlayer.DoubleDamageTurnsCooldown == 0 && !SpawnedPlayer.DoubleDamageActivated)
            DoubleDamageBtn.interactable = true;
        else
            DoubleDamageBtn.interactable = false;

        if (SpawnedPlayer.ShieldInstancesRemaining > 0 && !SpawnedPlayer.ShieldsActivated)
            ShieldBtn.interactable = true;
        else
            ShieldBtn.interactable = false;

        if (SpawnedPlayer.LifestealInstancesRemaining > 0 && !SpawnedPlayer.LifestealActivated)
            LifestealBtn.interactable = true;
        else
            LifestealBtn.interactable = false;
    }

    public void ProcessSkillsInteractability()
    {
        if (PlayerData.SmallHealCharges > 0)
        {
            HealChargesTMP.text = PlayerData.SmallHealCharges.ToString();
            HealBtn.interactable = true;
        }
        else
        {
            HealChargesTMP.text = "0";
            HealBtn.interactable = false;
        }

        if (PlayerData.MediumHealCharges > 0)
        {
            MediumHealChargesTMP.text = PlayerData.MediumHealCharges.ToString();
            MediumHealBtn.interactable = true;
        }
        else
        {
            MediumHealChargesTMP.text = "0";
            MediumHealBtn.interactable = false;
        }

        if (PlayerData.LargeHealCharges > 0)
        {
            LargeHealChargesTMP.text = PlayerData.LargeHealCharges.ToString();
            LargeHealBtn.interactable = true;
        }
        else
        {
            LargeHealChargesTMP.text = "0";
            LargeHealBtn.interactable = false;
        }

        if (PlayerData.BreakRemovalCharges > 0)
        {
            BreakChargesTMP.text = PlayerData.BreakRemovalCharges.ToString();
            BreakRemoveBtn.interactable = true;
        }
        else
        {
            BreakChargesTMP.text = "0";
            BreakRemoveBtn.interactable = false;
        }

        if (PlayerData.WeakRemovalCharges > 0)
        {
            WeakChargesTMP.text = PlayerData.WeakRemovalCharges.ToString();
            WeakRemoveBtn.interactable = true;
        }
        else
        {
            WeakChargesTMP.text = "0";
            WeakRemoveBtn.interactable = false;
        }

        if (PlayerData.FreezeRemovalCharges > 0)
        {
            FreezeChargesTMP.text = PlayerData.FreezeRemovalCharges.ToString();
            FreezeRemoveBtn.interactable = true;
        }
        else
        {
            FreezeChargesTMP.text = "0";
            FreezeRemoveBtn.interactable = false;
        }

        if (PlayerData.ParalyzeRemovalCharges > 0)
        {
            ParalyzeChargesTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
            ParalyzeRemoveBtn.interactable = true;
        }
        else
        {
            ParalyzeChargesTMP.text = "0";
            ParalyzeRemoveBtn.interactable = false;
        }

        if (PlayerData.ConfuseRemovalCharges > 0)
        {
            ConfuseChargesTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
            ConfuseRemoveBtn.interactable = true;
        }
        else
        {
            ConfuseChargesTMP.text = "0";
            ConfuseRemoveBtn.interactable = false;
        }
    }

    public void ShowPowerUps()
    {
        if(CurrentCombatState == CombatState.TIMER)
            UIAnimator.SetBool("ShowingPowerUps", true);
    }

    public void ShowItems()
    {
        if (CurrentCombatState == CombatState.TIMER)
            UIAnimator.SetBool("ShowingPowerUps", false);
    }
    #endregion

    #region GAME OVER
    public void UpdateLevelsWon()
    {
        if (!GameManager.Instance.DebugMode)
        {
            PlayerData.LevelsWon++;
            OpenLoadingPanel();
            Dictionary<string, int> quests = new Dictionary<string, int>();
            quests.Add("DailyCheckIn", PlayerData.DailyCheckIn);
            quests.Add("SocMedShared", PlayerData.SocMedShared);
            quests.Add("ItemsUsed", PlayerData.ItemsUsed);
            quests.Add("MonstersKilled", PlayerData.MonstersKilled);
            quests.Add("LevelsWon", PlayerData.LevelsWon);
            quests.Add("DailyQuestClaimed", PlayerData.DailyQuestClaimed);

            UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
            updateUserData.Data = new Dictionary<string, string>();
            updateUserData.Data.Add("Quests", JsonConvert.SerializeObject(quests));

            PlayFabClientAPI.UpdateUserData(updateUserData,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    GetEnergy();
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        UpdateLevelsWon,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }    
    public void GetEnergy()
    {
        if(GameManager.Instance.DebugMode)
        {
            if (PlayerData.EnergyCount > 0)
                GrantRewardedItems();
            else
            {
                DisplayDroppedRewards();
                IncreaseCurrentLevel();
            }
        }
        else
        {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    PlayerData.EnergyCount = resultCallback.VirtualCurrency["EN"];
                    if (PlayerData.EnergyCount > 0)
                        GrantRewardedItems();
                    else
                    {
                        DisplayDroppedRewards();
                        IncreaseCurrentLevel();
                    }
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                            GetEnergy,
                            () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    public void GrantRewardedItems()
    {
        OptibitDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedOPB, GameManager.Instance.CurrentLevelData.MaxDroppedOPB);
        NormalFragmentDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedNormalFragment, GameManager.Instance.CurrentLevelData.MaxDroppedNormalFragment+1);
        RareFragmentDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedRareFragment, GameManager.Instance.CurrentLevelData.MaxDroppedRareFragment + 1);
        EpicFragmentDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedEpicFragment, GameManager.Instance.CurrentLevelData.MaxDroppedEpicFragment + 1);
        LegendFragmentDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedLegendFragment, GameManager.Instance.CurrentLevelData.MaxDroppedLegendFragment + 1);
        SmallHealSkillDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedSmallHeal, GameManager.Instance.CurrentLevelData.MaxDroppedSmallHeal + 1);
        MediumHealSkillDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedMediumHeal, GameManager.Instance.CurrentLevelData.MaxDroppedMediumHeal + 1);
        LargeHealSkillDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedLargeHeal, GameManager.Instance.CurrentLevelData.MaxDroppedLargeHeal + 1);
        BreakRemoveDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedBreakRemove, GameManager.Instance.CurrentLevelData.MaxDroppedBreakRemove + 1);
        BurnRemoveDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedBurnRemove, GameManager.Instance.CurrentLevelData.MaxDroppedBurnRemove + 1);
        ConfuseRemoveDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedConfuseRemove, GameManager.Instance.CurrentLevelData.MaxDroppedConfuseRemove + 1);
        FreezeRemoveDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedFreezeRemove, GameManager.Instance.CurrentLevelData.MaxDroppedFreezeRemove + 1);
        ParalyzeRemoveDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedParalyzeRemove, GameManager.Instance.CurrentLevelData.MaxDroppedParalyzeRemove + 1);
        WeakRemoveDropped = UnityEngine.Random.Range(GameManager.Instance.CurrentLevelData.MinDroppedWeakRemove, GameManager.Instance.CurrentLevelData.MaxDroppedWeakRemove + 1);

        if (GameManager.Instance.DebugMode)
        {
            DisplayDroppedRewards();
            IncreaseCurrentLevel();
            PurchaseEnergyCharge();
        }
        else
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "GrantRewardedItems",
                FunctionParameter = new
                {
                    localLUID = PlayerData.LUID,
                    optibit = OptibitDropped,
                    normalFragment = NormalFragmentDropped,
                    rareFragment = RareFragmentDropped,
                    epicFragment = EpicFragmentDropped,
                    legendFragment = LegendFragmentDropped,
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
                    DisplayDroppedRewards();
                    CloseLoadingPanel();
                    IncreaseCurrentLevel();
                    PurchaseEnergyCharge();
                }
                else
                {
                    CloseLoadingPanel();
                    GameManager.Instance.DisplayDualLoginErrorPanel();
                }
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GrantRewardedItems,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
        }
    }

    public void IncreaseCurrentLevel()
    {
        if(PlayerData.CurrentStage == GameManager.Instance.CurrentLevelData.LevelIndex)
        {
            if (GameManager.Instance.DebugMode)
                PlayerData.CurrentStage++;
            else
            {
                UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
                updateUserData.Data = new Dictionary<string, string>();
                updateUserData.Data.Add("CurrentStage", (PlayerData.CurrentStage + 1).ToString());

                PlayFabClientAPI.UpdateUserData(updateUserData,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.CurrentStage++;
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            IncreaseCurrentLevel,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }

    private void PurchaseEnergyCharge()
    {
        if (GameManager.Instance.DebugMode)
            PlayerData.EnergyCount--;
        else
        {
            PurchaseItemRequest purchaseItem = new PurchaseItemRequest();
            purchaseItem.CatalogVersion = "Charges";
            purchaseItem.ItemId = "EnergyCharge";
            purchaseItem.VirtualCurrency = "EN";
            purchaseItem.Price = 1;

            PlayFabClientAPI.PurchaseItem(purchaseItem,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    ConsumeEnergyCharge(resultCallback.Items[0].ItemInstanceId);
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        PurchaseEnergyCharge,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }

    }

    private void ConsumeEnergyCharge(string chargeID)
    {
        ConsumeItemRequest consumeItem = new ConsumeItemRequest();
        consumeItem.ItemInstanceId = chargeID;
        consumeItem.ConsumeCount = 1;

        PlayFabClientAPI.ConsumeItem(consumeItem,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.EnergyCount--;
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => ConsumeEnergyCharge(chargeID),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    
    #endregion

    #region UTILITY
    public void DisplaySettings()
    {
        SettingsPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
    }

    private void DisplayDroppedRewards()
    {
        if (OptibitDropped > 0)
        {
            OptibitDroppedTMP.gameObject.SetActive(true);
            OptibitDroppedTMP.text = OptibitDropped.ToString();
            PlayerData.Optibit += OptibitDropped;
        }
        else
            OptibitDroppedTMP.gameObject.SetActive(false);

        #region FRAGMENTS
        Queue<RewardController> fragmentRewards = new Queue<RewardController>();
        RewardController currentFragment;
        foreach (RewardController fragmentReward in FragmentRewards)
        {
            fragmentReward.HideReward();
            fragmentRewards.Enqueue(fragmentReward);
        }

        if (NormalFragmentDropped > 0)
        {
            currentFragment = fragmentRewards.Dequeue();
            currentFragment.ShowReward();
            currentFragment.RewardImage.sprite = NormalFragmentSprite;
            currentFragment.RewardTMP.text = NormalFragmentDropped.ToString();
            PlayerData.NormalFragments += NormalFragmentDropped;
        }

        if (RareFragmentDropped > 0)
        {
            currentFragment = fragmentRewards.Dequeue();
            currentFragment.ShowReward();
            currentFragment.RewardImage.sprite = RareFragmentSprite;
            currentFragment.RewardTMP.text = RareFragmentDropped.ToString();
            PlayerData.RareFragments += RareFragmentDropped;
        }

        if (EpicFragmentDropped > 0)
        {
            currentFragment = fragmentRewards.Dequeue();
            currentFragment.ShowReward();
            currentFragment.RewardImage.sprite = EpicFragmentSprite;
            currentFragment.RewardTMP.text = EpicFragmentDropped.ToString();
            PlayerData.EpicFragments += EpicFragmentDropped;
        }

        if (LegendFragmentDropped > 0)
        {
            currentFragment = fragmentRewards.Dequeue();
            currentFragment.ShowReward();
            currentFragment.RewardImage.sprite = LegendFragmentSprite;
            currentFragment.RewardTMP.text = LegendFragmentDropped.ToString();
            PlayerData.LegendFragments += LegendFragmentDropped;
        }
        #endregion

        #region CONSUMABLES
        Queue<RewardController> consumableRewards = new Queue<RewardController>();
        RewardController currentConsumable;
        foreach (RewardController consumableReward in ConsumableRewards)
        {
            consumableReward.HideReward();
            consumableRewards.Enqueue(consumableReward);
        }
        if (SmallHealSkillDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = SmallHealSprite;
            currentConsumable.RewardTMP.text = SmallHealSkillDropped.ToString();
            PlayerData.SmallHealCharges += SmallHealSkillDropped;
        }

        if (MediumHealSkillDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = MediumHealSprite;
            currentConsumable.RewardTMP.text = MediumHealSkillDropped.ToString();
            PlayerData.MediumHealCharges += MediumHealSkillDropped;
        }

        if (LargeHealSkillDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = LargeHealSprite;
            currentConsumable.RewardTMP.text = LargeHealSkillDropped.ToString();
            PlayerData.LargeHealCharges += LargeHealSkillDropped;
        }

        if (BreakRemoveDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = BreakRemoveSprite;
            currentConsumable.RewardTMP.text = BreakRemoveDropped.ToString();
            PlayerData.BreakRemovalCharges += BreakRemoveDropped;
        }

        if (WeakRemoveDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = WeakRemoveSprite;
            currentConsumable.RewardTMP.text = WeakRemoveSprite.ToString();
            PlayerData.WeakRemovalCharges += WeakRemoveDropped;
        }

        if (FreezeRemoveDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = FreezeRemoveSprite;
            currentConsumable.RewardTMP.text = FreezeRemoveDropped.ToString();
            PlayerData.FreezeRemovalCharges += FreezeRemoveDropped;
        }

        if (ParalyzeRemoveDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = ParalyzeRemoveSprite;
            currentConsumable.RewardTMP.text = ParalyzeRemoveDropped.ToString();
            PlayerData.ParalyzeRemovalCharges += ParalyzeRemoveDropped;
        }

        if (ConfuseRemoveDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = ConfuseRemoveSprite;
            currentConsumable.RewardTMP.text = ConfuseRemoveSprite.ToString();
            PlayerData.ConfuseRemovalCharges += ConfuseRemoveDropped;
        }

        if (BurnRemoveDropped > 0)
        {
            currentConsumable = consumableRewards.Dequeue();
            currentConsumable.ShowReward();
            currentConsumable.RewardImage.sprite = BurnRemoveSprite;
            currentConsumable.RewardTMP.text = BurnRemoveDropped.ToString();
            PlayerData.BurnRemovalCharges += BurnRemoveDropped;
        }
        #endregion

        CloseLoadingPanel();
    }

    public void HideSettings()
    {
        SettingsPanel.SetActive(false);
        GameManager.Instance.PanelActivated = false;
    }

    public void OpenLobbyScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
    }

    public void OpenAdventureScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "AdventureScene";
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
    public void OpenLoadingPanel()
    {
        LoadingPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
    }

    public void CloseLoadingPanel()
    {
        LoadingPanel.SetActive(false);
        GameManager.Instance.PanelActivated = false;
    }
    #endregion
}
