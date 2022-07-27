using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    //===================================================================================
    [SerializeField][ReadOnly] private CombatState currentCombatState;
    [field: SerializeField] private PlayerData PlayerData;
    [field: SerializeField] private BoardCore BoardCore { get; set; }
    [field: SerializeField] private CombatCore CombatCore { get; set; }

    [field: Header("PLAYER DATA")]
    [field: SerializeField] private Animator PlayerAnimator { get; set; }
    [field: SerializeField] private float CurrentHealth { get; set; }
    [field: SerializeField] private int MaxHealth;
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }

    [field: Header("COSTUME DATA")]
    [field: SerializeField] private GameObject Costume { get; set; }

    [field: Header("WEAPON DATA")]
    [field: SerializeField] private GameObject Cannon { get; set; }
    [field: SerializeField] private GameObject CannonBlast { get; set; }
    [field: SerializeField] public GameObject Kaboom { get; set; }
    [field: SerializeField] private SpriteRenderer CannonBackSprite { get; set; }
    [field: SerializeField] private SpriteRenderer CannonMiddleSprite { get; set; }
    [field: SerializeField] private SpriteRenderer CannonFrontSprite { get; set; }

    [field: Header("POWER UP DATA")]
    [field: SerializeField] public bool DoubleDamageActivated { get; set; }
    [field: SerializeField] public int DoubleDamageTurnsCooldown { get; set; }
    [field: SerializeField] public bool ShieldsActivated { get; set; }
    [field: SerializeField] public int ShieldInstancesRemaining { get; set; }
    [field: SerializeField] public bool WarpActivated { get; set; }
    [field: SerializeField] public int WarpGunInstancesRemaining { get; set; }
    [field: SerializeField] public int MonstersToSkip { get; set; }

    [field: Header("SIDE EFFECTS")]
    [field: SerializeField] public SpriteRenderer StatusEffectImage { get; set; }
    [field: SerializeField] private SpriteRenderer StatusEffectText { get; set; }
    [field: SerializeField] public Animator StatusEffectTextAnimator { get; set; }
    [field: SerializeField][field: ReadOnly] public EnemyCombatController.SideEffect CurrentSideEffect { get; set; }
    [field: SerializeField][field: ReadOnly] public float SideEffectDamage { get; set; }
    [field: SerializeField][field: ReadOnly] public int SideEffectInstancesRemaining { get; set; }
    [field: SerializeField][field: ReadOnly] private bool BurnInstanceAccepted { get; set; }
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

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public float ShotAccuracy { get; set; }
    [field: SerializeField][field: ReadOnly] public bool EffectNewlyRemoved { get; set; }
    [field: SerializeField][field: ReadOnly] public bool ProjectileCoroutineAllowed { get; set; }

    //===================================================================================
    private void OnEnable()
    {
        playerCombatStateChange += CombatStateChange;
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
                CombatCore.WarpToNextEnemy();
            }
        }
        else if (ProjectileSpawned)
        {
            //  Projectile is still traveling
            /*if (Vector2.Distance(Projectile.transform.position, ProjectileEndPoint.position) > 0.01f)
                Projectile.transform.position = Vector2.MoveTowards(Projectile.transform.position, ProjectileEndPoint.position, 13 * Time.deltaTime);*/
            if (ProjectileCoroutineAllowed)
                StartCoroutine(Projectile.GetComponent<ProjectileController>().GoByTheRoute());
            // Projectile has reached its destination
            /*else
            {
                ProcessHitOrMiss();
            }*/
        }
    }

    public void ProcessHitOrMiss()
    {
        //  HIT
        if (ShotAccuracy >= UnityEngine.Random.Range(0, 100))
        {
            Projectile.SetActive(false);
            ProjectileSpawned = false;
            Kaboom.SetActive(true);
            if (DoubleDamageActivated)
            {
                if (CurrentSideEffect == EnemyCombatController.SideEffect.WEAK)
                {
                    if (WillCrit())
                        CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage * PlayerData.ActiveWeapon.CritMultiplier);
                    else
                        CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage);
                }
                else
                {
                    if (WillCrit())
                        CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage * PlayerData.ActiveWeapon.CritMultiplier * 2);
                    else
                        CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage * 2);
                }
                DoubleDamageActivated = false;
            }
            else
            {
                if (CurrentSideEffect == EnemyCombatController.SideEffect.WEAK)
                {
                    if (WillCrit())
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage * PlayerData.ActiveWeapon.CritMultiplier / 2);
                    else
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage / 2);
                }
                else
                {
                    if (WillCrit())
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage * PlayerData.ActiveWeapon.CritMultiplier);
                    else
                        CombatCore.CurrentEnemy.TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage);
                }
            }

            #region SIDE EFFECT
            for (int i = 0; i < PlayerData.ActiveWeapon.SideEffects.Count; i++)
            {
                //  Only inflict a weapon side effect if Opti is not under Break side effect and if the current enemy currently does not have a status effect
                if (CurrentSideEffect != EnemyCombatController.SideEffect.BREAK && CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().AfflictedSideEffect == WeaponData.SideEffect.NONE)
                {
                    if (CombatCore.RoundCounter % PlayerData.ActiveWeapon.SideEffectsFrequency[i] == 0 && UnityEngine.Random.Range(0, 100) <= PlayerData.ActiveWeapon.SideEffectsRate[i])
                    {
                        CombatCore.CurrentEnemy.AfflictedSideEffect = PlayerData.ActiveWeapon.SideEffects[i];
                        CombatCore.CurrentEnemy.AfflictedSideEffectInstancesLeft = PlayerData.ActiveWeapon.SideEffectsDuration[i];
                    }
                }
            }
            #endregion

            ProcessDoubleDamage();
        }
        //MISS
        else
        {
            Debug.Log("YOU MISSED");
            Projectile.SetActive(false);
            ProjectileSpawned = false;
            if (DoubleDamageActivated)
                DoubleDamageActivated = false;
            ProcessDoubleDamage();
            ProcessEndAttack();
            //CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
        }
    }

    public void InitializePlayer()
    {
        #region COSTUME
        if(PlayerData.ActiveCostume == null)
            Costume.SetActive(false);
        else
        {
            Costume.SetActive(true);
            Costume.GetComponent<SpriteRenderer>().sprite = PlayerData.ActiveCostume.CostumeSprite;
        }
        #endregion
        #region CANNON SPPRITES
        CannonBackSprite.sprite = PlayerData.ActiveWeapon.BackSprite;
        CannonMiddleSprite.sprite = PlayerData.ActiveWeapon.MiddleSprite;
        CannonFrontSprite.sprite = PlayerData.ActiveWeapon.FrontSprite;
        #endregion
        #region PROJECTILE SPRITES
        Projectile.GetComponent<SpriteRenderer>().sprite = PlayerData.ActiveWeapon.Ammo;
        if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C3 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C4 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C5)
        {
            Projectile.transform.GetChild(0).gameObject.SetActive(true);
            MonstersToSkip = 5;
        }
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B3 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B4)
        {
            Projectile.transform.GetChild(1).gameObject.SetActive(true);
            MonstersToSkip = 10;
        }
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A3)
        {
            Projectile.transform.GetChild(2).gameObject.SetActive(true);
            MonstersToSkip = 15;
        }
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.S1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.S2)
        {
            Projectile.transform.GetChild(3).gameObject.SetActive(true);
            MonstersToSkip = 20;
        }
        #endregion
        #region HEALTH SPRITES
        HealthBar.SetActive(true);
        HealthSlider.transform.localScale = new Vector3(1f, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(0f, 0f, 10f);
        #endregion
        RemoveSideEffects();
        ShieldInstancesRemaining = 5;
        MaxHealth = PlayerData.MaxHealth;
        CurrentHealth = MaxHealth;
        CombatCore.SetCorrectStage();
        CurrentCombatState = CombatState.WALKING;
        CombatCore.CurrentCombatState = CombatCore.CombatState.WALKING;

    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        Debug.Log("Current Opti state: " + CurrentCombatState);
        PlayerAnimator.SetInteger("index", (int)CurrentCombatState);
        /*if (CurrentCombatState == CombatState.IDLE || CurrentCombatState == CombatState.WALKING)
            Cannon.SetActive(false);
            
        if (CurrentCombatState == CombatState.ATTACKING)
            Cannon.SetActive(true);*/
    }

    #region POWERUPS
    public void ActivateDoubleDamage()
    {
        if (!DoubleDamageActivated && DoubleDamageTurnsCooldown == 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            DoubleDamageActivated = true;
            DoubleDamageTurnsCooldown = 3;
            CombatCore.DoubleDamageBtn.interactable = false;
        }
        else
            Debug.Log("Double damage is on cooldown for " + DoubleDamageTurnsCooldown + " more turns");
    }

    public void ActivateShield()
    {
        if (!ShieldsActivated && ShieldInstancesRemaining > 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            ShieldsActivated = true;
            CombatCore.ShieldBtn.interactable = false;
        }
        else
            Debug.Log("Shield is already activated");
    }

    public void ActivateWarpGun()
    {
        if(!WarpActivated && WarpGunInstancesRemaining > 0 && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            Debug.Log("Warp has been activated");
            CombatCore.Portal.SetActive(true);
            WarpGunInstancesRemaining--;
            WarpActivated = true;
            CombatCore.WarpBtn.interactable = false;
            CombatCore.CurrentCombatState = CombatCore.CombatState.WARPING;
        }
    }
    #endregion

    #region SKILLS
    public void UseHealSkill()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.HealCharges > 0)
            {
                PlayerData.HealCharges--;
                CombatCore.HealChargesTMP.text = PlayerData.HealCharges.ToString();
                if (PlayerData.HealCharges == 0)
                    CombatCore.HealBtn.interactable = false;
                CurrentHealth += 10f;
                HealthSlider.transform.localScale = new Vector3(CurrentHealth / MaxHealth, 1f, 0f);
                HealthSlider.transform.localPosition = new Vector3(HealthSlider.transform.localScale.x - 1, HealthSlider.transform.localPosition.y, HealthSlider.transform.localPosition.z);
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            }
        }
    }

    public void UseBreakRemove()
    {
        if(GameManager.Instance.DebugMode)
        {
            if(PlayerData.BreakRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.BREAK)
            {
                PlayerData.BreakRemovalCharges--;
                CombatCore.BreakChargesTMP.text = PlayerData.BreakRemovalCharges.ToString();
                if(PlayerData.BreakRemovalCharges == 0)
                    CombatCore.BreakRemoveBtn.interactable = false;
                RemoveSideEffects();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            }
        }
    }
    public void UseWeakRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.WeakRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.WEAK)
            {
                PlayerData.WeakRemovalCharges--;
                CombatCore.WeakChargesTMP.text = PlayerData.WeakRemovalCharges.ToString();
                if (PlayerData.WeakRemovalCharges == 0)
                    CombatCore.WeakRemoveBtn.interactable = false;
                RemoveSideEffects();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            }
        }
    }

    public void UseFreezeRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.FreezeRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.FREEZE)
            {
                PlayerData.FreezeRemovalCharges--;
                CombatCore.FreezeChargesTMP.text = PlayerData.FreezeRemovalCharges.ToString();
                if (PlayerData.FreezeRemovalCharges == 0)
                    CombatCore.FreezeRemoveBtn.interactable = false;
                RemoveSideEffects();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            }
        }
    }

    public void UseParalyzeRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.ParalyzeRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.PARALYZE)
            {
                PlayerData.ParalyzeRemovalCharges--;
                CombatCore.ParalyzeChargesTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
                if (PlayerData.ParalyzeRemovalCharges == 0)
                    CombatCore.ParalyzeRemoveBtn.interactable = false;
                RemoveSideEffects();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            }
        }
    }

    public void UseConfuseRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.ConfuseRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.CONFUSE)
            {
                PlayerData.ConfuseRemovalCharges--;
                CombatCore.ConfuseChargesTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
                if (PlayerData.ConfuseRemovalCharges == 0)
                    CombatCore.ConfuseRemoveBtn.interactable = false;
                RemoveSideEffects();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            }
        }
    }

    public void UseBurnRemove()
    {
        if (GameManager.Instance.DebugMode)
        {
            if (PlayerData.BurnRemovalCharges > 0 && CurrentSideEffect == EnemyCombatController.SideEffect.BURN)
            {
                PlayerData.BurnRemovalCharges--;
                /*if (PlayerData.BurnRemovalCharges == 0)
                    CombatCore.BurnRemoveBtn.interactable = false;*/
                RemoveSideEffects();
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
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
    public void ProcessHealth()
    {
        if (CurrentHealth > 0)
        {
            if (CurrentSideEffect == EnemyCombatController.SideEffect.NONE)
            {
                Debug.Log("had no status effect yet");
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
        BoardCore.ShotsEarned--;
        if (BoardCore.ShotsEarned == 0)
            CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
        else
            CurrentCombatState = CombatState.ATTACKING;
    }

    public void ProcessDeath()
    {
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
            Debug.Log("Frozen");
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
                ShieldsActivated = false;
                ShieldInstancesRemaining--;
                if(ShieldInstancesRemaining > 0)
                    CombatCore.ShieldBtn.interactable = true;
                else
                    CombatCore.ShieldBtn.interactable = false;
            }
            else
                CurrentHealth -= _damageReceived;

            UpdateHealthBar();
        }
    }

    private void TakeDamageFromSelf()
    {
        if(!ShieldsActivated)
        {
            CurrentHealth -= PlayerData.ActiveWeapon.BaseDamage;
            UpdateHealthBar();
            CurrentCombatState = CombatState.ATTACKED;
        }
    }

    public void ShowCannonBlast()
    {
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
        if (!PlayerData.ActiveWeapon.HasCrit)
            return false;
        if (UnityEngine.Random.Range(0, 100) > PlayerData.ActiveWeapon.CritRate)
            return false;
        if (CombatCore.RoundCounter % PlayerData.ActiveWeapon.CritFrequency != 0)
            return false;
        if (CombatCore.RoundCounter < PlayerData.ActiveWeapon.CritFrequency)
            return false;

        return true;
    }

    private void WillInflictStatusEffect()
    {

    }

    private void ProcessDoubleDamage()
    {
        if (DoubleDamageTurnsCooldown > 0)
        {
            DoubleDamageTurnsCooldown--;
            if (DoubleDamageTurnsCooldown == 0)
                CombatCore.DoubleDamageBtn.interactable = true;
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
    #endregion
}