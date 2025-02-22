using UnityEngine;
using TMPro;

using UnityEngine.UI;

public class ObjectSwitcher : MonoBehaviour
{
    public GameObject[] objects; // �؂�ւ���I�u�W�F�N�g�z��
    public GameObject Target;    // �q�I�u�W�F�N�g��T���e�I�u�W�F�N�g
    public Button switchButton;  // �{�^��
    public Button RankButton;  // �{�^��

    private int currentIndex = 0; // ���݂̃A�N�e�B�u�I�u�W�F�N�g�̃C���f�b�N�X

    void Start()
    {
        if (switchButton != null)
        {
            switchButton.onClick.AddListener(SwitchObject);
        }

        if (RankButton != null)
        {
            RankButton.onClick.AddListener(ShowRank);
        }

        // ������Ԃ�ݒ�
        UpdateActiveObjects();
    }

    void SwitchObject()
    {
        // �C���f�b�N�X��i�߂�i���[�v����j
        currentIndex = (currentIndex + 1) % objects.Length;

        // �A�N�e�B�u�ȃI�u�W�F�N�g���X�V
        UpdateActiveObjects();
        if (RankUI.activeSelf)
            playFabLeaderboardUI.GetLeaderboard();

        thankText.gameObject.SetActive(false);
    }
    public GameObject RankUI;
    public PlayFabLeaderboardUI playFabLeaderboardUI;
    void ShowRank()
    {
        if (RankUI.activeSelf)
        {
            RankUI.SetActive(false);
            Time.timeScale = 1f;
        }
        else { RankUI.SetActive(true);
            Time.timeScale = 0f;
            playFabLeaderboardUI.GetLeaderboard();
        }
    }
    public string activeObjectName;
    public TMP_Text nameText;
    public TMP_Text thankText;

    void UpdateActiveObjects()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            bool isActive = (i == currentIndex);
            objects[i].SetActive(isActive);
        }

        // `objects[currentIndex]` �Ɠ������O������ `Target` �̎q�I�u�W�F�N�g��T��
        activeObjectName = objects[currentIndex].name;

        nameText.text = activeObjectName;

        foreach (Transform child in Target.transform)
        {
            child.gameObject.SetActive(child.gameObject.name == activeObjectName);
        }
    }
}
