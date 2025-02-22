using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.TextCore.Text;

public class PlayFabLeaderboardUI : MonoBehaviour
{
    public GameObject rankingEntryPrefab; // ランキングの1行のテンプレート
    public Transform rankingContainer; // ランキングリストを表示する親オブジェクト

    private const string StatisticName = "HighScore"; // PlayFabの統計データ名
    public ObjectSwitcher objectSwitcher;
    public ScrollRect scrollRect; // ScrollViewのコンポーネント
    public Button renameButton;  // ボタン

    public TMP_InputField inputFieldPrefab; // 🎯 TMP_InputField のプレハブ

    private TMP_InputField activeInputField; // 現在編集中の入力フィールド
    private TMP_Text activeNameText; // 編集対象の元のテキスト

    private void Start()
    {
        if (renameButton != null)
        {
           renameButton.onClick.AddListener(ScrollToMyRank);
          

        }

    }
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
    TMP_Text myNameText;
    private void OnLeaderboardSuccess(GetLeaderboardResult result)
    {
        Debug.Log("LeaderboardSuccess");

        // 古いランキングをクリア（動的に変更しても壊れないように）
        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }
        errorText.gameObject.SetActive(false);

        // PlayFab のリーダーボードはデフォルトで「大きい順」なので、小さい順にソート
        result.Leaderboard.Sort((a, b) => a.StatValue.CompareTo(b.StatValue));
        myNameText = null;
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
            rankText.text = $"{i + 1}"; // 1位から始まるように
            nameText.text = string.IsNullOrEmpty(entry.DisplayName) ? "Unknown" : entry.DisplayName;
            scoreText.text =  (entry.StatValue / 100f).ToString()+"s";


            // 自分のランキング位置を記録
            if (entry.PlayFabId == PlayFabSettings.staticPlayer.PlayFabId)
            {
                myRankIndex = i;

                // 名前変更を可能にする
                nameText.gameObject.AddComponent<Button>().onClick.AddListener(() => EditName(nameText));

                myNameText=nameText; 
            }

            //  TextMeshProUGUI textComponent = newEntry.GetComponent<TextMeshProUGUI>();

            //  string playerName = string.IsNullOrEmpty(entry.DisplayName) ? "Unknown" : entry.DisplayName;
            // textComponent.text = $"{entry.Position + 1}. {playerName} - {entry.StatValue}";
        }
        if (myNameText==null)
            renameButton.gameObject.SetActive(false);
        else
            renameButton.gameObject.SetActive(true);

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

        scrollRect.verticalNormalizedPosition = targetPosition;
        EditName(myNameText);

    }
    private void EditName(TMP_Text nameText)
    {
        if (activeInputField != null) return; // すでに入力フィールドがある場合は何もしない

        activeNameText = nameText;
        activeNameText.gameObject.SetActive(false); // 元の名前を非表示にする

        // 🎯 TMP_InputField プレハブを正しく Instantiate
        activeInputField = Instantiate(inputFieldPrefab, nameText.transform.parent);
        activeInputField.gameObject.SetActive(true);
        activeInputField.text = nameText.text;
        activeInputField.pointSize = nameText.fontSize;
        activeInputField.transform.position = nameText.transform.position;

        // 🎯 Enter キーを押したら ConfirmRename() を発動
        activeInputField.onSubmit.AddListener(delegate { ConfirmRename(); });

        // 🎯 フォーカスを自動で設定
        activeInputField.Select();
        activeInputField.ActivateInputField();
    }

    /// <summary>
    /// ユーザーが `Rename` ボタンを押したときに名前を確定
    /// </summary>
    private void ConfirmRename()
    {
        if (activeInputField == null) return;

        string newUsername = activeInputField.text;
        SetUsername(newUsername);
    }

    /// <summary>
    /// PlayFab の `DisplayName` を変更
    /// </summary>
    /// 
    public TMP_Text errorText;
    private void SetUsername(string newUsername, int retryCount = 0)
    {

        // 🎯 1. ユーザー名の長さをチェック
        if (newUsername.Length > 8|| newUsername.Length <3)
        {
           // Debug.LogError("Username too long: Must be under 9 characters.");
            errorText.gameObject.SetActive(true);
            errorText.text = "between 3 and 8 characters";
            return; // PlayFab に送信しない
        }

        // 🎯 2. ユーザー名のフォーマットをチェック（英数字のみ）
        if (!System.Text.RegularExpressions.Regex.IsMatch(newUsername, "^[a-zA-Z0-9_-]+$"))
        {
            Debug.LogError("Invalid username format: Please use alphanumeric characters only.");
            errorText.gameObject.SetActive(true);
            errorText.text = "alphanumeric characters only";
            return; // PlayFab に送信しない
        }

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newUsername
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, result =>
        {
            Debug.Log($"ユーザー名を {newUsername} に設定しました！");

            // 🎯 確定したら元のテキストに戻す
            activeNameText.text = newUsername;
            activeNameText.gameObject.SetActive(true);
            errorText.gameObject.SetActive(false);
            Destroy(activeInputField.gameObject);
            activeInputField = null;

            // 🎯 ランキングを再取得して表示を更新
            //GetLeaderboard();

        }, error =>
        {
            if (error.ErrorMessage.Contains("Name not available") && retryCount < 5)
            {
                // ユーザー名がすでに存在する場合、ランダムな数字を追加して再試行
                //string newRandomName = $"{newUsername}-{UnityEngine.Random.Range(1000, 9999)}";
                //Debug.LogWarning($"ユーザー名が重複。新しい名前を試す: {newRandomName}");

                //SetUsername(newRandomName, retryCount + 1);
                //Debug.LogWarning($"The username is already in use. Trying a new one: {newRandomName}");
                errorText.gameObject.SetActive(true);
                errorText.text = "already in use";
            }
          
            else
            {
                Debug.LogError("ユーザー名設定失敗: " + error.GenerateErrorReport());
                errorText.gameObject.SetActive(true);
                errorText.text = "error";
            }
        });
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("LeaderboardError: " + error.GenerateErrorReport());
    }
}
