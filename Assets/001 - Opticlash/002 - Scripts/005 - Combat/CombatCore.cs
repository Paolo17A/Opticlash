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
        WALKING,
        WARPING
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
    [field: SerializeField][field: ReadOnly] public int AmmoCount { get; set; }
    [field: SerializeField][field: ReadOnly] public int RoundCounter { get; set; }
    [field: SerializeField] public TextMeshProUGUI RoundTMP { get; set; }
    [field: SerializeField][field: ReadOnly] public int StageCounter { get; set; }
    [field: SerializeField] public TextMeshProUGUI StageTMP { get; set; }

    [field: Header("POWER UPS")]
    [field: SerializeField] public Button DoubleDamageBtn { get; set; }
    [field: SerializeField] public Button ShieldBtn { get; set; }
    [field: SerializeField] public Button WarpBtn { get; set; }
    [field: SerializeField] public GameObject Portal { get; set; }
    [field: SerializeField] public Transform PortalEndPoint { get; set; }
    [field: SerializeField] public Animator FlashAnimator { get; set; }
    [field: SerializeField] public int MonstersSkipped { get; set; }

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
    [field: SerializeField] public GameObject EnemyProjectile { get; set; }
    [field: SerializeField] public Vector3 EnemyProjectileStartingPoint { get; set; }
    [field: SerializeField] public GameObject EnemyLaser { get; set; }
    [field: SerializeField] public Vector3 EnemyLaserStartingPoint { get; set; }

    [field: Header("GAME OVER")]
    [field: SerializeField] private TextMeshProUGUI RewardTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int NormalFragmentDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int RareFragmentDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int EpicFragmentDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int LegendFragmentDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int HealSkillDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int BreakRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int WeakRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int FreezeRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int ParalyzeRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int ConfuseRemoveDropped { get; set; }
    [field: SerializeField][field: ReadOnly] private int BurnRemoveDropped { get; set; }
    [Header("DEBUGGER")]
    public Coroutine timerCoroutine;
    private int randomDropper;
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
            PlayerData.CurrentStage++;
            StageCounter++;
            StageTMP.text = StageCounter.ToString();
            CurrentEnemy = EnemyQueue.Dequeue();
            SpawnedPlayer.GetComponent<CharacterCombatController>().ShotAccuracy = PlayerData.ActiveWeapon.Accuracy - CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().EvasionValue;
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

    #region WARPING
    public void SetCorrectStage()
    {
        for (int i = 0; i < PlayerData.CurrentStage; i++)
        {
            StageCounter++;
            StageTMP.text = StageCounter.ToString();
            CurrentEnemy = EnemyQueue.Dequeue();
            SpawnedPlayer.GetComponent<CharacterCombatController>().ShotAccuracy = PlayerData.ActiveWeapon.Accuracy - CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().EvasionValue;
            if(i < PlayerData.CurrentStage - 1)
                CurrentEnemy.transform.position = new Vector3(Enemies[Enemies.Count - 1].transform.position.x + 10 + (10 * i), CurrentEnemy.transform.position.y, CurrentEnemy.transform.position.z);
        }
        CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().InitializeEnemy();

        for (int i = 0; i < Enemies.Count; i++)
        {
            Enemies[i].transform.position = new Vector3(Enemies[i].transform.position.x - (10 * (PlayerData.CurrentStage - 1)), Enemies[i].transform.position.y, Enemies[i].transform.position.z);
        }
    }
    public void WarpToNextEnemy()
    {
        FlashAnimator.SetTrigger("Flash");
        if(EnemyQueue.Count > SpawnedPlayer.GetComponent<CharacterCombatController>().MonstersToSkip)
        {
            CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().IsCurrentEnemy = false;
            CurrentEnemy.transform.position = new Vector3(Enemies[Enemies.Count - 1].transform.position.x + 10, CurrentEnemy.transform.position.y, CurrentEnemy.transform.position.z);
            for (int i = 0; i < SpawnedPlayer.GetComponent<CharacterCombatController>().MonstersToSkip - 1; i++)
            {
                StageCounter++;
                StageTMP.text = StageCounter.ToString();
                CurrentEnemy = EnemyQueue.Dequeue();
                CurrentEnemy.transform.position = new Vector3(Enemies[Enemies.Count - 1].transform.position.x + 10 + (10 * (i + 1)), CurrentEnemy.transform.position.y, CurrentEnemy.transform.position.z);
            }
            StageCounter++;
            StageTMP.text = StageCounter.ToString();
            CurrentEnemy = EnemyQueue.Dequeue();
            CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().InitializeEnemy();
            
            for(int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].transform.position = new Vector3(Enemies[i].transform.position.x - (10 * SpawnedPlayer.GetComponent<CharacterCombatController>().MonstersToSkip), Enemies[i].transform.position.y, Enemies[i].transform.position.z);
            }
            SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentCombatState = CharacterCombatController.CombatState.WALKING;
            CurrentCombatState = CombatState.WALKING;
        }
        else
        {
            CurrentCombatState = CombatState.GAMEOVER;
        }
        
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

        if (SpawnedPlayer.GetComponent<CharacterCombatController>().WarpGunInstancesRemaining > 0 && !SpawnedPlayer.GetComponent<CharacterCombatController>().WarpActivated)
            WarpBtn.interactable = true;
        else
            WarpBtn.interactable = false;
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

    #region GAME OVER
    public void GrantRewardedItems()
    {
        #region RESET
        NormalFragmentDropped = 0;
        RareFragmentDropped = 0;
        EpicFragmentDropped = 0;
        LegendFragmentDropped = 0;
        #endregion
        #region DROPPERS
        if (StageCounter >= 1 && StageCounter <= 9)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1,2);

            // Random Skills
            randomDropper = UnityEngine.Random.Range(1,100);
            if (randomDropper < 5)
                DropRandomSkillItem(1);
        }
        else if (StageCounter >= 10 && StageCounter <= 19)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 3);

            // Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 5)
                DropRandomSkillItem(1);
        }
        else if (StageCounter >= 20 && StageCounter <= 29)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 4);

            // Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 5)
                DropRandomSkillItem(2);
        }
        else if (StageCounter >= 30 && StageCounter <= 39)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 6);

            // Rare Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 2);

            // Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 10)
                DropRandomSkillItem(2);
        }
        else if (StageCounter >= 40 && StageCounter <= 49)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 7);

            //  Rare Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 2);

            //  Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 10)
                DropRandomSkillItem(2);
        }
        else if (StageCounter >= 50 && StageCounter <= 59)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 8);

            //  Rare Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 3);

            // Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 15)
                DropRandomSkillItem(3);
        }
        else if (StageCounter >= 60 && StageCounter <= 69)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 9);

            //  Rare Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 4);

            //  Epic Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 25)
                EpicFragmentDropped = 1;

            //  Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 15)
                DropRandomSkillItem(3);
        }
        else if (StageCounter >= 70 && StageCounter <= 79)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 10);

            //  Rare Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 5);

            //  Epic Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 25)
                EpicFragmentDropped = UnityEngine.Random.Range(1, 2);

            //  Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 15)
                DropRandomSkillItem(4);
        }
        else if (StageCounter >= 80 && StageCounter <= 89)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 10);

            //  Rare Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 5);

            //  Epic Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 25)
                EpicFragmentDropped = UnityEngine.Random.Range(1, 2);

            //  Legend Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 15)
                LegendFragmentDropped = 1;

            //  Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 15)
                DropRandomSkillItem(5);
        }
        else if (StageCounter >= 90 && StageCounter <= 99)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 10);

            //  Rare Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 5);

            //  Epic Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 25)
                EpicFragmentDropped = UnityEngine.Random.Range(1, 2);

            //  Legend Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 15)
                LegendFragmentDropped = 1;

            //  Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 20)
                DropRandomSkillItem(5);
        }
        else if (StageCounter == 100)
        {
            NormalFragmentDropped = UnityEngine.Random.Range(1, 10);

            //  Rare Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 10);

            //  Epic Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 25)
                EpicFragmentDropped = UnityEngine.Random.Range(1, 10);

            //  Legend Fragments
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 15)
                LegendFragmentDropped = UnityEngine.Random.Range(1, 10);

            //  Random Skills
            randomDropper = UnityEngine.Random.Range(1, 100);
            if (randomDropper < 20)
                DropRandomSkillItem(10);
        }
        #endregion

        RewardTMP.text = "Normal Fragments: " + NormalFragmentDropped + "\n Rare Framents: " + RareFragmentDropped + "\n Epic Fragments: " + EpicFragmentDropped + "\n " +
            "Legendary Fragments: " + LegendFragmentDropped;
    }

    private void DropRandomSkillItem(int maxSkillDrop)
    {
        int skillsToDrop = UnityEngine.Random.Range(1, maxSkillDrop);
        for (int i = 0; i < skillsToDrop; i++)
        {
            int randomSkill = UnityEngine.Random.Range(0, 6);
            switch(randomSkill)
            {
                case 0:
                    HealSkillDropped++;
                    break;
                case 1:
                    BreakRemoveDropped++;
                    break;
                case 2:
                    WeakRemoveDropped++;
                    break;
                case 3:
                    FreezeRemoveDropped++;
                    break;
                case 4:
                    ParalyzeRemoveDropped++;
                    break;
                case 5:
                    ConfuseRemoveDropped++;
                    break;
                case 6:
                    BurnRemoveDropped++;
                    break;
            }
        }
        
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
