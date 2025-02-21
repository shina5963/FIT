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

        // PlayerPrefs�ɕۑ����ꂽID�����邩�m�F
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            // ������ID���擾
            uniqueID = PlayerPrefs.GetString(PlayerPrefsKey);
        }
        else
        {
            // �V����GUID�𐶐�
            uniqueID = Guid.NewGuid().ToString();
            // PlayerPrefs�ɕۑ�
            PlayerPrefs.SetString(PlayerPrefsKey, uniqueID);
            PlayerPrefs.Save();
        }

        // PlayFab�Ƀ��O�C��
        var request = new LoginWithCustomIDRequest
        {
            CustomId = uniqueID,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("���O�C�������I");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("���O�C�����s: " + error.GenerateErrorReport());
    }
}
