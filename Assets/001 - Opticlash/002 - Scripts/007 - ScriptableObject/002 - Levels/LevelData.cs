using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "Level", menuName = "Opticlash/Data/LevelData")]
public class LevelData : ScriptableObject
{
    //===============================================================================
    [field: SerializeField] public int LevelIndex { get; set; }
    [field: SerializeField] public Sprite SelectedLevelSprite { get; set; }

    [field: Header("ENEMIES")]
    [field: SerializeField] public List<string> MonsterList { get; set; }
    [field: SerializeField] public List<int> MonsterLevels { get; set; }

    [field: Header("OPTIBIT")]
    [field: SerializeField] public int MinDroppedOPB { get; set; }
    [field: SerializeField] public int MaxDroppedOPB { get; set; }

    [field: Header("NORMAL FRAGMENT")]
    [field: SerializeField] public int MinDroppedNormalFragment { get; set; }
    [field: SerializeField] public int MaxDroppedNormalFragment { get; set; }

    [field: Header("RARE FRAGMENT")]
    [field: SerializeField] public int MinDroppedRareFragment { get; set; }
    [field: SerializeField] public int MaxDroppedRareFragment { get; set; }

    [field: Header("EPIC FRAGMENT")]
    [field: SerializeField] public int MinDroppedEpicFragment { get; set; }
    [field: SerializeField] public int MaxDroppedEpicFragment { get; set; }

    [field: Header("LEGEND FRAGMENT")]
    [field: SerializeField] public int MinDroppedLegendFragment { get; set; }
    [field: SerializeField] public int MaxDroppedLegendFragment { get; set; }

    [field: Header("BREAK REMOVE")]
    [field: SerializeField] public int MinDroppedBreakRemove { get; set; }
    [field: SerializeField] public int MaxDroppedBreakRemove { get; set; }

    [field: Header("BURN REMOVE")]
    [field: SerializeField] public int MinDroppedBurnRemove { get; set; }
    [field: SerializeField] public int MaxDroppedBurnRemove { get; set; }

    [field: Header("CONFUSE REMOVE")]
    [field: SerializeField] public int MinDroppedConfuseRemove { get; set; }
    [field: SerializeField] public int MaxDroppedConfuseRemove { get; set; }

    [field: Header("FREEZE REMOVE")]
    [field: SerializeField] public int MinDroppedFreezeRemove { get; set; }
    [field: SerializeField] public int MaxDroppedFreezeRemove { get; set; }

    [field: Header("PARALYZE REMOVE")]
    [field: SerializeField] public int MinDroppedParalyzeRemove { get; set; }
    [field: SerializeField] public int MaxDroppedParalyzeRemove { get; set; }

    [field: Header("WEAK REMOVE")]
    [field: SerializeField] public int MinDroppedWeakRemove { get; set; }
    [field: SerializeField] public int MaxDroppedWeakRemove { get; set; }

    [field: Header("SMALL HEAL")]
    [field: SerializeField] public int MinDroppedSmallHeal { get; set; }
    [field: SerializeField] public int MaxDroppedSmallHeal { get; set; }

    [field: Header("MEDIUM HEAL")]
    [field: SerializeField] public int MinDroppedMediumHeal { get; set; }
    [field: SerializeField] public int MaxDroppedMediumHeal { get; set; }

    [field: Header("LARGE HEAL")]
    [field: SerializeField] public int MinDroppedLargeHeal { get; set; }
    [field: SerializeField] public int MaxDroppedLargeHeal { get; set; }
    //===============================================================================
}
