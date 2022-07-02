using MyBox;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Linq;

public class CombatCore : MonoBehaviour
{
    #region STATE MACHINE
    public enum CombatState
    {
        NONE,
        SPAWNING,
        TIMER,
        PLAYERTURN,
        ENEMYTURN,
        GAMEOVER,
        WALKING
    }

    private event EventHandler combatStateChange;
    public event EventHandler onCombatStateChange
    {
        add
        {
            if (combatStateChange == null || !combatStateChange.GetInvocationList().Contains(value))
            {
                combatStateChange += value;
            }
        }
        remove
        {
            combatStateChange -= value;
        }
    }

    public CombatState CurrentCombatState
    {
        get { return currentCombatState; }
        set
        {
            currentCombatState = value;
            combatStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion
    //=================================================================================
    [SerializeField][ReadOnly] private CombatState currentCombatState;
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    [Header("LOADING")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingTextTMP;
    
    [field: Header("GAME VARIABLES")]
    [field: SerializeField][field: ReadOnly] public int QuestionTimerLeft { get; set; }
    [field: SerializeField] private TextMeshProUGUI TimerTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private float CurrentCountdownNumber { get; set; }
    [field: SerializeField] public TextMeshProUGUI AmmoTMP { get; set; }
    [field: SerializeField][field: ReadOnly] public int RoundCounter { get; set; }
    [field: SerializeField] public TextMeshProUGUI RoundTMP { get; set; }

    [field: Header("LOCAL PLAYER DATA")]
    [field: SerializeField] public GameObject SpawnedPlayer { get; set; }

    [field: Header("LOCAL ENEMY DATA")]
    [field: SerializeField][field: ReadOnly] public GameObject CurrentEnemy { get; set; }
    [field: SerializeField] private Transform EnemyContainer { get; set; }
    [field: SerializeField][field: ReadOnly] public Queue<GameObject> Enemies { get; set; }
    [SerializeField] private Transform enemyAttackPoint;
    [SerializeField] private Transform enemyOriginalPosition;

    [Header("DEBUGGER")]
    public Coroutine timerCoroutine;
    //=================================================================================

    #region SPAWNING
    public void SpawnEnemies()
    {
        foreach (Transform child in EnemyContainer.transform)
        {
            child.gameObject.SetActive(false);
            Enemies.Enqueue(child.gameObject);
        }

        SpawnNextEnemy();
    }

    public void SpawnNextEnemy()
    {
        CurrentEnemy = Enemies.Dequeue();
        CurrentEnemy.SetActive(true);
        CurrentEnemy.GetComponent<EnemyCombatController>().InitializeEnemy();
        if(Enemies.Count == 0)
        {
            foreach (Transform child in EnemyContainer.transform)
                Enemies.Enqueue(child.gameObject);
        }
    }
    #endregion

    #region TIMER
    public IEnumerator StartQuestionTimer()
    {
        CurrentCountdownNumber = 60f;
        QuestionTimerLeft = 60;
        TimerTMP.text = QuestionTimerLeft.ToString();
        while (QuestionTimerLeft > 0f && CurrentCombatState == CombatState.TIMER)
        {
            CurrentCountdownNumber -= Time.deltaTime;
            QuestionTimerLeft = (int)CurrentCountdownNumber;
            TimerTMP.text = QuestionTimerLeft.ToString();

            yield return null;
        }

        CurrentCombatState = CombatState.ENEMYTURN;
    }
    public void StopTimerCoroutine()
    {
        StopCoroutine(timerCoroutine);
    }
    #endregion

    #region ENEMY
    /*public IEnumerator AttackPlayer()
    {
        spawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState = CombatStateData.CombatState.APPROACH;

        yield return new WaitUntil(() => spawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.IDLE ||
                                         spawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.DYING);

        if (spawnedPlayer.GetComponent<CharacterCombatController>().IsDead)
        {
            GameManager.Instance.CombatData.CurrentCombatState = CombatData.CombatState.GAMEOVER;
        }

        if (!spawnedPlayer.GetComponent<CharacterCombatController>().IsDead)
        {
            GameManager.Instance.CombatData.CurrentCombatState = CombatData.CombatState.TIMER;
        }
    }

    private void SpawnNextEnemy()
    {
        EnemyData thisEnemyData = enemyDataQueue.Dequeue();
        SpawnedEnemy = Instantiate(thisEnemyData.enemyPrefab);
        SpawnedEnemy.transform.SetParent(enemyContainer);
        SpawnedEnemy.transform.localPosition = Vector3.zero;
        SpawnedEnemy.GetComponent<EnemyCombatController>().CombatCore = this;
        SpawnedEnemy.GetComponent<EnemyCombatController>().ThisEnemyData = thisEnemyData;
        SpawnedEnemy.GetComponent<EnemyCombatController>().PlayerTarget = SpawnedPlayer;
        SpawnedEnemy.GetComponent<EnemyCombatController>().OriginalEnemyPosition = enemyOriginalPosition;
        SpawnedEnemy.GetComponent<EnemyCombatController>().EnemyAttackPoint = enemyAttackPoint;
        SpawnedEnemy.GetComponent<EnemyCombatController>().EnemyTransformParent = enemyContainer;
    }

    public IEnumerator ProcessEnemyDeath(GameObject deadEnemy)
    {
        Destroy(deadEnemy);

        if (enemyDataQueue.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            questionCore.StopQuestionTimer();
            SpawnNextEnemy();
            GameManager.Instance.CombatData.CurrentCombatState = CombatData.CombatState.TIMER;
        }
        else
        {
            questionCore.StopQuestionTimer();
            GameManager.Instance.CombatData.CurrentCombatState = CombatData.CombatState.GAMEOVER;
        }
    }*/
    #endregion

    #region UTILITY
    /*public void OpenSettings()
    {
        GameManager.Instance.SettingsPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
    }

    public void OpenLoadingPanel(string _message)
    {
        LoadingPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
        LoadingTMP.text = _message;
    }

    public void CloseLoadingPanel()
    {
        LoadingPanel.SetActive(false);
        GameManager.Instance.PanelActivated = false;
    }*/
    #endregion
}
