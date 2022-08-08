using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [field: SerializeField] private CombatCore CombatCore { get; set; }
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    private void OnEnable()
    {
        CombatCore.onCombatStateChange += CombatStateChange;
    }

    private void OnDisable()
    {
        CombatCore.onCombatStateChange -= CombatStateChange;
    }

    private void Awake()
    {
        CombatCore.EnemyQueue = new Queue<GameObject>();
    }

    private void Start()
    {
        foreach (CustomWeaponData weapon in PlayerData.OwnedWeapons)
            if (weapon.WeaponInstanceID == PlayerData.ActiveWeaponID)
            {
                PlayerData.ActiveCustomWeapon.BaseWeaponData = weapon.BaseWeaponData;
                break;
            }
        CombatCore.CurrentCombatState = CombatCore.CombatState.SPAWNING;
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        //Debug.Log(CombatCore.CurrentCombatState);
        if (CombatCore.CurrentCombatState == CombatCore.CombatState.SPAWNING)
        {
            CombatCore.MonstersKilled = 0;
            PlayerData.CurrentHealth = PlayerData.MaxHealth;
            CombatCore.AmmoCount = PlayerData.ActiveCustomWeapon.BaseWeaponData.StartingAmmo;
            CombatCore.UIAnimator.SetBool("GameOver", false);
            CombatCore.AmmoTMP.text = "Ammo: " + CombatCore.AmmoCount.ToString();
            CombatCore.SpawnEnemies();
            CombatCore.SpawnedPlayer.InitializePlayer();
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            CombatCore.ProcessPowerUpInteractability();
            CombatCore.ProcessSkillsInteractability();
            CombatCore.RoundCounter++;
            CombatCore.RoundTMP.text = "Round: " + CombatCore.RoundCounter.ToString();
            CombatCore.timerCoroutine = StartCoroutine(CombatCore.StartQuestionTimer());
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.PLAYERTURN)
        {
            CombatCore.DoubleDamageBtn.interactable = false;
            CombatCore.ShieldBtn.interactable = false;
            CombatCore.SpawnedPlayer.ProjectileSpawned = false;
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.ENEMYTURN)
        {
            if (CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.NONE)
            {
                CombatCore.CurrentEnemy.MayAttack = true;
                CombatCore.CurrentEnemy.ProcessAttackType();
            }
            else
            {
                if (CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.BREAK || CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.WEAK)
                {
                    CombatCore.CurrentEnemy.StatusEffectTextAnimator.SetTrigger("ShowStatus");
                    CombatCore.CurrentEnemy.MayAttack = true;
                    CombatCore.CurrentEnemy.ProcessAttackType();
                }

                else if (CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.PARALYZE)
                {
                    if(UnityEngine.Random.Range(0,5) == 1)
                    {
                        CombatCore.CurrentEnemy.StatusEffectTextAnimator.SetTrigger("ShowStatus");
                        CombatCore.CurrentEnemy.MayAttack = true;
                        CombatCore.CurrentEnemy.DoneAttacking = true;
                    }
                    else
                    {
                        CombatCore.CurrentEnemy.MayAttack = true;
                        CombatCore.CurrentEnemy.ProcessAttackType();
                    }
                    
                }
                else if (CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.FREEZE)
                {
                    CombatCore.CurrentEnemy.StatusEffectTextAnimator.SetTrigger("ShowStatus");
                    CombatCore.CurrentEnemy.MayAttack = true;
                    CombatCore.CurrentEnemy.DoneAttacking = true;
                }
                else if (CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.CONFUSE)
                {
                    if(UnityEngine.Random.Range(0,5) == 1)
                    {
                        CombatCore.CurrentEnemy.StatusEffectTextAnimator.SetTrigger("ShowStatus");
                        CombatCore.CurrentEnemy.TakeDamageFromSelf();
                        CombatCore.CurrentEnemy.MayAttack = true;
                        CombatCore.CurrentEnemy.DoneAttacking = true;
                    }
                    else
                    {
                        CombatCore.CurrentEnemy.MayAttack = true;
                        CombatCore.CurrentEnemy.ProcessAttackType();
                    }
                }
                else if (CombatCore.CurrentEnemy.AfflictedSideEffect == WeaponData.SideEffect.BURN)
                {
                    CombatCore.CurrentEnemy.MayAttack = true;
                    CombatCore.CurrentEnemy.ProcessAttackType();
                }
            }
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.GAMEOVER)
        {
            CombatCore.UIAnimator.SetBool("GameOver", true);
            CombatCore.StopTimerCoroutine();
            CombatCore.MonstersKilledTMP.text = CombatCore.MonstersKilled.ToString();
            CombatCore.GrantRewardedItems();
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            CombatCore.SpawnedPlayer.CurrentCombatState = CharacterCombatController.CombatState.WALKING;
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WARPING)
        {
            CombatCore.StopTimerCoroutine();
        }
    }

    public void CombatStateToIndex(int state)
    {
        switch (state)
        {
            case (int)CombatCore.CombatState.SPAWNING:
                CombatCore.CurrentCombatState = CombatCore.CombatState.SPAWNING;
                break;
            case (int)CombatCore.CombatState.TIMER:
                CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                break;
            case (int)CombatCore.CombatState.PLAYERTURN:
                CombatCore.CurrentCombatState = CombatCore.CombatState.PLAYERTURN;
                break;
            case (int)CombatCore.CombatState.ENEMYTURN:
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
                break;
            case (int)CombatCore.CombatState.GAMEOVER:
                CombatCore.CurrentCombatState = CombatCore.CombatState.GAMEOVER;
                break;
            case (int)CombatCore.CombatState.WALKING:
                CombatCore.CurrentCombatState = CombatCore.CombatState.WALKING;
                break;
            case (int)CombatCore.CombatState.WARPING:
                CombatCore.CurrentCombatState = CombatCore.CombatState.WARPING;
                break;
        }
    }
}
