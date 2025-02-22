using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class PlayFabLeaderboardUI : MonoBehaviour
{
    public GameObject rankingEntryPrefab; // �����L���O��1�s�̃e���v���[�g
    public Transform rankingContainer; // �����L���O���X�g��\������e�I�u�W�F�N�g

    private const string StatisticName = "HighScore"; // PlayFab�̓��v�f�[�^��
    public ObjectSwitcher objectSwitcher;
    public ScrollRect scrollRect; // ScrollView�̃R���|�[�l���g

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = objectSwitcher.activeObjectName,
            StartPosition = 0,
            MaxResultsCount = 100 // ���10�l���擾
        };

        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardSuccess, OnError);
    }
    private int myRankIndex = -1; // �v���C���[�̃����L���O�ʒu
    private void OnLeaderboardSuccess(GetLeaderboardResult result)
    {
        Debug.Log("LeaderboardSuccess");

        // �Â������L���O���N���A�i���I�ɕύX���Ă����Ȃ��悤�Ɂj
        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }
      
        // �����L���O��\��
        for (int i = 0; i < result.Leaderboard.Count; i++)
        {
            var entry = result.Leaderboard[i];

            GameObject newEntry = Instantiate(rankingEntryPrefab, rankingContainer);
            // Rank, Name, Score�̃I�u�W�F�N�g���擾
            Transform rankObj = newEntry.transform.Find("Rank");
            Transform nameObj = newEntry.transform.Find("Name");
            Transform scoreObj = newEntry.transform.Find("Score");

            // ���ꂼ��̃I�u�W�F�N�g�̎q�iTMP_Text�j���擾
            TMP_Text rankText = rankObj.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text nameText = nameObj.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text scoreText = scoreObj.GetChild(0).GetComponent<TMP_Text>();

            // �f�[�^��ݒ�
            rankText.text = $"{entry.Position + 1}"; // 1�ʂ���n�܂�悤��
            nameText.text = string.IsNullOrEmpty(entry.DisplayName) ? "Unknown" : entry.DisplayName;
            scoreText.text =  (entry.StatValue / 100f).ToString()+"s";


            // �����̃����L���O�ʒu���L�^
            if (entry.PlayFabId == PlayFabSettings.staticPlayer.PlayFabId)
            {
                myRankIndex = i;
            }

            //  TextMeshProUGUI textComponent = newEntry.GetComponent<TextMeshProUGUI>();

            //  string playerName = string.IsNullOrEmpty(entry.DisplayName) ? "Unknown" : entry.DisplayName;
            // textComponent.text = $"{entry.Position + 1}. {playerName} - {entry.StatValue}";
        }
    }
    public void ScrollToMyRank()
    {
        if (myRankIndex == -1)
        {
            Debug.Log("�����̃����L���O��������܂���I");
            return;
        }

        // �X�N���[���ʒu���v�Z
        float totalEntries = rankingContainer.childCount;
        float targetPosition = 1f - (myRankIndex / totalEntries);

        Debug.Log($"�X�N���[���ʒu: {targetPosition}");

        // ScrollRect���X���[�Y�Ɉړ�
        StartCoroutine(SmoothScroll(targetPosition));
    }

    private System.Collections.IEnumerator SmoothScroll(float target)
    {
        float duration = 0.5f; // �X�N���[���ɂ����鎞��
        float elapsedTime = 0f;
        float startValue = scrollRect.verticalNormalizedPosition;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startValue, target, elapsedTime / duration);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = target;
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("LeaderboardError: " + error.GenerateErrorReport());
    }
}
