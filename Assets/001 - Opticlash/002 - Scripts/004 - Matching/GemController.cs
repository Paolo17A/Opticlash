using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class GemController : MonoBehaviour
{
    public enum GemType { NONE, BLUE, RED, GREEN, YELLOW, PURPLE };
    [field: SerializeField][field: ReadOnly] public CombatCore CombatCore;
    //==============================================================================
    [field: Header("GEM ATTRIBUTES")]
    [field: SerializeField] public GemType ThisGemType { get; set; }
    [field: SerializeField] public GameObject BurstEffect { get; set; }
    [field: SerializeField][field: ReadOnly] public Vector2Int PositionIndex { get; set; }
    [field: SerializeField][field: ReadOnly] private Vector2Int PreviousPosition { get; set; }
    [field: SerializeField] public Vector2 BoardPosition { get; set; }
    [field: SerializeField][field: ReadOnly] private Vector2 PreviousBoardPosition { get; set; }
    [field: SerializeField][field: ReadOnly] public BoardCore BoardCore { get; set; }

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] public bool isMatched;
    [field: SerializeField][field: ReadOnly] private Vector2 initialTouchPoint;
    [field: SerializeField][field: ReadOnly] private Vector2 endingTouchPoint;
    [field: SerializeField][field: ReadOnly] private bool mousePressed;
    [field: SerializeField][field: ReadOnly] private float swipeAngle;
    [field: SerializeField][field: ReadOnly] private GemController otherGem;
    //==============================================================================

    private void Update()
    {
        if (Vector2.Distance(transform.position, BoardPosition) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, new Vector3(BoardPosition.x, BoardPosition.y), BoardCore.moveSpeed * Time.deltaTime);
        else
        {
            transform.position = BoardPosition;
            BoardCore.AllGems[PositionIndex.x, PositionIndex.y] = this;
        }
        if (mousePressed && Input.GetMouseButton(0))
        {
            //mousePressed = false;
            if(BoardCore.CurrentBoardState == BoardCore.BoardState.MOVING && CombatCore.CurrentCombatState == CombatCore.CombatState.TIMER)
            {
                endingTouchPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }
    }

    private void OnMouseDown()
    {
        if(BoardCore.CurrentBoardState == BoardCore.BoardState.MOVING)
        {
            initialTouchPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePressed = true;
        }
    }
    public void InititalizeGem(Vector2Int pos, BoardCore board, Vector2 boardPosition, CombatCore combatCore)
    {
        PositionIndex = pos;
        BoardCore = board;
        BoardPosition = boardPosition;
        CombatCore = combatCore;
    }

    
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(endingTouchPoint.y - initialTouchPoint.y, endingTouchPoint.x - initialTouchPoint.x);
        swipeAngle = swipeAngle * 180 / Mathf.PI;
        if(Vector3.Distance(initialTouchPoint, endingTouchPoint) > 0.5f)
            MovePieces();
    }

    private void MovePieces()
    {
        GameManager.Instance.SFXAudioManager.PlayMoveSFX();
        PreviousPosition = PositionIndex;
        PreviousBoardPosition = BoardPosition;

        //  Swipe Right
        if(swipeAngle < 45 && swipeAngle > -45 && PositionIndex.x < BoardCore.Width - 1)
        {
            otherGem = BoardCore.AllGems[PositionIndex.x + 1, PositionIndex.y];
            otherGem.PositionIndex = new Vector2Int(otherGem.PositionIndex.x - 1, otherGem.PositionIndex.y);
            otherGem.BoardPosition = BoardPosition;
            PositionIndex = new Vector2Int(PositionIndex.x + 1, PositionIndex.y);
            BoardPosition = new Vector2(BoardPosition.x + 1.3f, BoardPosition.y);
        }
        //  Swipe Up
        else if (swipeAngle > 45 && swipeAngle <= 135 && PositionIndex.y < BoardCore.Height - 1)
        {
            otherGem = BoardCore.AllGems[PositionIndex.x, PositionIndex.y + 1];
            otherGem.PositionIndex = new Vector2Int(otherGem.PositionIndex.x, otherGem.PositionIndex.y - 1);
            otherGem.BoardPosition = BoardPosition;
            PositionIndex = new Vector2Int(PositionIndex.x, PositionIndex.y + 1);
            BoardPosition = new Vector2(BoardPosition.x, BoardPosition.y + 1.3f);
        }
        //Swipe Down
        else if (swipeAngle < -45 && swipeAngle >= -135 && PositionIndex.y > 0)
        {
            otherGem = BoardCore.AllGems[PositionIndex.x, PositionIndex.y - 1];
            otherGem.PositionIndex = new Vector2Int(otherGem.PositionIndex.x, otherGem.PositionIndex.y + 1);
            otherGem.BoardPosition = BoardPosition;
            PositionIndex = new Vector2Int(PositionIndex.x, PositionIndex.y - 1);
            BoardPosition = new Vector2(BoardPosition.x, BoardPosition.y - 1.3f);
        }
        //Swipe Left
        else if (swipeAngle > 135 || swipeAngle < -135 && PositionIndex.x > 0)
        {
            otherGem = BoardCore.AllGems[PositionIndex.x - 1, PositionIndex.y];
            otherGem.PositionIndex = new Vector2Int(otherGem.PositionIndex.x + 1, otherGem.PositionIndex.y);
            otherGem.BoardPosition = BoardPosition;
            PositionIndex = new Vector2Int(PositionIndex.x - 1, PositionIndex.y);
            BoardPosition = new Vector2(BoardPosition.x - 1.3f, BoardPosition.y);
        }

        BoardCore.AllGems[PositionIndex.x, PositionIndex.y] = this;
        BoardCore.AllGems[otherGem.PositionIndex.x, otherGem.PositionIndex.y] = otherGem;
        StartCoroutine(CheckMoveCoroutine());
    }

    public IEnumerator CheckMoveCoroutine()
    {
        BoardCore.CurrentBoardState = BoardCore.BoardState.WAITING;
        yield return new WaitForSeconds(0.5f);
        BoardCore.MatchFinder.FindAllMatches();

        if(otherGem != null)
        {
            if (!isMatched && !otherGem.isMatched)
            {
                otherGem.PositionIndex = PositionIndex;
                otherGem.BoardPosition = BoardPosition;
                PositionIndex = PreviousPosition;
                BoardPosition = PreviousBoardPosition;
                PreviousPosition = Vector2Int.zero;
                PreviousBoardPosition = Vector2.zero;

                BoardCore.AllGems[PositionIndex.x, PositionIndex.y] = this;
                BoardCore.AllGems[otherGem.PositionIndex.x, otherGem.PositionIndex.y] = otherGem;

                yield return new WaitForSeconds(0.3f);
                BoardCore.CurrentBoardState = BoardCore.BoardState.MOVING;
                mousePressed = false;
            }
            else
            {
                if(CombatCore.CurrentCombatState != CombatCore.CombatState.WALKING)
                {
                    BoardCore.ShotsEarned = 0;
                    CombatCore.CurrentCombatState = CombatCore.CombatState.PLAYERTURN;
                }
                mousePressed = false;
                CombatCore.StopTimerCoroutine();
                BoardCore.DestroyMatches();
            }
        }
    }
}
