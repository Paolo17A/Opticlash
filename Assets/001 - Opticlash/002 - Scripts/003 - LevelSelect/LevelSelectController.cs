using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class LevelSelectController : MonoBehaviour
{
    [field: Header("LEVEL DATA")]
    [field: SerializeField] private LevelData LevelData { get; set; }
    [field: SerializeField] [field: ReadOnly] private bool Accessible { get; set; }
}
