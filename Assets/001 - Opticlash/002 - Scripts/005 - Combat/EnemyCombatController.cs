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
    [field: SerializeField][field: ReadOnly] public int CurrentHealth { get; set; }
    [field: SerializeField][field: ReadOnly] private int DamageDeal { get; set; }
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }

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
        CurrentHealth = EnemyData.Health;
        DamageDeal = EnemyData.Damage;
        HealthSlider.transform.localScale = new Vector3((float)CurrentHealth / EnemyData.Health, 1f, 0f);
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
                if (Vector2.Distance(transform.parent.position, OriginalEnemyPosition) > 0.01f)
                    transform.parent.position = Vector2.MoveTowards(transform.parent.position, OriginalEnemyPosition, 7 * Time.deltaTime);
                else
                    CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
            }
            else
            {
                if (Vector2.Distance(transform.parent.position, EnemyAttackPosition) > 0.01f)
                    transform.parent.position = Vector2.MoveTowards(transform.parent.position, EnemyAttackPosition, 7 * Time.deltaTime);
                else
                    CurrentCombatState = CombatState.ATTACKING;
            }
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            foreach (GameObject enemy in CombatCore.Enemies)
                enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemy.transform.GetChild(0).GetComponent<EnemyCombatController>().OriginalEnemyPosition, 0.3f * Time.deltaTime);

            if (Vector2.Distance(CombatCore.CurrentEnemy.transform.position, CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().OriginalEnemyPosition) < 0.01f)
            {
                CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentCombatState = CharacterCombatController.CombatState.IDLE;
                CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
            }
        }
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        Debug.Log("Current state: " + CurrentCombatState);
        EnemyAnim.SetInteger("index", (int)CurrentCombatState);
    }


    #region ANIMATION EVENTS
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
        if(CurrentCombatState == CombatState.DYING)
        {
            IsCurrentEnemy = false;
            CurrentCombatState = CombatState.IDLE;
            transform.parent.position = new Vector3(65f, transform.parent.position.y, transform.parent.position.z);
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
    public void TakeDamageFromPlayer(int _damageReceived)
    {
        CurrentCombatState = CombatState.ATTACKED;
        CurrentHealth -= _damageReceived;
        HealthSlider.transform.localScale = new Vector3((float)CurrentHealth / EnemyData.Health, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(HealthSlider.transform.localScale.x - 1, HealthSlider.transform.localPosition.y, HealthSlider.transform.localPosition.z);
    }

    private void ResetHealthBar()
    {
        HealthBar.SetActive(true);
        HealthSlider.transform.localScale = new Vector3(1f, 1f, 0f);
    }
    #endregion
}
