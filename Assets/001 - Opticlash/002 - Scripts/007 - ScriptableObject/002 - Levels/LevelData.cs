using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "Level", menuName = "Opticlash/Data/LevelData")]
public class LevelData : ScriptableObject
{
    //===============================================================================
    [field: SerializeField] public int LevelIndex { get; set; }

    [field: Header("ENEMIES")]
    [field: SerializeField] private List<string> MonsterList { get; set; }
    [field: SerializeField] private List<int> MonsterLevels { get; set; }

    [field: Header("OPTIBIT")]
    [field: SerializeField] public int minDroppedOPB;
    [field: SerializeField] public int maxDroppedOPB;
    //===============================================================================
}
