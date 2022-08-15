using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    private void OnEnable()
    {
        LobbyCore.onLobbyStateChange += LobbyStateChange;

        GameManager.Instance.SceneController.ActionPass = true;
        LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CORE;
    }

    private void OnDisable()
    {
        LobbyCore.onLobbyStateChange -= LobbyStateChange;
    }

    private void LobbyStateChange(object sender, EventArgs e)
    {
        LobbyCore.DropdownAnimator.SetBool("Showing", false);
        StartCoroutine(DelayForPanel());
    }

    private void Awake()
    {
        LobbyCore.ActualCostumesOwned = new List<CustomCostumeData>();
        LobbyCore.ActualOwnedWeapons = new List<CustomWeaponData>();
    }
    private void Start()
    {
        //testing purposes
        if (GameManager.Instance.DebugMode)
        {
            LobbyCore.StageTMP.text = PlayerData.CurrentStage.ToString();
            LobbyCore.GetActiveCannon();
            LobbyCore.GetActiveCostume();
            LobbyCore.DisplayOptibits();
        }
        else
            LobbyCore.InitializeLobby();
    }

    private IEnumerator DelayForPanel()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        LobbyCore.LobbyAnimator.SetInteger("index", (int)LobbyCore.CurrentLobbyState);
        if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.RANK)
            LeaderboardCore.InitializeLeaderboard();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.SHOP)
            ShopCore.DisplayShopItem();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.INVENTORY)
            InventoryCore.InitializeInventory();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.LOOTBOX)
            LootboxCore.InitializeLootbox();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.QUEST)
            QuestCore.InitializeQuest();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.EQUIP || LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.CANNON)
        {
            LobbyCore.WeaponBtn.GetComponent<Image>().sprite = LobbyCore.ActiveWeaponSprite;
            LobbyCore.CostumeBtn.GetComponent<Image>().sprite = LobbyCore.InactiveCostumeSprite;
            LobbyCore.InitializeWeapons();
        }
            
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.COSTUME)
        {
            LobbyCore.WeaponBtn.GetComponent<Image>().sprite = LobbyCore.InactiveWeaponSprite;
            LobbyCore.CostumeBtn.GetComponent<Image>().sprite = LobbyCore.ActiveCostumeSprite;
            LobbyCore.InitializeCostumes();
        }
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.CURRENTCANNON)
            LobbyCore.DisplayOptibits();
    }

    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }
    [field: SerializeField] private QuestCore QuestCore { get; set; }
    [field: SerializeField] private LeaderboardCore LeaderboardCore { get; set; }
    [field: SerializeField] private LootboxCore LootboxCore { get; set; }
    [field: SerializeField] private ShopCore ShopCore { get; set; }
    [field: SerializeField] private InventoryCore InventoryCore { get; set; }

    public void LobbyStateToIndex(int state)
    {
        switch(state)
        {
            case (int)LobbyCore.LobbyStates.CORE:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CORE;
                break;
            case (int)LobbyCore.LobbyStates.RANK:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.RANK;
                break;
            case (int)LobbyCore.LobbyStates.SHOP:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.SHOP;
                break;
            case (int)LobbyCore.LobbyStates.INVENTORY:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.INVENTORY;
                break;
            case (int)LobbyCore.LobbyStates.LOOTBOX:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.LOOTBOX;
                break;
            case (int)LobbyCore.LobbyStates.QUEST:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.QUEST;
                break;
            case (int)LobbyCore.LobbyStates.SETTINGS:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.SETTINGS;
                break;
            case (int)LobbyCore.LobbyStates.EQUIP:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.EQUIP;
                break;
            case (int)LobbyCore.LobbyStates.COSTUME:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.COSTUME;
                break;
            case (int)LobbyCore.LobbyStates.CANNON:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CANNON;
                break;
            case (int)LobbyCore.LobbyStates.CURRENTCANNON:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CURRENTCANNON;
                break;
            case (int)LobbyCore.LobbyStates.CLAIM:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CLAIM;
                break;
        }
    }
}
