using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    #region STATE MACHINE
    public enum CombatState
    {
        NONE,
        IDLE,
        ATTACKING,
        ATTACKED,
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
    #region EVENTS
    private void OnEnable()
    {
        onPlayerCombatStateChange += CombatStateChange;
    }

    private void OnDisable()
    {
        onPlayerCombatStateChange -= CombatStateChange;
    }
    #endregion
    //========================================================================================
    public enum SideEffect { NONE, BREAK, PIERCE, WEAK, PARALYZE, CONFUSE, BURN, FREEZE }
    public enum AttackType { NONE, MELEE, RANGED}
    [SerializeField][ReadOnly] private CombatState currentCombatState;
    [field: SerializeField] private CombatCore CombatCore { get; set; }

    [field: Header("ENEMY DATA")]
    [field: SerializeField] private Animator EnemyAnim { get; set; }
    [field: SerializeField] public AttackType EnemyAttackType { get; set; }
    [field: SerializeField][field: ReadOnly] public float CurrentHealth { get; set; }
    [field: SerializeField] public int MaxHealth { get; set; }
    [field: SerializeField] public float DamageDeal { get; set; }
    [field: SerializeField] public float EvasionValue { get; set; }
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }

    [field: Header("AFFLICTED SIDE EFFECT")]
    [field: SerializeField][field: ReadOnly] public WeaponData.SideEffect AfflictedSideEffect { get; set; }
    [field: SerializeField][field: ReadOnly] public float AfflictedSideEffectDamage { get; set; }
    [field: SerializeField][field: ReadOnly] public int AfflictedSideEffectInstancesLeft { get; set; }
    [field: SerializeField][field: ReadOnly] private bool BurnInstanceAccepted { get; set; }

    [field: Header("PASSIVE SIDE EFFECT")]
    [field: SerializeField] public SideEffect ThisSideEffect { get; set; }
    [field: SerializeField] private int SideEffectRate { get; set; }
    [field: SerializeField] public int SideEffectDuration { get; set; }
    [field: SerializeField] public float SideEffectDamage { get; set; }

    [field: Header("LASER ABILITY")]
    [field: SerializeField] private bool CanShootLaser { get; set; }
    [field: SerializeField] private int LaserFrequency { get; set; }    

    [field: Header("TRANSFORMS")]
    [field: SerializeField] public Vector3 OriginalEnemyPosition { get; set; }
    [field: SerializeField] private Vector3 EnemyAttackPosition { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool DoneAttacking { get; set; }
    [field: SerializeField][field: ReadOnly] public bool IsCurrentEnemy { get; set; }
    [field:SerializeField][field: ReadOnly] private bool ShootingLaser { get; set; }
    [field: SerializeField][field: ReadOnly] private CharacterCombatController Opti { get; set; }

    //========================================================================================

    private void Start()
    {
        Opti = CombatCore.SpawnedPlayer;
    }

    public void InitializeEnemy()
    {
        ResetHealthBar();
        CurrentHealth = MaxHealth;
        HealthSlider.transform.localScale = new Vector3((float)CurrentHealth / MaxHealth, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(0f, 0f, 10f);
        CurrentCombatState = CombatState.IDLE;
        IsCurrentEnemy = true;
    }

    private void FixedUpdate()
    {
        HealthBar.transform.rotation = Quaternion.identity;
        if(CombatCore.CurrentCombatState == CombatCore.CombatState.ENEMYTURN && CurrentCombatState == CombatState.IDLE && IsCurrentEnemy)
        {
            if(EnemyAttackType == AttackType.MELEE)
            {
                if (DoneAttacking)
                {
                    if (Vector2.Distance(transform.parent.position, OriginalEnemyPosition) > 0.01f)
                        transform.parent.position = Vector2.MoveTowards(transform.parent.position, OriginalEnemyPosition, 11 * Time.deltaTime);
                    else
                    {
                        CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                        if (AfflictedSideEffect != WeaponData.SideEffect.NONE && AfflictedSideEffectInstancesLeft > 0)
                        {
                            AfflictedSideEffectInstancesLeft--;
                            if (AfflictedSideEffectInstancesLeft == 0)
                            {
                                AfflictedSideEffect = WeaponData.SideEffect.NONE;
                                Opti.StatusEffectImage.gameObject.SetActive(false);
                            }
                        }
                    }
                }
                else
                {
                    if (AfflictedSideEffect == WeaponData.SideEffect.PARALYZE)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 20)
                        {
                            CurrentCombatState = CombatState.ATTACKED;
                            DoneAttacking = true;
                        }
                        else
                        {
                            if (Vector2.Distance(transform.parent.position, EnemyAttackPosition) > 0.01f)
                                transform.parent.position = Vector2.MoveTowards(transform.parent.position, EnemyAttackPosition, 7 * Time.deltaTime);
                            else
                                CurrentCombatState = CombatState.ATTACKING;
                        }
                    }
                    else if (AfflictedSideEffect == WeaponData.SideEffect.CONFUSE)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 20)
                            TakeDamageFromPlayer(DamageDeal);
                        else
                        {
                            if (Vector2.Distance(transform.parent.position, EnemyAttackPosition) > 0.01f)
                                transform.parent.position = Vector2.MoveTowards(transform.parent.position, EnemyAttackPosition, 7 * Time.deltaTime);
                            else
                                CurrentCombatState = CombatState.ATTACKING;
                        }
                    }
                    else if (AfflictedSideEffect == WeaponData.SideEffect.FREEZE)
                    {
                        DoneAttacking = true;
                    }
                    // Enemy has no side effects to process and will move and attack regularly
                    else
                    {
                        if (Vector2.Distance(transform.parent.position, EnemyAttackPosition) > 0.01f)
                            transform.parent.position = Vector2.MoveTowards(transform.parent.position, EnemyAttackPosition, 7 * Time.deltaTime);
                        else
                            CurrentCombatState = CombatState.ATTACKING;
                    }
                }
            }
            else if (EnemyAttackType == AttackType.RANGED)
            {
                if(DoneAttacking)
                {
                    CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                    if (AfflictedSideEffect != WeaponData.SideEffect.NONE && AfflictedSideEffectInstancesLeft > 0)
                    {
                        AfflictedSideEffectInstancesLeft--;
                        if (AfflictedSideEffectInstancesLeft == 0)
                        {
                            AfflictedSideEffect = WeaponData.SideEffect.NONE;
                            Opti.StatusEffectImage.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (!ShootingLaser && Vector2.Distance(CombatCore.EnemyProjectile.transform.position, CombatCore.SpawnedPlayer.gameObject.transform.position) > 0.01f)
                        CombatCore.EnemyProjectile.transform.position = Vector2.MoveTowards(CombatCore.EnemyProjectile.transform.position, CombatCore.SpawnedPlayer.gameObject.transform.position, 7 * Time.deltaTime);
                    else if (ShootingLaser && Vector2.Distance(CombatCore.EnemyLaser.transform.position, CombatCore.SpawnedPlayer.gameObject.transform.position) > 0.01f)
                        CombatCore.EnemyLaser.transform.position = Vector2.MoveTowards(CombatCore.EnemyLaser.transform.position, CombatCore.SpawnedPlayer.gameObject.transform.position, 7 * Time.deltaTime);

                    else
                    {
                        CombatCore.EnemyProjectile.SetActive(false);
                        CombatCore.EnemyLaser.SetActive(false);
                        DoneAttacking = true;
                        if(ShootingLaser)
                        {
                            Opti.TakeDamageFromEnemy(DamageDeal * 3);
                        }
                        else
                        {
                            if (AfflictedSideEffect == WeaponData.SideEffect.WEAK)
                                Opti.TakeDamageFromEnemy(DamageDeal / 2);
                            else
                                Opti.TakeDamageFromEnemy(DamageDeal);
                        }
                        
                    }
                }
            }
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            foreach (GameObject enemy in CombatCore.Enemies)
                enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemy.transform.GetChild(0).GetComponent<EnemyCombatController>().OriginalEnemyPosition, 0.2f * Time.deltaTime);

            if (Vector2.Distance(CombatCore.CurrentEnemy.gameObject.transform.parent.position, CombatCore.CurrentEnemy.OriginalEnemyPosition) < 0.01f)
            {
                CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                Opti.CurrentCombatState = CharacterCombatController.CombatState.IDLE;
                
            }
        }
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        Debug.Log("Current enemy state: " + CurrentCombatState);
        EnemyAnim.SetInteger("index", (int)CurrentCombatState);
    }


    #region ANIMATION EVENTS
    public void AttackPlayer()
    {
        if(EnemyAttackType == AttackType.MELEE)
        {
            if (AfflictedSideEffect == WeaponData.SideEffect.WEAK)
                Opti.TakeDamageFromEnemy(DamageDeal / 2);
            else
                Opti.TakeDamageFromEnemy(DamageDeal);
        }
        else if (EnemyAttackType == AttackType.RANGED)
        {
            DoneAttacking = false;

            if (CanShootLaser && (CombatCore.RoundCounter % LaserFrequency == 0 || CombatCore.RoundCounter == LaserFrequency))
            {
                ShootingLaser = true;
                CombatCore.EnemyLaser.transform.position = CombatCore.EnemyLaserStartingPoint;
                CombatCore.EnemyLaser.SetActive(true);
            }
            else
            {
                ShootingLaser = false;
                CombatCore.EnemyProjectile.transform.position = CombatCore.EnemyProjectileStartingPoint;
                CombatCore.EnemyProjectile.SetActive(true);
            }
            
        }
    }

    public void ReturnToStarting()
    {
        CurrentCombatState = CombatState.IDLE;
        if(EnemyAttackType == AttackType.MELEE)
            DoneAttacking = true;
    }

    // Process health is only called AFTER getting hit
    public void ProcessHealth()
    {
        if (CurrentHealth > 0)
        {
            CurrentCombatState = CombatState.IDLE;
            #region BURN
            if (AfflictedSideEffect == WeaponData.SideEffect.BURN && !BurnInstanceAccepted)
            {
                BurnInstanceAccepted = true;
                //CurrentCombatState = CombatState.ATTACKED;
                CurrentHealth -= AfflictedSideEffectDamage;
                AfflictedSideEffectInstancesLeft--;
                if (AfflictedSideEffectInstancesLeft == 0)
                    AfflictedSideEffect = WeaponData.SideEffect.NONE;
            }
            else if (AfflictedSideEffect == WeaponData.SideEffect.PARALYZE)
            {
                Debug.Log("Enemy is paralyzed");
            }
            else if (AfflictedSideEffect == WeaponData.SideEffect.FREEZE)
            {
                Debug.Log("Enemy is frozen");

            }
            #endregion
            Opti.ProcessEndAttack();
        }
        else
        {
            CurrentCombatState = CombatState.DYING;
            HealthBar.SetActive(false);
        }
    }

    public void ProcessDeath()
    {
        if(CurrentCombatState == CombatState.DYING)
        {
            if(CombatCore.SpawnedPlayer.CurrentSideEffect != SideEffect.NONE)
                CombatCore.SpawnedPlayer.ProcessStatusEffectInstances();
            
            CombatCore.MonstersKilled++;
            IsCurrentEnemy = false;
            CurrentCombatState = CombatState.IDLE;
            transform.parent.position = new Vector3(305f, transform.parent.position.y, transform.parent.position.z);
            ResetHealthBar();
            CombatCore.SpawnNextEnemy();
            if(CombatCore.CurrentCombatState != CombatCore.CombatState.GAMEOVER)
                CombatCore.CurrentCombatState = CombatCore.CombatState.WALKING;
        }
    }
    #endregion

    #region DAMAGE
    public void TakeDamageFromPlayer(float _damageReceived)
    {
        CurrentCombatState = CombatState.ATTACKED;
        CurrentHealth -= _damageReceived;
        HealthSlider.transform.localScale = new Vector3((float)CurrentHealth / MaxHealth, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(HealthSlider.transform.localScale.x - 1, HealthSlider.transform.localPosition.y, HealthSlider.transform.localPosition.z);
    
    }

    private void ResetHealthBar()
    {
        HealthBar.SetActive(true);
        HealthSlider.transform.localScale = new Vector3(1f, 1f, 0f);
    }
    #endregion

    #region UTILITY
    public bool WillInflictStatusEffect()
    {
        if (ThisSideEffect == SideEffect.NONE)
            return false;

        if (CombatCore.RoundCounter % SideEffectRate != 0)
            return false;

        if (CombatCore.SpawnedPlayer.CurrentSideEffect != SideEffect.NONE)
            return false;

        if (AfflictedSideEffect == WeaponData.SideEffect.BREAK)
            return false;

        return true;
    }
    #endregion
}