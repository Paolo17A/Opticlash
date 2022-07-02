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
        CombatCore.Enemies = new Queue<GameObject>();
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        if (CombatCore.CurrentCombatState == CombatCore.CombatState.SPAWNING)
        {
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
            Debug.Log("You are now attacking the enemy");
            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().ProjectileSpawned = false;
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.ENEMYTURN)
        {
            Debug.Log("Enemy will now attack you");
            CombatCore.CurrentEnemy.GetComponent<EnemyCombatController>().DoneAttacking = false;
            //CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.GAMEOVER)
        {
            Debug.Log("Stage is over");
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            Debug.Log("Character and enemy are walking");

        }
    }

    private void Start()
    {
        CombatCore.CurrentCombatState = CombatCore.CombatState.SPAWNING;
    }
}
