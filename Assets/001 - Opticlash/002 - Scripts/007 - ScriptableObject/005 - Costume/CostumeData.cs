using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CostumeData", menuName = "Opticlash/Data/CostumeData")]

public class CostumeData : ScriptableObject
{
    [field: SerializeField] public string CostumeID { get; set; }
    [field: SerializeField] public string CostumeName { get; set; }
    [field: SerializeField] public Sprite CostumeSprite { get; set; }
    [field: SerializeField] public Sprite LobbyCostumeSprite { get; set; }
    [field: SerializeField] public Sprite InfoSprite { get; set; }
    [field: SerializeField] public Sprite DroppedSprite { get; set; }
    [field: SerializeField] public EnemyCombatController.SideEffect ProvidedImmunity { get; set; }
}
