using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "Level", menuName = "Opticlash/Data/LevelData")]
public class LevelData : ScriptableObject
{
    //===============================================================================
    [field: SerializeField] public int LevelIndex { get; set; }

    [field: Header("BOSS")]
    [field: SerializeField] public bool IsBossLevel { get; set; }

    //[Header("ENEMIES")]
    /*[SerializeField] private List<EnemyData> enemyDataInLevel;*/

    [field: Header("OPTIBIT")]
    [SerializeField] public int minDroppedOPB;
    [SerializeField] public int maxDroppedOPB;
    //===============================================================================
}
