using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class PlayFabLeaderboardUI : MonoBehaviour
{
    public GameObject rankingEntryPrefab; // ランキングの1行のテンプレート
    public Transform rankingContainer; // ランキングリストを表示する親オブジェクト

    private const string StatisticName = "HighScore"; // PlayFabの統計データ名
    public ObjectSwitcher objectSwitcher;
    public ScrollRect scrollRect; // ScrollViewのコンポーネント

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = objectSwitcher.activeObjectName,
            StartPosition = 0,
            MaxResultsCount = 100 // 上位10人を取得
        };

        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardSuccess, OnError);
    }
    private int myRankIndex = -1; // プレイヤーのランキング位置
    private void OnLeaderboardSuccess(GetLeaderboardResult result)
    {
        Debug.Log("LeaderboardSuccess");

        // 古いランキングをクリア（動的に変更しても壊れないように）
        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }
      
        // ランキングを表示
        for (int i = 0; i < result.Leaderboard.Count; i++)
        {
            var entry = result.Leaderboard[i];

            GameObject newEntry = Instantiate(rankingEntryPrefab, rankingContainer);
            // Rank, Name, Scoreのオブジェクトを取得
            Transform rankObj = newEntry.transform.Find("Rank");
            Transform nameObj = newEntry.transform.Find("Name");
            Transform scoreObj = newEntry.transform.Find("Score");

            // それぞれのオブジェクトの子（TMP_Text）を取得
            TMP_Text rankText = rankObj.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text nameText = nameObj.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text scoreText = scoreObj.GetChild(0).GetComponent<TMP_Text>();

            // データを設定
            rankText.text = $"{entry.Position + 1}"; // 1位から始まるように
            nameText.text = string.IsNullOrEmpty(entry.DisplayName) ? "Unknown" : entry.DisplayName;
            scoreText.text =  (entry.StatValue / 100f).ToString()+"s";


            // 自分のランキング位置を記録
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
            Debug.Log("自分のランキングが見つかりません！");
            return;
        }

        // スクロール位置を計算
        float totalEntries = rankingContainer.childCount;
        float targetPosition = 1f - (myRankIndex / totalEntries);

        Debug.Log($"スクロール位置: {targetPosition}");

        // ScrollRectをスムーズに移動
        StartCoroutine(SmoothScroll(targetPosition));
    }

    private System.Collections.IEnumerator SmoothScroll(float target)
    {
        float duration = 0.5f; // スクロールにかかる時間
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
