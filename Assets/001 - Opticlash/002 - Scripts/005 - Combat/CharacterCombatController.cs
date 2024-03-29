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
    [field: SerializeField] public GameObject EntireCannon { get; set; }
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
    [field: SerializeField] private Renderer CannonBackGlow { get; set; }
    [field: SerializeField] private Renderer CannonMiddleGlow { get; set; }
    [field: SerializeField] private Renderer CannonFrontGlow { get; set; }

    [field: Header("POWER UP DATA")]
    [field: SerializeField] private GameObject DoubleDamageEffect {get;set;}
    [field: SerializeField] public bool DoubleDamageActivated { get; set; }
    [field: SerializeField] public int DoubleDamageTurnsCooldown { get; set; }
    [field: SerializeField] private GameObject ShieldEffect { get; set; }
    [field: SerializeField] public bool ShieldsActivated { get; set; }
    [field: SerializeField] public int ShieldTurnsCooldown { get; set; }
    [field: SerializeField] public GameObject LifestealEffect { get; set; }
    [field: SerializeField] public int LifestealTurnsCooldown { get; set; }
    [field: SerializeField] public bool Lifestealing { get; set; }


    [field: Header("ITEM EFFECTS")]
    [field: SerializeField] private GameObject HealUsed { get; set; }
    [field: SerializeField] private GameObject BreakUsed { get; set; }
    [field: SerializeField] private GameObject BurnUsed { get; set; }
    [field: SerializeField] private GameObject ConfuseUsed { get; set; }
    [field: SerializeField] private GameObject FreezeUsed { get; set; }
    [field: SerializeField] private GameObject ParalyzeUsed { get; set; }
    [field: SerializeField] private GameObject WeakUsed { get; set; }


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
    [field: SerializeField][field: ReadOnly] public bool ProjectileSpawned { get; set; }

    [field: Header("PLAYFAB VARIABLES")]
    [field: SerializeField][field: ReadOnly] private ConsumeItemRequest consumeItem { get; set; }
    [field: SerializeField][field: ReadOnly] private UpdateUserDataRequest updateUserData { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool EffectNewlyRemoved { get; set; }
    [field: SerializeField][field: ReadOnly] public bool ProjectileCoroutineAllowed { get; set; }
    [field: SerializeField][field: ReadOnly] private int failedCallbackCounter { get; set; }
    [field: SerializeField][field: ReadOnly] public bool SkillButtonPressed { get; set; }
    private Dictionary<string, int> quests;

    //===================================================================================
    #endregion

    private void OnEnable()
    {
        playerCombatStateChange += CombatStateChange;

        consumeItem = new ConsumeItemRequest();
        updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
        quests = new Dictionary<string, int>();
    }

    private void OnDisable()
    {
        playerCombatStateChange -= CombatStateChange;
    }

    private void Start()
    {
        ProjectileCoroutineAllowed = true;
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

        int weaponIndex = (int)PlayerData.ActiveCustomWeapon.BaseWeaponData.ThisWeaponCode;
        if (weaponIndex >= 1 && weaponIndex <= 5)
            SetCannonGlowColor(Color.green, 12f);

        else if (weaponIndex >= 6 && weaponIndex <= 9)
            SetCannonGlowColor(Color.cyan, 12f);

        else if (weaponIndex >= 10 && weaponIndex <= 12)
            SetCannonGlowColor(Color.yellow, 12f);

        else if (weaponIndex >= 13 && weaponIndex <= 14)
            SetCannonGlowColor(Color.magenta, 12f);
        #endregion
        #region PROJECTILE SPRITES
        Projectile.GetComponent<SpriteRenderer>().sprite = PlayerData.ActiveCustomWeapon.BaseWeaponData.Ammo;
        #endregion
        #region HEALTH SPRITES
        HealthBar.SetActive(true);
        HealthSlider.transform.localScale = new Vector3(1f, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(0f, 0f, 0f);
        #endregion
        RemoveSideEffects();
        DoubleDamageTurnsCooldown = 0;
        ShieldTurnsCooldown = 0;
        LifestealTurnsCooldown = 0;
        DoubleDamageEffect.SetActive(false);
        ShieldEffect.SetActive(false);
        LifestealEffect.SetActive(false);
        DoubleDamageActivated = false;
        ShieldsActivated = false;
        Lifestealing = false;
        CombatCore.DoubleDamageImage.SetActive(false);
        CombatCore.ShieldImage.SetActive(false);
        CombatCore.LifestealImage.SetActive(false);
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

    private void SetCannonGlowColor(Color color, float intensity)
    {
        CannonBackGlow.material.SetColor("GlowColor", color * intensity);
        CannonMiddleGlow.material.SetColor("GlowColor", color * intensity);
        CannonFrontGlow.material.SetColor("GlowColor", color * intensity);
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
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

    private IEnumerator DelayUsedItemEffect()
    {
        yield return new WaitForSecondsRealtime(1f);
        HealUsed.SetActive(false);
        BurnUsed.SetActive(false);
        BreakUsed.SetActive(false);
        ConfuseUsed.SetActive(false);
        FreezeUsed.SetActive(false);
        ParalyzeUsed.SetActive(false);
        WeakUsed.SetActive(false);
    }

    #region POWERUPS
    public void ActivateDoubleDamage()
    {
        if (!DoubleDamageActivated && DoubleDamageTurnsCooldown == 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            GameManager.Instance.SFXAudioManager.PlayDoubleDamageSFX();
            DoubleDamageEffect.SetActive(true);
            CombatCore.DoubleDamageImage.SetActive(true);
            DoubleDamageActivated = true;
            DoubleDamageTurnsCooldown = 5;
            CombatCore.DoubleDamageTMP.text = "Turns Left: " + DoubleDamageTurnsCooldown;
            CombatCore.DisablePowerups();
        }
    }

    public void ActivateShield()
    {
        if (!ShieldsActivated && ShieldTurnsCooldown == 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            GameManager.Instance.SFXAudioManager.PlayShieldSFX();
            ShieldEffect.SetActive(true);
            CombatCore.ShieldImage.SetActive(true);
            ShieldsActivated = true;
            ShieldTurnsCooldown = 5;
            CombatCore.ShieldTMP.text = "Turns Left: " + ShieldTurnsCooldown;
            CombatCore.DisablePowerups();
        }
    }
    public void ActivateLifesteal()
    {
        if(!Lifestealing && LifestealTurnsCooldown == 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            Lifestealing = true;
            LifestealEffect.SetActive(true);
            CombatCore.LifestealImage.SetActive(true);
            LifestealTurnsCooldown = 5;
            CombatCore.LifestealTMP.text = "Turns Left: " + LifestealTurnsCooldown;
            CombatCore.DisablePowerups();
        }
    }
    #endregion

    #region ITEMS
    public void UseHealSkill()
    {
        if (PlayerData.SmallHealCharges > 0 && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            Debug.Log(CombatCore.CurrentEnemy.CurrentCombatState);
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.SmallHealInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            HealUsed.SetActive(true);
            SkillButtonPressed = true;

            PlayerData.SmallHealCharges--;
            CombatCore.HealChargesTMP.text = PlayerData.SmallHealCharges.ToString();
            if (PlayerData.SmallHealCharges == 0)
            {
                PlayerData.SmallHealInstanceID = "";
                CombatCore.HealBtn.interactable = false;
            }
            CurrentHealth += 10f;
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;

            StartCoroutine(DelayUsedItemEffect());
            UpdateHealthBar();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
        }
            
    }
    public void UseMediumHealSkill()
    {
        if (PlayerData.MediumHealCharges > 0 && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.MediumHealInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            HealUsed.SetActive(true);
            SkillButtonPressed = true;

            PlayerData.MediumHealCharges--;
            CombatCore.MediumHealChargesTMP.text = PlayerData.MediumHealCharges.ToString();
            if (PlayerData.MediumHealCharges == 0)
            {
                PlayerData.MediumHealInstanceID = "";
                CombatCore.MediumHealBtn.interactable = false;
            }
            CurrentHealth += 15f;
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;

            StartCoroutine(DelayUsedItemEffect());
            UpdateHealthBar();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
        }
    }
    public void UseLargeHealSkill()
    {
        if (PlayerData.LargeHealCharges > 0 && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.LargeHealInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            HealUsed.SetActive(true);
            SkillButtonPressed = true;

            PlayerData.LargeHealCharges--;
            CombatCore.LargeHealChargesTMP.text = PlayerData.LargeHealCharges.ToString();
            if (PlayerData.LargeHealCharges == 0)
            {
                PlayerData.LargeHealInstanceID = "";
                CombatCore.LargeHealBtn.interactable = false;
            }
            CurrentHealth += 25f;
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;

            StartCoroutine(DelayUsedItemEffect());
            UpdateHealthBar();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
        }
    }
    public void UseBreakRemove()
    {
        if (PlayerData.BreakRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.BREAK && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.BreakRemovalInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            BreakUsed.SetActive(true);
            SkillButtonPressed = true;
            PlayerData.BreakRemovalCharges--;
            CombatCore.BreakChargesTMP.text = PlayerData.BreakRemovalCharges.ToString();
            if (PlayerData.BreakRemovalCharges == 0)
            {
                PlayerData.BreakRemovalInstanceID = "";
                CombatCore.BreakRemoveBtn.interactable = false;
            }
            StartCoroutine(DelayUsedItemEffect());
            RemoveSideEffects();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
        }
    }
    public void UseWeakRemove()
    {
        if (PlayerData.WeakRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.WEAK && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.WeakRemovalInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            BreakUsed.SetActive(true);
            SkillButtonPressed = true;
            PlayerData.WeakRemovalCharges--;
            CombatCore.WeakChargesTMP.text = PlayerData.WeakRemovalCharges.ToString();
            if (PlayerData.WeakRemovalCharges == 0)
            {
                PlayerData.WeakRemovalInstanceID = "";
                CombatCore.WeakRemoveBtn.interactable = false;
            }
            StartCoroutine(DelayUsedItemEffect());
            RemoveSideEffects();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
        }
    }
    public void UseFreezeRemove()
    {
        if (PlayerData.FreezeRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.FREEZE && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.FreezeRemovalInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            FreezeUsed.SetActive(true);
            SkillButtonPressed = true;
            PlayerData.FreezeRemovalCharges--;
            CombatCore.FreezeChargesTMP.text = PlayerData.FreezeRemovalCharges.ToString();
            if (PlayerData.FreezeRemovalCharges == 0)
            {
                PlayerData.FreezeRemovalInstanceID = "";
                CombatCore.FreezeRemoveBtn.interactable = false;
            }
            StartCoroutine(DelayUsedItemEffect());
            RemoveSideEffects();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
        }
    }
    public void UseParalyzeRemove()
    {
        if (PlayerData.ParalyzeRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.PARALYZE && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.ParalyzeRemovalInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            ParalyzeUsed.SetActive(true);
            SkillButtonPressed = true;
            PlayerData.ParalyzeRemovalCharges--;
            CombatCore.ParalyzeChargesTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
            if (PlayerData.ParalyzeRemovalCharges == 0)
            {
                PlayerData.ParalyzeRemovalInstanceID = "";
                CombatCore.ParalyzeRemoveBtn.interactable = false;
            }
            StartCoroutine(DelayUsedItemEffect());
            RemoveSideEffects();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
        }
    }
    public void UseConfuseRemove()
    {
        if (PlayerData.ConfuseRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.CONFUSE && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.ConfuseRemovalInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            ConfuseUsed.SetActive(true);
            SkillButtonPressed = true;
            PlayerData.ConfuseRemovalCharges--;
            CombatCore.ConfuseChargesTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
            if (PlayerData.ConfuseRemovalCharges == 0)
            {
                PlayerData.ConfuseRemovalInstanceID = "";
                CombatCore.ConfuseRemoveBtn.interactable = false;
            }
            StartCoroutine(DelayUsedItemEffect());
            RemoveSideEffects();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
        }
    }
    public void UseBurnRemove()
    {
        if (PlayerData.BurnRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.BURN && !SkillButtonPressed && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER && CombatCore.CurrentEnemy.CurrentCombatState == EnemyCombatController.CombatState.IDLE)
        {
            PlayerData.ItemsUsed++;
            if (!GameManager.Instance.DebugMode)
                ConsumeItem(PlayerData.BurnRemovalInstanceID);

            CombatCore.WaitPanel.SetActive(true);
            CombatCore.DisableItems();
            BurnUsed.SetActive(true);
            SkillButtonPressed = true;
            PlayerData.BurnRemovalCharges--;
            CombatCore.BurnChargesTMP.text = PlayerData.BurnRemovalCharges.ToString();
            if (PlayerData.BurnRemovalCharges == 0)
            {
                PlayerData.BurnRemovalInstanceID = "";
                CombatCore.BurnRemoveBtn.interactable = false;
            }
            StartCoroutine(DelayUsedItemEffect());
            RemoveSideEffects();
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            SkillButtonPressed = false;
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
        BoardCore.ShotsEarned--;
        if (BoardCore.ShotsEarned == 0)
        {
            ProcessDoubleDamage();
            ProcessShield();
            ProcessLifesteal();
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
                _damageReceived = 0;
                CombatCore.PlayerTakenDamage.text = "-" + Mathf.CeilToInt(_damageReceived).ToString();
                CombatCore.PlayerTakenDamageAnimator.SetTrigger("ShowStatus");
                if (CurrentSideEffect == EnemyCombatController.SideEffect.PIERCE)
                    CurrentHealth -= _damageReceived;
                else
                    CurrentHealth -= _damageReceived / 3;
                ShieldEffect.SetActive(false);
                ShieldsActivated = false;
                ShieldTurnsCooldown--;
                CombatCore.ShieldTMP.text = "Turns Left: " + ShieldTurnsCooldown;
                if(ShieldTurnsCooldown == 0)
                    CombatCore.ShieldImage.SetActive(false);
            }
            else
            {
                CurrentHealth -= _damageReceived;
                CombatCore.PlayerTakenDamage.text = "-" + Mathf.CeilToInt(_damageReceived).ToString();
                CombatCore.PlayerTakenDamageAnimator.SetTrigger("ShowStatus");
            }

            if (CurrentHealth < 0)
                CurrentHealth = 0;
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

    public void ProcessDoubleDamage()
    {
        if (DoubleDamageTurnsCooldown > 0)
        {
            DoubleDamageTurnsCooldown--;
            CombatCore.DoubleDamageTMP.text = "Turns Left: " + DoubleDamageTurnsCooldown;
            if (DoubleDamageTurnsCooldown == 0)
                CombatCore.DoubleDamageImage.SetActive(false);
        }
    }

    public void ProcessShield()
    {
        if (ShieldTurnsCooldown > 0)
            ShieldTurnsCooldown--;
        CombatCore.ShieldTMP.text = "Turns Left: " + ShieldTurnsCooldown;
        if (ShieldTurnsCooldown == 0)
            CombatCore.ShieldImage.SetActive(false);
    }

    public void ProcessLifesteal()
    {
        if(LifestealTurnsCooldown > 0)
        {
            LifestealTurnsCooldown--;
            CombatCore.LifestealTMP.text = "Turns Left: " + LifestealTurnsCooldown;
            if (LifestealTurnsCooldown == 0)
                CombatCore.LifestealImage.SetActive(false);
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
        HealthBar.transform.localPosition = new Vector3(HealthBar.transform.localPosition.x, HealthBar.transform.localPosition.y, 0);
        HealthSlider.transform.localScale = new Vector3(CurrentHealth / MaxHealth, 1f, 1f);
        HealthSlider.transform.localPosition = new Vector3(HealthSlider.transform.localScale.x - 1, HealthSlider.transform.localPosition.y, HealthSlider.transform.localPosition.z);
    }

    private void InvokeLifesteal()
    {
        CurrentHealth += DamageDeal;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        UpdateHealthBar();
        LifestealEffect.SetActive(false);
        Lifestealing = false;
        LifestealTurnsCooldown--;
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
        CombatCore.CloseLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        CombatCore.CloseLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    public void ConsumeItem(string instanceID)
    {
        consumeItem.ItemInstanceId = instanceID;
        consumeItem.ConsumeCount = 1;

        PlayFabClientAPI.ConsumeItem(consumeItem,
            resultCallback =>
            {
                failedCallbackCounter = 0;
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => ConsumeItem(instanceID),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    #endregion
}