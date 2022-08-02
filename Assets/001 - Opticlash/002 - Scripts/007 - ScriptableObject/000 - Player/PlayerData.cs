using System;   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Opticlash/Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    //============================================================================
    [field: Header("USER TITLE DATA")]
    [field: SerializeField] public string PlayfabID { get; set; }
    [field: SerializeField] public string DisplayName { get; set; }
    [field: SerializeField] public string LUID { get; set; }
    [field: SerializeField] public int CurrentStage { get; set; }

    [field: Header("VIRTUAL CURRENCY")]
    [field: SerializeField] public int EnergyCount { get; set; }
    [field: SerializeField] public int Optibit { get; set; }

    [field: Header("STATS")]
    [field: SerializeField] public int CurrentHealth { get; set; }
    [field: SerializeField] public int MaxHealth { get; set; }

    [field: Header("WEAPONS")]
    [field: SerializeField] public string ActiveWeaponID { get; set; }
    [field: SerializeField] public WeaponData ActiveWeapon { get; set; }
    [field: SerializeField] public List<CustomWeaponData> OwnedWeapons { get; set; }

    [field: Header("COSTUMES")]
    [field: SerializeField] public string ActiveConstumeInstanceID { get; set; }
    [field: SerializeField] public CostumeData ActiveCostume { get; set; }
    [field: SerializeField] public List<CustomCostumeData> OwnedCostumes { get; set; }

    [field: Header("ITEMS")]
    [field: SerializeField] public string HealInstanceID { get; set; }
    [field: SerializeField] public int HealCharges { get; set; }
    [field: SerializeField] public string BreakRemovalInstanceID { get; set; }
    [field: SerializeField] public int BreakRemovalCharges { get; set; }
    [field: SerializeField] public string WeakRemovalInstanceID { get; set; }
    [field: SerializeField] public int WeakRemovalCharges { get; set; }
    [field: SerializeField] public string FreezeRemovalInstanceID { get; set; }
    [field: SerializeField] public int FreezeRemovalCharges { get; set; }
    [field: SerializeField] public string ParalyzeRemovalInstanceID { get; set; }
    [field: SerializeField] public int ParalyzeRemovalCharges { get; set; }
    [field: SerializeField] public string ConfuseRemovalInstanceID { get; set; }
    [field: SerializeField] public int ConfuseRemovalCharges { get; set; }
    [field: SerializeField] public string BurnRemovalInstanceID { get; set; }
    [field: SerializeField] public int BurnRemovalCharges { get; set; }

    [field: Header("FRAGMENTS")]
    [field: SerializeField] public int NormalFragments { get; set; }
    [field: SerializeField] public int RareFragments { get; set; }
    [field: SerializeField] public int EpicFragments { get; set; }
    [field: SerializeField] public int LegendFragments { get; set; }

    [field: Header("DAILY QUEST CLAIMED")]
    [field: SerializeField] public bool Quest1Claimed { get; set; }
    [field: SerializeField] public bool Quest2Claimed { get; set; }
    [field: SerializeField] public bool Quest3Claimed { get; set; }
    [field: SerializeField] public bool Quest4Claimed { get; set; }
    [field: SerializeField] public bool Quest5Claimed { get; set; }

    [field: Header("DAILY QUEST DATA")]
    [field: SerializeField] public int DailyCheckIn { get; set; }
    [field: SerializeField] public int SocMedShared { get; set; }
    [field: SerializeField] public int ItemsUsed { get; set; }
    [field: SerializeField] public int MonstersKilled { get; set; }
    [field: SerializeField] public int BossesKilled { get; set; }
    [field: SerializeField] public int DailyQuestClaimed { get; set; }

    //============================================================================

    private void OnEnable()
    {
        if(!GameManager.Instance.DebugMode)
            ResetPlayerData();
    }

    private void OnDisable()
    {
        if (!GameManager.Instance.DebugMode)
            ResetPlayerData();
    }

    public void ResetPlayerData()
    {
        PlayfabID = "";
        DisplayName = "";
        LUID = "";

        Optibit = 0;
        EnergyCount = 0;

        HealInstanceID = "";
        HealCharges = 0;
        BreakRemovalInstanceID = "";
        BreakRemovalCharges = 0;
        WeakRemovalInstanceID = "";
        WeakRemovalCharges = 0;
        FreezeRemovalInstanceID = "";
        FreezeRemovalCharges = 0;
        ParalyzeRemovalInstanceID = "";
        ParalyzeRemovalCharges = 0;
        ConfuseRemovalInstanceID = "";
        ConfuseRemovalCharges = 0;
        BurnRemovalInstanceID = "";
        BurnRemovalCharges = 0;
    }
}
