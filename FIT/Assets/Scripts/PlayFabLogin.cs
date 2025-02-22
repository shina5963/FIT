using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayFabLogin : MonoBehaviour
{
    private const string PlayerPrefsKey = "PlayFabUniqueID";

    void Start()
    {
        string uniqueID;

        // PlayerPrefsに保存されたIDがあるか確認
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            // 既存のIDを取得
            uniqueID = PlayerPrefs.GetString(PlayerPrefsKey);
            print("ID"+uniqueID);
        }
        else
        {
            // 新しいGUIDを生成
            uniqueID = Guid.NewGuid().ToString();
            print("NewID" + uniqueID);
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
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("LoginFailure: " + error.GenerateErrorReport());
    }
}
