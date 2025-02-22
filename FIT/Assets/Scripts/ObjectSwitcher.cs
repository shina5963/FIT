using UnityEngine;
using TMPro;

using UnityEngine.UI;

public class ObjectSwitcher : MonoBehaviour
{
    public GameObject[] objects; // 切り替えるオブジェクト配列
    public GameObject Target;    // 子オブジェクトを探す親オブジェクト
    public Button switchButton;  // ボタン
    public Button RankButton;  // ボタン

    private int currentIndex = 0; // 現在のアクティブオブジェクトのインデックス

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

        // 初期状態を設定
        UpdateActiveObjects();
    }

    void SwitchObject()
    {
        // インデックスを進める（ループする）
        currentIndex = (currentIndex + 1) % objects.Length;

        // アクティブなオブジェクトを更新
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

        // `objects[currentIndex]` と同じ名前を持つ `Target` の子オブジェクトを探す
        activeObjectName = objects[currentIndex].name;

        nameText.text = activeObjectName;

        foreach (Transform child in Target.transform)
        {
            child.gameObject.SetActive(child.gameObject.name == activeObjectName);
        }
    }
}
