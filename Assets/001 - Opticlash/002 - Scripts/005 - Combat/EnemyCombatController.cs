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
    [field: SerializeField] private AttackType EnemyAttackType { get; set; }
    [field: SerializeField][field: ReadOnly] public float CurrentHealth { get; set; }
    [field: SerializeField] public int MaxHealth { get; set; }
    [field: SerializeField] private float DamageDeal { get; set; }
    [field: SerializeField] public float EvasionValue { get; set; }
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }

    [field: Header("AFFLICTED SIDE EFFECT")]
    [field: SerializeField] public WeaponData.SideEffect AfflictedSideEffect { get; set; }
    [field: SerializeField] public float AfflictedSideEffectDamage { get; set; }
    [field: SerializeField] public int AfflictedSideEffectInstancesLeft { get; set; }
    [field: SerializeField][field: ReadOnly] private bool BurnInstanceAccepted { get; set; }

    [field: Header("PASSIVE SIDE EFFECT")]
    [field: SerializeField] private SideEffect ThisSideEffect { get; set; }
    [field: SerializeField] private int SideEffectRate { get; set; }
    [field: SerializeField] private int SideEffectDuration { get; set; }
    [field: SerializeField] private float SideEffectDamage { get; set; }

    [field: Header("TRANSFORMS")]
    [field: SerializeField] public Vector3 OriginalEnemyPosition { get; set; }
    [field: SerializeField] private Vector3 EnemyAttackPosition { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool DoneAttacking { get; set; }
    [field: SerializeField][field: ReadOnly] public bool IsCurrentEnemy { get; set; }
    //========================================================================================

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
            if(DoneAttacking)
            {
                if(EnemyAttackType == AttackType.MELEE)
                {

                }
                else if (EnemyAttackType == AttackType.RANGED)
                {

                }
                if (Vector2.Distance(transform.parent.position, OriginalEnemyPosition) > 0.01f)
                    transform.parent.position = Vector2.MoveTowards(transform.parent.position, OriginalEnemyPosition, 7 * Time.deltaTime);
                else
                {
                    if(CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentSideEffect == SideEffect.FREEZE)
                    {
                        DoneAttacking = false;
                        CurrentCombatState = CombatState.IDLE;
                        CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().SideEffectInstancesRemaining--;
                        if (CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().SideEffectInstancesRemaining == 0)
                            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentSideEffect = SideEffect.NONE;
                    }
                    else
                    {
                        CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                        if (AfflictedSideEffect != WeaponData.SideEffect.NONE && AfflictedSideEffectInstancesLeft > 0)
                        {
                            AfflictedSideEffectInstancesLeft--;
                            if (AfflictedSideEffectInstancesLeft == 0)
                                AfflictedSideEffect = WeaponData.SideEffect.NONE;
                        }
                    }
                }
            }
            else
            {
                int randomNum = UnityEngine.Random.Range(0, 100);
                if (AfflictedSideEffect == WeaponData.SideEffect.PARALYZE)
                {
                    if (randomNum < 20)
                        CurrentCombatState = CombatState.ATTACKED;
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
                    if (randomNum < 20)
                        TakeDamageFromPlayer(DamageDeal);
                    else
                    {
                        if (Vector2.Distance(transform.parent.position, EnemyAttackPosition) > 0.01f)
                            transform.parent.position = Vector2.MoveTowards(transform.parent.position, EnemyAttackPosition, 7 * Time.deltaTime);
                        else
                            CurrentCombatState = CombatState.ATTACKING;
                    }
                }
                else
                {
                    if (Vector2.Distance(transform.parent.position, EnemyAttackPosition) > 0.01f)
                        transform.parent.position = Vector2.MoveTowards(transform.parent.position, EnemyAttackPosition, 7 * Time.deltaTime);
                    else
                        CurrentCombatState = CombatState.ATTACKING;
                }
            }
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            foreach (GameObject enemy in CombatCore.Enemies)
                enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemy.transform.GetChild(0).GetComponent<EnemyCombatController>().OriginalEnemyPosition, 0.1f * Time.deltaTime);

            if (Vector2.Distance(CombatCore.CurrentEnemy.transform.position, CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().OriginalEnemyPosition) < 0.01f)
            {
                CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentCombatState = CharacterCombatController.CombatState.IDLE;
                CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
            }
        }
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        EnemyAnim.SetInteger("index", (int)CurrentCombatState);
    }


    #region ANIMATION EVENTS
    public void AttackPlayer()
    {
        if(AfflictedSideEffect == WeaponData.SideEffect.WEAK)
            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().TakeDamageFromEnemy(DamageDeal / 2);
        else
            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().TakeDamageFromEnemy(DamageDeal);

        #region SIDE EFFECTS
        //  Only inflict a side effect if the modulo between the current round and the side effect rate is zero AND if the player is not inflicted with a status effect yet
        if (ThisSideEffect != SideEffect.NONE && (CombatCore.RoundCounter % SideEffectRate == 0) && CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentSideEffect == SideEffect.NONE && AfflictedSideEffect != WeaponData.SideEffect.BREAK)
        {
            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentSideEffect = ThisSideEffect;
            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().SideEffectDamage = SideEffectDamage;
            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().SideEffectInstancesRemaining = SideEffectDuration;
        }
        #endregion
    }

    public void ReturnToStarting()
    {
        CurrentCombatState = CombatState.IDLE;
        DoneAttacking = true;
    }

    public void ProcessHealth()
    {
        if (CurrentHealth > 0)
        {
            CurrentCombatState = CombatState.IDLE;
            #region BURN
            if (AfflictedSideEffect == WeaponData.SideEffect.BURN && !BurnInstanceAccepted)
            {
                BurnInstanceAccepted = true;
                CurrentCombatState = CombatState.ATTACKED;
                CurrentHealth -= AfflictedSideEffectDamage;
                AfflictedSideEffectInstancesLeft--;
                if (AfflictedSideEffectInstancesLeft == 0)
                    AfflictedSideEffect = WeaponData.SideEffect.NONE;
            }
            #endregion
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
            IsCurrentEnemy = false;
            CurrentCombatState = CombatState.IDLE;
            transform.parent.position = new Vector3(325f, transform.parent.position.y, transform.parent.position.z);
            ResetHealthBar();
            CombatCore.SpawnNextEnemy();
            if(CombatCore.CurrentCombatState != CombatCore.CombatState.GAMEOVER)
            {
                CombatCore.CurrentCombatState = CombatCore.CombatState.WALKING;
                CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentCombatState = CharacterCombatController.CombatState.WALKING;
            }
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
}
