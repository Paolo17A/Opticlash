using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterCombatController : MonoBehaviour
{
    #region STATE MACHINE
    public enum CombatState
    {
        NONE,
        IDLE,
        ATTACKING,
        ATTACKED,
        WALKING,
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
    //===================================================================================
    [SerializeField][ReadOnly] private CombatState currentCombatState;
    [field: SerializeField] private PlayerData PlayerData;
    [field: SerializeField] private CombatCore CombatCore { get; set; }

    [field: Header("PLAYER DATA")]
    [field: SerializeField] private Animator PlayerAnimator { get; set; }
    [field: SerializeField] private int MaxHealth;
    [field: SerializeField] private GameObject HealthBar { get; set; }
    [field: SerializeField] private GameObject HealthSlider { get; set; }

    [field: Header("WEAPON DATA")]
    [field: SerializeField] private GameObject Cannon { get; set; }
    [field: SerializeField] private SpriteRenderer CannonBackSprite { get; set; }
    [field: SerializeField] private SpriteRenderer CannonMiddleSprite { get; set; }
    [field: SerializeField] private SpriteRenderer CannonFrontSprite { get; set; }

    [Header("DEBUFFS")]
    [SerializeField] [ReadOnly] private int debuffDamage;
    [SerializeField] [ReadOnly] private int debuffInstancesRemaining;

    [field: Header("PROJECTILES")]
    [field: SerializeField] private GameObject Projectile { get; set; }
    [field: SerializeField] private Transform ProjectileStartingPoint { get; set; }
    [field: SerializeField] private Transform ProjectileEndPoint { get; set; }
    [field: SerializeField] [field: ReadOnly] public bool ProjectileSpawned { get; set; }
    //===================================================================================
    private void OnEnable()
    {
        playerCombatStateChange += CombatStateChange;
    }

    private void OnDisable()
    {
        playerCombatStateChange -= CombatStateChange;
    }

    private void Update()
    {
        if(ProjectileSpawned)
        {
            if(Vector2.Distance(Projectile.transform.position, ProjectileEndPoint.position) > 0.01f)
            {
                Projectile.transform.position = Vector2.MoveTowards(Projectile.transform.position, ProjectileEndPoint.position, 11 * Time.deltaTime);
            }
            else
            {
                Projectile.SetActive(false);
                ProjectileSpawned = false;
                CombatCore.CurrentEnemy.GetComponent<EnemyCombatController>().TakeDamageFromPlayer(PlayerData.ActiveWeapon.BaseDamage);
            }
        }
    }

    public void InitializePlayerCannon()
    {
        CannonBackSprite.sprite = PlayerData.ActiveWeapon.BackSprite;
        CannonMiddleSprite.sprite = PlayerData.ActiveWeapon.MiddleSprite;
        CannonFrontSprite.sprite = PlayerData.ActiveWeapon.FrontSprite;
        Projectile.GetComponent<SpriteRenderer>().sprite = PlayerData.ActiveWeapon.Ammo;
        MaxHealth = PlayerData.MaxHealth;
        CurrentCombatState = CombatState.WALKING;
        CombatCore.CurrentCombatState = CombatCore.CombatState.WALKING;
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        PlayerAnimator.SetInteger("index", (int)CurrentCombatState);
        if(CurrentCombatState == CombatState.IDLE || CurrentCombatState == CombatState.WALKING)
        {
            Cannon.SetActive(false);
        }
        if (CurrentCombatState == CombatState.ATTACKING)
        {
            Cannon.SetActive(true);
        }
    }

    public void ProcessHealth()
    {
        if (PlayerData.CurrentHealth > 0)
            CurrentCombatState = CombatState.IDLE;
        else
        {
            PlayerData.CurrentHealth = 0;
            HealthSlider.transform.localScale = new Vector3((float)PlayerData.CurrentHealth / MaxHealth, 1f, 0f);
            CurrentCombatState = CombatState.DYING;
        }
    }

    public void ProcessDeath()
    {
        CombatCore.CurrentCombatState = CombatCore.CombatState.GAMEOVER;
    }

    public void AttackEnemy()
    {
        Debug.Log("Hitting enemy");
        
        Projectile.SetActive(true);
        Projectile.transform.position = ProjectileStartingPoint.position;
        ProjectileSpawned = true;
        PlayerData.AmmoCount--;
        CombatCore.AmmoTMP.text = "Ammo: " + PlayerData.AmmoCount.ToString();
    }

    public void TakeDamageFromEnemy(int _damageReceived)
    {
        if (_damageReceived> 0)
        {
            CurrentCombatState = CombatState.ATTACKED;
            PlayerData.CurrentHealth -= _damageReceived;
            HealthSlider.transform.localScale = new Vector3((float)PlayerData.CurrentHealth / MaxHealth, 1f, 0f);

        }
    }

        public void CharacterCombatStateToIndex(int state)
    {
        switch (state)
        {
            case (int)CombatState.IDLE:
                CurrentCombatState = CombatState.IDLE;
                break;
            case (int)CombatState.ATTACKING:
                CurrentCombatState = CombatState.ATTACKING;
                break;
            case (int)CombatState.ATTACKED:
                CurrentCombatState = CombatState.ATTACKED;
                break;
            case (int)CombatState.WALKING:
                CurrentCombatState = CombatState.WALKING;
                break;
            case (int)CombatState.DYING:
                CurrentCombatState = CombatState.DYING;
                break;
        }
    }

    #region ANIMATION COROUTINES
    /*private IEnumerator AttackEnemyRanged()
    {
        if (!characterPowerupController.UsedOffense && !characterPowerupController.UsedDefense)
        {
            PlayerAnimator.SetBool("isAttacking", true);
            yield return new WaitForSeconds(0.66f);
            PlayerAnimator.SetBool("isAttacking", false);
            projectile = Instantiate(GameManager.Instance.ActiveCharacter.BaseCharacter.regularProjectilePrefab);
        }
        else if (characterPowerupController.UsedOffense && !characterPowerupController.UsedDefense)
        {
            PlayerAnimator.SetBool("isOffense", true);
            yield return new WaitForSeconds(0.66f);
            PlayerAnimator.SetBool("isOffense", false);
            projectile = Instantiate(GameManager.Instance.ActiveCharacter.BaseCharacter.offensiveProjectilePrefab);
        }
        else if (!characterPowerupController.UsedOffense && characterPowerupController.UsedDefense)
        {
            PlayerAnimator.SetBool("isDefense", true);
            yield return new WaitForSeconds(0.66f);
            PlayerAnimator.SetBool("isDefense", false);
            projectile = Instantiate(GameManager.Instance.ActiveCharacter.BaseCharacter.defensiveProjectilePrefab);
        }

        projectile.transform.SetParent(gameObject.transform);
        projectile.transform.localPosition = new Vector3(0.72f, -0.27f, 0f);
        projectile.transform.localScale = new Vector3(1f, 1f, 0f);
        projectileSpawned = true;
    }

    private IEnumerator GetHitByEnemy()
    {
        PlayerAnimator.SetBool("isHit", true);
        HitEffect.SetActive(true);
        if (GameManager.Instance.ActiveCharacter.BaseCharacter.characterID == "defaultCharacter")
            yield return new WaitForSeconds(0.083f);
        else if (GameManager.Instance.ActiveCharacter.BaseCharacter.characterID == "Archer")
            yield return new WaitForSeconds(0.33f);
        HitEffect.SetActive(false);
        PlayerAnimator.SetBool("isHit", false);
    }

    public IEnumerator PlayDebuffDamage()
    {
        anim.SetBool("isHit", true);
        yield return new WaitForSeconds(0.25f);
        anim.SetBool("isHit", false);
    }

    private IEnumerator Die()
    {
        anim.SetBool("isDead", true);
        if (GameManager.Instance.ActiveCharacter.BaseCharacter.characterID == "defaultCharacter")
            yield return new WaitForSeconds(0.45f);
        else if (GameManager.Instance.ActiveCharacter.BaseCharacter.characterID == "Archer")
            yield return new WaitForSeconds(0.66f);
    }*/
    #endregion

    #region DAMAGE
    //==========================================================================================================================================================
    private IEnumerator ProcessDamage()
    {
        /*if (PlayerBuff == PlayerBuffs.BARBED && UnityEngine.Random.Range(0, 100) < 25)
        {
            WillCrit = true;
            PlayerDamageDeal += CritAmount;
        }

        //  Damage dealt is more than the enemy's passive damage block
        if (PlayerDamageDeal - combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().ThisEnemyData.damageBlock > 0)
        {
            if (PlayerDamageDeal - combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().ThisEnemyData.damageBlock >= combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().CurrentHealth)
                combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState = CombatStateData.CombatState.DYING;
            else
                combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState = CombatStateData.CombatState.ATTACKED;

            combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().TakeDamageFromPlayer(playerDamageDeal - combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().ThisEnemyData.damageBlock);
        }

        else
        {
            combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState = CombatStateData.CombatState.ATTACKED;
            combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().TakeDamageFromPlayer(1);    // enemy only takes 1 HP of damage
        }

        if (willCrit)
        {
            playerDamageDeal -= critAmount;
            willCrit = false;
        }

        if (characterPowerupController.UsedOffense)
        {
            if (GameManager.Instance.ActiveCharacter.BaseCharacter.characterID == "defaultCharacter")
            {
                PlayerDamageDeal /= 2;
                characterPowerupController.UsedOffense = false;
            }
            else if (GameManager.Instance.ActiveCharacter.BaseCharacter.characterID == "Archer")
            {
                PlayerDamageDeal = GameManager.Instance.ActiveCharacter.DamageDeal;
                if (string.IsNullOrEmpty(GameManager.Instance.ActiveCharacter.EquippedWeapon))
                    PlayerDamageDeal += GameManager.Instance.InventoryItem.GetProperWeaponData(GameManager.Instance.ActiveCharacter.EquippedWeapon).damage;
                characterPowerupController.UsedOffense = false;
            }
        }

        else if (characterPowerupController.UsedDefense)
        {
            if (GameManager.Instance.ActiveCharacter.BaseCharacter.characterID == "Archer")
            {
                PlayerCurrentHealth += playerDamageDeal;
                PlayerDamageDeal *= 2;
                combatCore.PlayerHealthSlider.value = ((float)PlayerCurrentHealth) / ((float)GameManager.Instance.ActiveCharacter.MaxHealth);
                characterPowerupController.UsedDefense = false;
            }
        }

        if (playerBuff == PlayerBuffs.BLESSED)
        {
            PlayerCurrentHealth += Mathf.CeilToInt(playerDamageDeal * 0.25f);
            if (PlayerCurrentHealth > GameManager.Instance.ActiveCharacter.MaxHealth)
                PlayerCurrentHealth = GameManager.Instance.ActiveCharacter.MaxHealth;
            combatCore.PlayerHealthSlider.value = ((float)PlayerCurrentHealth) / ((float)GameManager.Instance.ActiveCharacter.MaxHealth);
        }

        yield return new WaitUntil(() => combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.IDLE ||
                                         combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.DYING);

        if (GameManager.Instance.ActiveCharacter.BaseCharacter.attackType == CharacterData.AttackType.MELEE)
            CombatStateData.CurrentCombatState = CombatStateData.CombatState.RETURN;
        else if (GameManager.Instance.ActiveCharacter.BaseCharacter.attackType == CharacterData.AttackType.RANGED)
        {
            CombatStateData.CurrentCombatState = CombatStateData.CombatState.IDLE;
            ProcessDebuffDamage();
        }*/

        yield return new WaitForSeconds(0.5f);
        /*if (combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().CombatStateData.CurrentCombatState == CombatStateData.CombatState.DYING)
        {
            StartCoroutine(combatCore.ProcessEnemyDeath(combatCore.SpawnedEnemy));
        }*/
    }

    /*public void ProcessDebuffDamage()
    {
        //  Process debuff damages if currently debuffed
        if (CurrentDebuff != EnemyData.StatusEffect.NONE)
        {
            switch (CurrentDebuff)
            {
                case EnemyData.StatusEffect.POISONED:
                    if (DebuffInstancesRemaining % 2 == 0)
                        IntakeDebuffDamage();
                    else
                        DebuffInstancesRemaining--;
                    break;
                case EnemyData.StatusEffect.ELECTROCUTED:
                    IntakeDebuffDamage();
                    break;
                case EnemyData.StatusEffect.BURNED:
                    IntakeDebuffDamage();
                    break;
                case EnemyData.StatusEffect.SCARED:
                    DebuffInstancesRemaining--;
                    break;
                case EnemyData.StatusEffect.DECAYED:
                    DebuffInstancesRemaining--;
                    break;
                case EnemyData.StatusEffect.HALLUCINATING:
                    DebuffInstancesRemaining--;
                    break;
                case EnemyData.StatusEffect.SPICED:
                    DebuffInstancesRemaining--;
                    break;
            }

            if (DebuffInstancesRemaining == 0)
            {
                if (CurrentDebuff == EnemyData.StatusEffect.SCARED)
                    PlayerDamageBlock = GameManager.Instance.ActiveCharacter.DamageBlock;
                else if (CurrentDebuff == EnemyData.StatusEffect.DIZZY)
                    DizzyEffect.SetActive(false);
                else if (CurrentDebuff == EnemyData.StatusEffect.HALLUCINATING)
                    combatCore.QuestionCore.QuestionTMP.gameObject.SetActive(true);

                CurrentDebuff = EnemyData.StatusEffect.NONE;
                gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, 1f);
            }

            //  Process current still has health and may continue to pick a question
            if (PlayerCurrentHealth > 0)
                GameManager.Instance.CombatData.CurrentCombatState = CombatData.CombatState.TIMER;

            //  Player is dead
            else
            {
                CombatStateData.CurrentCombatState = CombatStateData.CombatState.DYING;
                isDead = true;
                GameManager.Instance.CombatData.CurrentCombatState = CombatData.CombatState.GAMEOVER;
            }
        }
        //  Go back to picking a question if player is not debuffed
        else
            GameManager.Instance.CombatData.CurrentCombatState = CombatData.CombatState.TIMER;
    }*/

    /*public void IntakeDebuffDamage()
    {
        StartCoroutine(PlayDebuffDamage()); //  This is just for playing the animations
        if (CurrentDebuff == EnemyData.StatusEffect.DECAYED)
            PlayerCurrentHealth -= Mathf.CeilToInt(GameManager.Instance.ActiveCharacter.MaxHealth * (DebuffDamage / 100));
        else if (CurrentDebuff == EnemyData.StatusEffect.DIZZY)
            PlayerCurrentHealth -= PlayerDamageDeal;
        else
            PlayerCurrentHealth -= DebuffDamage;
        combatCore.PlayerHealthSlider.value = ((float)PlayerCurrentHealth) / ((float)GameManager.Instance.ActiveCharacter.MaxHealth);
        DebuffInstancesRemaining--;
    }*/

    /*public void TakeDamageFromEnemy(int _damageReceived)
    {
        //  Make some calculations if the shiled is active
        if (characterPowerupController.UsedDefense)
        {
            anim.SetBool("isDefense", false);
            characterPowerupController.UsedDefense = false;
        }
        //  If player has no shield active, take all the damage
        else
        {
            if (ArmorActive)
            {
                //Debug.Log("Armor is active");
                if (CurrentArmorHealth >= _damageReceived)
                {
                    CurrentArmorHealth -= _damageReceived;
                    combatCore.PlayerArmorSlider.value = (float)CurrentArmorHealth / (float)MaxArmorHealth;
                    //  if the shield is out of health, deactivate it
                    if (CurrentArmorHealth == 0)
                    {
                        ArmorActive = false;
                        combatCore.PlayerArmorSlider.value = 0;
                    }
                }
                //  if the armor health is less than the enemy's damage, the shield tanks part of the damage while the player takes the rest
                else
                {
                    int piercingDamage = _damageReceived - CurrentArmorHealth;
                    //Debug.Log("Armor is not enough to tank this slime's damage");
                    if (piercingDamage > playerDamageBlock)
                    {
                        int reducedDamage = piercingDamage - playerDamageBlock;
                        PlayerCurrentHealth -= reducedDamage;
                    }
                    else
                    {
                        //Debug.Log("Player wont take any damage");
                    }

                    combatCore.PlayerHealthSlider.value = ((float)PlayerCurrentHealth) / ((float)GameManager.Instance.ActiveCharacter.MaxHealth);
                    CurrentArmorHealth = 0;
                    ArmorActive = false;
                    combatCore.PlayerArmorSlider.value = 0;
                }
            }
            else
            {
                //  Only take damage if the damageReceived is higher than the player's passive damage block
                if (_damageReceived - playerDamageBlock > 0)
                {
                    int reducedDamage = _damageReceived - playerDamageBlock;
                    PlayerCurrentHealth -= reducedDamage;
                    combatCore.PlayerHealthSlider.value = ((float)PlayerCurrentHealth) / ((float)GameManager.Instance.ActiveCharacter.MaxHealth);

                }

                //  If the enemy can give a debuff
                if (combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().ThisEnemyData.statusEffect != EnemyData.StatusEffect.NONE &&
                    currentDebuff != EnemyData.StatusEffect.FROZEN &&
                    currentDebuff != EnemyData.StatusEffect.ELECTROCUTED)
                {
                    //  if the player is currently not yet debuffed, then debuff the player
                    //StartCoroutine(TakeDebuffDamage(enemyTarget.GetComponent<EnemyData>().statusEffectDamage));
                    debuffDamage = combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().ThisEnemyData.statusEffectDamage;
                    currentDebuff = combatCore.SpawnedEnemy.GetComponent<EnemyCombatController>().ThisEnemyData.statusEffect;
                    switch (currentDebuff)
                    {
                        case EnemyData.StatusEffect.POISONED:
                            gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 255f, 0f, 1f);
                            debuffInstancesRemaining = 6;
                            break;
                        case EnemyData.StatusEffect.ELECTROCUTED:
                            gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 0f, 1f);
                            debuffInstancesRemaining = 4;
                            break;
                        case EnemyData.StatusEffect.BURNED:
                            gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 0f, 0f, 1f);
                            debuffInstancesRemaining = 5;
                            break;
                        case EnemyData.StatusEffect.FROZEN:
                            gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 255f, 255f, 1f);
                            anim.speed = 0;
                            debuffInstancesRemaining = 2;
                            break;
                        case EnemyData.StatusEffect.SCARED:
                            gameObject.GetComponent<SpriteRenderer>().color = new Color(255f, 0f, 255f, 1f);
                            playerDamageBlock -= 20;
                            debuffInstancesRemaining = 7;
                            if (playerDamageBlock < 0)
                                playerDamageBlock = 0;
                            break;
                        case EnemyData.StatusEffect.DECAYED:
                            gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 125, 255, 1f);
                            debuffInstancesRemaining = 3;
                            break;
                        case EnemyData.StatusEffect.DIZZY:
                            dizzyEffect.SetActive(true);
                            debuffInstancesRemaining = 2;
                            break;
                        case EnemyData.StatusEffect.SPICED:
                            gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 165, 0, 1f);
                            debuffInstancesRemaining = 8;
                            break;
                    }
                }
            }
        }
        //  Player is dead
        if (PlayerCurrentHealth <= 0)
        {
            CombatStateData.CurrentCombatState = CombatStateData.CombatState.DYING;
            //ChangePlayerCombatState(6); //  Game over
            isDead = true;
        }
        //  Player is still alive
        else
        {
            CombatStateData.CurrentCombatState = CombatStateData.CombatState.IDLE;
            //ChangePlayerCombatState(1); //  back to idle
            isDead = false;
        }*/
    }
    #endregion

