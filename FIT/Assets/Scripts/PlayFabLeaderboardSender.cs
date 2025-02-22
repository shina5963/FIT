using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;


public class PlayFabLeaderboardSender : MonoBehaviour
{
    public ObjectSwitcher objectSwitcher;

    public TMP_Text thankText;
    public Color color1;
    public Color color2;
    public void SendScore(float score)
    {
        int newScore = Mathf.RoundToInt(score * 100); // 100倍して小数点以下2桁を保持

        // まず現在のスコアを取得して比較
        GetCurrentScore((currentScore) =>
        {
            if (currentScore == -1 || newScore < currentScore||currentScore == 0) // データがない or より良いスコアなら更新
            {
                UpdateScore(newScore);
               thankText.text = "BEST!";
                thankText.color = color1;

            }
            else
            {
                Debug.Log("newScore > currentScore. currentScore: " + currentScore / 100f + "s, currentScore: " + score + "s）");
               thankText.text = "THANK YOU!";
                thankText.color = color2;


            }
        });
    }

    private void GetCurrentScore(System.Action<int> callback)
    {
        var request = new GetPlayerStatisticsRequest();

        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == objectSwitcher.activeObjectName)
                {
                    callback(stat.Value);
                    return;
                }
            }
            // 統計データが存在しない場合
            Debug.Log("CurrentScoreNone");
            callback(-1);
        }, error =>
        {
            Debug.LogError("CurrentScoreError: " + error.GenerateErrorReport());
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
