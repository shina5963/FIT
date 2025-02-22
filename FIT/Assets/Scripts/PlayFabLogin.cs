using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class PlayFabLogin : MonoBehaviour
{
    private const string PlayerPrefsKey = "PlayFabUniqueID";
    private const string PlayerNumberKey = "PlayFabPlayerNumber"; // 何人目のユーザーかを保存するキー
    private const string RegistrationRankStatistic = "PlayerRegistration"; // 累積登録数の統計名

    void Start()
    {
        string uniqueID;

        // PlayerPrefsに保存されたIDがあるか確認
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            // 既存のIDを取得
            uniqueID = PlayerPrefs.GetString(PlayerPrefsKey);
            print("ID: " + uniqueID);
        }
        else
        {
            // 新しいGUIDを生成
            uniqueID = Guid.NewGuid().ToString();
            print("NewID: " + uniqueID);
            // PlayerPrefsに保存
            PlayerPrefs.SetString(PlayerPrefsKey, uniqueID);
            PlayerPrefs.Save();
        }

        // PlayFabにログイン
        var request = new LoginWithCustomIDRequest
        {
            CustomId = uniqueID,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("LoginSuccess");

        // DisplayNameがすでに設定されているか確認
        CheckDisplayName();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("LoginFailure: " + error.GenerateErrorReport());
    }

    /// <summary>
    /// `DisplayName` が設定されているかチェックし、未設定なら `user-番号` に設定
    /// </summary>
    private void CheckDisplayName()
    {
        var request = new GetAccountInfoRequest();

        PlayFabClientAPI.GetAccountInfo(request, result =>
        {
            string displayName = result.AccountInfo.TitleInfo.DisplayName;

            if (!string.IsNullOrEmpty(displayName))
            {
                Debug.Log($"既存の DisplayName を使用: {displayName}");
                return; // すでに設定されている場合、`user-番号` に変更しない
            }

            // `PlayerRegistration` に登録済みか確認
            CheckIfPlayerRegistered();
        }, error =>
        {
            Debug.LogError("DisplayName 確認エラー: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// `PlayerRegistration` に登録済みかチェックし、未登録なら追加
    /// </summary>
    private void CheckIfPlayerRegistered()
    {
        var request = new GetPlayerStatisticsRequest();

        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            bool isRegistered = false;

            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == RegistrationRankStatistic)
                {
                    isRegistered = true;
                    break;
                }
            }

            if (!isRegistered)
            {
                RegisterPlayerInLeaderboard();
            }

            // すでに「何人目の会員か」が保存されているか確認
            if (!PlayerPrefs.HasKey(PlayerNumberKey))
            {
                GetAndSavePlayerNumber();
            }
            else
            {
                int playerNumber = PlayerPrefs.GetInt(PlayerNumberKey);
                SetUsername($"user-{playerNumber}"); // すでに保存されていればユーザー名を設定
            }
        }, error =>
        {
            Debug.LogError("PlayerRegistration 確認エラー: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// `PlayerRegistration` にデータを送信し、新しいプレイヤーを登録する
    /// </summary>
    private void RegisterPlayerInLeaderboard()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = RegistrationRankStatistic, Value = 1 } // 初回登録時に1をセット
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            Debug.Log("ユーザー登録ランキングに追加完了！");
        }, error =>
        {
            Debug.LogError("ユーザー登録ランキング追加失敗: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// プレイヤーの登録番号を取得し、`PlayerPrefs` に保存 & `user-番号` を `DisplayName` に設定
    /// </summary>
    private void GetAndSavePlayerNumber()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = RegistrationRankStatistic,
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, result =>
        {
            if (result.Leaderboard.Count > 0)
            {
                int playerNumber = result.Leaderboard[0].Position + 1; // 0-based index → 1-based に修正
                PlayerPrefs.SetInt(PlayerNumberKey, playerNumber);
                PlayerPrefs.Save();
                Debug.Log("プレイヤー番号を保存: " + playerNumber);

                // `user-番号` を設定
                SetUsername($"user-{playerNumber}");
            }
            else
            {
                Debug.LogError("プレイヤー番号が取得できませんでした。");
            }
        }, error =>
        {
            Debug.LogError("プレイヤー番号取得エラー: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// `DisplayName` を `user-番号` に設定する
    /// </summary>
    private void SetUsername(string newUsername)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newUsername
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, result =>
        {
            Debug.Log($"ユーザー名を {newUsername} に設定しました！");
        }, error =>
        {
            Debug.LogError("ユーザー名設定失敗: " + error.GenerateErrorReport());
        });
    }
}
