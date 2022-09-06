using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CombatController : MonoBehaviour
{
    [field: SerializeField] private CombatCore CombatCore { get; set; }
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    private void OnEnable()
    {
        GameManager.Instance.SceneController.ActionPass = true;
        CombatCore.onCombatStateChange += CombatStateChange;
    }

    private void OnDisable()
    {
        CombatCore.onCombatStateChange -= CombatStateChange;
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
        if (CombatCore.CurrentCombatState == CombatCore.CombatState.SPAWNING)
        {
            GameManager.Instance.MainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            GameManager.Instance.MyUICamera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            CombatCore.SpawnedPlayer.EntireCannon.SetActive(true);
            CombatCore.MonstersKilled = 0;
            PlayerData.CurrentHealth = PlayerData.MaxHealth;
            CombatCore.AmmoCount = PlayerData.ActiveCustomWeapon.BaseWeaponData.StartingAmmo;
            CombatCore.UIAnimator.SetBool("GameOver", false);
            CombatCore.UIAnimator.SetBool("StageClear", false);
            CombatCore.AmmoTMP.text = "Ammo: " + CombatCore.AmmoCount.ToString();
            CombatCore.SpawnEnemies();
            CombatCore.SpawnedPlayer.InitializePlayer();
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            CombatCore.WaitPanel.SetActive(false);
            CombatCore.PowerUpsBtn.interactable = true;
            CombatCore.ItemsBtn.interactable = true;
            CombatCore.ShuffleBtn.interactable = true;
            CombatCore.ProcessPowerUpInteractability();
            CombatCore.ProcessSkillsInteractability();
            CombatCore.EnablePowerups();
            CombatCore.RoundCounter++;
            CombatCore.RoundTMP.text = "Round: " + CombatCore.RoundCounter.ToString();
            CombatCore.timerCoroutine = StartCoroutine(CombatCore.StartQuestionTimer());
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.PLAYERTURN)
        {
            CombatCore.PowerUpsBtn.interactable = false;
            CombatCore.ItemsBtn.interactable = false;
            CombatCore.ShuffleBtn.interactable = false;
            CombatCore.SpawnedPlayer.ProjectileSpawned = false;
            CombatCore.DisableItems();
            CombatCore.DisablePowerups();
            CombatCore.WaitPanel.SetActive(true);
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.ENEMYTURN)
        {
            CombatCore.DisableItems();
            CombatCore.DisablePowerups();
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
            GameManager.Instance.MyUICamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            CombatCore.SpawnedPlayer.EntireCannon.SetActive(false);
            CombatCore.isPlaying = false;
            CombatCore.CurrentEnemy.gameObject.SetActive(false);
            CombatCore.MonsterParent.transform.position = new Vector3(15, 21, 0);
            CombatCore.UIAnimator.SetBool("GameOver", true);
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            CombatCore.SpawnedPlayer.CurrentCombatState = CharacterCombatController.CombatState.WALKING;
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WARPING)
        {
            CombatCore.StopTimerCoroutine();
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.STAGECLEAR)
        {
            GameManager.Instance.MyUICamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            CombatCore.SpawnedPlayer.EntireCannon.SetActive(false);
            CombatCore.isPlaying = false;
            CombatCore.CurrentEnemy.gameObject.SetActive(false);
            CombatCore.MonsterParent.transform.position = new Vector3(15, 21, 0);
            CombatCore.UIAnimator.SetBool("StageClear", true);
            CombatCore.StopTimerCoroutine();
            CombatCore.UpdateLevelsWon();
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
