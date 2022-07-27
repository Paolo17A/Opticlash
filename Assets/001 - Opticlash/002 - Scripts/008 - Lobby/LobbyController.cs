using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        LobbyCore.DisplayOptibits();

        //testing purposes
        LobbyCore.GetActiveCannon();
        LobbyCore.GetActiveCostume();
    }

    private IEnumerator DelayForPanel()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        LobbyCore.LobbyAnimator.SetInteger("index", (int)LobbyCore.CurrentLobbyState);
        if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.SHOP)
            LobbyCore.DisplayShopItem();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.INVENTORY)
            LobbyCore.InitializeInventory();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.CRAFT)
            LobbyCore.InitializeCrafting();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.EQUIP)
            LobbyCore.InitializeWeapons();
        else if (LobbyCore.CurrentLobbyState == LobbyCore.LobbyStates.COSTUME)
            LobbyCore.InitializeCostumes();
    }

    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }

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
            case (int)LobbyCore.LobbyStates.CRAFT:
                LobbyCore.CurrentLobbyState = LobbyCore.LobbyStates.CRAFT;
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
        }
    }
}
