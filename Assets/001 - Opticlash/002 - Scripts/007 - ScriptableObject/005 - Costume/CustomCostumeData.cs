using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Costume", menuName = "Opticlash/Data/CustomCostumeData")]
public class CustomCostumeData : ScriptableObject
{
    [field: SerializeField] public string CostumeInstanceID { get; set; }
    [field: SerializeField] public CostumeData BaseCostumeData { get; set; }
    [field: SerializeField] public bool CostumeIsOwned { get; set; }
}
