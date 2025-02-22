using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


public class PlayFabLeaderboardSender : MonoBehaviour
{
    public ObjectSwitcher objectSwitcher;

    public void SendScore(float score)
    {
        int newScore = Mathf.RoundToInt(score * 100); // 100倍して小数点以下2桁を保持

        // まず現在のスコアを取得して比較
        GetCurrentScore((currentScore) =>
        {
            if (currentScore == -1 || newScore < currentScore) // データがない or より良いスコアなら更新
            {
                UpdateScore(newScore);
            }
            else
            {
                Debug.Log("newScore > currentScore. currentScore: " + currentScore / 100f + "s, currentScore: " + score + "s）");
            }
        });
    }

    private void GetCurrentScore(System.Action<int> callback)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = objectSwitcher.activeObjectName,
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, result =>
        {
            if (result.Leaderboard.Count > 0)
            {
                int currentScore = result.Leaderboard[0].StatValue;
                callback(currentScore);
            }
            else
            {
                Debug.Log("NoCurrentScore");
                callback(-1); // データがない場合
            }
        }, error =>
        {
            Debug.LogError("NoCurrentScoreError: " + error.GenerateErrorReport());
            callback(-1); // エラー時も更新できるように
        });
    }

    private void UpdateScore(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = objectSwitcher.activeObjectName, Value = score }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnSuccess, OnError);
    }

    private void OnSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("SendScoreSuccess");
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("SendScoreError: " + error.GenerateErrorReport());
    }
}
