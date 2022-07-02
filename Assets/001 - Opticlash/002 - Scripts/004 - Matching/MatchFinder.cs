using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchFinder : MonoBehaviour
{
    //==========================================================
    [field: SerializeField] private BoardCore BoardCore { get; set; }

    [field: Header("MATCHES")]
    [field: SerializeField] public List<GemController> CurrentMatches { get; set; }

    [field: Header("DEBUGGER")]
    private GemController currentGem;
    private GemController leftGem;
    private GemController rightGem;
    private GemController topGem;
    private GemController bottomGem;
    //==========================================================

    private void Awake()
    {
        CurrentMatches = new List<GemController>();
    }

    public void FindAllMatches()
    {
        CurrentMatches.Clear();
        for (int x = 0; x < BoardCore.Width; x++)
        {
            for (int y = 0; y < BoardCore.Height; y++)
            {
                currentGem = BoardCore.AllGems[x, y];
                if(currentGem != null)
                {
                    if (x > 0 && x < BoardCore.Width - 1)
                    {
                        leftGem = BoardCore.AllGems[x - 1, y];
                        rightGem = BoardCore.AllGems[x + 1, y];
                        if (leftGem != null && rightGem != null)
                        {
                            if(leftGem.ThisGemType == currentGem.ThisGemType && rightGem.ThisGemType == currentGem.ThisGemType)
                            {
                                currentGem.isMatched = true;
                                leftGem.isMatched = true;
                                rightGem.isMatched = true;

                                CurrentMatches.Add(currentGem);
                                CurrentMatches.Add(leftGem);
                                CurrentMatches.Add(rightGem);
                            }
                        }
                    }
                    if (y > 0 && y < BoardCore.Height - 1)
                    {
                        topGem = BoardCore.AllGems[x, y + 1];
                        bottomGem = BoardCore.AllGems[x, y - 1];
                        if (topGem != null && bottomGem != null)
                        {
                            if (topGem.ThisGemType == currentGem.ThisGemType && bottomGem.ThisGemType == currentGem.ThisGemType)
                            {
                                currentGem.isMatched = true;
                                topGem.isMatched = true;
                                bottomGem.isMatched = true;

                                CurrentMatches.Add(currentGem);
                                CurrentMatches.Add(topGem);
                                CurrentMatches.Add(bottomGem);
                            }
                        }
                    }

                }
            }
        }

        if(CurrentMatches.Count > 0)
        {
            CurrentMatches = CurrentMatches.Distinct().ToList();
        }
    }
}
