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
    [field: SerializeField] private int MaxHealth;
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }

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

    [Header("DEBUFFS")]
    [SerializeField][ReadOnly] private int debuffDamage;
    [SerializeField][ReadOnly] private int debuffInstancesRemaining;

    [field: Header("PROJECTILES")]
    [field: SerializeField] private GameObject Projectile { get; set; }
    [field: SerializeField] private Transform ProjectileStartingPoint { get; set; }
    [field: SerializeField] private Transform ProjectileEndPoint { get; set; }
    [field: SerializeField][field: ReadOnly] public bool ProjectileSpawned { get; set; }
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
                Projectile.SetActive(false);
                ProjectileSpawned = false;
                Kaboom.SetActive(true);
                if(DoubleDamageActivated)
                {
                    CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage * 2);
                    DoubleDamageTurnsCooldown--;
                    if(DoubleDamageTurnsCooldown == 0)
                        DoubleDamageActivated = false;
                }
                else
                CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage);
            }
        }
    }

    public void InitializePlayerCannon()
    {
        #region CANNON SPPRITES
        CannonBackSprite.sprite = PlayerData.ActiveWeapon.BackSprite;
        CannonMiddleSprite.sprite = PlayerData.ActiveWeapon.MiddleSprite;
        CannonFrontSprite.sprite = PlayerData.ActiveWeapon.FrontSprite;
        #endregion
        #region PROJECTILE SPRITES
        Projectile.GetComponent<SpriteRenderer>().sprite = PlayerData.ActiveWeapon.Ammo;
        if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C3 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C4 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.C5)
            Projectile.transform.GetChild(0).gameObject.SetActive(true);
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B3 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.B4)
            Projectile.transform.GetChild(1).gameObject.SetActive(true);
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A2 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.A3)
            Projectile.transform.GetChild(2).gameObject.SetActive(true);
        else if (PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.S1 || PlayerData.ActiveWeapon.ThisWeaponCode == WeaponData.WeaponCode.S2)
            Projectile.transform.GetChild(3).gameObject.SetActive(true);
        #endregion
        #region HEALTH SPRITES
        HealthBar.SetActive(true);
        HealthSlider.transform.localScale = new Vector3(1f, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(0f, 0f, 10f);
        #endregion
        MaxHealth = PlayerData.MaxHealth;
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
        if(!DoubleDamageActivated)
        {
            DoubleDamageActivated = true;
            DoubleDamageTurnsCooldown = 3;
        }
    }
    #endregion

    #region ANIMATION EVENTS
    public void ProcessHealth()
    {
        if (PlayerData.CurrentHealth > 0)
            CurrentCombatState = CombatState.IDLE;
        else
        {
            PlayerData.CurrentHealth = 0;
            HealthBar.SetActive(false);
            HealthSlider.transform.localScale = new Vector3((float)PlayerData.CurrentHealth / MaxHealth, 1f, 0f);
            CurrentCombatState = CombatState.DYING;
        }
    }

    public void ProcessDeath()
    {
        CombatCore.CurrentCombatState = CombatCore.CombatState.GAMEOVER;
    }

    public void AttackEnemy()
    {
        CannonBlast.SetActive(false);
        Projectile.SetActive(true);
        Projectile.transform.position = ProjectileStartingPoint.position;
        ProjectileSpawned = true;
        PlayerData.AmmoCount--;
        CombatCore.AmmoTMP.text = "Ammo: " + PlayerData.AmmoCount.ToString();
    }

    public void TakeDamageFromEnemy(int _damageReceived)
    {
        if (_damageReceived > 0)
        {
            CurrentCombatState = CombatState.ATTACKED;
            PlayerData.CurrentHealth -= _damageReceived;
            HealthSlider.transform.localScale = new Vector3((float)PlayerData.CurrentHealth / MaxHealth, 1f, 0f);
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