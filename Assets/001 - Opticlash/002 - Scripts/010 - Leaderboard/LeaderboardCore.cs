using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using TMPro;

public class LeaderboardCore : MonoBehaviour
{
    #region VARIABLES
    //============================================================
    [field: SerializeField] private PlayerData PlayerData { get; set; }
    [field: SerializeField] private LobbyCore LobbyCore { get; set; }
    [field: SerializeField] private List<PlacementCore> PlacementList { get; set; }
    [field: SerializeField] private PlacementCore MyPlacement { get; set; }
    [field: SerializeField] private TextMeshProUGUI MyPlacementTMP { get; set; }


    [Header("DEBUGGER")]
    private int failedCallbackCounter;
    //============================================================
    #endregion

    public void InitializeLeaderboard()
    {
        foreach(PlacementCore placement in PlacementList)
            placement.gameObject.SetActive(false);
        

        if(GameManager.Instance.DebugMode)
        {
            PlacementList[0].gameObject.SetActive(true);
            PlacementList[0].NameTMP.text = PlayerData.DisplayName;
            PlacementList[0].ScoreTMP.text = PlayerData.TotalKillCount.ToString();

            MyPlacement.gameObject.SetActive(true);
            MyPlacementTMP.text = "1st";
            MyPlacement.NameTMP.text = PlayerData.DisplayName;
            MyPlacement.ScoreTMP.text = PlayerData.TotalKillCount.ToString();
        }
        else
        {
            GetLeaderboardRequest getLeaderboard = new GetLeaderboardRequest();
            getLeaderboard.MaxResultsCount = 10;
            getLeaderboard.StartPosition = 0;
            getLeaderboard.StatisticName = "TotalKillCount";

            PlayFabClientAPI.GetLeaderboard(getLeaderboard,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    foreach(PlayerLeaderboardEntry player in resultCallback.Leaderboard)
                    {
                        PlacementList[player.Position].gameObject.SetActive(true);
                        PlacementList[player.Position].NameTMP.text = player.DisplayName;
                        PlacementList[player.Position].ScoreTMP.text = player.StatValue.ToString();
                    }
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        InitializeLeaderboard,
                        () => ProcessError(errorCallback.ErrorMessage));
                });

            GetLeaderboardAroundPlayerRequest getLeaderboardAroundPlayer = new GetLeaderboardAroundPlayerRequest();
            getLeaderboardAroundPlayer.PlayFabId = PlayerData.PlayfabID;
            getLeaderboardAroundPlayer.StatisticName = "TotalKillCount";
            getLeaderboardAroundPlayer.MaxResultsCount = 1;
            PlayFabClientAPI.GetLeaderboardAroundPlayer(getLeaderboardAroundPlayer,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    MyPlacement.gameObject.SetActive(true);
                    if (resultCallback.Leaderboard[0].Position == 0)
                        MyPlacementTMP.text = "1st";
                    else if (resultCallback.Leaderboard[0].Position == 1)
                        MyPlacementTMP.text = "2nd";
                    else if (resultCallback.Leaderboard[0].Position == 2)
                        MyPlacementTMP.text = "3rd";
                    else
                        MyPlacementTMP.text = (resultCallback.Leaderboard[0].Position + 1).ToString() + "th";

                    MyPlacement.NameTMP.text = resultCallback.Leaderboard[0].DisplayName.ToString();
                    MyPlacement.ScoreTMP.text = resultCallback.Leaderboard[0].StatValue.ToString();
                },
                errorCallback =>
                {
                    ErrorCallback(errorCallback.Error,
                        InitializeLeaderboard,
                        () => ProcessError(errorCallback.ErrorMessage));
                });
        }
    }

    #region UTILITY
    private void ErrorCallback(PlayFabErrorCode errorCode, Action restartAction, Action errorAction)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                ProcessError("Connectivity error. Please connect to strong internet");
            else
                restartAction();
        }
        else if (errorCode == PlayFabErrorCode.InternalServerError)
            ProcessSpecialError();
        else
            errorAction();
    }

    private void ProcessError(string errorMessage)
    {
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        LobbyCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
