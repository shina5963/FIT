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
        int newScore = Mathf.RoundToInt(score * 100); // 100�{���ď����_�ȉ�2����ێ�

        // �܂����݂̃X�R�A���擾���Ĕ�r
        GetCurrentScore((currentScore) =>
        {
            if (currentScore == -1 || newScore < currentScore||currentScore == 0) // �f�[�^���Ȃ� or ���ǂ��X�R�A�Ȃ�X�V
            {
                UpdateScore(newScore);
               thankText.text = "BEST!";
                thankText.color = color1;

            }
            else
            {
                Debug.Log("newScore > currentScore. currentScore: " + currentScore / 100f + "s, currentScore: " + score + "s�j");
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
            // ���v�f�[�^�����݂��Ȃ��ꍇ
            Debug.Log("CurrentScoreNone");
            callback(-1);
        }, error =>
        {
            Debug.LogError("CurrentScoreError: " + error.GenerateErrorReport());
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
