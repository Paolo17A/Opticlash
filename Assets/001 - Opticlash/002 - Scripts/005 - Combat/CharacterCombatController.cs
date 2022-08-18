using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

public class CharacterCombatController : MonoBehaviour
{
    #region STATE MACHINE
    public enum CombatState
    {
        NONE,
        IDLE,
        ATTACKING,
        ATTACKED,
        WALKING,
        DYING
    }

    private event EventHandler playerCombatStateChange;
    public event EventHandler onPlayerCombatStateChange
    {
        add
        {
            if (playerCombatStateChange == null || !playerCombatStateChange.GetInvocationList().Contains(value))
                playerCombatStateChange += value;
        }
        remove
        {
            playerCombatStateChange -= value;
        }
    }
    public CombatState CurrentCombatState
    {
        get { return currentCombatState; }
        set
        {
            currentCombatState = value;
            playerCombatStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion

    #region VARIABLES
    //===================================================================================
    [SerializeField][ReadOnly] private CombatState currentCombatState;
    [field: SerializeField] private PlayerData PlayerData;
    [field: SerializeField] private BoardCore BoardCore { get; set; }
    [field: SerializeField] private CombatCore CombatCore { get; set; }

    [field: Header("PLAYER DATA")]
    [field: SerializeField] private Animator PlayerAnimator { get; set; }
    [field: SerializeField] private float CurrentHealth { get; set; }
    [field: SerializeField] private float MaxHealth { get; set; }
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }

    [field: Header("OPTI STATS")]
    [field: SerializeField] [field: ReadOnly] public float Attack { get; set; }
    [field: SerializeField] [field: ReadOnly] public float ShotAccuracy { get; set; }
    [field: SerializeField] [field: ReadOnly] public float Evasion { get; set; }
    [field: SerializeField] [field: ReadOnly] public float Defense { get; set; }
    [field: SerializeField] [field: ReadOnly] public float DamageDeal { get; set; }

    [field: Header("COSTUME DATA")]
    [field: SerializeField] private GameObject Costume { get; set; }

    [field: Header("WEAPON DATA")]
    [field: SerializeField] private GameObject CannonBlast { get; set; }
    [field: SerializeField] public GameObject NormalKaboom { get; set; }
    [field: SerializeField] public GameObject BurnKaboom { get; set; }
    [field: SerializeField] public GameObject BreakKaboom { get; set; }
    [field: SerializeField] public GameObject ConfuseKaboom { get; set; }
    [field: SerializeField] public GameObject FreezeKaboom { get; set; }
    [field: SerializeField] public GameObject ParalyzeKaboom { get; set; }
    [field: SerializeField] public GameObject WeakKaboom { get; set; }
    [field: SerializeField] private SpriteRenderer CannonBackSprite { get; set; }
    [field: SerializeField] private SpriteRenderer CannonMiddleSprite { get; set; }
    [field: SerializeField] private SpriteRenderer CannonFrontSprite { get; set; }
    [field: SerializeField] private bool Lifestealing { get; set; }
    [field: SerializeField] private float LifestealHP { get; set; }

    [field: Header("POWER UP DATA")]
    [field: SerializeField] private GameObject DoubleDamageEffect {get;set;}
    [field: SerializeField] public bool DoubleDamageActivated { get; set; }
    [field: SerializeField] public int DoubleDamageTurnsCooldown { get; set; }
    [field: SerializeField] private GameObject ShieldEffect { get; set; }
    [field: SerializeField] public bool ShieldsActivated { get; set; }
    [field: SerializeField] public int ShieldInstancesRemaining { get; set; }
    [field: SerializeField] public int ShieldTurnsCooldown { get; set; }
    [field: SerializeField] public bool WarpActivated { get; set; }
    [field: SerializeField] public int WarpGunInstancesRemaining { get; set; }
    [field: SerializeField] public int LifestealInstancesRemaining { get; set; }
    [field: SerializeField] public bool LifestealActivated { get; set; }
    [field: SerializeField] public GameObject LifestealEffect { get; set; }

    [field: Header("SIDE EFFECTS")]
    [field: SerializeField] public SpriteRenderer StatusEffectImage { get; set; }
    [field: SerializeField] private SpriteRenderer StatusEffectText { get; set; }
    [field: SerializeField] public Animator StatusEffectTextAnimator { get; set; }
    [field: SerializeField][field: ReadOnly] public EnemyCombatController.SideEffect CurrentSideEffect { get; set; }
    [field: SerializeField][field: ReadOnly] public float SideEffectDamage { get; set; }
    [field: SerializeField][field: ReadOnly] public int SideEffectInstancesRemaining { get; set; }
    [field: SerializeField][field: ReadOnly] public bool StatusEffectActivated { get; set; }

    [field: Header("SIDE EFFECT ROSTER")]
    [field: SerializeField] private Sprite BreakLogoSprite { get; set; }
    [field: SerializeField] private Sprite BreakTextSprite { get; set; }
    [field: SerializeField] private Sprite BurnLogoSprite { get; set; }
    [field: SerializeField] private Sprite BurnTextSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseLogoSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseTextSprite { get; set; }
    [field: SerializeField] private Sprite FreezeLogoSprite { get; set; }
    [field: SerializeField] private Sprite FreezeTextSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeLogoSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeTextSprite { get; set; }
    [field: SerializeField] private Sprite WeakLogoSprite { get; set; }
    [field: SerializeField] private Sprite WeakTextSprite { get; set; }

    [field: Header("PROJECTILES")]
    [field: SerializeField] private GameObject Projectile { get; set; }
    [field: SerializeField] private Transform ProjectileStartingPoint { get; set; }
    [field: SerializeField] private Transform ProjectileEndPoint { get; set; }
    [field: SerializeField][field: ReadOnly] public bool ProjectileSpawned { get; set; }
    [field: SerializeField] private TrailRenderer ProjectileTrail { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool EffectNewlyRemoved { get; set; }
    [field: SerializeField][field: ReadOnly] public bool ProjectileCoroutineAllowed { get; set; }
    [field: SerializeField][field: ReadOnly] private int failedCallbackCounter { get; set; }
    [field: SerializeField][field: ReadOnly] private bool SkillButtonPressed { get; set; }

    //===================================================================================
    #endregion

    private void OnEnable()
    {
        playerCombatStateChange += CombatStateChange;
        Debug.Log("ccharacter combat controller enabled");
    }

    private void OnDisable()
    {
        playerCombatStateChange -= CombatStateChange;
    }

    private void Start()
    {
        ProjectileCoroutineAllowed = true;
        Debug.Log("character combat controller start");
    }

    private void Update()
    {
        if(CombatCore.CurrentCombatState == CombatCore.CombatState.WARPING)
        {
            if (Vector2.Distance(CombatCore.Portal.transform.position, CombatCore.PortalEndPoint.position) > 0.01f)
                CombatCore.Portal.transform.position = Vector2.MoveTowards(CombatCore.Portal.transform.position, CombatCore.PortalEndPoint.position, 11 * Time.deltaTime);
            else
            {
                CombatCore.Portal.SetActive(false);
                //CombatCore.WarpToNextEnemy();
            }
        }
        else if (ProjectileSpawned)
        {
            if (ProjectileCoroutineAllowed)
                StartCoroutine(Projectile.GetComponent<ProjectileController>().GoByTheRoute());
        }
    }

    public void ProcessHitOrMiss()
    {
        //  HIT
        if ((ShotAccuracy * 100) / CombatCore.CurrentEnemy.EvasionValue >= UnityEngine.Random.Range(0,100))
        {
            Projectile.SetActive(false);
            ProjectileSpawned = false;

            switch(CombatCore.CurrentEnemy.AfflictedSideEffect)
            {
                case WeaponData.SideEffect.NONE:
                    NormalKaboom.SetActive(true);
                    break;
                case WeaponData.SideEffect.BURN:
                    BurnKaboom.SetActive(true);
                    break;
                case WeaponData.SideEffect.BREAK:
                    BreakKaboom.SetActive(true);
                    break;
                case WeaponData.SideEffect.CONFUSE:
                    ConfuseKaboom.SetActive(true);
                    break;
                case WeaponData.SideEffect.FREEZE:
                    FreezeKaboom.SetActive(true);
                    break;
                case WeaponData.SideEffect.PARALYZE:
                    ParalyzeKaboom.SetActive(true);
                    break;
                case WeaponData.SideEffect.WEAK:
                    WeakKaboom.SetActive(true);
                    break;
            }
            StartCoroutine(DelayKaboom());
            if (DoubleDamageActivated)
            {
                if (CurrentSideEffect == EnemyCombatController.SideEffect.WEAK)
                {
                    if (WillCrit())
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(DamageDeal * PlayerData.ActiveCustomWeapon.BaseWeaponData.CritMultiplier);
                    else
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(DamageDeal);
                }
                else
                {
                    if (WillCrit())
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(DamageDeal* PlayerData.ActiveCustomWeapon.BaseWeaponData.CritMultiplier * 2);
                    else
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(DamageDeal * 2);
                }
                DoubleDamageActivated = false;
                DoubleDamageEffect.SetActive(false);
            }
            else
            {
                if (CurrentSideEffect == EnemyCombatController.SideEffect.WEAK)
                {
                    if (WillCrit())
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(DamageDeal * PlayerData.ActiveCustomWeapon.BaseWeaponData.CritMultiplier / 2);
                    else
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(DamageDeal / 2);
                }
                else
                {
                    if (WillCrit())
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(DamageDeal * PlayerData.ActiveCustomWeapon.BaseWeaponData.CritMultiplier);
                    else
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(DamageDeal);
                }
            }
            if (Lifestealing)
                InvokeLifesteal();
        }
        //MISS
        else
        {
            StartCoroutine(ShowMissedSprite());
            Projectile.SetActive(false);
            ProjectileSpawned = false;
            if (DoubleDamageActivated)
                DoubleDamageActivated = false;
            //ProcessDoubleDamage();
            ProcessEndAttack();
        }
    }

    public void InitializePlayer()
    {
        CombatCore.isPlaying = true;
        #region COSTUME
        if(PlayerData.ActiveCostume == null)
        {
            Costume.SetActive(false);
            CombatCore.OptiCostume.gameObject.SetActive(false);
        }
        else
        {
            Costume.SetActive(true);
            Costume.GetComponent<SpriteRenderer>().sprite = PlayerData.ActiveCostume.CostumeSprite;
            CombatCore.OptiCostume.sprite = PlayerData.ActiveCostume.LobbyCostumeSprite;
        }
        #endregion
        #region CANNON SPPRITES
        CannonBackSprite.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.BackSprite;
        CannonMiddleSprite.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.MiddleSprite;
        CannonFrontSprite.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.FrontSprite;
        CombatCore.OptiCannon.sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.AnimatedSprite;
        #endregion
        #region PROJECTILE SPRITES
        Projectile.GetComponent<SpriteRenderer>().sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.Ammo;
        //ProjectileTrail.colorGradient = new Gradient();
        /*if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C4 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.C5)
        {
            Projectile.transform.GetChild(0).gameObject.SetActive(true);
            MonstersToSkip = 5;
        }
        else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B3 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.B4)
        {
            Projectile.transform.GetChild(1).gameObject.SetActive(true);
            MonstersToSkip = 10;
        }
        else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A2 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.A3)
        {
            Projectile.transform.GetChild(2).gameObject.SetActive(true);
            MonstersToSkip = 15;
        }
        else if (PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S1 || PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode == WeaponData.WeaponCode.S2)
        {
            Projectile.transform.GetChild(3).gameObject.SetActive(true);
            MonstersToSkip = 20;
        }*/
        #endregion
        #region HEALTH SPRITES
        HealthBar.SetActive(true);
        HealthSlider.transform.localScale = new Vector3(1f, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(0f, 0f, 10f);
        #endregion
        RemoveSideEffects();
        ShieldInstancesRemaining = 5;
        LifestealInstancesRemaining = 1;
        #region STATS
        Attack = PlayerData.ActiveCustomWeapon.Attack;
        ShotAccuracy = PlayerData.ActiveCustomWeapon.Accuracy;
        if (PlayerData.ActiveConstumeInstanceID == "NONE")
        {
            Evasion = 10;
            Defense = 10;
        }
            
        else
        {
            Evasion = (5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10;
            Defense = (5 * (PlayerData.ActiveCostume.CostumeLevel * 0.5f)) + 10;
        }
        MaxHealth = 5 * ((Attack * 0.5f) + (Defense * 0.3f) + (Evasion * 0.1f) + (ShotAccuracy * 0.1f));
        CurrentHealth = MaxHealth;
        #endregion
        CombatCore.SetCorrectStage();
        CurrentCombatState = CombatState.WALKING;
        CombatCore.CurrentCombatState = CombatCore.CombatState.WALKING;

    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        //Debug.Log("Current Opti state: " + CurrentCombatState);
        PlayerAnimator.SetInteger("index", (int)CurrentCombatState);
    }

    private IEnumerator DelayKaboom()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        NormalKaboom.SetActive(false);
        BurnKaboom.SetActive(false);
        BreakKaboom.SetActive(false);
        ConfuseKaboom.SetActive(false);
        FreezeKaboom.SetActive(false);
        ParalyzeKaboom.SetActive(false);
        WeakKaboom.SetActive(false);
    }

    #region POWERUPS
    public void ActivateDoubleDamage()
    {
        if (!DoubleDamageActivated && DoubleDamageTurnsCooldown == 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            GameManager.Instance.SFXAudioManager.PlayDoubleDamageSFX();
            DoubleDamageEffect.SetActive(true);
            CombatCore.DoubleDamageImage.SetActive(true);
            DoubleDamageActivated = true;
            DoubleDamageTurnsCooldown = 5;
            CombatCore.DoubleDamageTMP.text = "Turns Left: " + DoubleDamageTurnsCooldown;
            //CombatCore.DoubleDamageBtn.interactable = false;
        }
        else
            Debug.Log("Double damage is on cooldown for " + DoubleDamageTurnsCooldown + " more turns");
    }

    public void ActivateShield()
    {
        if (!ShieldsActivated && ShieldInstancesRemaining > 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            GameManager.Instance.SFXAudioManager.PlayShieldSFX();
            ShieldEffect.SetActive(true);
            CombatCore.ShieldImage.SetActive(true);
            ShieldsActivated = true;
            ShieldTurnsCooldown = 5;
            CombatCore.ShieldTMP.text = "Turns Left: " + ShieldTurnsCooldown;
            //CombatCore.ShieldBtn.interactable = false;
        }
        else
            Debug.Log("Shield is already activated");
    }
    public void ActivateLifesteal()
    {
        if(!Lifestealing && LifestealInstancesRemaining > 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            Lifestealing = true;
            LifestealHP = DamageDeal / 2;
            LifestealEffect.SetActive(true);
        }
    }
    #endregion

    #region SKILLS
    public void UseHealSkill()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.SmallHealCharges > 0 && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.ItemsUsed++;
                PlayerData.SmallHealCharges--;
                CombatCore.HealChargesTMP.text = PlayerData.SmallHealCharges.ToString();
                if (PlayerData.SmallHealCharges == 0)
                    CombatCore.HealBtn.interactable = false;
                CurrentHealth += 10f;
                UpdateHealthBar();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
                SkillButtonPressed = false;
            }
        }
        else
        {
            if(PlayerData.SmallHealCharges > 0 && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.SmallHealInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.SmallHealCharges = resultCallback.RemainingUses;
                        CombatCore.HealChargesTMP.text = PlayerData.SmallHealCharges.ToString();
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.SmallHealInstanceID = "";
                            CombatCore.HealBtn.interactable = false;
                        }
                        CurrentHealth += 10f;
                        UpdateHealthBar();
                        UpdateQuestData();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseHealSkill,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }
    public void UseMediumHealSkill()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.MediumHealCharges > 0 && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.ItemsUsed++;
                PlayerData.MediumHealCharges--;
                CombatCore.MediumHealChargesTMP.text = PlayerData.MediumHealCharges.ToString();
                if (PlayerData.MediumHealCharges == 0)
                    CombatCore.MediumHealBtn.interactable = false;
                CurrentHealth += 15f;
                UpdateHealthBar();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
                SkillButtonPressed = false;
            }
        }
        else
        {
            if (PlayerData.MediumHealCharges > 0 && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.MediumHealInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.MediumHealCharges = resultCallback.RemainingUses;
                        CombatCore.MediumHealChargesTMP.text = PlayerData.MediumHealCharges.ToString();
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.MediumHealInstanceID = "";
                            CombatCore.MediumHealBtn.interactable = false;
                        }
                        CurrentHealth += 15f;
                        UpdateHealthBar();
                        UpdateQuestData();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseMediumHealSkill,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }
    public void UseLargeHealSkill()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.LargeHealCharges > 0 && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.ItemsUsed++;
                PlayerData.LargeHealCharges--;
                CombatCore.LargeHealChargesTMP.text = PlayerData.LargeHealCharges.ToString();
                if (PlayerData.LargeHealCharges == 0)
                    CombatCore.LargeHealBtn.interactable = false;
                CurrentHealth += 25f;
                UpdateHealthBar();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
                SkillButtonPressed = false;
            }
        }
        else
        {
            if (PlayerData.LargeHealCharges > 0 && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.LargeHealInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.LargeHealCharges = resultCallback.RemainingUses;
                        CombatCore.LargeHealChargesTMP.text = PlayerData.LargeHealCharges.ToString();
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.LargeHealInstanceID = "";
                            CombatCore.LargeHealBtn.interactable = false;
                        }
                        CurrentHealth += 25f;
                        UpdateHealthBar();
                        UpdateQuestData();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseLargeHealSkill,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }
    public void UseBreakRemove()
    {
        if(GameManager.Instance.DebugMode)
        {
            if(PlayerData.BreakRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.BREAK && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.ItemsUsed++;
                PlayerData.BreakRemovalCharges--;
                CombatCore.BreakChargesTMP.text = PlayerData.BreakRemovalCharges.ToString();
                if(PlayerData.BreakRemovalCharges == 0)
                    CombatCore.BreakRemoveBtn.interactable = false;
                RemoveSideEffects();
                UpdateQuestData();
            }
        }
        else
        {
            if (PlayerData.BreakRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.BREAK && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.BreakRemovalInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.BreakRemovalCharges = resultCallback.RemainingUses;
                        CombatCore.BreakChargesTMP.text = PlayerData.BreakRemovalCharges.ToString();
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.BreakRemovalInstanceID = "";
                            CombatCore.BreakRemoveBtn.interactable = false;
                        }
                        RemoveSideEffects();
                        UpdateQuestData();

                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseBreakRemove,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }
    public void UseWeakRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.WeakRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.WEAK && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.ItemsUsed++;
                PlayerData.WeakRemovalCharges--;
                CombatCore.WeakChargesTMP.text = PlayerData.WeakRemovalCharges.ToString();
                if (PlayerData.WeakRemovalCharges == 0)
                    CombatCore.WeakRemoveBtn.interactable = false;
                RemoveSideEffects();
                UpdateQuestData();
            }
        }
        else
        {
            if (PlayerData.WeakRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.WEAK && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.WeakRemovalInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.WeakRemovalCharges = resultCallback.RemainingUses;
                        CombatCore.WeakChargesTMP.text = PlayerData.WeakRemovalCharges.ToString();
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.WeakRemovalInstanceID = "";
                            CombatCore.WeakRemoveBtn.interactable = false;
                        }
                        RemoveSideEffects();
                        UpdateQuestData();

                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseWeakRemove,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }

    public void UseFreezeRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.FreezeRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.FREEZE && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.FreezeRemovalCharges--;
                CombatCore.FreezeChargesTMP.text = PlayerData.FreezeRemovalCharges.ToString();
                if (PlayerData.FreezeRemovalCharges == 0)
                    CombatCore.FreezeRemoveBtn.interactable = false;
                RemoveSideEffects();
                UpdateQuestData();
            }
        }
        else
        {
            if (PlayerData.FreezeRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.FREEZE && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.FreezeRemovalInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.FreezeRemovalCharges = resultCallback.RemainingUses;
                        CombatCore.FreezeChargesTMP.text = PlayerData.FreezeRemovalCharges.ToString();
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.FreezeRemovalInstanceID = "";
                            CombatCore.FreezeRemoveBtn.interactable = false;
                        }
                        RemoveSideEffects();
                        UpdateQuestData();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseFreezeRemove,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }

    public void UseParalyzeRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.ParalyzeRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.PARALYZE && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.ParalyzeRemovalCharges--;
                CombatCore.ParalyzeChargesTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
                if (PlayerData.ParalyzeRemovalCharges == 0)
                    CombatCore.ParalyzeRemoveBtn.interactable = false;
                RemoveSideEffects();
                UpdateQuestData();
            }
        }
        else
        {
            if (PlayerData.ParalyzeRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.PARALYZE && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.ParalyzeRemovalInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.ParalyzeRemovalCharges = resultCallback.RemainingUses;
                        CombatCore.ParalyzeChargesTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.ParalyzeRemovalInstanceID = "";
                            CombatCore.ParalyzeRemoveBtn.interactable = false;
                        }
                        RemoveSideEffects();
                        UpdateQuestData();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseParalyzeRemove,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }

    public void UseConfuseRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.ConfuseRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.CONFUSE && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.ConfuseRemovalCharges--;
                CombatCore.ConfuseChargesTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
                if (PlayerData.ConfuseRemovalCharges == 0)
                    CombatCore.ConfuseRemoveBtn.interactable = false;
                RemoveSideEffects();
                UpdateQuestData();
            }
        }
        else
        {
            if (PlayerData.ConfuseRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.CONFUSE && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.ConfuseRemovalInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.ConfuseRemovalCharges = resultCallback.RemainingUses;
                        CombatCore.ConfuseChargesTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.ConfuseRemovalInstanceID = "";
                            CombatCore.ConfuseRemoveBtn.interactable = false;
                        }
                        RemoveSideEffects();
                        UpdateQuestData();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseConfuseRemove,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }

    public void UseBurnRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.BurnRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.BURN && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                PlayerData.BurnRemovalCharges--;
                /*if (PlayerData.BurnRemovalCharges == 0)
                    CombatCore.BurnRemoveBtn.interactable = false;*/
                RemoveSideEffects();
                UpdateQuestData();
            }
        }
        else
        {
            if (PlayerData.BurnRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.BURN && !SkillButtonPressed)
            {
                SkillButtonPressed = true;
                ConsumeItemRequest consumeItem = new ConsumeItemRequest();
                consumeItem.ItemInstanceId = PlayerData.BurnRemovalInstanceID;
                consumeItem.ConsumeCount = 1;
                PlayFabClientAPI.ConsumeItem(consumeItem,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        PlayerData.BurnRemovalCharges = resultCallback.RemainingUses;
                        if (resultCallback.RemainingUses == 0)
                        {
                            PlayerData.BurnRemovalInstanceID = "";
                            //CombatCore.Burn.interactable = false;
                        }
                        RemoveSideEffects();
                        CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
                        SkillButtonPressed = false;
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            UseBurnRemove,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
        }
    }

    private void RemoveSideEffects()
    {
        SideEffectInstancesRemaining = 0;
        SideEffectDamage = 0;
        StatusEffectImage.gameObject.SetActive(false);
        StatusEffectText.color = new Color(255, 255, 255, 0);
        CurrentSideEffect = EnemyCombatController.SideEffect.NONE;
    }
    #endregion

    #region SIDE EFFECTS
    public void SetBreakEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = BreakLogoSprite;
        StatusEffectText.sprite = BreakTextSprite;
    }

    public void SetBurnEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = BurnLogoSprite;
        StatusEffectText.sprite = BurnTextSprite;
    }
    public void SetConfuseEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = ConfuseLogoSprite;
        StatusEffectText.sprite = ConfuseTextSprite;
    }
    public void SetFreezeEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = FreezeLogoSprite;
        StatusEffectText.sprite = FreezeTextSprite;
    }
    public void SetParalyzeEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = ParalyzeLogoSprite;
        StatusEffectText.sprite = ParalyzeTextSprite;
    }
    public void SetWeakEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = WeakLogoSprite;
        StatusEffectText.sprite = WeakTextSprite;
    }

    public void ActivateSideEffect()
    {

    }
    #endregion

    #region ANIMATION EVENTS
    private IEnumerator ShowMissedSprite()
    {
        CombatCore.MissedSprite.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.15f);
        CombatCore.MissedSprite.gameObject.SetActive(false);
    }
    public void ProcessHealth()
    {
        GameManager.Instance.SFXAudioManager.PlayHitSFX();
        if (CurrentHealth > 0)
        {
            if (CurrentSideEffect == EnemyCombatController.SideEffect.NONE)
            {
                if (CombatCore.CurrentEnemy.WillInflictStatusEffect())
                {
                    CurrentSideEffect = CombatCore.CurrentEnemy.ThisSideEffect;
                    SideEffectDamage = CombatCore.CurrentEnemy.SideEffectDamage;
                    SideEffectInstancesRemaining = CombatCore.CurrentEnemy.SideEffectDuration;

                    switch (CurrentSideEffect)
                    {
                        case EnemyCombatController.SideEffect.BREAK:
                            SetBreakEffect();
                            break;
                        case EnemyCombatController.SideEffect.BURN:
                            SetBurnEffect();
                            break;
                        case EnemyCombatController.SideEffect.CONFUSE:
                            SetConfuseEffect();
                            break;
                        case EnemyCombatController.SideEffect.FREEZE:
                            SetFreezeEffect();
                            break;
                        case EnemyCombatController.SideEffect.PARALYZE:
                            SetParalyzeEffect();
                            break;
                        case EnemyCombatController.SideEffect.WEAK:
                            SetWeakEffect();
                            break;
                    }
                }
            }
            else if (StatusEffectActivated)
            {
                Debug.Log("has status effect");
                
                if (CurrentSideEffect == EnemyCombatController.SideEffect.PARALYZE || CurrentSideEffect == EnemyCombatController.SideEffect.CONFUSE)
                    CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
                ProcessStatusEffectInstances();
            }
            else if (SideEffectInstancesRemaining > 0)
                ProcessStatusEffectInstances();

           CurrentCombatState = CombatState.IDLE;
        }
        else
        {
            CurrentHealth = 0;
            HealthBar.SetActive(false);
            HealthSlider.transform.localScale = new Vector3(CurrentHealth / MaxHealth, 1f, 0f);
            CurrentCombatState = CombatState.DYING;
        }
    }

    public void ProcessEndAttack()
    {
        Debug.Log("reducing shots earned");
        BoardCore.ShotsEarned--;
        if (BoardCore.ShotsEarned == 0)
        {
            ProcessDoubleDamage();
            if (CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.NONE)
                InflictStatusEffect();
            else
                CombatCore.CurrentEnemy.ProcessAfflictedSideEffect();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
        }
        else
            CurrentCombatState = CombatState.ATTACKING;
    }

    public void ProcessDeath()
    {
        GameManager.Instance.SFXAudioManager.PlayGameOverSFX();
        CombatCore.CurrentCombatState = CombatCore.CombatState.GAMEOVER;
    }

    public void AttackEnemy()
    {
        //BurnInstanceAccepted = false;
        if (CurrentSideEffect == EnemyCombatController.SideEffect.PARALYZE)
        {
            if (UnityEngine.Random.Range(0, 100) < 20)
            {
                StatusEffectActivated = true;
                StatusEffectTextAnimator.SetTrigger("ShowStatus");
            }
            else
                AttackSequence();
        }
        else if (CurrentSideEffect == EnemyCombatController.SideEffect.CONFUSE)
        {
            if (UnityEngine.Random.Range(0, 100) < 20)
            {
                StatusEffectActivated = true;
                StatusEffectTextAnimator.SetTrigger("ShowStatus");
                TakeDamageFromSelf();
            }
            else
                AttackSequence();
        }
        else if (CurrentSideEffect ==  EnemyCombatController.SideEffect.WEAK || CurrentSideEffect == EnemyCombatController.SideEffect.BREAK)
        {
            StatusEffectTextAnimator.SetTrigger("ShowStatus");
            AttackSequence();
        }
        else if (CurrentSideEffect == EnemyCombatController.SideEffect.FREEZE)
        {
            StatusEffectActivated = true;
            StatusEffectTextAnimator.SetTrigger("ShowStatus");
        }
        else
            AttackSequence();
    }

    public void TakeDamageFromEnemy(float _damageReceived)
    {
        if (_damageReceived > 0)
        {
            CurrentCombatState = CombatState.ATTACKED;
            if(ShieldsActivated)
            {
                // Do not mitigate the damages if you are pierced
                if(CurrentSideEffect == EnemyCombatController.SideEffect.PIERCE)
                    CurrentHealth -= _damageReceived;
                else
                    CurrentHealth -= _damageReceived / 3;
                ShieldEffect.SetActive(false);
                ShieldsActivated = false;
                ShieldInstancesRemaining--;
                ShieldTurnsCooldown--;
                CombatCore.ShieldTMP.text = "Turns Left: " + ShieldTurnsCooldown;
                if(ShieldTurnsCooldown == 0)
                    CombatCore.ShieldImage.SetActive(false);
            }
            else
                CurrentHealth -= _damageReceived;

            UpdateHealthBar();
        }
    }

    public void PlayWalkSFX()
    {
        if(CombatCore.isPlaying)
            GameManager.Instance.SFXAudioManager.PlayWalkSFX();
    }

    private void TakeDamageFromSelf()
    {
        if(!ShieldsActivated)
        {
            CurrentHealth -= PlayerData.ActiveCustomWeapon.BaseWeaponData.BaseDamage;
            UpdateHealthBar();
            CurrentCombatState = CombatState.ATTACKED;
        }
    }

    public void ShowCannonBlast()
    {
        GameManager.Instance.SFXAudioManager.PlayCannonSFX();
        CannonBlast.SetActive(true);
    }

    public void CharacterCombatStateToIndex(int state)
    {
        if(!StatusEffectActivated)
        {
            switch (state)
            {
                case (int)CombatState.IDLE:
                    CurrentCombatState = CombatState.IDLE;
                    break;
                case (int)CombatState.ATTACKING:
                    CurrentCombatState = CombatState.ATTACKING;
                    break;
                case (int)CombatState.ATTACKED:
                    CurrentCombatState = CombatState.ATTACKED;
                    break;
                case (int)CombatState.WALKING:
                    CurrentCombatState = CombatState.WALKING;
                    break;
                case (int)CombatState.DYING:
                    CurrentCombatState = CombatState.DYING;
                    break;
            }
        }
        else if (CurrentSideEffect == EnemyCombatController.SideEffect.PARALYZE || CurrentSideEffect == EnemyCombatController.SideEffect.CONFUSE)
            CurrentCombatState = CombatState.ATTACKED;
        else if (CurrentSideEffect == EnemyCombatController.SideEffect.FREEZE)
        {
            CurrentCombatState = CombatState.IDLE;
            ProcessStatusEffectInstances();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
        }

        if (CurrentSideEffect == EnemyCombatController.SideEffect.BURN)
        {
            //StatusEffectActivated = true;
            Debug.Log("intaking burn damage");
            CurrentHealth -= SideEffectDamage;
            UpdateHealthBar();
            StatusEffectTextAnimator.SetTrigger("ShowStatus");
            if (CurrentHealth <= 0)
                CurrentCombatState = CombatState.DYING;
        }
    }
    #endregion

    #region UTILITY
    private void AttackSequence()
    {
        CannonBlast.SetActive(false);
        Projectile.SetActive(true);
        Projectile.transform.position = ProjectileStartingPoint.position;
        ProjectileSpawned = true;
        CombatCore.AmmoCount--;
        CombatCore.AmmoTMP.text = "Ammo: " + CombatCore.AmmoCount.ToString();
    }

    private bool WillCrit()
    {
        if (!PlayerData.ActiveCustomWeapon.BaseWeaponData.HasCrit)
            return false;
        if (UnityEngine.Random.Range(0, 100) > PlayerData.ActiveCustomWeapon.BaseWeaponData.CritRate)
            return false;
        if (CombatCore.RoundCounter % PlayerData.ActiveCustomWeapon.BaseWeaponData.CritFrequency != 0)
            return false;
        if (CombatCore.RoundCounter < PlayerData.ActiveCustomWeapon.BaseWeaponData.CritFrequency)
            return false;

        return true;
    }

    public void InflictStatusEffect()
    {
        for (int i = 0; i < PlayerData.ActiveCustomWeapon.BaseWeaponData.SideEffects.Count; i++)
        {
            //  Only inflict a weapon side effect if Opti is not under Break side effect and if the current enemy currently does not have a status effect
            if (CurrentSideEffect != EnemyCombatController.SideEffect.BREAK && CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.NONE)
            {
                if (CombatCore.RoundCounter % PlayerData.ActiveCustomWeapon.BaseWeaponData.SideEffectsFrequency[i] == 0 && UnityEngine.Random.Range(0, 100) <= PlayerData.ActiveCustomWeapon.BaseWeaponData.SideEffectsRate[i])
                {
                    Debug.Log("Afflicting new side effect");
                    CombatCore.CurrentEnemy.AfflictedSideEffect = PlayerData.ActiveCustomWeapon.BaseWeaponData.SideEffects[i];
                    CombatCore.CurrentEnemy.AfflictedSideEffectInstancesLeft = PlayerData.ActiveCustomWeapon.BaseWeaponData.SideEffectsDuration[i];

                    switch (CombatCore.CurrentEnemy.AfflictedSideEffect)
                    {
                        case WeaponData.SideEffect.BREAK:
                            CombatCore.CurrentEnemy.SetBreakEffect();
                            break;
                        case WeaponData.SideEffect.BURN:
                            CombatCore.CurrentEnemy.SetBurnEffect();
                            break;
                        case WeaponData.SideEffect.CONFUSE:
                            CombatCore.CurrentEnemy.SetConfuseEffect();
                            break;
                        case WeaponData.SideEffect.FREEZE:
                            CombatCore.CurrentEnemy.SetFreezeEffect();
                            break;
                        case WeaponData.SideEffect.PARALYZE:
                            CombatCore.CurrentEnemy.SetParalyzeEffect();
                            break;
                        case WeaponData.SideEffect.WEAK:
                            CombatCore.CurrentEnemy.SetWeakEffect();
                            break;
                    }
                    break;
                }
            }
        }
    }

    private void ProcessDoubleDamage()
    {
        if (DoubleDamageTurnsCooldown > 0)
        {
            DoubleDamageTurnsCooldown--;
            CombatCore.DoubleDamageTMP.text = "Turns Left: " + DoubleDamageTurnsCooldown;
            if (DoubleDamageTurnsCooldown == 0)
                CombatCore.DoubleDamageImage.SetActive(false);
        }
    }

    public void ProcessStatusEffectInstances()
    {
        StatusEffectActivated = false;
        SideEffectInstancesRemaining--; 
        if (SideEffectInstancesRemaining == 0)
        {
            CurrentSideEffect = EnemyCombatController.SideEffect.NONE;
            StatusEffectImage.gameObject.SetActive(false);
            EffectNewlyRemoved = true;
        }
    }

    private void UpdateHealthBar()
    {
        HealthSlider.transform.localScale = new Vector3(CurrentHealth / MaxHealth, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(HealthSlider.transform.localScale.x - 1, HealthSlider.transform.localPosition.y, HealthSlider.transform.localPosition.z);
    }

    private void InvokeLifesteal()
    {
        Debug.Log("Opti will lifesteal");
        CurrentHealth += (DamageDeal / 2);
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        UpdateHealthBar();
        LifestealEffect.SetActive(false);
        Lifestealing = false;
        LifestealInstancesRemaining--;
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

    private void UpdateQuestData()
    {
        PlayerData.ItemsUsed++;
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
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
                SkillButtonPressed = false;
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    UpdateQuestData,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    #endregion
}