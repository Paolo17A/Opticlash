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
    [field: SerializeField] public Animator UIAnimator { get; set; }

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
    [field: SerializeField][field: ReadOnly] public int StageCounter { get; set; }
    [field: SerializeField] public TextMeshProUGUI StageTMP { get; set; }

    [field: Header("POWER UPS")]
    [field: SerializeField] public Button DoubleDamageBtn { get; set; }
    [field: SerializeField] public Button ShieldBtn { get; set; }

    [field: Header("SKILLS")]
    [field: SerializeField] public Button HealBtn { get; set; }
    [field: SerializeField] public Button BreakRemoveBtn { get; set; }
    [field: SerializeField] public Button WeakRemoveBtn { get; set; }
    [field: SerializeField] public Button FreezeRemoveBtn { get; set; }
    [field: SerializeField] public Button ParalyzeRemoveBtn { get; set; }
    [field: SerializeField] public Button ConfuseRemoveBtn { get; set; }

    [field: Header("LOCAL PLAYER DATA")]
    [field: SerializeField] public GameObject SpawnedPlayer { get; set; }

    [field: Header("LOCAL ENEMY DATA")]
    [field: SerializeField][field: ReadOnly] public GameObject CurrentEnemy { get; set; }
    [field: SerializeField] public List<GameObject> Enemies { get; set; }
    [field: SerializeField][field: ReadOnly] public Queue<GameObject> EnemyQueue { get; set; }

    [Header("DEBUGGER")]
    public Coroutine timerCoroutine;
    //=================================================================================

    #region SPAWNING
    public void SpawnEnemies()
    {
        StageCounter = 0;
        StageTMP.text = StageCounter.ToString();
        RoundCounter = 0;
        RoundTMP.text = RoundCounter.ToString();
        float startingPos = 15f;
        EnemyQueue.Clear();
        foreach (GameObject enemy in Enemies)
        {
            enemy.SetActive(true);
            enemy.transform.position = new Vector3(startingPos, enemy.transform.position.y, enemy.transform.position.z);
            startingPos += 10f;
            EnemyQueue.Enqueue(enemy);
        }
    }

    public void SpawnNextEnemy()
    {
        if(EnemyQueue.Count > 0)
        {
            StageCounter++;
            StageTMP.text = StageCounter.ToString();
            CurrentEnemy = EnemyQueue.Dequeue();
            CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().InitializeEnemy();
        }
        else
            CurrentCombatState = CombatState.GAMEOVER;
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

    #region UI
    public void ProcessPowerUpInteractability()
    {
        if (SpawnedPlayer.GetComponent<CharacterCombatController>().DoubleDamageTurnsCooldown == 0 && !SpawnedPlayer.GetComponent<CharacterCombatController>().DoubleDamageActivated)
            DoubleDamageBtn.interactable = true;
        else
            DoubleDamageBtn.interactable = false;

        if (SpawnedPlayer.GetComponent<CharacterCombatController>().ShieldInstancesRemaining > 0 && !SpawnedPlayer.GetComponent<CharacterCombatController>().ShieldsActivated)
            ShieldBtn.interactable = true;
        else
            ShieldBtn.interactable = false;
    }

    public void ProcessSkillsInteractability()
    {
        if (PlayerData.HealCharges > 0)
            HealBtn.interactable = true;
        else
            HealBtn.interactable = false;

        if (PlayerData.BreakRemovalCharges > 0)
            BreakRemoveBtn.interactable = true;
        else
            BreakRemoveBtn.interactable = false;

        if (PlayerData.WeakRemovalCharges > 0)
            WeakRemoveBtn.interactable = true;
        else
            WeakRemoveBtn.interactable = false;

        if (PlayerData.FreezeRemovalCharges > 0)
            FreezeRemoveBtn.interactable = true;
        else
            FreezeRemoveBtn.interactable = false;

        if (PlayerData.ParalyzeRemovalCharges > 0)
            ParalyzeRemoveBtn.interactable = true;
        else
            ParalyzeRemoveBtn.interactable = false;

        if (PlayerData.ConfuseRemovalCharges > 0)
            ConfuseRemoveBtn.interactable = true;
        else
            ConfuseRemoveBtn.interactable = false;
    }

    public void ShowPowerUps()
    {
        if(CurrentCombatState == CombatState.TIMER)
            UIAnimator.SetBool("ShowingPowerUps", true);
    }

    public void ShowItems()
    {
        if (CurrentCombatState == CombatState.TIMER)
            UIAnimator.SetBool("ShowingPowerUps", false);
    }
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
