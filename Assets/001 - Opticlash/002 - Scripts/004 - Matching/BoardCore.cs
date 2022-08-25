using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardCore : MonoBehaviour
{
    public enum BoardState { NONE, MOVING, WAITING};
    //===================================================================================
    [field: SerializeField] public MatchFinder MatchFinder { get; set; }
    [field: SerializeField] public CombatCore CombatCore { get; set; }
    [field: SerializeField] private PlayerData PlayerData { get; set; }

    [field: Header("BOARD ATTRIBUTES")]
    [field: SerializeField] public Button ShuffleBtn { get; set; }
    [field: SerializeField] public int Width { get; set; }
    [field: SerializeField] public int Height { get; set; }
    [field: SerializeField] private GemController[] Gems { get; set; }
    [field: SerializeField] public GemController[,] AllGems { get; set; }
    [field: SerializeField] public float moveSpeed { get; set; }
    [field: SerializeField] public BoardState CurrentBoardState { get; set; }
    [field: SerializeField] public int ShotsEarned { get; set; }

    [field: Header("DEBUGGER")]
    private int randomGemIndex;
    private GemController spawnedGem;
    private bool shuffling;

    //===================================================================================
    private void OnEnable()
    {
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void Awake()
    {
        AllGems = new GemController[Width, Height];
    }

    private void Start()
    {
        InitializeBoard();
        CurrentBoardState = BoardState.MOVING;
    }

    private void InitializeBoard()
    {
        for(int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                randomGemIndex = Random.Range(0, Gems.Length);
                while (MatchesAt(new Vector2Int(x,y), Gems[randomGemIndex]))
                {
                    randomGemIndex = Random.Range(0, Gems.Length);
                }
                SpawnGem(new Vector2Int(x , y), Gems[randomGemIndex]);
            }
        }
    }

    private void SpawnGem(Vector2Int spawnPosition, GemController gemToSpawn)
    {
        spawnedGem = Instantiate(gemToSpawn, new Vector3(0.5f + spawnPosition.x + (0.37f * spawnPosition.x), 1f + spawnPosition.y + Height + (0.37f * spawnPosition.y)), Quaternion.identity);
        spawnedGem.transform.SetParent(transform);
        spawnedGem.name = "Gem - " + spawnPosition.x + ", " + spawnPosition.y;
        AllGems[spawnPosition.x, spawnPosition.y] = spawnedGem;
        spawnedGem.InititalizeGem(spawnPosition, this, spawnedGem.transform.position, CombatCore);
    }

    private bool MatchesAt(Vector2Int posToCheck, GemController gemToCheck)
    {
        if(posToCheck.x > 1 && AllGems[posToCheck.x - 1, posToCheck.y].ThisGemType == gemToCheck.ThisGemType && AllGems[posToCheck.x - 2, posToCheck.y].ThisGemType == gemToCheck.ThisGemType)
                return true;
            
        if (posToCheck.y > 1 && AllGems[posToCheck.x, posToCheck.y - 1].ThisGemType == gemToCheck.ThisGemType && AllGems[posToCheck.x, posToCheck.y - 2].ThisGemType == gemToCheck.ThisGemType)
                return true;

        return false;
    }

    private void DestroyMatchedGemAt(Vector2Int pos, Vector2 burstPos)
    {
        if (AllGems[pos.x, pos.y] != null)
        {
            if (AllGems[pos.x, pos.y].isMatched)
            {
                Instantiate(AllGems[pos.x, pos.y].BurstEffect, new Vector2(burstPos.x, burstPos.y), Quaternion.identity);

                Destroy(AllGems[pos.x, pos.y].gameObject);
                AllGems[pos.x, pos.y] = null;
            }
        }
    }

    public void DestroyMatches()
    {
        ShuffleBtn.interactable = false;
        if (MatchFinder.CurrentMatches.Count == 3)
            CombatCore.SpawnedPlayer.DamageDeal = (CombatCore.SpawnedPlayer.Attack * CombatCore.SpawnedPlayer.Attack) / (CombatCore.CurrentEnemy.Attack + CombatCore.CurrentEnemy.Defense);
        else if (MatchFinder.CurrentMatches.Count == 4)
            CombatCore.SpawnedPlayer.DamageDeal = 1.5f * (CombatCore.SpawnedPlayer.Attack * CombatCore.SpawnedPlayer.Attack) / (CombatCore.CurrentEnemy.Attack + CombatCore.CurrentEnemy.Defense);
        else if (MatchFinder.CurrentMatches.Count >= 5)
            CombatCore.SpawnedPlayer.DamageDeal = 1.8f * (CombatCore.SpawnedPlayer.Attack * CombatCore.SpawnedPlayer.Attack) / (CombatCore.CurrentEnemy.Attack + CombatCore.CurrentEnemy.Defense);
        for (int i = 0; i < MatchFinder.CurrentMatches.Count; i++)
        {
            if(MatchFinder.CurrentMatches[i] != null)
            {
                DestroyMatchedGemAt(MatchFinder.CurrentMatches[i].PositionIndex, MatchFinder.CurrentMatches[i].BoardPosition);
            }
        }
        GameManager.Instance.SFXAudioManager.PlayMatchSFX();

        if (CombatCore.CurrentEnemy.CurrentHealth > 0 && CombatCore.CurrentCombatState != CombatCore.CombatState.WALKING && CombatCore.SpawnedPlayer.CurrentCombatState != CharacterCombatController.CombatState.WALKING)
        {
            ShotsEarned++;
        }
        StartCoroutine(DecreaseRowCoroutine());
    }

    private IEnumerator DecreaseRowCoroutine()
    {
        yield return new WaitForSeconds(.1f);

        int nullCounter = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (AllGems[x, y] == null)
                    nullCounter++;
                else if (nullCounter > 0)
                {
                    AllGems[x, y].PositionIndex = new Vector2Int(x, y - nullCounter);
                    AllGems[x, y].BoardPosition = new Vector2(0.5f + (1.37f * x),1f + (1.37f * (y - nullCounter) + Height));
                    AllGems[x, y - nullCounter] = AllGems[x, y];
                    AllGems[x, y] = null;
                }
            }
            nullCounter = 0;
        }
        StartCoroutine(FillBoardCoroutine());
    }

    private IEnumerator FillBoardCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        #region REFILL BOARD
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if(AllGems[x, y] == null)
                {
                    randomGemIndex = Random.Range(0, Gems.Length);
                    SpawnGem(new Vector2Int(x, y), Gems[randomGemIndex]);
                }
            }
        }
        #endregion

        yield return new WaitForSeconds(0.2f);
        MatchFinder.FindAllMatches();

        if(MatchFinder.CurrentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.2f);
            DestroyMatches();
        }    
        else
        {
            yield return new WaitForSeconds(0.2f);
            
            if(CombatCore.CurrentCombatState != CombatCore.CombatState.WALKING)
            {
                if (PlayerData.ActiveCustomWeapon.BaseWeaponData.HasBonusBullets && CombatCore.RoundCounter % PlayerData.ActiveCustomWeapon.BaseWeaponData.BonusFrequency == 0 && Random.Range(0, 100) < PlayerData.ActiveCustomWeapon.BaseWeaponData.BonusRate)
                    ShotsEarned += PlayerData.ActiveCustomWeapon.BaseWeaponData.BonusBullets;              

                if(shuffling)
                {
                    CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
                    shuffling = false;
                }
                else
                    CombatCore.SpawnedPlayer.CurrentCombatState = CharacterCombatController.CombatState.ATTACKING;
            }
            CurrentBoardState = BoardState.MOVING;
        }
    }

    public void ShuffleBoard()
    {
        if(CurrentBoardState != BoardState.WAITING && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
        {
            CombatCore.ShuffleBtn.interactable = false;
            CurrentBoardState = BoardState.WAITING;
            GameManager.Instance.SFXAudioManager.PlayShuffleSFX();
            foreach (Transform child in transform)
            {
                Instantiate(child.gameObject.GetComponent<GemController>().BurstEffect, child.gameObject.GetComponent<GemController>().BoardPosition, Quaternion.identity);
                Destroy(child.gameObject);
            }
            InitializeBoard();
            CombatCore.AmmoCount -= 10;
            CombatCore.AmmoTMP.text = CombatCore.AmmoCount.ToString();
            shuffling = true;
            StartCoroutine(FillBoardCoroutine());
        }
    }
}
