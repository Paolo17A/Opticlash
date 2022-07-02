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
    [SerializeField][ReadOnly] private CombatState currentCombatState;
    [field: SerializeField] private CombatCore CombatCore { get; set; }

    [field: Header("ENEMY DATA")]
    [field: SerializeField] private EnemyData EnemyData { get; set; }
    [field: SerializeField] private Animator EnemyAnim { get; set; }
    [field: SerializeField][field: ReadOnly] private int CurrentHealth { get; set; }
    [field: SerializeField][field: ReadOnly] private int DamageDeal { get; set; }
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }

    [field: Header("TRANSFORMS")]
    [field: SerializeField] private Vector3 OriginalEnemyPosition { get; set; }
    [field: SerializeField] private Transform EnemyAttackPoint { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool DoneAttacking { get; set; }
    [SerializeField] private Vector3 worldTransform;
    //========================================================================================

    public void InitializeEnemy()
    {
        //OriginalEnemyPosition = gameObject.transform.position;
        transform.position = OriginalEnemyPosition;
        CurrentHealth = EnemyData.Health;
        DamageDeal = EnemyData.Damage;
        CurrentCombatState = CombatState.IDLE;
    }

    private void FixedUpdate()
    {
        HealthBar.transform.rotation = Quaternion.identity;
        if(CombatCore.CurrentCombatState == CombatCore.CombatState.ENEMYTURN && CurrentCombatState == CombatState.IDLE)
        {
            if(DoneAttacking)
            {
                if (Vector2.Distance(transform.parent.position, OriginalEnemyPosition) > 0.01f)
                    transform.parent.position = Vector2.MoveTowards(transform.parent.position, OriginalEnemyPosition, 7 * Time.deltaTime);
                else
                    CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                
            }
            else
            {
                if (Vector2.Distance(transform.parent.position, EnemyAttackPoint.position) > 0.01f)
                    transform.parent.position = Vector2.MoveTowards(transform.parent.position, EnemyAttackPoint.position, 7 * Time.deltaTime);
                else
                    CurrentCombatState = CombatState.ATTACKING;
            }
            
        }
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        EnemyAnim.SetInteger("index", (int)CurrentCombatState);
    }

    public void AttackPlayer()
    {
        CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().TakeDamageFromEnemy(DamageDeal);
    }

    public void ReturnToStarting()
    {
        CurrentCombatState = CombatState.IDLE;
        DoneAttacking = true;
    }

    public void ProcessHealth()
    {
        if (CurrentHealth > 0)
            CurrentCombatState = CombatState.IDLE;
        else
        {
            CurrentCombatState = CombatState.DYING;
            HealthBar.SetActive(false);
        }
    }

    public void ProcessDeath()
    {
        gameObject.SetActive(false);
        CombatCore.SpawnNextEnemy();
        CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
    }

    #region ANIMATION COROUTINES
    /*private IEnumerator AttackPlayer()
    {
        EnemyAnimator.SetBool("isAttacking", true);
        yield return new WaitForSeconds(0.33f);
        EnemyAnimator.SetBool("isAttacking", false);

        //  Player has SWIFT buff and has 15% chance to dodge incoming attack
        if (playerTarget.GetComponent<CharacterCombatController>().PlayerBuff == CharacterCombatController.PlayerBuffs.SWIFT && UnityEngine.Random.Range(0, 100) <= 15)
            CombatStateData.CurrentCombatState = CombatStateData.CombatState.RETURN;

        else
        {
            PlayerTarget.GetComponent<CharacterCombatController>().CombatStateData.CurrentCombatState = CombatStateData.CombatState.ATTACKED;   //  Plays player attacked animation

            if (combatCore.ConsecutiveMistakes == 3 && ThisEnemyData.buff == EnemyData.Buff.TIMEBOMB)
                DamageDeal += 500;

            if (UnityEngine.Random.Range(0, 10) > 7)
                PlayerTarget.GetComponent<CharacterCombatController>().TakeDamageFromEnemy(DamageDeal + ThisEnemyData.bonusDamage);
            else
                PlayerTarget.GetComponent<CharacterCombatController>().TakeDamageFromEnemy(DamageDeal);

            if (ThisEnemyData.buff != EnemyData.Buff.NONE)
            {
                switch (ThisEnemyData.buff)
                {
                    case EnemyData.Buff.BLESSED:
                        int healAmount = Mathf.CeilToInt(damageDeal / 4);

                        if (CurrentHealth + healAmount >= ThisEnemyData.health)
                            CurrentHealth = (int)ThisEnemyData.health;
                        else
                            CurrentHealth += healAmount;

                        healthBar.transform.localScale = new Vector3((float)currentHealth / ThisEnemyData.health, 0.1f, 0f);
                        break;
                    case EnemyData.Buff.TIMEBOMB:
                        if (combatCore.ConsecutiveMistakes == 3)
                        {
                            DamageDeal -= 500;
                            combatCore.ConsecutiveMistakes = 0;
                        }
                        break;
                }
            }

            yield return new WaitUntil(() => playerTarget.GetComponent<CharacterCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.IDLE ||
                                             playerTarget.GetComponent<CharacterCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.DYING);
            if (playerTarget.GetComponent<CharacterCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.IDLE)
            {
                CombatStateData.CurrentCombatState = CombatStateData.CombatState.RETURN;
            }
            else if (playerTarget.GetComponent<CharacterCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.DYING)
            {
                GameManager.Instance.CombatData.CurrentCombatState = CombatData.CombatState.GAMEOVER;
            }
        }
    }

    private IEnumerator GetHitByPlayer()
    {
        EnemyAnimator.SetBool("isHit", true);
        HitEffect.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        HitEffect.SetActive(false);
        DamageReceived.gameObject.SetActive(false);
        EnemyAnimator.SetBool("isHit", false);
    }

    private IEnumerator Die()
    {
        anim.SetBool("isDead", true);
        yield return new WaitForSeconds(1f);
    }*/
    #endregion

    #region DAMAGE
    public void TakeDamageFromPlayer(int _damageReceived)
    {
        CurrentCombatState = CombatState.ATTACKED;
        CurrentHealth -= _damageReceived;
        HealthSlider.transform.localScale = new Vector3((float)CurrentHealth / EnemyData.Health, 1f, 0f);
    }
    #endregion
}
