using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class PlayFabLogin : MonoBehaviour
{
    private const string PlayerPrefsKey = "PlayFabUniqueID";
    private const string PlayerNumberKey = "PlayFabPlayerNumber"; // ���l�ڂ̃��[�U�[����ۑ�����L�[
    private const string RegistrationRankStatistic = "PlayerRegistration"; // �ݐϓo�^���̓��v��

    void Start()
    {
        string uniqueID;

        // PlayerPrefs�ɕۑ����ꂽID�����邩�m�F
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            // ������ID���擾
            uniqueID = PlayerPrefs.GetString(PlayerPrefsKey);
            print("ID: " + uniqueID);
        }
        else
        {
            // �V����GUID�𐶐�
            uniqueID = Guid.NewGuid().ToString();
            print("NewID: " + uniqueID);
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
        Debug.Log("LoginSuccess");

        // DisplayName�����łɐݒ肳��Ă��邩�m�F
        CheckDisplayName();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("LoginFailure: " + error.GenerateErrorReport());
    }

    /// <summary>
    /// `DisplayName` ���ݒ肳��Ă��邩�`�F�b�N���A���ݒ�Ȃ� `user-�ԍ�` �ɐݒ�
    /// </summary>
    private void CheckDisplayName()
    {
        var request = new GetAccountInfoRequest();

        PlayFabClientAPI.GetAccountInfo(request, result =>
        {
            string displayName = result.AccountInfo.TitleInfo.DisplayName;

            if (!string.IsNullOrEmpty(displayName))
            {
                Debug.Log($"������ DisplayName ���g�p: {displayName}");
                return; // ���łɐݒ肳��Ă���ꍇ�A`user-�ԍ�` �ɕύX���Ȃ�
            }

            // `PlayerRegistration` �ɓo�^�ς݂��m�F
            CheckIfPlayerRegistered();
        }, error =>
        {
            Debug.LogError("DisplayName �m�F�G���[: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// `PlayerRegistration` �ɓo�^�ς݂��`�F�b�N���A���o�^�Ȃ�ǉ�
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

            // ���łɁu���l�ڂ̉�����v���ۑ�����Ă��邩�m�F
            if (!PlayerPrefs.HasKey(PlayerNumberKey))
            {
                GetAndSavePlayerNumber();
            }
            else
            {
                int playerNumber = PlayerPrefs.GetInt(PlayerNumberKey);
                SetUsername($"user-{playerNumber}"); // ���łɕۑ�����Ă���΃��[�U�[����ݒ�
            }
        }, error =>
        {
            Debug.LogError("PlayerRegistration �m�F�G���[: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// `PlayerRegistration` �Ƀf�[�^�𑗐M���A�V�����v���C���[��o�^����
    /// </summary>
    private void RegisterPlayerInLeaderboard()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = RegistrationRankStatistic, Value = 1 } // ����o�^����1���Z�b�g
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            Debug.Log("���[�U�[�o�^�����L���O�ɒǉ������I");
        }, error =>
        {
            Debug.LogError("���[�U�[�o�^�����L���O�ǉ����s: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// �v���C���[�̓o�^�ԍ����擾���A`PlayerPrefs` �ɕۑ� & `user-�ԍ�` �� `DisplayName` �ɐݒ�
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
                int playerNumber = result.Leaderboard[0].Position + 1; // 0-based index �� 1-based �ɏC��
                PlayerPrefs.SetInt(PlayerNumberKey, playerNumber);
                PlayerPrefs.Save();
                Debug.Log("�v���C���[�ԍ���ۑ�: " + playerNumber);

                // `user-�ԍ�` ��ݒ�
                SetUsername($"user-{playerNumber}");
            }
            else
            {
                Debug.LogError("�v���C���[�ԍ����擾�ł��܂���ł����B");
            }
        }, error =>
        {
            Debug.LogError("�v���C���[�ԍ��擾�G���[: " + error.GenerateErrorReport());
        });
    }

    /// <summary>
    /// `DisplayName` �� `user-�ԍ�` �ɐݒ肷��
    /// </summary>
    private void SetUsername(string newUsername)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newUsername
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, result =>
        {
            Debug.Log($"���[�U�[���� {newUsername} �ɐݒ肵�܂����I");
        }, error =>
        {
            Debug.LogError("���[�U�[���ݒ莸�s: " + error.GenerateErrorReport());
        });
    }
}
