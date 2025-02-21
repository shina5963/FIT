using UnityEngine;
using UnityEngine.UI;

public class ObjectSwitcher : MonoBehaviour
{
    public GameObject[] objects; // 切り替えるオブジェクト配列
    public GameObject Target;    // 子オブジェクトを探す親オブジェクト
    public Button switchButton;  // ボタン

    private int currentIndex = 0; // 現在のアクティブオブジェクトのインデックス

    void Start()
    {
        if (switchButton != null)
        {
            switchButton.onClick.AddListener(SwitchObject);
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
    }

    void UpdateActiveObjects()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            bool isActive = (i == currentIndex);
            objects[i].SetActive(isActive);
        }

        // `objects[currentIndex]` と同じ名前を持つ `Target` の子オブジェクトを探す
        string activeObjectName = objects[currentIndex].name;
        foreach (Transform child in Target.transform)
        {
            child.gameObject.SetActive(child.gameObject.name == activeObjectName);
        }
    }
}
