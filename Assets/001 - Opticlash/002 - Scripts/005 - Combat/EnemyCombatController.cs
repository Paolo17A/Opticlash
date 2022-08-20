using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

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
        Debug.Log("denemy combat controller enabled");
    }

    private void OnDisable()
    {
        onPlayerCombatStateChange -= CombatStateChange;
    }
    #endregion
    #region VARIABLES
    //========================================================================================
    public enum SideEffect { NONE, BREAK, PIERCE, WEAK, PARALYZE, CONFUSE, BURN, FREEZE }
    public enum AttackType { NONE, MELEE, RANGED }
    public enum MonsterPlacement { NONE, HIGH, LOW }

    [SerializeField] [ReadOnly] private CombatState currentCombatState;
    [field: SerializeField] private CombatCore CombatCore { get; set; }
    [field: SerializeField] private SettingsData SettingsData { get; set; }

    [field: Header("ENEMY DATA")]
    [field: SerializeField] public string MonsterID { get; set; }
    [field: SerializeField] private Animator EnemyAnim { get; set; }
    [field: SerializeField] public AttackType EnemyAttackType { get; set; }
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }
    [field: SerializeField] public MonsterPlacement ThisMonsterPlacement { get; set; }
    [field: SerializeField] [field: ReadOnly] public bool MayAttack { get; set; }
    [field: SerializeField] public bool IsBoss { get; set; }
    [field: SerializeField] public GameObject SpecificProjectile { get; set; }
    [field: SerializeField] private Vector3 ProjectileStartingPoint { get; set; }
    [field: SerializeField] private AudioSource MonsterAudioSource { get; set; }

    [field: Header("SOUND EFFECTS")]
    [field: SerializeField] private AudioClip HitSFX { get; set; }
    [field: SerializeField] private List<AudioClip> DeathSFX { get; set; }

    [field: Header("STATS")]
    [field: SerializeField] public int MonsterLevel {get;set;}
    [field: SerializeField] [field: ReadOnly] public float CurrentHealth { get; set; }
    [field: SerializeField] [field: ReadOnly] public float MaxHealth { get; set; }
    [field: SerializeField] [field: ReadOnly] public float DamageDeal { get; set; }
    [field: SerializeField] [field: ReadOnly] public float EvasionValue { get; set; }
    [field: SerializeField] [field: ReadOnly] public float Attack { get; set; }
    [field: SerializeField] [field: ReadOnly] public float Accuracy { get; set; }
    [field: SerializeField] [field: ReadOnly] public float Defense { get; set; }

    [field: Header("KABOOM")]
    [field: SerializeField] public GameObject NormalKaboom { get; set; }
    [field: SerializeField] public GameObject BurnKaboom { get; set; }
    [field: SerializeField] public GameObject BreakKaboom { get; set; }
    [field: SerializeField] public GameObject ConfuseKaboom { get; set; }
    [field: SerializeField] public GameObject FreezeKaboom { get; set; }
    [field: SerializeField] public GameObject ParalyzeKaboom { get; set; }
    [field: SerializeField] public GameObject WeakKaboom { get; set; }

    [field: Header("AFFLICTED SIDE EFFECT")]
    [field: SerializeField] public SpriteRenderer StatusEffectImage { get; set; }
    [field: SerializeField] private SpriteRenderer StatusEffectText { get; set; }
    [field: SerializeField] public Animator StatusEffectTextAnimator { get; set; }
    [field: SerializeField][field: ReadOnly] public WeaponData.SideEffect AfflictedSideEffect { get; set; }
    [field: SerializeField][field: ReadOnly] public float AfflictedSideEffectDamage { get; set; }
    [field: SerializeField][field: ReadOnly] public int AfflictedSideEffectInstancesLeft { get; set; }

    [field: Header("PASSIVE SIDE EFFECT")]
    [field: SerializeField] public SideEffect ThisSideEffect { get; set; }
    [field: SerializeField] private int SideEffectRate { get; set; }
    [field: SerializeField] public int SideEffectDuration { get; set; }
    [field: SerializeField] public float SideEffectDamage { get; set; }

    [field: Header("SIDE EFFECT ROSTER")]
    [field: SerializeField] private Sprite BreakLogoSprite { get; set; }
    [field: SerializeField] private Sprite BreakTextSprite { get; set; }
    [field: SerializeField] private Sprite BurnLogoSprite { get; set; }
    [field: SerializeField] private Sprite BurnTextSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseLogoSprite { get; set; }
    [field: SerializeField] private Sprite ConfuseTextSprite { get; set; }
    [field: SerializeField] private Sprite FreezeLogoSprite { get; set; }
    [field: SerializeField] private Sprite FreezeTextSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeLogoSprite { get; set; }
    [field: SerializeField] private Sprite ParalyzeTextSprite { get; set; }
    [field: SerializeField] private Sprite WeakLogoSprite { get; set; }
    [field: SerializeField] private Sprite WeakTextSprite { get; set; }

    [field: Header("LASER ABILITY")]
    [field: SerializeField] private bool CanShootLaser { get; set; }
    [field: SerializeField] private int LaserFrequency { get; set; }    

    [field: Header("TRANSFORMS")]
    [field: SerializeField] public Vector3 OriginalEnemyPosition { get; set; }
    [field: SerializeField] private Vector3 EnemyAttackPosition { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool DoneAttacking { get; set; }
    [field: SerializeField][field: ReadOnly] public bool IsCurrentEnemy { get; set; }
    [field:SerializeField][field: ReadOnly] private bool ShootingLaser { get; set; }
    [field: SerializeField][field: ReadOnly] private CharacterCombatController Opti { get; set; }
    private int failedCallbackCounter;
    //========================================================================================
    #endregion

    private void Start()
    {
        Opti = CombatCore.SpawnedPlayer;
    }

    public void InitializeEnemy()
    {
        ResetHealthBar();
        if(IsBoss)
        {
            Attack = (5 * (MonsterLevel * 0.5f)) + 10;
            Accuracy = (5 * (MonsterLevel * 0.5f)) + 10;
            Defense = (5 * (MonsterLevel * 0.5f)) + 10;
            EvasionValue = (5 * (MonsterLevel * 0.5f)) + 10;
        }
        else
        {
            Attack = (5 * (MonsterLevel * 0.3f)) + 10;
            Accuracy = (5 * (MonsterLevel * 0.3f)) + 10;
            Defense = (5 * (MonsterLevel * 0.3f)) + 10;
            EvasionValue = (5 * (MonsterLevel * 0.1f)) + 10;
        }
        MaxHealth = 5 * ((Attack * 0.5f) + (Defense * 0.3f) + (EvasionValue * 0.1f) + (Accuracy * 0.1f));
        CurrentHealth = MaxHealth;
        DamageDeal = ((Attack * Attack) / (CombatCore.SpawnedPlayer.Attack + CombatCore.SpawnedPlayer.Defense)) / 1.2f;
        HealthSlider.transform.localScale = new Vector3((float)CurrentHealth / MaxHealth, 1f, 0f);
        HealthSlider.transform.localPosition = new Vector3(0f, 0f, 10f);
        CurrentCombatState = CombatState.IDLE;
        IsCurrentEnemy = true;
    }

    private void FixedUpdate()
    {
        HealthBar.transform.rotation = Quaternion.identity;
        if(CombatCore.CurrentCombatState == CombatCore.CombatState.ENEMYTURN && CurrentCombatState == CombatState.IDLE && IsCurrentEnemy && MayAttack)
        {
            if (EnemyAttackType == AttackType.MELEE)
                ProcessMeleeAttack();
            else if (EnemyAttackType == AttackType.RANGED)
                ProcessRangedAttack();
        }
        else if (CombatCore.CurrentCombatState == CombatCore.CombatState.WALKING)
        {
            CombatCore.MonsterParent.transform.position = Vector3.MoveTowards(CombatCore.MonsterParent.transform.position, CombatCore.CurrentEnemy.OriginalEnemyPosition, 2.5f * Time.deltaTime);
            if (Vector2.Distance(CombatCore.CurrentEnemy.gameObject.transform.parent.position, CombatCore.CurrentEnemy.OriginalEnemyPosition) < 0.01f)
            {
                CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                Opti.CurrentCombatState = CharacterCombatController.CombatState.IDLE;
                
            }
        }
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        //Debug.Log("Current enemy state: " + CurrentCombatState);
        EnemyAnim.SetInteger("index", (int)CurrentCombatState);

        if (CurrentCombatState == CombatState.DYING)
        {
            CombatCore.PlayerData.MonstersKilled++;
            if (!GameManager.Instance.DebugMode)
            {
                UpdateQuestData();
                if (IsBoss)
                    GetStatistics();
            }
        }
    }

    private void ProcessMeleeAttack()
    {
        if (DoneAttacking)
        {
            if (Vector2.Distance(transform.parent.position, OriginalEnemyPosition) > 0.01f)
                transform.parent.position = Vector2.MoveTowards(transform.parent.position, OriginalEnemyPosition, 13 * Time.deltaTime);
            else
            {
                MayAttack = false; 
                if (AfflictedSideEffect == WeaponData.SideEffect.BURN)
                {
                    //StatusEffectActivated = true;
                    Debug.Log("intaking burn damage");
                    CurrentHealth -= SideEffectDamage;
                    UpdateHealthBar();
                    StatusEffectTextAnimator.SetTrigger("ShowStatus");
                    if (CurrentHealth <= 0)
                        CurrentCombatState = CombatState.DYING;
                    else
                        CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
                }
                else
                    CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
            }
        }
        else
        {
            if (Vector2.Distance(transform.parent.position, EnemyAttackPosition) > 0.01f)
                transform.parent.position = Vector2.MoveTowards(transform.parent.position, EnemyAttackPosition, 13 * Time.deltaTime);
            else
                CurrentCombatState = CombatState.ATTACKING;
        }
    }

    private void ProcessRangedAttack()
    {
        if (DoneAttacking)
        {
            MayAttack = false; 
            if (AfflictedSideEffect == WeaponData.SideEffect.BURN)
            {
                //StatusEffectActivated = true;
                Debug.Log("intaking burn damage");
                CurrentHealth -= SideEffectDamage;
                UpdateHealthBar();
                StatusEffectTextAnimator.SetTrigger("ShowStatus");
                if (CurrentHealth <= 0)
                    CurrentCombatState = CombatState.DYING;
                else
                    CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
            }
            else
                CombatCore.CurrentCombatState = CombatCore.CombatState.TIMER;
        }
        else
        {
            if (!CanShootLaser && Vector2.Distance(CombatCore.EnemyProjectile.transform.position, CombatCore.SpawnedPlayer.gameObject.transform.position) > 0.01f)
                CombatCore.EnemyProjectile.transform.position = Vector2.MoveTowards(CombatCore.EnemyProjectile.transform.position, CombatCore.SpawnedPlayer.gameObject.transform.position, 15 * Time.deltaTime);
            else if (CanShootLaser && Vector2.Distance(CombatCore.EnemyProjectile.transform.position, EnemyAttackPosition) > 0.01f)
                CombatCore.EnemyProjectile.transform.position = Vector2.MoveTowards(CombatCore.EnemyProjectile.transform.position, EnemyAttackPosition, 0.5f * Time.deltaTime);

            else
            {
                Debug.Log("enemy should be done attacking");
                CombatCore.EnemyProjectile.SetActive(false);
                //CombatCore.EnemyLaser.SetActive(false);
                SpecificProjectile.SetActive(false);
                DoneAttacking = true;
                if (AfflictedSideEffect == WeaponData.SideEffect.WEAK)
                    Opti.TakeDamageFromEnemy(DamageDeal / 2);
                else
                    Opti.TakeDamageFromEnemy(DamageDeal);
                /*if (ShootingLaser)
                {
                    Opti.TakeDamageFromEnemy(DamageDeal * 3);
                }
                else
                {
                    if (AfflictedSideEffect == WeaponData.SideEffect.WEAK)
                        Opti.TakeDamageFromEnemy(DamageDeal / 2);
                    else
                        Opti.TakeDamageFromEnemy(DamageDeal);
                }*/

            }
        }
    }

    #region SIDE EFFECTS
    public void SetBreakEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = BreakLogoSprite;
        StatusEffectText.sprite = BreakTextSprite;
    }

    public void SetBurnEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = BurnLogoSprite;
        StatusEffectText.sprite = BurnTextSprite;
        SideEffectDamage = CombatCore.PlayerData.ActiveCustomWeapon.BaseWeaponData.BaseDamage / 3;
    }
    public void SetConfuseEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = ConfuseLogoSprite;
        StatusEffectText.sprite = ConfuseTextSprite;
    }
    public void SetFreezeEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = FreezeLogoSprite;
        StatusEffectText.sprite = FreezeTextSprite;
    }
    public void SetParalyzeEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = ParalyzeLogoSprite;
        StatusEffectText.sprite = ParalyzeTextSprite;
    }
    public void SetWeakEffect()
    {
        StatusEffectImage.gameObject.SetActive(true);
        StatusEffectImage.sprite = WeakLogoSprite;
        StatusEffectText.sprite = WeakTextSprite;
    }

    public void ActivateSideEffect()
    {

    }
    #endregion

    #region ANIMATION EVENTS
    private IEnumerator ShowDamageSprite(int _damage)
    {
        CombatCore.EnemyTakenDamage.text = "-" + _damage.ToString();
        CombatCore.EnemyTakenDamage.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);
        CombatCore.EnemyTakenDamage.gameObject.SetActive(false);
    }

    private IEnumerator DelayKaboom()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        NormalKaboom.SetActive(false);
        BurnKaboom.SetActive(false);
        BreakKaboom.SetActive(false);
        ConfuseKaboom.SetActive(false);
        FreezeKaboom.SetActive(false);
        ParalyzeKaboom.SetActive(false);
        WeakKaboom.SetActive(false);
    }

    public void AttackPlayer()
    {
        if (WillInflictStatusEffect())
        {
            switch (ThisSideEffect)
            {
                case SideEffect.BREAK:
                    BreakKaboom.SetActive(true);
                    break;
                case SideEffect.BURN:
                    BurnKaboom.SetActive(true);
                    break;
                case SideEffect.FREEZE:
                    FreezeKaboom.SetActive(true);
                    break;
                case SideEffect.PARALYZE:
                    ParalyzeKaboom.SetActive(true);
                    break;
                case SideEffect.WEAK:
                    WeakKaboom.SetActive(true);
                    break;
                case SideEffect.CONFUSE:
                    ConfuseKaboom.SetActive(true);
                    break;
            }
        }
        else
            NormalKaboom.SetActive(true);
        StartCoroutine(DelayKaboom());

        if (EnemyAttackType == AttackType.MELEE)
        {
            if (AfflictedSideEffect == WeaponData.SideEffect.WEAK)
                Opti.TakeDamageFromEnemy(DamageDeal / 2);
            else
                Opti.TakeDamageFromEnemy(DamageDeal);
        }
        else if (EnemyAttackType == AttackType.RANGED)
        {
            DoneAttacking = false;
            CombatCore.EnemyProjectile.transform.position = CombatCore.EnemyProjectileStartingPoint;
            CombatCore.EnemyProjectile.transform.position = ProjectileStartingPoint;
            CombatCore.EnemyProjectile.SetActive(true);
            SpecificProjectile.SetActive(true);
        }
    }

    public void ReturnToStarting()
    {
        CurrentCombatState = CombatState.IDLE;
        ProcessAfflictedSideEffect();
        if(EnemyAttackType == AttackType.MELEE)
            DoneAttacking = true;
    }

    // Process health is only called AFTER getting hit
    public void ProcessHealth()
    {
        //Debug.Log("entered here");
        if (CurrentHealth > 0)
        {
            CurrentCombatState = CombatState.IDLE;
            Opti.ProcessEndAttack();
        }
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
            if(CombatCore.SpawnedPlayer.CurrentSideEffect != SideEffect.NONE)
                CombatCore.SpawnedPlayer.ProcessStatusEffectInstances();

            MonsterAudioSource.volume = SettingsData.EffectsVolume;
            MonsterAudioSource.clip = DeathSFX[UnityEngine.Random.Range(0, DeathSFX.Count)];
            MonsterAudioSource.Play();
            CombatCore.MonstersKilled++;
            IsCurrentEnemy = false;
            AfflictedSideEffect = WeaponData.SideEffect.NONE;
            SideEffectDamage = 0;
            StatusEffectImage.gameObject.SetActive(false);
            ResetHealthBar();
            gameObject.SetActive(false);
            CombatCore.MonsterParent.transform.position = new Vector3(15, 21, 0);
            CombatCore.SpawnNextEnemy();
            if(CombatCore.CurrentCombatState != CombatCore.CombatState.GAMEOVER)
                CombatCore.CurrentCombatState = CombatCore.CombatState.WALKING;
        }
    }
    #endregion

    #region DAMAGE
    public void TakeDamageFromPlayer(float _damageReceived)
    {
        MonsterAudioSource.volume = SettingsData.EffectsVolume;
        MonsterAudioSource.clip = HitSFX;
        MonsterAudioSource.Play();
        CurrentHealth -= _damageReceived;
        //StartCoroutine(ShowDamageSprite(Mathf.CeilToInt(_damageReceived)));
        CombatCore.EnemyTakenDamage.text = "-" + Mathf.CeilToInt(_damageReceived).ToString();
        CombatCore.EnemyTakenDamageAnimator.SetTrigger("ShowStatus");
        UpdateHealthBar();
        CurrentCombatState = CombatState.ATTACKED;
    }

    public void TakeDamageFromSelf()
    {
        CurrentHealth -= DamageDeal;
        UpdateHealthBar();
        if (CurrentHealth > 0)
        {
            CurrentCombatState = CombatState.IDLE;
        }
        else
        {
            CurrentCombatState = CombatState.DYING;
            HealthBar.SetActive(false);
        }
        //CurrentCombatState = CombatState.ATTACKED;
    }

    private void ResetHealthBar()
    {
        HealthBar.SetActive(true);
        HealthSlider.transform.localScale = new Vector3(1f, 1f, 0f);
    }
    #endregion

    #region PLAYFAB API
    private void UpdateQuestData()
    {
        Dictionary<string, int> quests = new Dictionary<string, int>();
        quests.Add("DailyCheckIn", CombatCore.PlayerData.DailyCheckIn);
        quests.Add("SocMedShared", CombatCore.PlayerData.SocMedShared);
        quests.Add("ItemsUsed", CombatCore.PlayerData.ItemsUsed);
        quests.Add("MonstersKilled", CombatCore.PlayerData.MonstersKilled);
        quests.Add("LevelsWon", CombatCore.PlayerData.LevelsWon);
        quests.Add("DailyQuestClaimed", CombatCore.PlayerData.DailyQuestClaimed);

        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
        updateUserData.Data.Add("Quests", JsonConvert.SerializeObject(quests));

        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                Debug.Log("Quest data has been updated");
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    UpdateQuestData,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void GetStatistics()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
            resultCallback =>
            {
                foreach (StatisticValue stat in resultCallback.Statistics)
                    if (stat.StatisticName == "TotalKillCount")
                    {
                        UpdateStatistics(stat.Value);
                        break;
                    }

            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    GetStatistics,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    private void UpdateStatistics(int _stat)
    {
        StatisticUpdate statisticUpdate = new StatisticUpdate();
        statisticUpdate.StatisticName = "TotalKillCount";
        statisticUpdate.Value = _stat + 1;

        UpdatePlayerStatisticsRequest updatePlayerStatistics = new UpdatePlayerStatisticsRequest();
        updatePlayerStatistics.Statistics = new List<StatisticUpdate>();
        updatePlayerStatistics.Statistics.Add(statisticUpdate);

        PlayFabClientAPI.UpdatePlayerStatistics(updatePlayerStatistics,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                Debug.Log("boss kill count increased");
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => UpdateStatistics(_stat),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }
    #endregion

    #region UTILITY
    private void UpdateHealthBar()
    {
        if (CurrentHealth > 0)
        {
            HealthSlider.transform.localScale = new Vector3((float)CurrentHealth / MaxHealth, 1f, 0f);
            HealthSlider.transform.localPosition = new Vector3(HealthSlider.transform.localScale.x - 1, HealthSlider.transform.localPosition.y, HealthSlider.transform.localPosition.z);
        }
        else
            HealthBar.SetActive(false);
    }
    public bool WillInflictStatusEffect()
    {
        if (ThisSideEffect == SideEffect.NONE)
            return false;

        if (CombatCore.RoundCounter % SideEffectRate != 0)
            return false;

        if (CombatCore.SpawnedPlayer.CurrentSideEffect != SideEffect.NONE)
            return false;

        if (AfflictedSideEffect == WeaponData.SideEffect.BREAK)
            return false;

        if (CombatCore.PlayerData.ActiveCostume != null && CombatCore.PlayerData.ActiveCostume.ProvidedImmunity == ThisSideEffect)
            return false;

        return true;
    }

    public void ProcessAfflictedSideEffect()
    {
        if(AfflictedSideEffect != WeaponData.SideEffect.NONE && AfflictedSideEffectInstancesLeft > 0)
        {
            
            AfflictedSideEffectInstancesLeft--;
            if (AfflictedSideEffectInstancesLeft == 0)
            {
                AfflictedSideEffect = WeaponData.SideEffect.NONE;
                SideEffectDamage = 0;
                StatusEffectImage.gameObject.SetActive(false);
            }
        }
    }

    public void ProcessAttackType()
    {
        if (EnemyAttackType == AttackType.MELEE)
            DoneAttacking = false;
        else if (EnemyAttackType == AttackType.RANGED)
            CurrentCombatState = CombatState.ATTACKING;
    }

    private void ErrorCallback(PlayFabErrorCode errorCode, Action restartAction, Action errorAction)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                ProcessError("Connectivity error. Please connect to strong internet");
            else
                restartAction();
        }
        else
            errorAction();
    }

    private void ProcessError(string errorMessage)
    {
        //HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }
    #endregion
}