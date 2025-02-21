using System.Collections;
using UnityEngine;
using TMPro;

using UnityEngine.UI;

public class RotateMatchGame : MonoBehaviour
{
    public Transform targetObject; // �^�[�Q�b�g�I�u�W�F�N�g
    public Rigidbody playerRigidbody; // �v���C���[�I�u�W�F�N�g��Rigidbody
    public TMP_Text scoreText;
    public TMP_Text resultText;
    public Slider scoreSlider; // 🟢 追加（スライダー）
    private int score = 0;
    private Quaternion targetRotation;
    private bool isGameOver = false;
    private Vector3 previousMousePosition;

    public float rotationSpeed = 5f;
    public float damping = 0.95f;
    public float highAngularDrag = 5f;
    public float lowAngularDrag = 0.1f;

    private float startTime;
    void Start()
    {
        startTime = Time.time;
        GenerateNewTarget();
       // UpdateTimeText();
     
            //GenerateNewTarget();
          //  UpdateScoreText();
        UpdateUI(); // 🟢 変更（UI更新関数を呼ぶ）

    }

    private bool isSnapping = false;
    private float snapSpeed = 5f; // スナップのスピード

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousMousePosition = Input.mousePosition;
            playerRigidbody.angularDamping = lowAngularDrag; // 低い角速度減衰
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            if (delta.magnitude > 0)
            {
                float yaw = -delta.x * rotationSpeed;
                float pitch = delta.y * rotationSpeed;

                Vector3 torque = new Vector3(pitch, yaw, 0);
                playerRigidbody.AddTorque(torque);
                playerRigidbody.angularDamping = lowAngularDrag;
            }
            else
            {
                playerRigidbody.angularDamping = highAngularDrag;
            }

            previousMousePosition = Input.mousePosition;
        }
        else
        {
            playerRigidbody.angularDamping = lowAngularDrag;
        }

        float angleDifference = Quaternion.Angle(playerRigidbody.rotation, targetRotation);


        // 🟢 ここでカメラの背景色を変更
       
        UpdateBackgroundColor(angleDifference);
        if (angleDifference < 25f && !isGameOver && !isSnapping)
        {
            isSnapping = true; // スナップ開始
        }


        if (isSnapping)
        {
            playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, targetRotation, Time.deltaTime * snapSpeed);
            playerRigidbody.angularDamping = highAngularDrag;
            // スナップ完了判定
            if (Quaternion.Angle(playerRigidbody.rotation, targetRotation) < 5f)
            {
                playerRigidbody.rotation = targetRotation; // 最終的に完全一致
                isSnapping = false; // スナップ完了
                ScorePoint(); // スコア加算
            }
        }
    }

    // 🟢 カメラの背景色を変更する関数を追加
    private float currentIntensity = 0f; // 現在の背景色の明るさ
    public float colorLerpSpeed = 5f; // 色変化のスピード

    void UpdateBackgroundColor(float angleDifference)
    {
        if (isGameOver) return;
        // 角度差を 0（黒）～ 180（白）にマッピング
        float targetIntensity = Mathf.Clamp01(angleDifference / 180f);

        // 徐々に targetIntensity に近づくように補間
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * colorLerpSpeed);

        // 背景色を変更
        Camera.main.backgroundColor = new Color(currentIntensity, currentIntensity, currentIntensity);
    }


    void FixedUpdate()
    {
        if (!isGameOver)
        {
            playerRigidbody.angularVelocity *= damping; // ������K�p���ď��X�Ɍ���
        }

      
    }
    void ScorePoint()
    {
        score++;
        UpdateUI(); // 🟢 変更（UI更新）
        if (score >= 5)
        {
            GameOver();
        }
        else
        {
            GenerateNewTarget();
        }
    }
    void GenerateNewTarget()
    {
        targetRotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        targetObject.rotation = targetRotation;
       // playerRigidbody.rotation = Quaternion.identity; // ������
        //playerRigidbody.angularVelocity = Vector3.zero;
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score+ "/5";
    }

    void UpdateUI() // 🟢 追加（ゲージUIの更新）
    {
        scoreText.text = "Score: " + score + "/5";
        scoreSlider.value = score; // 🟢 ゲージの更新
    }

    void UpdateTimeText()
    {
      //  float elapsedTime = Time.time - startTime;
      //  timeText.text = "Time: " + elapsedTime.ToString("F2") + " seconds";
    }



    void GameOver()
    {
        //resultText.gameObject.SetActive(true);
        Camera.main.backgroundColor = new Color(0,0,0);

        isGameOver = true;
        float elapsedTime = Time.time - startTime;
        scoreText.text = "Thank you for playing!\nTime: " + elapsedTime.ToString("F2") + " seconds";
        //resultText.text = "Game Over! Time Taken: " + elapsedTime.ToString("F2") + " seconds";
    }
}
