using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


public class PlayFabLeaderboardSender : MonoBehaviour
{
    public ObjectSwitcher objectSwitcher;

    public void SendScore(float score)
    {
        int newScore = Mathf.RoundToInt(score * 100); // 100�{���ď����_�ȉ�2����ێ�

        // �܂����݂̃X�R�A���擾���Ĕ�r
        GetCurrentScore((currentScore) =>
        {
            if (currentScore == -1 || newScore < currentScore) // �f�[�^���Ȃ� or ���ǂ��X�R�A�Ȃ�X�V
            {
                UpdateScore(newScore);
            }
            else
            {
                Debug.Log("newScore > currentScore. currentScore: " + currentScore / 100f + "s, currentScore: " + score + "s�j");
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
                callback(-1); // �f�[�^���Ȃ��ꍇ
            }
        }, error =>
        {
            Debug.LogError("NoCurrentScoreError: " + error.GenerateErrorReport());
            callback(-1); // �G���[�����X�V�ł���悤��
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
