using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class EntryCore : MonoBehaviour
{
    #region STATE MACHINE
    //==================================================================
    public enum EntryStates
    {
        NONE,
        PLAY,
        LOGIN,
        LINKS,
        SETTINGS,
        METHODS
    }

    private event EventHandler entryStateChange;
    public event EventHandler onEntryStateChange
    {
        add
        {
            if (entryStateChange == null || !entryStateChange.GetInvocationList().Contains(value))
                entryStateChange += value;
        }
        remove { entryStateChange -= value; }
    }

    public EntryStates CurrentEntryState
    {
        get => entryStates;
        set
        {
            entryStates = value;
            entryStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private EntryStates entryStates;
    //==================================================================
    #endregion

    //======================================================================
    [field: SerializeField] public Animator EntryAnimator { get; set; }
    [field: SerializeField] private LoginCore LoginCore { get; set; }
    [field: SerializeField] private WalletController WalletController { get; set; }
    //[field: SerializeField] private PlayerData PlayerData { get; set; }*/

    [field: Header("LOADING")]
    [field: SerializeField] private GameObject LoadingPanel { get; set; }

    [field: Header("LOGIN")]
    [field: SerializeField] public TMP_InputField UsernameLoginTMP { get; set; }
    [field: SerializeField] public TMP_InputField PasswordLoginTMP { get; set; }

    [field: Header("REMEMBER")]
    [field: SerializeField] public Toggle RememberMeToggle { get; set; }
    //======================================================================

    public void LoginButton()
    {
        if(UsernameLoginTMP.text.Length == 0)
        {
            GameManager.Instance.DisplayErrorPanel("Please input your username");
            return;
        }
        else if(PasswordLoginTMP.text.Length == 0)
        {
            GameManager.Instance.DisplayErrorPanel("Please input your password");
            return;
        }

        ProcessLogin();
    }

    public void MetaMaskButton()
    {
        DisplayLoadingPanel();
        WalletController.ConnectWallet();
    }

    private void ProcessLogin()
    {
        if(GameManager.Instance.DebugMode)
        {
            PlayerPrefs.SetString("Username", UsernameLoginTMP.text);
            PlayerPrefs.SetString("Password", PasswordLoginTMP.text);

            ResetLoginPanel();
            CurrentEntryState = EntryStates.NONE;
            GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
        }
        else
        {
            Debug.Log("made it here");
            LoginCore.LoginUserPlayfab(UsernameLoginTMP.text, PasswordLoginTMP.text);
        }
    }

    public void MetamaskLoginButton()
    {

    }

    public void ShowComingSoon()
    {
        GameManager.Instance.DisplayErrorPanel("COMING SOON");
    }
    

    public void LogOutButton()
    {
        //PlayerData.ResetPlayerData();
        PlayerPrefs.DeleteAll();
        CurrentEntryState = EntryCore.EntryStates.PLAY;
    }

    #region LINKS
    public void OpenWebsite()
    {
        Application.OpenURL("https://optibit.tech/");
    }
    public void OpenFacebook()
    {
        Application.OpenURL("https://www.facebook.com/EZMoneyPH/");
    }
    public void OpenTwitter()
    {
        Application.OpenURL("https://twitter.com/RealEZMoneyPH");
    }
    public void OpenInstagram()
    {
        Application.OpenURL("https://www.instagram.com/officialezmoneyph/");
    }
    #endregion

    #region UTILITY
    public void ResetLoginPanel()
    {
        UsernameLoginTMP.text = "";
        PasswordLoginTMP.text = "";
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void DisplayLoadingPanel()
    {
        LoadingPanel.SetActive(true);
        GameManager.Instance.PanelActivated = true;
    }

    public void HideLoadingPanel()
    {
        LoadingPanel.SetActive(false);
        GameManager.Instance.PanelActivated = false;
    }

    public void OpenMiningScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "MiningScene";
    }    
    #endregion
}
