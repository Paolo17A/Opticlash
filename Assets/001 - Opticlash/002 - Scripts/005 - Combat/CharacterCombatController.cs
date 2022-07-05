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
    [field: SerializeField][field: ReadOnly] public EnemyCombatController.SideEffect CurrentSideEffect { get; set; }
    [field: SerializeField][field: ReadOnly] public float SideEffectDamage;
    [field: SerializeField][field: ReadOnly] public int SideEffectInstancesRemaining;
    [field: SerializeField][field: ReadOnly] private bool BurnInstanceAccepted { get; set; } 

    [field: Header("PROJECTILES")]
    [field: SerializeField] private GameObject Projectile { get; set; }
    [field: SerializeField] private Transform ProjectileStartingPoint { get; set; }
    [field: SerializeField] private Transform ProjectileEndPoint { get; set; }
    [field: SerializeField][field: ReadOnly] public bool ProjectileSpawned { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public float ShotAccuracy { get; set; }
    //===================================================================================
    private void OnEnable()
    {
        playerCombatStateChange += CombatStateChange;
    }

    private void OnDisable()
    {
        playerCombatStateChange -= CombatStateChange;
    }

    private void Update()
    {
        if (ProjectileSpawned)
        {
            if (Vector2.Distance(Projectile.transform.position, ProjectileEndPoint.position) > 0.01f)
            {
                Projectile.transform.position = Vector2.MoveTowards(Projectile.transform.position, ProjectileEndPoint.position, 11 * Time.deltaTime);
            }
            else
            {
                int randomNum = UnityEngine.Random.Range(0, 100);
                Debug.Log(randomNum);
                //  HIT
                if(ShotAccuracy >= randomNum)
                {
                    Projectile.SetActive(false);
                    ProjectileSpawned = false;
                    Kaboom.SetActive(true);
                    if (DoubleDamageActivated)
                    {
                        if (CurrentSideEffect == EnemyCombatController.SideEffect.WEAK)
                            CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage);
                        else
                            CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage * 2);
                        DoubleDamageActivated = false;
                    }
                    else
                    {
                        if (CurrentSideEffect == EnemyCombatController.SideEffect.WEAK)
                            CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage / 2);
                        else
                            CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage);
                    }

                    if (DoubleDamageTurnsCooldown > 0)
                    {
                        DoubleDamageTurnsCooldown--;
                        if (DoubleDamageTurnsCooldown == 0)
                            CombatCore.DoubleDamageBtn.interactable = true;
                    }
                }
                //MISS
                else
                {
                    Debug.Log("YOU MISSED");
                    Projectile.SetActive(false);
                    ProjectileSpawned = false;
                    if (DoubleDamageActivated)
                        DoubleDamageActivated = false;
                    if (DoubleDamageTurnsCooldown > 0)
                    {
                        DoubleDamageTurnsCooldown--;
                        if (DoubleDamageTurnsCooldown == 0)
                            CombatCore.DoubleDamageBtn.interactable = true;
                    }
                    CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                }
                
            }
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
        ShieldInstancesRemaining = 5;
        MaxHealth = PlayerData.MaxHealth;
        CurrentHealth = MaxHealth;
        CombatCore.SpawnNextEnemy();
        CurrentCombatState = CombatState.WALKING;
        CombatCore.CurrentCombatState = CombatCore.CombatState.WALKING;

    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        PlayerAnimator.SetInteger("index", (int)CurrentCombatState);
        if (CurrentCombatState == CombatState.IDLE || CurrentCombatState == CombatState.WALKING)
            Cannon.SetActive(false);
        if (CurrentCombatState == CombatState.ATTACKING)
            Cannon.SetActive(true);
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
        if(!WarpActivated && WarpGunInstancesRemaining > 0)
        {
            Debug.Log("Warp has been activated");
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
        CurrentSideEffect = EnemyCombatController.SideEffect.NONE;
    }
    #endregion

    #region ANIMATION EVENTS
    public void ProcessHealth()
    {
        if (CurrentHealth > 0)
        {
            CurrentCombatState = CombatState.IDLE;
            #region BURN
            if (CurrentSideEffect == EnemyCombatController.SideEffect.BURN && !BurnInstanceAccepted)
            {
                BurnInstanceAccepted = true;
                CurrentCombatState = CombatState.ATTACKED;
                CurrentHealth -= SideEffectDamage;
                SideEffectInstancesRemaining--;
                if (SideEffectInstancesRemaining == 0)
                    CurrentSideEffect = EnemyCombatController.SideEffect.NONE;
            }
            #endregion
        }
        else
        {
            CurrentHealth = 0;
            HealthBar.SetActive(false);
            HealthSlider.transform.localScale = new Vector3(CurrentHealth / MaxHealth, 1f, 0f);
            CurrentCombatState = CombatState.DYING;
        }
    }

    public void ProcessDeath()
    {
        CombatCore.CurrentCombatState = CombatCore.CombatState.GAMEOVER;
    }

    public void AttackEnemy()
    {
        BurnInstanceAccepted = false;
        int randomNum = UnityEngine.Random.Range(0, 100);
        if (CurrentSideEffect == EnemyCombatController.SideEffect.PARALYZE)
        {
            if (randomNum < 20)
                CurrentCombatState = CombatState.ATTACKED;
            else
                AttackSequence();
        }
        else if (CurrentSideEffect == EnemyCombatController.SideEffect.CONFUSE)
        {
            if (randomNum < 20)
                TakeDamageFromEnemy(PlayerData.ActiveWeapon.BaseDamage);
            else
                AttackSequence();
        }
        else
            AttackSequence();

        if(SideEffectInstancesRemaining > 0)
        {
            SideEffectInstancesRemaining--;
            if (SideEffectInstancesRemaining == 0)
                CurrentSideEffect = EnemyCombatController.SideEffect.NONE;
        }
    }

    private void AttackSequence()
    {
        CannonBlast.SetActive(false);
        Projectile.SetActive(true);
        Projectile.transform.position = ProjectileStartingPoint.position;
        ProjectileSpawned = true;
        CombatCore.AmmoCount--;
        CombatCore.AmmoTMP.text = "Ammo: " + CombatCore.AmmoCount.ToString();
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
                {
                    CurrentHealth -= _damageReceived;
                    SideEffectInstancesRemaining--;
                    if (SideEffectInstancesRemaining == 0)
                        CurrentSideEffect = EnemyCombatController.SideEffect.NONE;
                }
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

            HealthSlider.transform.localScale = new Vector3(CurrentHealth / MaxHealth, 1f, 0f);
            HealthSlider.transform.localPosition = new Vector3(HealthSlider.transform.localScale.x - 1, HealthSlider.transform.localPosition.y, HealthSlider.transform.localPosition.z);
        }
    }

    public void ShowCannonBlast()
    {
        CannonBlast.SetActive(true);
    }

    public void CharacterCombatStateToIndex(int state)
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
    #endregion
}