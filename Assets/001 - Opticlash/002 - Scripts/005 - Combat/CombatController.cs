using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [field: SerializeField] private CombatCore CombatCore { get; set; }
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private GameObject SpawnedPlayer { get; set; }

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
        CombatCore.CurrentCombatState = CombatCore.CombatState.SPAWNING;
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        if (CombatCore.CurrentCombatState == CombatCore.CombatState.SPAWNING)
        {
            PlayerData.CurrentHealth = PlayerData.MaxHealth;
            PlayerData.AmmoCount = 200;
            CombatCore.UIAnimator.SetBool("GameOver", false);
            CombatCore.AmmoTMP.text = "Ammo: " + PlayerData.AmmoCount.ToString();
            CombatCore.SpawnEnemies();
            SpawnedPlayer.GetComponent<CharacterCombatController>().InitializePlayerCannon();
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            CombatCore.RoundCounter++;
            CombatCore.RoundTMP.text = "Round: " + CombatCore.RoundCounter.ToString();
            CombatCore.timerCoroutine = StartCoroutine(CombatCore.StartQuestionTimer());
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.PLAYERTURN)
        {
            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().ProjectileSpawned = false;
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.ENEMYTURN)
        {
            CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().DoneAttacking = false;
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.GAMEOVER)
        {
            CombatCore.UIAnimator.SetBool("GameOver", true);
            CombatCore.StopTimerCoroutine();
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentCombatState = CharacterCombatController.CombatState.WALKING;
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
        }
    }
}
