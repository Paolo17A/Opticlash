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

    [field: Header("DEBUGGER")]
    private int randomGemIndex;
    private GemController spawnedGem;

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
        spawnedGem = Instantiate(gemToSpawn, new Vector3(spawnPosition.x + (0.3f * spawnPosition.x), spawnPosition.y + Height + (0.3f * spawnPosition.y)), Quaternion.identity);
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
        for(int i = 0; i < MatchFinder.CurrentMatches.Count; i++)
        {
            if(MatchFinder.CurrentMatches[i] != null)
            {
                DestroyMatchedGemAt(MatchFinder.CurrentMatches[i].PositionIndex, MatchFinder.CurrentMatches[i].BoardPosition);
            }
        }
        if(CombatCore.CurrentEnemy.transform.GetChild(0).GetComponent<EnemyCombatController>().CurrentHealth > 0 && CombatCore.CurrentCombatState != CombatCore.CombatState.WALKING && CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentCombatState != CharacterCombatController.CombatState.WALKING)
            CombatCore.SpawnedPlayer.GetComponent<CharacterCombatController>().CurrentCombatState = CharacterCombatController.CombatState.ATTACKING;
        StartCoroutine(DecreaseRowCoroutine());
    }

    private IEnumerator DecreaseRowCoroutine()
    {
        yield return new WaitForSeconds(.2f);

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
                    AllGems[x, y].BoardPosition = new Vector2(1.3f * x, 1.3f * (y - nullCounter) + Height);
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
        yield return new WaitForSeconds(0.5f);
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

        yield return new WaitForSeconds(0.5f);
        MatchFinder.FindAllMatches();

        if(MatchFinder.CurrentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }    
        else
        {
            yield return new WaitForSeconds(0.5f);
            ShuffleBtn.interactable = true;
            if(CombatCore.CurrentCombatState != CombatCore.CombatState.WALKING)
                CombatCore.CurrentCombatState = CombatCore.CombatState.ENEMYTURN;
            CurrentBoardState = BoardState.MOVING;
        }
    }

    public void ShuffleBoard()
    {
        if(CurrentBoardState != BoardState.WAITING && CombatCore.CurrentCombatState != CombatCore.CombatState.WALKING)
        {
            ShuffleBtn.interactable = false;
            CurrentBoardState = BoardState.WAITING;
            List<GemController> gemsFromBoard = new List<GemController>();

            //  Get all the already instantiated gems, place them in a list, then empty the 2D array
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    gemsFromBoard.Add(AllGems[x, y]);
                    AllGems[x, y] = null;
                }
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    randomGemIndex = Random.Range(0, gemsFromBoard.Count);
                    int iterations = 0;
                    while (MatchesAt(new Vector2Int(x, y), gemsFromBoard[randomGemIndex]) && iterations < 100 && gemsFromBoard.Count > 1)
                    {
                        randomGemIndex = Random.Range(0, Gems.Length);
                        iterations++;
                    }
                    gemsFromBoard[randomGemIndex].InititalizeGem(new Vector2Int(x, y), this, new Vector2(x + (0.3f * x), y + Height + (0.3f * y)), CombatCore);
                    AllGems[x, y] = gemsFromBoard[randomGemIndex];
                    gemsFromBoard.RemoveAt(randomGemIndex);
                }
            }
            PlayerData.AmmoCount -= 10;
            CombatCore.RoundTMP.text = PlayerData.AmmoCount.ToString();
            StartCoroutine(FillBoardCoroutine());
        }
    }
}
