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
    [field: SerializeField] public PlayerData PlayerData { get; set; }
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
    [field: SerializeField] [field: ReadOnly] public int MonstersKilled { get; set; }
    [field: SerializeField] public SpriteRenderer MissedSprite { get; set; }

    [field: Header("POWER UPS")]
    [field: SerializeField] public Button DoubleDamageBtn { get; set; }
    [field: SerializeField] public GameObject DoubleDamageImage { get; set; }
    [field: SerializeField] public TextMeshProUGUI DoubleDamageTMP { get; set; }
    [field: SerializeField] public Button ShieldBtn { get; set; }
    [field: SerializeField] public GameObject ShieldImage { get; set; }
    [field: SerializeField] public TextMeshProUGUI ShieldTMP { get; set; }
    [field: SerializeField] public Button WarpBtn { get; set; }
    [field: SerializeField] public GameObject Portal { get; set; }
    [field: SerializeField] public Transform PortalEndPoint { get; set; }
    [field: SerializeField] public Animator FlashAnimator { get; set; }
    [field: SerializeField] public int MonstersSkipped { get; set; }

    [field: Header("SKILLS")]
    [field: SerializeField] public Button HealBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI HealChargesTMP { get; set; }
    [field: SerializeField] public Button BreakRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI BreakChargesTMP { get; set; }
    [field: SerializeField] public Button WeakRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI WeakChargesTMP { get; set; }
    [field: SerializeField] public Button FreezeRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI FreezeChargesTMP { get; set; }
    [field: SerializeField] public Button ParalyzeRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI ParalyzeChargesTMP { get; set; }
    [field: SerializeField] public Button ConfuseRemoveBtn { get; set; }
    [field: SerializeField] public TextMeshProUGUI ConfuseChargesTMP { get; set; }

    [field: Header("LOCAL PLAYER DATA")]
    [field: SerializeField] public CharacterCombatController SpawnedPlayer { get; set; }

    [field: Header("LOCAL ENEMY DATA")]
    [field: SerializeField][field: ReadOnly] public EnemyCombatController CurrentEnemy { get; set; }
    [field: SerializeField] public List<GameObject> Enemies { get; set; }
    [field: SerializeField][field: ReadOnly] public Queue<GameObject> EnemyQueue { get; set; }
    [field: SerializeField] public GameObject EnemyProjectile { get; set; }
    [field: SerializeField] public Vector3 EnemyProjectileStartingPoint { get; set; }
    [field: SerializeField] public GameObject EnemyLaser { get; set; }
    [field: SerializeField] public Vector3 EnemyLaserStartingPoint { get; set; }

    [field: Header("GAME OVER")]
    [field: SerializeField] public TextMeshProUGUI MonstersKilledTMP { get; set; }
    [field: SerializeField] private TextMeshProUGUI RewardTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int NormalFragmentDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI NormalFragmentTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int RareFragmentDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI RareFragmentTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int EpicFragmentDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI EpicFragmentTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int LegendFragmentDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI LegendFragmentTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int HealSkillDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI HealGainedTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int BreakRemoveDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI BreakGainedTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int WeakRemoveDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI WeakGainedTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int FreezeRemoveDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI FreezeGainedTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int ParalyzeRemoveDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI ParalyzeGainedTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int ConfuseRemoveDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI ConfuseGainedTMP { get; set; }
    [field: SerializeField][field: ReadOnly] private int BurnRemoveDropped { get; set; }
    [field: SerializeField] private TextMeshProUGUI BurnGainedTMP { get; set; }
    [field: SerializeField] private List<CustomCostumeData> CostumeRoster { get; set; }
    [field: SerializeField] private Image DroppedCostume { get; set; }

    [field: Header("SETTINGS")]
    [field: SerializeField] private GameObject SettingsPanel { get; set; }


    [Header("DEBUGGER")]
    public Coroutine timerCoroutine;
    //=================================================================================

    #region SPAWNING
    public void SpawnEnemies()
    {
        StageCounter = 0;
        StageTMP.text = StageCounter.ToString();
        RoundCounter = 0;
        RoundTMP.text = "Round 1";
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
            CurrentEnemy = EnemyQueue.Dequeue().transform.GetChild(0).GetComponent<EnemyCombatController>();
            SpawnedPlayer.ShotAccuracy = PlayerData.ActiveCustomWeapon.BaseWeaponData.Accuracy - CurrentEnemy.EvasionValue;
            CurrentEnemy.InitializeEnemy();
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
            if(!GameManager.Instance.PanelActivated)
            {
                CurrentCountdownNumber -= Time.deltaTime;
                QuestionTimerLeft = (int)CurrentCountdownNumber;
                TimerTMP.text = QuestionTimerLeft.ToString();
            }
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
        /*for (int i = 0; i < PlayerData.CurrentStage; i++)
        {
            StageCounter++;
            StageTMP.text = StageCounter.ToString();
            CurrentEnemy = EnemyQueue.Dequeue().transform.GetChild(0).GetComponent<EnemyCombatController>();
            SpawnedPlayer.ShotAccuracy = PlayerData.ActiveWeapon.Accuracy - CurrentEnemy.EvasionValue;
            if(i < PlayerData.CurrentStage - 1)
                CurrentEnemy.gameObject.transform.parent.position = new Vector3(Enemies[Enemies.Count - 1].transform.position.x + 10 + (10 * i), CurrentEnemy.gameObject.transform.parent.position.y, CurrentEnemy.gameObject.transform.parent.position.z);
        }
        CurrentEnemy.InitializeEnemy();

        for (int i = 0; i < Enemies.Count; i++)
        {
            Enemies[i].transform.position = new Vector3(Enemies[i].transform.position.x - (10 * (PlayerData.CurrentStage - 1)), Enemies[i].transform.position.y, Enemies[i].transform.position.z);
        }*/

        StageCounter++;
        StageTMP.text = StageCounter.ToString();
        CurrentEnemy = EnemyQueue.Dequeue().transform.GetChild(0).GetComponent<EnemyCombatController>();
        SpawnedPlayer.ShotAccuracy = PlayerData.ActiveCustomWeapon.BaseWeaponData.Accuracy - CurrentEnemy.EvasionValue;
        CurrentEnemy.InitializeEnemy();
    }
    public void WarpToNextEnemy()
    {
        FlashAnimator.SetTrigger("Flash");
        SpawnedPlayer.MonstersToSkip = PlayerData.CurrentStage - StageCounter;
        if(EnemyQueue.Count > SpawnedPlayer.MonstersToSkip)
        {
            CurrentEnemy.IsCurrentEnemy = false;
            CurrentEnemy.transform.position = new Vector3(Enemies[Enemies.Count - 1].transform.position.x + 10, CurrentEnemy.gameObject.transform.position.y, CurrentEnemy.gameObject.transform.position.z);
            for (int i = 0; i < SpawnedPlayer.MonstersToSkip - 1; i++)
            {
                StageCounter++;
                StageTMP.text = StageCounter.ToString();
                CurrentEnemy = EnemyQueue.Dequeue().transform.GetChild(0).GetComponent<EnemyCombatController>();
                CurrentEnemy.transform.position = new Vector3(Enemies[Enemies.Count - 1].transform.position.x + 10 + (10 * (i + 1)), CurrentEnemy.gameObject.transform.position.y, CurrentEnemy.gameObject.transform.position.z);
            }
            StageCounter++;
            StageTMP.text = StageCounter.ToString();
            CurrentEnemy = EnemyQueue.Dequeue().transform.GetChild(0).GetComponent<EnemyCombatController>();
            CurrentEnemy.InitializeEnemy();
            
            for(int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].transform.position = new Vector3(Enemies[i].transform.position.x - (10 * SpawnedPlayer.MonstersToSkip), Enemies[i].transform.position.y, Enemies[i].transform.position.z);
            }
            SpawnedPlayer.CurrentCombatState = CharacterCombatController.CombatState.WALKING;
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
        if (SpawnedPlayer.DoubleDamageTurnsCooldown == 0 && !SpawnedPlayer.DoubleDamageActivated)
            DoubleDamageBtn.interactable = true;
        else
            DoubleDamageBtn.interactable = false;

        if (SpawnedPlayer.ShieldInstancesRemaining > 0 && !SpawnedPlayer.ShieldsActivated)
            ShieldBtn.interactable = true;
        else
            ShieldBtn.interactable = false;

        if (SpawnedPlayer.WarpGunInstancesRemaining > 0 && !SpawnedPlayer.WarpActivated)
            WarpBtn.interactable = true;
        else
            WarpBtn.interactable = false;
    }

    public void ProcessSkillsInteractability()
    {
        if (PlayerData.HealCharges > 0)
        {
            HealChargesTMP.text = PlayerData.HealCharges.ToString();
            HealBtn.interactable = true;
        }
        else
        {
            HealChargesTMP.text = "0";
            HealBtn.interactable = false;
        }

        if (PlayerData.BreakRemovalCharges > 0)
        {
            BreakChargesTMP.text = PlayerData.BreakRemovalCharges.ToString();
            BreakRemoveBtn.interactable = true;
        }
        else
        {
            BreakChargesTMP.text = "0";
            BreakRemoveBtn.interactable = false;
        }

        if (PlayerData.WeakRemovalCharges > 0)
        {
            WeakChargesTMP.text = PlayerData.WeakRemovalCharges.ToString();
            WeakRemoveBtn.interactable = true;
        }
        else
        {
            WeakChargesTMP.text = "0";
            WeakRemoveBtn.interactable = false;
        }

        if (PlayerData.FreezeRemovalCharges > 0)
        {
            FreezeChargesTMP.text = PlayerData.FreezeRemovalCharges.ToString();
            FreezeRemoveBtn.interactable = true;
        }
        else
        {
            FreezeChargesTMP.text = "0";
            FreezeRemoveBtn.interactable = false;
        }

        if (PlayerData.ParalyzeRemovalCharges > 0)
        {
            ParalyzeChargesTMP.text = PlayerData.ParalyzeRemovalCharges.ToString();
            ParalyzeRemoveBtn.interactable = true;
        }
        else
        {
            ParalyzeChargesTMP.text = "0";
            ParalyzeRemoveBtn.interactable = false;
        }

        if (PlayerData.ConfuseRemovalCharges > 0)
        {
            ConfuseChargesTMP.text = PlayerData.ConfuseRemovalCharges.ToString();
            ConfuseRemoveBtn.interactable = true;
        }
        else
        {
            ConfuseChargesTMP.text = "0";
            ConfuseRemoveBtn.interactable = false;
        }
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
        HealSkillDropped = 0;
        BreakRemoveDropped = 0;
        BurnRemoveDropped = 0;
        WeakRemoveDropped = 0;
        ParalyzeRemoveDropped = 0;
        FreezeRemoveDropped = 0;
        ConfuseRemoveDropped = 0;
        #endregion

        #region DROPPERS
        if (StageCounter >= 1 && StageCounter <= 9)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 100).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1,2);

            if (UnityEngine.Random.Range(1, 100) < 5)
                DropRandomSkillItem(1);
        }
        else if (StageCounter >= 10 && StageCounter <= 19)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 200).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 3);

            if (UnityEngine.Random.Range(1, 100) < 5)
                DropRandomSkillItem(1);
        }
        else if (StageCounter >= 20 && StageCounter <= 29)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 300).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 4);

            if (UnityEngine.Random.Range(1, 100) < 5)
                DropRandomSkillItem(2);
        }
        else if (StageCounter >= 30 && StageCounter <= 39)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 400).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 6);

            if (UnityEngine.Random.Range(1, 100) < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 2);

            if (UnityEngine.Random.Range(1, 100) < 10)
                DropRandomSkillItem(2);
        }
        else if (StageCounter >= 40 && StageCounter <= 49)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 500).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 7);

            if (UnityEngine.Random.Range(1, 100) < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 2);

            if (UnityEngine.Random.Range(1, 100) < 10)
                DropRandomSkillItem(2);
        }
        else if (StageCounter >= 50 && StageCounter <= 59)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 600).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 8);

            if (UnityEngine.Random.Range(1, 100) < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 3);

            if (UnityEngine.Random.Range(1, 100) < 15)
                DropRandomSkillItem(3);
        }
        else if (StageCounter >= 60 && StageCounter <= 69)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 700).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 9);

            if (UnityEngine.Random.Range(1, 100) < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 4);

            if (UnityEngine.Random.Range(1, 100) < 25)
                EpicFragmentDropped = 1;

            if (UnityEngine.Random.Range(1, 100) < 15)
                DropRandomSkillItem(3);
        }
        else if (StageCounter >= 70 && StageCounter <= 79)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 800).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 10);

            if (UnityEngine.Random.Range(1, 100) < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 5);

            if (UnityEngine.Random.Range(1, 100) < 25)
                EpicFragmentDropped = UnityEngine.Random.Range(1, 2);

            if (UnityEngine.Random.Range(1, 100) < 15)
                DropRandomSkillItem(4);
        }
        else if (StageCounter >= 80 && StageCounter <= 89)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 900).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 10);

            if (UnityEngine.Random.Range(1, 100) < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 5);

            if (UnityEngine.Random.Range(1, 100) < 25)
                EpicFragmentDropped = UnityEngine.Random.Range(1, 2);

            if (UnityEngine.Random.Range(1, 100) < 15)
                LegendFragmentDropped = 1;

            if (UnityEngine.Random.Range(1, 100) < 15)
                DropRandomSkillItem(5);
        }
        else if (StageCounter >= 90 && StageCounter <= 99)
        {
            RewardTMP.text = UnityEngine.Random.Range(0, 900).ToString();
            NormalFragmentDropped = UnityEngine.Random.Range(1, 10);

            if (UnityEngine.Random.Range(1, 100) < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 5);

            if (UnityEngine.Random.Range(1, 100) < 25)
                EpicFragmentDropped = UnityEngine.Random.Range(1, 2);

            if (UnityEngine.Random.Range(1, 100) < 15)
                LegendFragmentDropped = 1;

            if (UnityEngine.Random.Range(1, 100) < 20)
                DropRandomSkillItem(5);
        }
        else if (StageCounter == 100)
        {
            RewardTMP.text = "1000";
            NormalFragmentDropped = UnityEngine.Random.Range(1, 10);

            if (UnityEngine.Random.Range(1, 100) < 50)
                RareFragmentDropped = UnityEngine.Random.Range(1, 10);

            if (UnityEngine.Random.Range(1, 100) < 25)
                EpicFragmentDropped = UnityEngine.Random.Range(1, 10);

            if (UnityEngine.Random.Range(1, 100) < 15)
                LegendFragmentDropped = UnityEngine.Random.Range(1, 10);

            if (UnityEngine.Random.Range(1, 100) < 20)
                DropRandomSkillItem(10);
        }
        #endregion

        #region DISPLAY
        if (NormalFragmentDropped > 0)
        {
            NormalFragmentTMP.gameObject.SetActive(true);
            NormalFragmentTMP.text = NormalFragmentDropped.ToString();
        }
        else
            NormalFragmentTMP.gameObject.SetActive(false);

        if (RareFragmentDropped > 0)
        {
            RareFragmentTMP.gameObject.SetActive(true);
            RareFragmentTMP.text = RareFragmentDropped.ToString();
        }
        else
            RareFragmentTMP.gameObject.SetActive(false);

        if (EpicFragmentDropped > 0)
        {
            EpicFragmentTMP.gameObject.SetActive(true);
            EpicFragmentTMP.text = EpicFragmentDropped.ToString();
        }
        else
            EpicFragmentTMP.gameObject.SetActive(false);

        if (LegendFragmentDropped > 0)
        {
            LegendFragmentTMP.gameObject.SetActive(true);
            LegendFragmentTMP.text = LegendFragmentDropped.ToString();
        }
        else
            LegendFragmentTMP.gameObject.SetActive(false);

        if (HealSkillDropped > 0)
        {
            HealGainedTMP.gameObject.SetActive(true);
            HealGainedTMP.text = HealSkillDropped.ToString();
        }
        else
            HealGainedTMP.gameObject.SetActive(false);

        if (BreakRemoveDropped > 0)
        {
            BreakGainedTMP.gameObject.SetActive(true);
            BreakGainedTMP.text = BreakRemoveDropped.ToString();
        }
        else
            BreakGainedTMP.gameObject.SetActive(false);

        if (WeakRemoveDropped > 0)
        {
            WeakGainedTMP.gameObject.SetActive(true);
            WeakGainedTMP.text = WeakRemoveDropped.ToString();
        }
        else
            WeakGainedTMP.gameObject.SetActive(false);

        if (FreezeRemoveDropped > 0)
        {
            FreezeGainedTMP.gameObject.SetActive(true);
            FreezeGainedTMP.text = FreezeRemoveDropped.ToString();
        }
        else
            FreezeGainedTMP.gameObject.SetActive(false);

        if (ParalyzeRemoveDropped > 0)
        {
            ParalyzeGainedTMP.gameObject.SetActive(true);
            ParalyzeGainedTMP.text = ParalyzeRemoveDropped.ToString();
        }
        else
            ParalyzeGainedTMP.gameObject.SetActive(false);

        if (ConfuseRemoveDropped > 0)
        {
            ConfuseGainedTMP.gameObject.SetActive(true);
            ConfuseGainedTMP.text = ConfuseRemoveDropped.ToString();
        }
        else
            ConfuseGainedTMP.gameObject.SetActive(false);

        if (BurnRemoveDropped > 0)
        {
            BurnGainedTMP.gameObject.SetActive(true);
            BurnGainedTMP.text = BurnRemoveDropped.ToString();
        }
        else
            BurnGainedTMP.gameObject.SetActive(false);
        #endregion
        DropRandomCostume();
    }

    private void DropRandomSkillItem(int maxSkillDrop)
    {
        for (int i = 0; i < UnityEngine.Random.Range(1, maxSkillDrop); i++)
        {
            switch (UnityEngine.Random.Range(0, 6))
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

    private void DropRandomCostume()
    {
        if(StageCounter < Enemies.Count / 2)
        {
            bool mayGrantCostume = false;
            foreach (CustomCostumeData costume in CostumeRoster)
                if (!costume.CostumeIsOwned)
                {
                    mayGrantCostume = true;
                    break;
                }

            if(mayGrantCostume)
            {
                int randomNum = UnityEngine.Random.Range(0, CostumeRoster.Count);
                while (CostumeRoster[randomNum].CostumeIsOwned)
                    randomNum = UnityEngine.Random.Range(0, CostumeRoster.Count);

                if (UnityEngine.Random.Range(0, 100) >= 1)
                {
                    DroppedCostume.gameObject.SetActive(true);
                    CostumeRoster[randomNum].CostumeIsOwned = true;
                    DroppedCostume.sprite = CostumeRoster[randomNum].BaseCostumeData.DroppedSprite;
                }
                else
                    DroppedCostume.gameObject.SetActive(false);
            }    
        }
    }
    #endregion

    #region UTILITY
    public void DisplaySettings()
    {
        SettingsPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
    }

    public void HideSettings()
    {
        SettingsPanel.SetActive(false);
        GameManager.Instance.PanelActivated = false;
    }

    public void OpenLobbyScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
    }
    /*public void OpenLoadingPanel(string _message)
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
