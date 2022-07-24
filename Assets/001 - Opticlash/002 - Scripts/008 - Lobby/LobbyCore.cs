using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.Linq;

public class LobbyCore : MonoBehaviour
{
    #region STATE MACHINE
    //==================================================================
    public enum LobbyStates
    {
        NONE,
        CORE,
        RANK,
        SHOP,
        INVENTORY,
        EVENT,
        QUEST,
        SETTINGS,
        EQUIP,
        COSTUME,
        CANNON
    }

    private event EventHandler lobbyStateChange;
    public event EventHandler onLobbyStateChange
    {
        add
        {
            if (lobbyStateChange == null || !lobbyStateChange.GetInvocationList().Contains(value))
                lobbyStateChange += value;
        }
        remove { lobbyStateChange -= value; }
    }

    public LobbyStates CurrentLobbyState
    {
        get => lobbyStates;
        set
        {
            lobbyStates = value;
            lobbyStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private LobbyStates lobbyStates;
    //==================================================================
    #endregion

    #region VARIABLES
    //==============================================================
    [field: Header("ANIMATORS")]
    [field: SerializeField] public Animator LobbyAnimator { get; set; }
    [field: SerializeField] public Animator DropdownAnimator { get; set; }

    //==============================================================
    #endregion

    public void ToggleDropdown()
    {
        DropdownAnimator.SetBool("Showing", !DropdownAnimator.GetBool("Showing"));
    }

    public void OpenCombatScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "CombatScene";
    }
}
